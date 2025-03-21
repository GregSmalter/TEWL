using System.Globalization;

namespace Tewl.InputValidation;

/// <summary>
/// Contains high-level validation methods. Each validation method returns an object that is the value of the validated
/// result. This value is meaningless if
/// ValidationErrorHandler.LastResult is anything other than ErrorCondition.NoError. This property or the ErrorsOccurred
/// property on this class should be
/// checked before using returned values.
/// </summary>
[ PublicAPI ]
public class Validator {
	internal static readonly DateTime SqlSmallDateTimeMinValue = new DateTime( 1900, 1, 1 );
	internal static readonly DateTime SqlSmallDateTimeMaxValue = new DateTime( 2079, 6, 6 );

	/// <summary>
	/// Most of our SQL Server decimal columns are specified as (9,2). This is the minimum value that will fit in such a
	/// column.
	/// </summary>
	public const decimal SqlDecimalDefaultMin = -9999999.99m;

	/// <summary>
	/// Most of our SQL Server decimal columns are specified as (9,2). This is the maximum value that will fit in such a
	/// column.
	/// </summary>
	public const decimal SqlDecimalDefaultMax = 9999999.99m;

	internal delegate ValidationError? ValidationMethod<out ValType, in InputType>( Action<ValType> valueSetter, InputType trimmedInput );

	private static bool isEmpty<InputType>( InputType input, out InputType trimmedInput ) {
		var type = typeof( InputType );

		if( type == typeof( string ) ) {
			var trimmedString = ( (string)(object)input! ).Trim();
			trimmedInput = (InputType)(object)trimmedString;
			return trimmedString.Length == 0;
		}

		trimmedInput = input;
		return input is null;
	}

	private readonly List<Error> errors = new List<Error>();

	/// <summary>
	/// Returns true if any errors have been encountered during validation so far.  This can be true even while
	/// ErrorMessages.Count == 0
	/// and Errors.Count == 0, since NoteError may have been called.
	/// </summary>
	public bool ErrorsOccurred { get; private set; }

	/// <summary>
	/// Returns true if at least one unusable value has been returned since this Validator was created.  An unusable value
	/// return
	/// is defined as any time a Get... fails validation and the Validator is forced to return something other than
	/// a good default value.  An example of an unusable value would be a call to GetInt that fails validation.  An
	/// example of a usable value is a call to GetNullableInt, with allowEmpty = true, that fails validation.
	/// </summary>
	public bool UnusableValuesReturned {
		get {
			foreach( var error in errors )
				if( error.UnusableValueReturned )
					return true;

			return false;
		}
	}

	/// <summary>
	/// Returns a deep copy of the list of error messages associated with the validation performed by this validator so far.
	/// It's possible for ErrorsOccurred to
	/// be true while ErrorsMessages.Count == 0, since NoteError may have been called.
	/// </summary>
	public List<string> ErrorMessages {
		get {
			var errorMessages = new List<string>();
			foreach( var error in errors )
				errorMessages.Add( error.Message );
			return errorMessages;
		}
	}

	/// <summary>
	/// Returns a deep copy of the list of errors associated with the validation performed by this validator so far. It's
	/// possible for ErrorsOccurred to
	/// be true while Errors.Count == 0, since NoteError may have been called.
	/// </summary>
	public List<Error> Errors => new List<Error>( errors );

	/// <summary>
	/// Sets the ErrorsOccurred flag.
	/// </summary>
	public void NoteError() => ErrorsOccurred = true;

	/// <summary>
	/// Sets the ErrorsOccurred flag and adds an error message to this validator. Use this if you want to add your own error
	/// message to the same collection
	/// that the error handlers use.
	/// </summary>
	public void NoteErrorAndAddMessage( string message ) {
		NoteError();
		errors.Add( new Error( message, false ) );
	}

	/// <summary>
	/// Sets the ErrorsOccurred flag and add the given error messages to this validator. Use this if you want to add your own
	/// error messages to the same collection
	/// that the error handlers use.
	/// </summary>
	public void NoteErrorAndAddMessages( params string[] messages ) {
		foreach( var message in messages )
			NoteErrorAndAddMessage( message );
	}

	/// <summary>
	/// Gets a validated United States zip code object given the complete zip code with optional +4 digits.
	/// </summary>
	public ValidationResult<ZipCode> GetZipCode( ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsZipCode, emptyValue: new ZipCode() )!;

	/// <summary>
	/// Gets a validated US or Canadian zip code.
	/// </summary>
	public ValidationResult<ZipCode> GetUsOrCanadianZipCode( ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsOrCanadianZipCode, emptyValue: new ZipCode() )!;

