using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// LocalDateTime extensions.
/// </summary>
[ PublicAPI ]
public static class LocalDateTimeTools {
	/// <summary>
	/// Returns whether this date/time is between the specified dates/times, inclusive at the beginning and exclusive at the end. Passing null for either of the
	/// two endpoints is considered to be infinity in that direction. Therefore, passing null for both endpoints will always return true.
	/// </summary>
	public static bool IsBetween( this LocalDateTime dateAndTime, LocalDateTime? begin, LocalDateTime? end ) =>
		( begin is not {} b || dateAndTime >= b ) && ( end is not {} e || dateAndTime < e );
}