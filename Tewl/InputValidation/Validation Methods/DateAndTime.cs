using System.Globalization;

namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	private static readonly DateTime sqlSmallDateTimeMinValue = new( 1900, 1, 1 );
	private static readonly DateTime sqlSmallDateTimeMaxValue = new( 2079, 6, 6 );

	/// <summary>
	/// Returns the validated DateTime type from the given string and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<DateTime> GetSqlSmallDateTime( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.ExecuteValidation<DateTime, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime( valueSetter, trimmedInput, null, sqlSmallDateTimeMinValue, sqlSmallDateTimeMaxValue ) );

	/// <summary>
	/// Returns the validated DateTime type from the given string and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<DateTime?> GetNullableSqlSmallDateTime(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.ExecuteValidation<DateTime?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime(
				value => valueSetter( value ),
				trimmedInput,
				null,
				sqlSmallDateTimeMinValue,
				sqlSmallDateTimeMaxValue ) );

	/// <summary>
	/// Returns the validated DateTime type from the given date part strings and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// Passing an empty string or null for each date part will result in ErrorCondition.Empty.
	/// Passing an empty string or null for only some date parts will result in ErrorCondition.Invalid.
	/// </summary>
	public static ValidationResult<DateTime> GetSqlSmallDateTimeFromParts(
		this Validator validator, ValidationErrorHandler? errorHandler, string month, string day, string year ) =>
		validator.GetSqlSmallDateTime( errorHandler, makeDateFromParts( month, day, year ) );

	/// <summary>
	/// Returns the validated DateTime type from the given date part strings and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// If allowEmpty is true and each date part string is empty, null will be returned.
	/// Passing an empty string or null for only some date parts will result in ErrorCondition.Invalid.
	/// </summary>
	public static ValidationResult<DateTime?> GetNullableSqlSmallDateTimeFromParts(
		this Validator validator, ValidationErrorHandler? errorHandler, string month, string day, string year, bool allowEmpty ) =>
		validator.GetNullableSqlSmallDateTime( errorHandler, makeDateFromParts( month, day, year ), allowEmpty );

	private static string makeDateFromParts( string month, string day, string year ) {
		var date = month + '/' + day + '/' + year;
		if( date == "//" )
			date = "";
		return date;
	}

	/// <summary>
	/// Returns the validated DateTime type from a date string and an exact match pattern.
	/// Pattern specifies the date format, such as "MM/dd/yyyy".
	/// </summary>
	public static ValidationResult<DateTime> GetSqlSmallDateTimeExact(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, string pattern ) =>
		validator.ExecuteValidation<DateTime, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateSqlSmallDateTimeExact( valueSetter, trimmedInput, pattern ) );

	/// <summary>
	/// Returns the validated DateTime type from a date string and an exact match pattern.
	/// Pattern specifies the date format, such as "MM/dd/yyyy".
	/// </summary>
	public static ValidationResult<DateTime?> GetNullableSqlSmallDateTimeExact(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, string pattern, bool allowEmpty ) =>
		validator.ExecuteValidation<DateTime?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateSqlSmallDateTimeExact( value => valueSetter( value ), trimmedInput, pattern ) );

	private static ValidationError? validateSqlSmallDateTimeExact( Action<DateTime> valueSetter, string trimmedInput, string pattern ) {
		if( !DateTime.TryParseExact( trimmedInput, pattern, Cultures.EnglishUnitedStates, DateTimeStyles.None, out var date ) )
			return ValidationError.Invalid();
		if( validateNativeDateTime( date, false, sqlSmallDateTimeMinValue, sqlSmallDateTimeMaxValue ) is {} error )
			return error;

		valueSetter( date );
		return null;
	}

	private static ValidationError? validateDateTime( Action<DateTime> valueSetter, string trimmedInput, string[]? formats, DateTime min, DateTime max ) {
		DateTime date;
		try {
			date = formats is not null
				       ? DateTime.ParseExact( trimmedInput, formats, null, DateTimeStyles.None )
				       : DateTime.Parse( trimmedInput, Cultures.EnglishUnitedStates );
			if( validateNativeDateTime( date, false, min, max ) is {} error )
				return error;
		}
		catch( FormatException ) {
			return ValidationError.Invalid();
		}
		catch( ArgumentOutOfRangeException ) {
			// Undocumented exception that there are reports of being thrown
			return ValidationError.Invalid();
		}
		catch( ArgumentNullException ) {
			return ValidationError.Empty();
		}

		valueSetter( date );
		return null;
	}

	private static ValidationError? validateNativeDateTime( DateTime? date, bool allowEmpty, DateTime minDate, DateTime maxDate ) {
		if( date is null && !allowEmpty )
			return ValidationError.Empty();
		if( date.HasValue ) {
			var minMaxMessage = " It must be between " + minDate.ToDayMonthYearString( false ) + " and " + maxDate.ToDayMonthYearString( false ) + ".";
			if( date < minDate )
				return ValidationError.Custom( ValidationErrorType.TooEarly, "The {0} is too early." + minMaxMessage );
			if( date >= maxDate )
				return ValidationError.Custom( ValidationErrorType.TooLate, "The {0} is too late." + minMaxMessage );
		}
		return null;
	}

	/// <summary>
	/// Validates the date using given allowEmpty, min, and max constraints.
	/// </summary>
	public static ValidationResult<DateTime?> GetNullableDateTime(
		this Validator validator, ValidationErrorHandler? handler, string input, string[]? formats, bool allowEmpty, DateTime minDate, DateTime maxDate ) =>
		validator.ExecuteValidation<DateTime?, string>(
			handler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime( value => valueSetter( value ), trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the date using given min and max constraints.
	/// </summary>
	public static ValidationResult<DateTime> GetDateTime(
		this Validator validator, ValidationErrorHandler? handler, string input, string[]? formats, DateTime minDate, DateTime maxDate ) =>
		validator.ExecuteValidation<DateTime, string>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime( valueSetter, trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public static ValidationResult<TimeSpan?>
		GetNullableTimeSpan( this Validator validator, ValidationErrorHandler? handler, TimeSpan? input, bool allowEmpty ) =>
		validator.ExecuteValidation<TimeSpan?, TimeSpan?>(
			handler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				valueSetter( trimmedInput!.Value );
				return null;
			} );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public static ValidationResult<TimeSpan> GetTimeSpan( this Validator validator, ValidationErrorHandler? handler, TimeSpan? input ) =>
		validator.ExecuteValidation<TimeSpan, TimeSpan?>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => {
				valueSetter( trimmedInput!.Value );
				return null;
			} );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public static ValidationResult<TimeSpan?> GetNullableTimeOfDayTimeSpan(
		this Validator validator, ValidationErrorHandler? handler, string input, string[]? formats, bool allowEmpty ) =>
		validator.ExecuteValidation<TimeSpan?, string>(
			handler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime(
				value => valueSetter( value.TimeOfDay ),
				trimmedInput,
				formats,
				DateTime.MinValue,
				DateTime.MaxValue ) );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public static ValidationResult<TimeSpan> GetTimeOfDayTimeSpan( this Validator validator, ValidationErrorHandler? handler, string input, string[]? formats ) =>
		validator.ExecuteValidation<TimeSpan, string>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime(
				value => valueSetter( value.TimeOfDay ),
				trimmedInput,
				formats,
				DateTime.MinValue,
				DateTime.MaxValue ) );
}