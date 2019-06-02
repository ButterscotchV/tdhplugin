using System;

namespace TDHPlugin.TimedObject
{
	public static class TimeSpanExtensions
	{
		public static long ToTimeUnit(this TimeSpan timeSpan, TimeUnit timeUnit = TimeUnit.Seconds)
		{
			return TimeUtils.TimeSpanToTimeUnit(timeSpan, timeUnit);
		}
	}
}