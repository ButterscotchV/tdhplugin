using System;
using JetBrains.Annotations;

namespace TDHPlugin.TimedObject
{
	public class TimedObject<T>
	{
		[NotNull] public readonly T obj;

		private TimeSpan timeout;

		public TimeSpan Timeout
		{
			get => timeout;

			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentException("Timeout must not be less than 0", nameof(Timeout));

				timeout = value;
				TimeoutMillis = value.TotalMilliseconds;
				ExpirationDateTime = StartDateTime + value;
			}
		}

		public double TimeoutMillis { get; private set; }

		private DateTime startDateTime;

		public DateTime StartDateTime
		{
			get => startDateTime;

			set
			{
				startDateTime = value;
				ExpirationDateTime = value + timeout;
			}
		}

		public DateTime ExpirationDateTime { get; private set; }

		public Action<TimedObject<T>> onFinish;
		public Action<TimedObject<T>> onExpire;

		public bool finished;

		public TimedObject([NotNull] T obj, TimeSpan timeout, DateTime? startDateTime = null, Action<TimedObject<T>> onFinish = null, Action<TimedObject<T>> onExpire = null)
		{
			this.obj = obj;
			Timeout = timeout;
			StartDateTime = startDateTime ?? DateTime.Now;
			this.onFinish = onFinish;
			this.onExpire = onExpire;
		}

		public TimedObject([NotNull] T obj, long timeout, TimeUnit timeoutUnit, DateTime? startDateTime = null, Action<TimedObject<T>> onFinish = null, Action<TimedObject<T>> onExpire = null) : this(obj, TimeUtils.TimeUnitToTimeSpan(timeout, timeoutUnit), startDateTime, onFinish, onExpire)
		{
		}

		public long GetTimeoutTime(TimeUnit timeUnit = TimeUnit.Seconds)
		{
			return Timeout.ToTimeUnit(timeUnit);
		}

		public long GetTimeToExpiration(DateTime? dateTime = null, TimeUnit timeUnit = TimeUnit.Seconds)
		{
			DateTime time = dateTime ?? DateTime.Now;

			return (ExpirationDateTime - time).ToTimeUnit(timeUnit);
		}

		public bool IsExpired(DateTime? dateTime = null)
		{
			DateTime time = dateTime ?? DateTime.Now;

			return ExpirationDateTime - time < TimeSpan.Zero;
		}
	}
}