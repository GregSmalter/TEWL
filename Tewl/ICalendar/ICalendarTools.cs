using Ical.Net.DataTypes;
using NodaTime;

namespace Tewl.ICalendar;

/// <summary>
/// Static methods pertaining to the iCalendar format.
/// </summary>
[ PublicAPI ]
public static class ICalendarTools {
	/// <summary>
	/// Creates an iCalendar date/time from this ZonedDateTime.
	/// </summary>
	public static CalDateTime ToICalendarTime( this ZonedDateTime time ) =>
		new( time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Zone.Id );

	/// <summary>
	/// Creates an iCalendar date from this LocalDate.
	/// </summary>
	public static CalDateTime ToICalendarDate( this LocalDate date ) => new( date.Year, date.Month, date.Day );
}