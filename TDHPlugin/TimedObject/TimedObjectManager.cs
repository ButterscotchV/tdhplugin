using System;
using System.Collections.Generic;
using System.Threading;

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
					throw new ArgumentException("Timeout must not be less than 0", nameof(CheckDelay));

				checkDelay = value;
			}
		}

		public List<TimedObject<T>> timedObjects = new List<TimedObject<T>>();

		public Thread CheckThread { get; private set; }

		public TimedObjectManager(TimeSpan checkDelay)
		{
			CheckDelay = checkDelay;
			Run();
		}

		public TimedObjectManager(long checkDelay, TimeUnit checkUnit = TimeUnit.Seconds) : this(TimeUtils.TimeUnitToTimeSpan(checkDelay, checkUnit))
		{
		}

		public void Run()
		{
			CheckThread = new Thread(TimeoutCheckThread);
			CheckThread.Start();
		}

		public void Shutdown()
		{
			CheckThread.Abort();
		}

		public void TimeoutCheckThread()
		{
			while (true)
			{
				Thread.Sleep(CheckDelay);

				lock (timedObjects)
				{
					for (int i = timedObjects.Count - 1; i >= 0; i--)
					{
						TimedObject<T> timedObject = timedObjects[i];

						if (timedObject.IsExpired())
						{
							timedObjects.RemoveAt(i);

							timedObject.onExpire?.Invoke(timedObject);
						}
					}
				}
			}
		}

		public TimedObject<T> FinishTimedObject(TimedObject<T> timedObject, bool executeOnFinish = true)
		{
			lock (timedObjects)
			{
				if (!timedObjects.Contains(timedObject))
					return null;

				timedObjects.Remove(timedObject);
			}

			if (executeOnFinish)
				timedObject.onFinish?.Invoke(timedObject);

			return timedObject;
		}
	}
}