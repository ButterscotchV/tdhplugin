using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace TDHPlugin.TimedObject
{
	public class TimedObjectManager<T>
	{
		private TimeSpan checkDelay;

		public TimeSpan CheckDelay
		{
			get => checkDelay;

			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentException($"{nameof(CheckDelay)} must not be less than 0", nameof(CheckDelay));

				checkDelay = value;
			}
		}

		public List<TimedObject<T>> timedObjects = new List<TimedObject<T>>();

		public Thread CheckThread { get; private set; }

		public TimedObjectManager(TimeSpan? checkDelay = null, bool runOnInit = false)
		{
			CheckDelay = checkDelay ?? TimeSpan.FromSeconds(1);

			if (runOnInit)
				Run();
		}

		public TimedObjectManager(long checkDelay, TimeUnit checkUnit = TimeUnit.Seconds, bool runOnInit = false) : this(TimeUtils.TimeUnitToTimeSpan(checkDelay, checkUnit), runOnInit)
		{
		}

		public void Run()
		{
			CheckThread = new Thread(TimeoutCheckThread);
			CheckThread.Start();
		}

		public void Shutdown()
		{
			CheckThread?.Abort();
			CheckThread = null;
			CheckAndExpireObjects();
		}

		public void TimeoutCheckThread()
		{
			while (true)
			{
				Thread.Sleep(CheckDelay);

				CheckAndExpireObjects();
			}
		}

		public void CheckAndExpireObjects()
		{
			try
			{
				lock (timedObjects)
				{
					for (int i = timedObjects.Count - 1; i >= 0; i--)
					{
						TimedObject<T> timedObject = timedObjects[i];

						if (timedObject.finished)
						{
							FinishTimedObject(i);
						}
						else if (timedObject.IsExpired())
						{
							timedObjects.RemoveAt(i);

							timedObject.onExpire?.Invoke(timedObject);
						}
					}
				}
			}
			catch (Exception e)
			{
				TDHPlugin.WriteError($"Error in {nameof(CheckAndExpireObjects)}:\n{e}");
			}
		}

		public TimedObject<T> FinishTimedObject(int timedObjectIndex, bool executeOnFinish = true)
		{
			TimedObject<T> timedObject;

			lock (timedObjects)
			{
				if (timedObjectIndex < 0 || timedObjectIndex >= timedObjects.Count)
					return null;

				timedObject = timedObjects[timedObjectIndex];
				timedObjects.RemoveAt(timedObjectIndex);
			}

			if (executeOnFinish)
				timedObject.onFinish?.Invoke(timedObject);

			return timedObject;
		}

		public TimedObject<T> FinishTimedObject([NotNull] TimedObject<T> timedObject, bool executeOnFinish = true)
		{
			lock (timedObjects)
			{
				int timedObjectIndex = timedObjects.IndexOf(timedObject);

				return FinishTimedObject(timedObjectIndex, executeOnFinish);
			}
		}
	}
}