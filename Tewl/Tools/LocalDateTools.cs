using NodaTime;
using NodaTime.Text;

namespace Tewl.Tools;

/// <summary>
/// LocalDate extensions.
/// </summary>
[ PublicAPI ]
public static class LocalDateTools {
	/// <summary>
	/// Format patterns for the “day month year” style, e.g. 5 Apr 2008.
	/// </summary>
	public static readonly string[] DayMonthYearFormats = [ dayMonthYearFormatLz, dayMonthYearFormat ];

	/// <summary>
	/// Format patterns for the month/day/year style.
	/// </summary>
	public static readonly string[] MonthDayYearFormats = [ monthDayYearFormat, "M/d/yyyy", "MM/dd/yy" ];

	private const string dayMonthYearFormatLz = "dd MMM yyyy";
	private const string dayMonthYearFormat = "d MMM yyyy";
	private const string monthDayYearFormat = "MM/dd/yyyy";
	private const string monthYearFormat = "MMMM yyyy";

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
	/// Returns the first date in this date’s week.
	/// </summary>
	public static LocalDate WeekBeginDate( this LocalDate date, IsoDayOfWeek firstDayOfWeek ) => date.PlusDays( 1 ).Previous( firstDayOfWeek );

	/// <summary>
	/// Formats this date in "day month year" style, e.g. 5 Apr 2008. Returns stringIfNull if the date is null.
	/// </summary>
	public static string ToDayMonthYearString( this LocalDate? date, string stringIfNull, bool useLeadingZero, bool includeDayOfWeek = false ) =>
		date?.ToDayMonthYearString( useLeadingZero, includeDayOfWeek: includeDayOfWeek ) ?? stringIfNull;

	/// <summary>
	/// Formats this date in "day month year" style, e.g. 5 Apr 2008.
	/// </summary>
	public static string ToDayMonthYearString( this LocalDate date, bool useLeadingZero, bool includeDayOfWeek = false ) =>
		LocalDatePattern.Create(
				( includeDayOfWeek ? "ddd, " : "" ) + ( useLeadingZero ? dayMonthYearFormatLz : dayMonthYearFormat ),
				Cultures.EnglishUnitedStates )
			.Format( date );

	/// <summary>
	/// Formats this date in "01/01/2001" style. Returns stringIfNull if the date is null.
	/// </summary>
	public static string ToMonthDayYearString( this LocalDate? date, string stringIfNull ) => date?.ToMonthDayYearString() ?? stringIfNull;

	/// <summary>
	/// Formats this date in "01/01/2001" style.
	/// </summary>
	public static string ToMonthDayYearString( this LocalDate date ) =>
		LocalDatePattern.Create( monthDayYearFormat, Cultures.EnglishUnitedStates ).Format( date );

	/// <summary>
	/// Formats this date in "month year" style, e.g. April 2008.
	/// </summary>
	public static string ToMonthYearString( this LocalDate date ) =>
		YearMonthPattern.Create( monthYearFormat, Cultures.EnglishUnitedStates ).Format( date.ToYearMonth() );
}