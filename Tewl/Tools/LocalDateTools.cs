using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// LocalDate extensions.
/// </summary>
[ PublicAPI ]
public static class LocalDateTools {
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