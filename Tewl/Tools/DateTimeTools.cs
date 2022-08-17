﻿using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Provides helpful DateTime methods.
	/// </summary>
	[ PublicAPI ]
	public static class DateTimeTools {
		internal static readonly string[] DayMonthYearFormats = { dayMonthYearFormatLz, dayMonthYearFormat };
		internal static readonly string[] MonthDayYearFormats = { monthDayYearFormat, "MM/dd/yy" };
		internal const string HourAndMinuteFormat = "h:mmt";

		private const string dayMonthYearFormatLz = "dd MMM yyyy";
		private const string dayMonthYearFormat = "d MMM yyyy";
		private const string monthDayYearFormat = "MM/dd/yyyy";
		private const string monthYearFormat = "MMMM yyyy";

		/// <summary>
		/// Formats the date portion of the specified date/time in "day month year" style, e.g. 5 Apr 2008. Returns stringIfNull if
		/// the
		/// specified date/time is null.
		/// </summary>
		public static string ToDayMonthYearString( this DateTime? dateTime, string stringIfNull, bool useLeadingZero, bool includeDayOfWeek = false ) =>
			dateTime.HasValue ? ToDayMonthYearString( dateTime.Value, useLeadingZero, includeDayOfWeek: includeDayOfWeek ) : stringIfNull;

		/// <summary>
		/// Formats the date portion of the specified date/time in "day month year" style, e.g. 5 Apr 2008.
		/// </summary>
		public static string ToDayMonthYearString( this DateTime dateTime, bool useLeadingZero, bool includeDayOfWeek = false ) =>
			dateTime.ToString( ( includeDayOfWeek ? "ddd, " : "" ) + ( useLeadingZero ? dayMonthYearFormatLz : dayMonthYearFormat ), Cultures.EnglishUnitedStates );

		/// <summary>
		/// Formats the date portion of the specified date/time in "01/01/2001" style. Returns stringIfNull if the
		/// specified date/time is null.
		/// </summary>
		public static string ToMonthDayYearString( this DateTime? dateTime, string stringIfNull ) => dateTime.HasValue ? ToMonthDayYearString( dateTime.Value ) : stringIfNull;

		/// <summary>
		/// Formats the date portion of the specified date/time in "01/01/2001" style.
		/// </summary>
		public static string ToMonthDayYearString( this DateTime dateTime ) => dateTime.ToString( monthDayYearFormat, Cultures.EnglishUnitedStates );

		/// <summary>
		/// Formats the date portion of the specified date/time in "month year" style, e.g. April 2008.
		/// </summary>
		public static string ToMonthYearString( this DateTimeOffset dateTime ) => dateTime.ToString( monthYearFormat, Cultures.EnglishUnitedStates );

		/// <summary>
		/// Formats the time portion of the specified date/time in hour:minute style followed by a single lowercase letter
		/// indicating AM or PM. Returns stringIfNull
		/// if the specified date/time is null.
		/// </summary>
		public static string ToHourAndMinuteString( this DateTime? dateTime, string stringIfNull ) => dateTime.HasValue ? ToHourAndMinuteString( dateTime.Value ) : stringIfNull;

		/// <summary>
		/// Formats the time portion of the specified date/time in hour:minute style followed by a single lowercase letter
		/// indicating AM or PM.
		/// </summary>
		public static string ToHourAndMinuteString( this DateTime dateTime ) => dateTime.ToString( HourAndMinuteFormat, Cultures.EnglishUnitedStates ).ToLower();

		/// <summary>
		/// Returns the begin date of the specified date's month.
		/// </summary>
		public static DateTime MonthBeginDate( this DateTime date ) => new DateTime( date.Year, date.Month, 1 );

		/// <summary>
		/// Returns the date that the given week starts on.
		/// </summary>
		public static DateTime WeekBeginDate( this DateTime dateTime ) => dateTime.AddDays( -(int)dateTime.DayOfWeek ).Date;

		/// <summary>
		/// Returns true if this date/time contains time information.
		/// </summary>
		public static bool HasTime( this DateTime? dateTime ) => dateTime.HasValue && dateTime.Value.HasTime();

		/// <summary>
		/// Returns true if this date/time contains time information.
		/// </summary>
		public static bool HasTime( this DateTime dateTime ) =>
			// See http://stackoverflow.com/a/681451/35349.
			dateTime.TimeOfDay != TimeSpan.Zero;

		/// <summary>
		/// Returns true if this date is in between the given DateTimes (inclusive at beginning of range, exclusive at end of
		/// range).
		/// Passing null for either of the two dates is considered to be infinity in that direction.
		/// Therefore, passing null for both dates will always result in true.
		/// </summary>
		public static bool IsBetweenDateTimes( this DateTime dateTime, DateTime? onOrAfterDate, DateTime? onOrBeforeDate ) =>
			( onOrAfterDate == null || dateTime >= onOrAfterDate ) && ( onOrBeforeDate == null || dateTime < onOrBeforeDate );

		/// <summary>
		/// Returns true if this date is in between (inclusive) the given dates.
		/// This method differs from IsBetweenDateTimes in that onOrAfterDate and onOrBeforeDate must be dates only
		/// (an exception will be thrown if time information is passed - use .Date if you have to.)
		/// and that it is inclusive on both ends of the range.
		/// This method also correctly returns true in the case where the dateTime parameter is 3:30PM on 11/15/09 and the
		/// onOrBeforeDate is 11/15/09.
		/// If you want to define a range with time information, use IsBetweenDateTimes instead.
		/// Passing null for either of the two dates is considered to be infinity in that direction. Therefore, passing null for
		/// both dates will always result in true.
		/// </summary>
		public static bool IsBetweenDates( this DateTime dateTime, DateTime? onOrAfterDate, DateTime? onOrBeforeDate ) {
			assertDateTimeHasNoTime( onOrAfterDate, "on or after date" );
			assertDateTimeHasNoTime( onOrBeforeDate, "on or before date" );

			onOrBeforeDate = onOrBeforeDate?.AddDays( 1 );
			return IsBetweenDateTimes( dateTime, onOrAfterDate, onOrBeforeDate );
		}

		/// <summary>
		/// Returns true if the two given DateTime ranges overlap. Passing null for any date/time means infinity in that direction.
		/// </summary>
		public static bool DateTimeRangesOverlap( DateTime? rangeOneBegin, DateTime? rangeOneEnd, DateTime? rangeTwoBegin, DateTime? rangeTwoEnd ) {
			// It is important to call IsBetweenDateTimes on the endings here because of the way IsBetweenDateTimes handles the beginning and end of the range
			// differently.
			if( rangeOneEnd.HasValue && !rangeOneEnd.Value.IsBetweenDateTimes( rangeOneBegin, null ) )
				throw new ApplicationException( "Range one ends before it begins." );
			if( rangeTwoEnd.HasValue && !rangeTwoEnd.Value.IsBetweenDateTimes( rangeTwoBegin, null ) )
				throw new ApplicationException( "Range two ends before it begins." );

			// It is important to call IsBetweenDateTimes on the beginnings here because of the way IsBetweenDateTimes handles the beginning and end of the range
			// differently.
			var oneBeginsBeforeTwoEnds = !rangeOneBegin.HasValue || rangeOneBegin.Value.IsBetweenDateTimes( null, rangeTwoEnd );
			var twoBeginsBeforeOneEnds = !rangeTwoBegin.HasValue || rangeTwoBegin.Value.IsBetweenDateTimes( null, rangeOneEnd );

			return oneBeginsBeforeTwoEnds && twoBeginsBeforeOneEnds;
		}

		/// <summary>
		/// Returns true if the specified date/time range overlaps the specified date range. Passing null for any date means
		/// infinity in that direction.
		/// Throws an exception if the date range contains time information. Use .Date if you have to.
		/// See documentation for IsBetweenDates for more information on the date range.
		/// </summary>
		public static bool DateTimeRangeOverlapsDateRange( DateTime? dateTimeRangeBegin, DateTime? dateTimeRangeEnd, DateTime? dateRangeBegin, DateTime? dateRangeEnd ) {
			assertDateTimeHasNoTime( dateRangeBegin, "date range begin" );
			assertDateTimeHasNoTime( dateRangeEnd, "date range end" );

			// It is important to call IsBetweenDateTimes on the ending here because of the way IsBetweenDateTimes handles the beginning and end of the range
			// differently.
			if( dateTimeRangeEnd.HasValue && !dateTimeRangeEnd.Value.IsBetweenDateTimes( dateTimeRangeBegin, null ) )
				throw new ApplicationException( "Date/time range ends before it begins." );

			if( dateRangeBegin.HasValue && !dateRangeBegin.Value.IsBetweenDates( null, dateRangeEnd ) )
				throw new ApplicationException( "Date range ends before it begins." );

			var dateTimeRangeBeginsBeforeDateRangeEnds = !dateTimeRangeBegin.HasValue || dateTimeRangeBegin.Value.IsBetweenDates( null, dateRangeEnd );

			// It is important to call IsBetweenDateTimes on the beginning here because of the way IsBetweenDateTimes handles the beginning and end of the range
			// differently.
			var dateRangeBeginsBeforeDateTimeRangeEnds = !dateRangeBegin.HasValue || dateRangeBegin.Value.IsBetweenDateTimes( null, dateTimeRangeEnd );

			return dateTimeRangeBeginsBeforeDateRangeEnds && dateRangeBeginsBeforeDateTimeRangeEnds;
		}

		/// <summary>
		/// Returns true if the two given date ranges overlap. Passing null for any date means infinity in that direction.
		/// Throws an exception if any of the given dates contains time information. Use .Date if you have to.
		/// See documentation for IsBetweenDates for more information on the date ranges.
		/// </summary>
		public static bool DateRangesOverlap( DateTime? rangeOneBegin, DateTime? rangeOneEnd, DateTime? rangeTwoBegin, DateTime? rangeTwoEnd ) {
			assertDateTimeHasNoTime( rangeOneBegin, "range one begin" );
			assertDateTimeHasNoTime( rangeOneEnd, "range one end" );
			assertDateTimeHasNoTime( rangeTwoBegin, "range two begin" );
			assertDateTimeHasNoTime( rangeTwoEnd, "range two end" );

			if( rangeOneBegin.HasValue && !rangeOneBegin.Value.IsBetweenDates( null, rangeOneEnd ) )
				throw new ApplicationException( "Range one ends before it begins." );
			if( rangeTwoBegin.HasValue && !rangeTwoBegin.Value.IsBetweenDates( null, rangeTwoEnd ) )
				throw new ApplicationException( "Range two ends before it begins." );

			return ( !rangeOneBegin.HasValue || rangeOneBegin.Value.IsBetweenDates( null, rangeTwoEnd ) ) &&
			       ( !rangeTwoBegin.HasValue || rangeTwoBegin.Value.IsBetweenDates( null, rangeOneEnd ) );
		}

		private static void assertDateTimeHasNoTime( DateTime? dateTime, string name ) {
			if( dateTime.HasTime() )
				throw new ApplicationException( "{0} contains time information.".FormatWith( name.CapitalizeString() ) );
		}

		/// <summary>
		/// Returns true when each day between begin and end dates is represented inside one of the date ranges in dateRanges,
		/// inclusive.
		/// Be sure time information is not included. No begin date may be after an end date.
		/// </summary>
		public static bool DateRangesCoverAllDates( DateTime beginDate, DateTime endDate, IEnumerable<Tuple<DateTime?, DateTime?>> dateRanges ) {
			for( var day = beginDate; day <= endDate; day = day.AddDays( 1 ) ) {
				if( !dateRanges.Any( dr => day.IsBetweenDates( dr.Item1, dr.Item2 ) ) )
					return false;
			}

			return true;
		}

		/// <summary>
		/// This is useful for calculating someone's age. Beginning date must be before or equal to end date.
		/// </summary>
		public static int GetNumberOfFullYearsBetweenDates( DateTime beginDate, DateTime endDate ) {
			if( beginDate > endDate )
				throw new ApplicationException( "The begin date must be before or the same as the end date." );

			// get the age by comparing the two years...
			var age = endDate.Year - beginDate.Year;

			// ...then check for the 1-year offset
			if( endDate.Month < beginDate.Month || endDate.Month == beginDate.Month && endDate.Day < beginDate.Day )
				age -= 1;

			return age;
		}

		/// <summary>
		/// Gets the date of the specified day of the week in the specified month.
		/// </summary>
		/// <param name="year">The year.</param>
		/// <param name="month">The month.</param>
		/// <param name="day">The day of the week.</param>
		/// <param name="weeksFromFirst">
		/// The number of weeks from the first occurrence of the specified day of the week in the month. Pass a negative value to
		/// count
		/// backward from the first occurrence in the next month.
		/// </param>
		// Based on http://stackoverflow.com/a/5422046/35349.
		public static DateTime GetDateOfDayOfWeekInMonth( int year, int month, DayOfWeek day, int weeksFromFirst ) {
			var date = new DateTime( year, month, 1 );

			if( weeksFromFirst < 0 )
				date = date.AddMonths( 1 );

			var offset = day - date.DayOfWeek;
			if( offset < 0 )
				offset += 7;

			date = date.AddDays( offset + weeksFromFirst * 7 );

			if( date.Year != year || date.Month != month )
				throw new ApplicationException( "nonexistent date" );

			return date;
		}

		/// <summary>
		/// Returns the number of weeks from the first occurrence of the specified date's day of the week in the month to the
		/// specified date.
		/// </summary>
		public static int WeeksFromFirstOccurrenceOfDayOfWeekInMonth( this DateTime date, bool countBackwardFromFirstOccurrenceInNextMonth ) {
			if( countBackwardFromFirstOccurrenceInNextMonth )
				return ( date.Day - DateTime.DaysInMonth( date.Year, date.Month ) ) / 7 - 1;
			return ( date.Day - 1 ) / 7;
		}

		/// <summary>
		/// Returns the given UTC <see cref="DateTime" /> to local time.
		/// Doesn't matter if <see cref="DateTime.Kind" /> is correctly set.
		/// Null values return null.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static DateTime? UtcToLocal( this DateTime? d ) => d != null ? TimeZoneInfo.ConvertTimeFromUtc( d.Value, TimeZoneInfo.Local ) : (DateTime?)null;

		/// <summary>
		/// Returns the given UTC <see cref="DateTime" /> to local time.
		/// Doesn't matter if <see cref="DateTime.Kind" /> is correctly set.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static DateTime UtcToLocal( this DateTime d ) => UtcToLocal( (DateTime?)d ).Value;
	}
}