	/// <summary>
	/// Returns the validated DateTime type from the given string and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<DateTime> GetSqlSmallDateTime( ValidationErrorHandler? errorHandler, string input ) =>
		ExecuteValidation<DateTime, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime( valueSetter, trimmedInput, null, SqlSmallDateTimeMinValue, SqlSmallDateTimeMaxValue ) );

	/// <summary>
	/// Returns the validated DateTime type from the given string and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTime( ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation<DateTime?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime(
				value => valueSetter( value ),
				trimmedInput,
				null,
				SqlSmallDateTimeMinValue,
				SqlSmallDateTimeMaxValue ) );

	/// <summary>
	/// Returns the validated DateTime type from the given date part strings and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// Passing an empty string or null for each date part will result in ErrorCondition.Empty.
	/// Passing an empty string or null for only some date parts will result in ErrorCondition.Invalid.
	/// </summary>
	public ValidationResult<DateTime> GetSqlSmallDateTimeFromParts( ValidationErrorHandler? errorHandler, string month, string day, string year ) =>
		GetSqlSmallDateTime( errorHandler, makeDateFromParts( month, day, year ) );

	/// <summary>
	/// Returns the validated DateTime type from the given date part strings and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// If allowEmpty is true and each date part string is empty, null will be returned.
	/// Passing an empty string or null for only some date parts will result in ErrorCondition.Invalid.
	/// </summary>
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTimeFromParts(
		ValidationErrorHandler? errorHandler, string month, string day, string year, bool allowEmpty ) =>
		GetNullableSqlSmallDateTime( errorHandler, makeDateFromParts( month, day, year ), allowEmpty );

	private static string makeDateFromParts( string month, string day, string year ) {
		var date = month + '/' + day + '/' + year;
		if( date == "//" )
			date = "";
		return date;
	}

	/// <summary>
	/// Returns the validated DateTime type from a date string and an exact match pattern.'
	/// Pattern specifies the date format, such as "MM/dd/yyyy".
	/// </summary>
	public ValidationResult<DateTime> GetSqlSmallDateTimeExact( ValidationErrorHandler? errorHandler, string input, string pattern ) =>
		ExecuteValidation<DateTime, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateSqlSmallDateTimeExact( valueSetter, trimmedInput, pattern ) );

	/// <summary>
	/// Returns the validated DateTime type from a date string and an exact match pattern.'
	/// Pattern specifies the date format, such as "MM/dd/yyyy".
	/// </summary>
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTimeExact( ValidationErrorHandler? errorHandler, string input, string pattern, bool allowEmpty ) =>
		ExecuteValidation<DateTime?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateSqlSmallDateTimeExact( value => valueSetter( value ), trimmedInput, pattern ) );

	private static ValidationError? validateSqlSmallDateTimeExact( Action<DateTime> valueSetter, string trimmedInput, string pattern ) {
		if( !DateTime.TryParseExact( trimmedInput, pattern, Cultures.EnglishUnitedStates, DateTimeStyles.None, out var date ) )
			return ValidationError.Invalid();
		if( validateNativeDateTime( date, false, SqlSmallDateTimeMinValue, SqlSmallDateTimeMaxValue ) is {} error )
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
	public ValidationResult<DateTime?> GetNullableDateTime(
		ValidationErrorHandler? handler, string input, string[]? formats, bool allowEmpty, DateTime minDate, DateTime maxDate ) =>
		ExecuteValidation<DateTime?, string>(
			handler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime( value => valueSetter( value ), trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the date using given min and max constraints.
	/// </summary>
	public ValidationResult<DateTime> GetDateTime( ValidationErrorHandler? handler, string input, string[]? formats, DateTime minDate, DateTime maxDate ) =>
		ExecuteValidation<DateTime, string>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime( valueSetter, trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public ValidationResult<TimeSpan?> GetNullableTimeSpan( ValidationErrorHandler? handler, TimeSpan? input, bool allowEmpty ) =>
		ExecuteValidation<TimeSpan?, TimeSpan?>(
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
	public ValidationResult<TimeSpan> GetTimeSpan( ValidationErrorHandler? handler, TimeSpan? input ) =>
		ExecuteValidation<TimeSpan, TimeSpan?>(
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
	public ValidationResult<TimeSpan?> GetNullableTimeOfDayTimeSpan( ValidationErrorHandler? handler, string input, string[]? formats, bool allowEmpty ) =>
		ExecuteValidation<TimeSpan?, string>(
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
	public ValidationResult<TimeSpan> GetTimeOfDayTimeSpan( ValidationErrorHandler? handler, string input, string[]? formats ) =>
		ExecuteValidation<TimeSpan, string>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime(
				value => valueSetter( value.TimeOfDay ),
				trimmedInput,
				formats,
				DateTime.MinValue,
				DateTime.MaxValue ) );

	/// <summary>
	/// Executes a validation and returns the result.
	/// </summary>
	/// <param name="handler">Pass null if you’re only validating a single value and don’t need to distinguish it from others in error messages.</param>
	/// <param name="input"></param>
	/// <param name="allowEmpty"></param>
	/// <param name="validationMethod"></param>
	/// <param name="emptyValue">The result value that will be used if the input value is empty or if there is a validation error.</param>
	internal ValidationResult<ValType?> ExecuteValidation<ValType, InputType>(
		ValidationErrorHandler? handler, InputType input, bool allowEmpty, ValidationMethod<ValType, InputType> validationMethod, ValType? emptyValue = default ) {
		handler ??= new ValidationErrorHandler( "value" );

		if( isEmpty( input, out var trimmedInput ) )
			return allowEmpty ? new ValidationResult<ValType?>( emptyValue, null ) : handleError( ValidationError.Empty() );

		var result = emptyValue;
		if( validationMethod( value => result = value, trimmedInput ) is {} validationMethodError )
			return handleError( validationMethodError );

		return new ValidationResult<ValType?>( result, null );

		ValidationResult<ValType?> handleError( ValidationError error ) {
			NoteError();

			var message = handler.HandleError( error );
			if( message.Length > 0 )
				errors.Add( new Error( message, !allowEmpty ) );

			return new ValidationResult<ValType?>( emptyValue, error );
		}
	}
}

/// <summary>
/// Validation methods for <see cref="Validator"/>.
/// </summary>
[ PublicAPI ]
public static partial class ValidatorExtensions;