using JetBrains.Annotations;
using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// LocalTime extensions.
/// </summary>
[ PublicAPI ]
public static class LocalTimeTools {
	/// <summary>
	/// Returns whether this time is within the specified range, inclusive on both ends.
	/// </summary>
	/// <param name="time"></param>
	/// <param name="min">The beginning of the range. Pass null for no beginning value, which is the same as passing midnight.</param>
	/// <param name="max">The end of the range. This can be earlier than <paramref name="min"/> to create a range spanning midnight. Pass null for no end value,
	/// which is the same as passing <see cref="LocalTime.MaxValue"/>.</param>
	public static bool InRange( this LocalTime time, LocalTime? min, LocalTime? max ) {
		min ??= LocalTime.Midnight;
		max ??= LocalTime.MaxValue;
		return max.Value >= min.Value ? time >= min.Value && time <= max.Value : time >= min.Value || time <= max.Value;
	}

	/// <summary>
	/// Returns a collection of time steps within the specified range (inclusive on both ends), starting with <paramref name="min"/> and spaced by
	/// <paramref name="minuteInterval"/>.
	/// </summary>
	/// <param name="min">The beginning of the range. Pass null for no beginning value, which is the same as passing midnight.</param>
	/// <param name="max">The end of the range. This can be earlier than <paramref name="min"/> to create a range spanning midnight. Pass null for no end value,
	/// which is the same as passing <see cref="LocalTime.MaxValue"/>.</param>
	/// <param name="minuteInterval">The interval between steps.</param>
	public static IReadOnlyCollection<LocalTime> GetStepsInRange( LocalTime? min, LocalTime? max, int minuteInterval ) {
		min ??= LocalTime.Midnight;
		max ??= LocalTime.MaxValue;

		var times = new List<LocalTime>();
		var time = min.Value;
		var wrapAllowed = max.Value < min.Value;
		while( true ) {
			times.Add( time );
			time = time.PlusMinutes( minuteInterval );

			if( time < times.Last() )
				if( wrapAllowed )
					wrapAllowed = false;
				else
					break;

			if( !wrapAllowed && time > max.Value )
				break;
		}
		return times;
	}
}