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

	/// <summary>
	/// Returns whether two date/time ranges overlap. Passing null for any endpoint means infinity in that direction.
	/// </summary>
	public static bool RangesOverlap( LocalDateTime? rangeOneBegin, LocalDateTime? rangeOneEnd, LocalDateTime? rangeTwoBegin, LocalDateTime? rangeTwoEnd ) {
		if( rangeOneEnd < rangeOneBegin )
			throw new ArgumentException( "Range one ends before it begins." );
		if( rangeTwoEnd < rangeTwoBegin )
			throw new ArgumentException( "Range two ends before it begins." );

		// It is important to call IsBetween on the beginnings here because of the way it handles the beginning and end of the range differently.
		var oneBeginsBeforeTwoEnds = rangeOneBegin is not {} oneBegin || oneBegin.IsBetween( null, rangeTwoEnd );
		var twoBeginsBeforeOneEnds = rangeTwoBegin is not {} twoBegin || twoBegin.IsBetween( null, rangeOneEnd );

		return oneBeginsBeforeTwoEnds && twoBeginsBeforeOneEnds;
	}

	/// <summary>
	/// Returns whether the specified date/time range overlaps the specified date range. Passing null for any endpoint means infinity in that direction. The end
	/// of the date/time range is considered exclusive, so if it falls on the beginning of the date range at midnight, the ranges will not overlap.
	/// </summary>
	public static bool RangeOverlapsDateRange( LocalDateTime? begin, LocalDateTime? end, LocalDate? dateRangeBegin, LocalDate? dateRangeEnd ) {
		if( end < begin )
			throw new ArgumentException( "Date/time range ends before it begins." );
		if( dateRangeEnd < dateRangeBegin )
			throw new ArgumentException( "Date range ends before it begins." );

		var dateTimeRangeBeginsBeforeDateRangeEnds = begin is not {} timeBegin || timeBegin.Date.IsBetween( null, dateRangeEnd );

		// It is important to call IsBetween on the beginning here because of the way it handles the beginning and end of the range differently.
		var dateRangeBeginsBeforeDateTimeRangeEnds = dateRangeBegin is not {} dateBegin || dateBegin.AtMidnight().IsBetween( null, end );

		return dateTimeRangeBeginsBeforeDateRangeEnds && dateRangeBeginsBeforeDateTimeRangeEnds;
	}
}