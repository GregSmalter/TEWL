using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// LocalDate extensions.
/// </summary>
[ PublicAPI ]
public static class LocalDateTools {
	/// <summary>
	/// Returns whether this date is between (inclusive) the specified dates. Passing null for either of the two dates is considered to be infinity in that
	/// direction. Therefore, passing null for both dates will always return true.
	/// </summary>
	public static bool IsBetween( this LocalDate date, LocalDate? begin, LocalDate? end ) =>
		new DateInterval( begin ?? LocalDate.MinIsoValue, end ?? LocalDate.MaxIsoValue ).Contains( date );

	/// <summary>
	/// Returns whether this date is between (inclusive) the specified dates. Passing null for either of the two dates is considered to be infinity in that
	/// direction. Therefore, passing null for both dates will always return true.
	/// </summary>
	[ Obsolete( "Please use IsBetween instead, which has exactly the same behavior." ) ]
	public static bool IsBetweenDates( this LocalDate date, LocalDate? begin, LocalDate? end ) => date.IsBetween( begin, end );

	/// <summary>
	/// Returns the first date in this date’s month.
	/// </summary>
	public static LocalDate MonthBeginDate( this LocalDate date ) => date.ToYearMonth().OnDayOfMonth( 1 );

	/// <summary>
	/// Formats this date in "day month year" style, e.g. 5 Apr 2008. Returns stringIfNull if the date is null.
	/// </summary>
	public static string ToDayMonthYearString( this LocalDate? date, string stringIfNull, bool useLeadingZero, bool includeDayOfWeek = false ) =>
		date.HasValue ? date.Value.ToDayMonthYearString( useLeadingZero, includeDayOfWeek: includeDayOfWeek ) : stringIfNull;

	/// <summary>
	/// Formats this date in "day month year" style, e.g. 5 Apr 2008.
	/// </summary>
	public static string ToDayMonthYearString( this LocalDate date, bool useLeadingZero, bool includeDayOfWeek = false ) =>
		date.ToDateTimeUnspecified().ToDayMonthYearString( useLeadingZero, includeDayOfWeek: includeDayOfWeek );

	/// <summary>
	/// Formats this date in "01/01/2001" style. Returns stringIfNull if the date is null.
	/// </summary>
	public static string ToMonthDayYearString( this LocalDate? date, string stringIfNull ) => date.HasValue ? date.Value.ToMonthDayYearString() : stringIfNull;

	/// <summary>
	/// Formats this date in "01/01/2001" style.
	/// </summary>
	public static string ToMonthDayYearString( this LocalDate date ) => date.ToDateTimeUnspecified().ToMonthDayYearString();
}