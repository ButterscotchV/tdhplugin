using System;

namespace TDHPlugin.TimedObject
{
	public enum TimeUnit
	{
		Ticks,
		Millis,
		Seconds,
		Minutes,
		Hours,
		Days
	}

	public static class TimeUtils
	{
		public static long TimeSpanToTimeUnit(TimeSpan timeSpan, TimeUnit timeUnit = TimeUnit.Seconds)
		{
			switch (timeUnit)
			{
				case TimeUnit.Ticks:
					return timeSpan.Ticks;
				case TimeUnit.Millis:
					return (long) timeSpan.TotalMilliseconds;
				case TimeUnit.Seconds:
					return (long) timeSpan.TotalSeconds;
				case TimeUnit.Minutes:
					return (long) timeSpan.TotalMinutes;
				case TimeUnit.Hours:
					return (long) timeSpan.TotalHours;
				case TimeUnit.Days:
					return (long) timeSpan.TotalDays;
				default:
					return timeSpan.Ticks;
			}
		}

		public static TimeSpan TimeUnitToTimeSpan(long time, TimeUnit timeUnit = TimeUnit.Seconds)
		{
			switch (timeUnit)
			{
				case TimeUnit.Ticks:
					return TimeSpan.FromTicks(time);
				case TimeUnit.Millis:
					return TimeSpan.FromMilliseconds(time);
				case TimeUnit.Seconds:
					return TimeSpan.FromSeconds(time);
				case TimeUnit.Minutes:
					return TimeSpan.FromMinutes(time);
				case TimeUnit.Hours:
					return TimeSpan.FromHours(time);
				case TimeUnit.Days:
					return TimeSpan.FromDays(time);
				default:
					return TimeSpan.FromTicks(time);
			}
		}
	}
}