using System.Globalization;
using System.Text.RegularExpressions;

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
	/// The maximum length for a URL as dictated by the limitations of Internet Explorer. This is safely the maximum size for a
	/// URL.
	/// </summary>
	public const int MaxUrlLength = 2048;

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
	public void NoteErrorAndAddMessage( string message ) => AddError( new Error( message, false ) );

	/// <summary>
	/// Sets the ErrorsOccurred flag and add the given error messages to this validator. Use this if you want to add your own
	/// error messages to the same collection
	/// that the error handlers use.
	/// </summary>
	public void NoteErrorAndAddMessages( params string[] messages ) {
		foreach( var message in messages )
			NoteErrorAndAddMessage( message );
	}

	internal void AddError( Error error ) {
		NoteError();
		errors.Add( error );
	}

	/// <summary>
	/// Accepts either true/false (case-sensitive) or 1/0.
	/// Returns the validated boolean type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<bool> GetBoolean( ValidationErrorHandler errorHandler, string input ) =>
		ExecuteValidation<bool, string>( errorHandler, input, false, validateBoolean );

	/// <summary>
	/// Accepts either true/false (case-sensitive) or 1/0.
	/// Returns the validated boolean type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<bool?> GetNullableBoolean( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation<bool?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateBoolean( value => valueSetter( value ), trimmedInput ) );

	private static ValidationError? validateBoolean( Action<bool> valueSetter, string trimmedInput ) {
		if( trimmedInput != "1" && trimmedInput != "0" && trimmedInput != true.ToString() && trimmedInput != false.ToString() )
			return ValidationError.Invalid();

		valueSetter( trimmedInput == "1" || trimmedInput == true.ToString() );
		return null;
	}

	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<byte> GetByte( ValidationErrorHandler errorHandler, string input ) => GetByte( errorHandler, input, byte.MinValue, byte.MaxValue );

	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<byte> GetByte( ValidationErrorHandler errorHandler, string input, byte min, byte max ) =>
		ExecuteValidation<byte, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<byte?> GetNullableByte( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation<byte?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<byte>( value => valueSetter( value ), trimmedInput, byte.MinValue, byte.MaxValue ) );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<short> GetShort( ValidationErrorHandler errorHandler, string input ) =>
		GetShort( errorHandler, input, short.MinValue, short.MaxValue );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<short> GetShort( ValidationErrorHandler errorHandler, string input, short min, short max ) =>
		ExecuteValidation<short, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<short?> GetNullableShort( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		GetNullableShort( errorHandler, input, allowEmpty, short.MinValue, short.MaxValue );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<short?> GetNullableShort( ValidationErrorHandler errorHandler, string input, bool allowEmpty, short min, short max ) =>
		ExecuteValidation<short?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<short>( value => valueSetter( value ), trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<int> GetInt( ValidationErrorHandler errorHandler, string input ) => GetInt( errorHandler, input, int.MinValue, int.MaxValue );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// <paramref name="min" /> and <paramref name="max" /> are inclusive.
	/// </summary>
	public ValidationResult<int> GetInt( ValidationErrorHandler errorHandler, string input, int min, int max ) =>
		ExecuteValidation<int, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<int?> GetNullableInt(
		ValidationErrorHandler errorHandler, string input, bool allowEmpty, int min = int.MinValue, int max = int.MaxValue ) =>
		ExecuteValidation<int?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<int>( value => valueSetter( value ), trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated long type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// <paramref name="min" /> and <paramref name="max" /> are inclusive.
	/// </summary>
	public ValidationResult<long> GetLong( ValidationErrorHandler errorHandler, string input, long min = long.MinValue, long max = long.MaxValue ) =>
		ExecuteValidation<long, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated long type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<long?> GetNullableLong(
		ValidationErrorHandler errorHandler, string input, bool allowEmpty, long min = long.MinValue, long max = long.MaxValue ) =>
		ExecuteValidation<long?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<long>( value => valueSetter( value ), trimmedInput, min, max ) );

	private static ValidationError? validateGenericIntegerType<T>( Action<T> valueSetter, string trimmedInput, long minValue, long maxValue ) {
		long intResult;

		try {
			intResult = Convert.ToInt64( trimmedInput );

			if( intResult > maxValue )
				return ValidationError.TooLarge( minValue, maxValue );
			if( intResult < minValue )
				return ValidationError.TooSmall( minValue, maxValue );
		}
		catch( FormatException ) {
			return ValidationError.Invalid();
		}
		catch( OverflowException ) {
			return ValidationError.Invalid();
		}

		valueSetter( (T)Convert.ChangeType( intResult, typeof( T ) ) );
		return null;
	}

	/// <summary>
	/// Returns a validated float type from the given string, validation package, and min/max restrictions.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<float> GetFloat( ValidationErrorHandler errorHandler, string input, float min, float max ) =>
		ExecuteValidation<float, string>( errorHandler, input, false, ( valueSetter, trimmedInput ) => validateFloat( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns a validated float type from the given string, validation package, and min/max restrictions.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<float?> GetNullableFloat( ValidationErrorHandler errorHandler, string input, bool allowEmpty, float min, float max ) =>
		ExecuteValidation<float?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateFloat( value => valueSetter( value ), trimmedInput, min, max ) );

	private static ValidationError? validateFloat( Action<float> valueSetter, string trimmedInput, float min, float max ) {
		float floatValue;
		try {
			floatValue = float.Parse( trimmedInput );
		}
		catch( FormatException ) {
			return ValidationError.Invalid();
		}
		catch( OverflowException ) {
			return ValidationError.Invalid();
		}

		if( floatValue < min )
			return ValidationError.TooSmall( min, max );
		if( floatValue > max )
			return ValidationError.TooLarge( min, max );

		valueSetter( floatValue );
		return null;
	}

	/// <summary>
	/// Returns a validated decimal type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<decimal> GetDecimal( ValidationErrorHandler errorHandler, string input ) =>
		GetDecimal( errorHandler, input, decimal.MinValue, decimal.MaxValue );

	/// <summary>
	/// Returns a validated decimal type from the given string, validation package, and min/max restrictions.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<decimal> GetDecimal( ValidationErrorHandler errorHandler, string input, decimal min, decimal max ) =>
		ExecuteValidation<decimal, string>( errorHandler, input, false, ( valueSetter, trimmedInput ) => validateDecimal( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns a validated decimal type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<decimal?> GetNullableDecimal( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		GetNullableDecimal( errorHandler, input, allowEmpty, decimal.MinValue, decimal.MaxValue );

	/// <summary>
	/// Returns a validated decimal type from the given string, validation package, and min/max restrictions.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public ValidationResult<decimal?> GetNullableDecimal( ValidationErrorHandler errorHandler, string input, bool allowEmpty, decimal min, decimal max ) =>
		ExecuteValidation<decimal?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDecimal( value => valueSetter( value ), trimmedInput, min, max ) );

	private static ValidationError? validateDecimal( Action<decimal> valueSetter, string trimmedInput, decimal min, decimal max ) {
		decimal decimalVal;
		try {
			decimalVal = decimal.Parse( trimmedInput );
			if( decimalVal < min )
				return ValidationError.TooSmall( min, max );
			if( decimalVal > max )
				return ValidationError.TooLarge( min, max );
		}
		catch {
			return ValidationError.Invalid();
		}

		valueSetter( decimalVal );
		return null;
	}

	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public ValidationResult<string> GetString( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		GetString( errorHandler, input, allowEmpty, int.MaxValue );

	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public ValidationResult<string> GetString( ValidationErrorHandler errorHandler, string input, bool allowEmpty, int maxLength ) =>
		GetString( errorHandler, input, allowEmpty, 0, maxLength );

	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public ValidationResult<string> GetString( ValidationErrorHandler errorHandler, string input, bool allowEmpty, int minLength, int maxLength ) =>
		handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				var errorMessage = "The length of the " + errorHandler.Subject + " must be between " + minLength + " and " + maxLength + " characters.";
				if( trimmedInput.Length > maxLength )
					return ValidationError.Custom( ErrorCondition.TooLong, errorMessage );
				if( trimmedInput.Length < minLength )
					return ValidationError.Custom( ErrorCondition.TooShort, errorMessage );

				valueSetter( trimmedInput );
				return null;
			} );

	/// <summary>
	/// Returns a validated email address from the given string and restrictions.
	/// If allowEmpty true and the empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// The maxLength defaults to 254 per this source: http://en.wikipedia.org/wiki/E-mail_address#Syntax
	/// If you pass a different value for maxLength, you'd better have a good reason.
	/// </summary>
	public ValidationResult<string> GetEmailAddress( ValidationErrorHandler errorHandler, string input, bool allowEmpty, int maxLength = 254 ) =>
		handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				// Validate as a string with same restrictions - if it fails on that, return
				if( GetString( errorHandler, trimmedInput, allowEmpty, maxLength ).Error( out _ ) is {} error )
					return error;

				// [^@ \n] means any character but a @ or a newline or a space.  This forces only one @ to exist.
				//([^@ \n\.]+\.)+ forces any positive number of (anything.)s to exist in a row.  Doesn't allow "..".
				// Allows anything.anything123-anything@anything.anything123.anything
				const string localPartUnconditionallyPermittedCharacters = @"[a-z0-9!#\$%&'\*\+\-/=\?\^_`\{\|}~]";
				const string localPart = "(" + localPartUnconditionallyPermittedCharacters + @"+\.?)*" + localPartUnconditionallyPermittedCharacters + "+";
				const string domainUnconditionallyPermittedCharacters = "[a-z0-9-]";
				const string domain = "(" + domainUnconditionallyPermittedCharacters + @"+\.)+" + domainUnconditionallyPermittedCharacters + "+";
				// The first two conditions are for performance only.
				if( !trimmedInput.Contains( "@" ) || !trimmedInput.Contains( "." ) || !Regex.IsMatch(
					    trimmedInput,
					    "^" + localPart + "@" + domain + "$",
					    RegexOptions.IgnoreCase ) )
					return ValidationError.Invalid();
				// Max length is already checked by the string validation
				// NOTE: We should really enforce the max length of the domain portion and the local portion individually as well.

				valueSetter( trimmedInput );
				return null;
			} );

	/// <summary>
	/// Returns a validated URL.
	/// </summary>
	public ValidationResult<string> GetUrl( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		GetUrl( errorHandler, input, allowEmpty, MaxUrlLength );

	private static readonly string[] validSchemes = { "http", "https", "ftp" };

	/// <summary>
	/// Returns a validated URL. Note that you may run into problems with certain browsers if you pass a length longer than
	/// 2048.
	/// </summary>
	public ValidationResult<string> GetUrl( ValidationErrorHandler errorHandler, string input, bool allowEmpty, int maxUrlLength ) =>
		handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				/* If the string is just a number, reject it right out. */
				if( int.TryParse( trimmedInput, out _ ) || double.TryParse( trimmedInput, out _ ) )
					return ValidationError.Invalid();

				/* If it's an email, it's not an URL. */
				if( new Validator().GetEmailAddress( new ValidationErrorHandler( "" ), trimmedInput, allowEmpty ).Error( out _ ) is null )
					return ValidationError.Invalid();

				/* If it doesn't start with one of our whitelisted schemes, add in the best guess. */
				if( GetString(
						    errorHandler,
						    validSchemes.Any( s => trimmedInput.StartsWithIgnoreCase( s ) ) ? trimmedInput : "http://" + trimmedInput,
						    true,
						    maxUrlLength )
					    .Error( out trimmedInput ) is {} error )
					return error;

				/* If the getstring didn't fail, keep on keepin keepin on. */
				try {
					if( !Uri.IsWellFormedUriString( trimmedInput, UriKind.Absolute ) )
						throw new UriFormatException();

					// Don't allow relative URLs
					var uri = new Uri( trimmedInput, UriKind.Absolute );

					// Must be a valid DNS-style hostname or IP address
					// Must contain at least one '.', to prevent just host names
					// Must be one of the common web browser-accessible schemes
					if( uri.HostNameType != UriHostNameType.Dns && uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6 ||
					    uri.Host.All( c => c != '.' ) || validSchemes.All( s => s != uri.Scheme ) )
						throw new UriFormatException();
				}
				catch( UriFormatException ) {
					return ValidationError.Invalid();
				}

				valueSetter( trimmedInput );
				return null;
			} );

	/// <summary>
	/// The same as GetPhoneNumber, except the given default area code will be prepended on the phone number if necessary.
	/// This is useful when working with data that had the area code omitted because the number was local.
	/// </summary>
	public ValidationResult<string> GetPhoneNumberWithDefaultAreaCode(
		ValidationErrorHandler errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage, string defaultAreaCode ) {
		var validator = new Validator(); // We need to use a separate one so that erroneous error messages don't get left in the collection
		var fakeHandler = new ValidationErrorHandler( "" );

		validator.GetPhoneNumber( fakeHandler, input, allowExtension, allowEmpty, allowSurroundingGarbage );
		if( fakeHandler.LastResult != ErrorCondition.NoError ) {
			fakeHandler = new ValidationErrorHandler( "" );
			validator.GetPhoneNumber( fakeHandler, defaultAreaCode + input, allowExtension, allowEmpty, allowSurroundingGarbage );
			// If the phone number was invalid without the area code, but is valid with the area code, we really validate using the default
			// area code and then return.  In all other cases, we return what would have happened without tacking on the default area code.
			if( fakeHandler.LastResult == ErrorCondition.NoError )
				return GetPhoneNumber( errorHandler, defaultAreaCode + input, allowExtension, allowEmpty, allowSurroundingGarbage );
		}

		return GetPhoneNumber( errorHandler, input, allowExtension, allowEmpty, allowSurroundingGarbage );
	}

	/// <summary>
	/// Returns a validated phone number as a standard phone number string given the complete phone number with optional
	/// extension as a string. If allow empty is true and an empty string or null is given, the empty string is returned.
	/// Pass true for allow surrounding garbage if you want to allow "The phone number is 585-455-6476yadayada." to be parsed
	/// into 585-455-6476
	/// and count as a valid phone number.
	/// </summary>
	public ValidationResult<string> GetPhoneNumber(
		ValidationErrorHandler errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage ) =>
		GetPhoneWithLastFiveMapping( errorHandler, input, allowExtension, allowEmpty, allowSurroundingGarbage, null );

	/// <summary>
	/// Returns a validated phone number as a standard phone number string given the complete phone number with optional
	/// extension or the last five digits of the number and a dictionary of single
	/// digits to five-digit groups that become the first five digits of the full number.  If allow empty is true and an empty
	/// string or null is given, the empty string is returned.
	/// </summary>
	public ValidationResult<string> GetPhoneWithLastFiveMapping(
		ValidationErrorHandler errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage,
		Dictionary<string, string>? firstFives ) {
		return handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				if( GetPhoneNumberAsObject( errorHandler, trimmedInput, allowExtension, allowEmpty, allowSurroundingGarbage, firstFives ).Error( out var value ) is
					    {} error )
					return error;

				valueSetter( value.StandardPhoneString );
				return null;
			} );
	}

	internal ValidationResult<PhoneNumber> GetPhoneNumberAsObject(
		ValidationErrorHandler errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage,
		Dictionary<string, string>? firstFives ) {
		return ExecuteValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				var invalidPrefix = "The " + errorHandler.Subject + " (" + trimmedInput + ") is invalid.";
				// Remove all of the valid delimiter characters so we can just deal with numbers and whitespace
				trimmedInput = trimmedInput.RemoveCharacters( '-', '(', ')', '.' ).Trim();

				var invalidMessage = invalidPrefix +
				                     " Phone numbers may be entered in any format, such as or xxx-xxx-xxxx, with an optional extension up to 5 digits long.  International numbers should begin with a '+' sign.";
				var phoneNumber = PhoneNumber.CreateFromParts( "", "", "" );

				// NOTE: AllowSurroundingGarbage does not apply to first five or international numbers.

				// First-five shortcut (intra-org phone numbers)
				if( firstFives != null && Regex.IsMatch( trimmedInput, @"^\d{5}$" ) ) {
					if( firstFives.ContainsKey( trimmedInput.Substring( 0, 1 ) ) ) {
						var firstFive = firstFives[ trimmedInput.Substring( 0, 1 ) ];
						phoneNumber = PhoneNumber.CreateFromParts( firstFive.Substring( 0, 3 ), firstFive.Substring( 3 ) + trimmedInput, "" );
					}
					else
						return ValidationError.Custom( ErrorCondition.Invalid, "The five digit phone number you entered isn't recognized." );
				}
				// International phone numbers
				// We require a country code and then at least 7 digits (but if country code is more than one digit, we require fewer subsequent digits).
				// We feel this is a reasonable limit to ensure that they are entering an actual phone number, but there is no source for this limit.
				// We have no idea why we ever began accepting letters, but it's risky to stop accepting them and the consequences of accepting them are small.
				else if( Regex.IsMatch( trimmedInput, @"\+\s*[0|2-9]([a-zA-Z,#/ \.\(\)\*]*[0-9]){7}" ) )
					phoneNumber = PhoneNumber.CreateInternational( trimmedInput );
				// Validated it as a North American Numbering Plan phone number
				else {
					var regex = @"(?<lead>\+?1)?\s*(?<ac>\d{3})\s*(?<num1>\d{3})\s*(?<num2>\d{4})\s*?(?:(?:x|\s|ext|ext\.|extension)\s*(?<ext>\d{1,5}))?\s*";
					if( !allowSurroundingGarbage )
						regex = "^" + regex + "$";

					var match = Regex.Match( trimmedInput, regex );

					if( match.Success ) {
						var areaCode = match.Groups[ "ac" ].Value;
						var number = match.Groups[ "num1" ].Value + match.Groups[ "num2" ].Value;
						var extension = match.Groups[ "ext" ].Value;
						phoneNumber = PhoneNumber.CreateFromParts( areaCode, number, extension );
						if( !allowExtension && phoneNumber.Extension.Length > 0 )
							return ValidationError.Custom(
								ErrorCondition.Invalid,
								invalidPrefix + " Extensions are not permitted in this field. Use the separate extension field." );
					}
					else
						return ValidationError.Custom( ErrorCondition.Invalid, invalidMessage );
				}

				valueSetter( phoneNumber );
				return null;
			},
			PhoneNumber.CreateFromParts( "", "", "" ) )!;
	}

	/// <summary>
	/// Returns a validated phone number extension as a string.
	/// If allow empty is true and the empty string or null is given, the empty string is returned.
	/// </summary>
	public ValidationResult<string> GetPhoneNumberExtension( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				if( !Regex.IsMatch( trimmedInput, @"^ *(?<ext>\d{1,5}) *$" ) )
					return ValidationError.Invalid();

				valueSetter( trimmedInput );
				return null;
			} );

	/// <summary>
	/// Returns a validated social security number from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// </summary>
	public ValidationResult<string> GetSocialSecurityNumber( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		GetNumber( errorHandler, input, 9, allowEmpty, "-" );

	/// <summary>
	/// Gets a string of the given length whose characters are only numeric values, after throwing out all acceptable garbage
	/// characters.
	/// Example: A social security number (987-65-4321) would be GetNumber( errorHandler, ssn, 9, true, "-" ).
	/// </summary>
	public ValidationResult<string> GetNumber(
		ValidationErrorHandler errorHandler, string input, int numberOfDigits, bool allowEmpty, params string[] acceptableGarbageStrings ) =>
		handleEmptyAndReturnEmptyStringIfInvalid(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				foreach( var garbageString in acceptableGarbageStrings )
					trimmedInput = trimmedInput.Replace( garbageString, "" );
				trimmedInput = trimmedInput.Trim();
				if( !Regex.IsMatch( trimmedInput, @"^\d{" + numberOfDigits + "}$" ) )
					return ValidationError.Invalid();
				valueSetter( trimmedInput );
				return null;
			} );

	/// <summary>
	/// Gets a validated United States zip code object given the complete zip code with optional +4 digits.
	/// </summary>
	public ValidationResult<ZipCode> GetZipCode( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsZipCode, new ZipCode() )!;

	/// <summary>
	/// Gets a validated US or Canadian zip code.
	/// </summary>
	public ValidationResult<ZipCode> GetUsOrCanadianZipCode( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
		ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsOrCanadianZipCode, new ZipCode() )!;

	/// <summary>
	/// Returns the validated DateTime type from the given string and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public ValidationResult<DateTime> GetSqlSmallDateTime( ValidationErrorHandler errorHandler, string input ) =>
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
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTime( ValidationErrorHandler errorHandler, string input, bool allowEmpty ) =>
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
	public ValidationResult<DateTime> GetSqlSmallDateTimeFromParts( ValidationErrorHandler errorHandler, string month, string day, string year ) =>
		GetSqlSmallDateTime( errorHandler, makeDateFromParts( month, day, year ) );

	/// <summary>
	/// Returns the validated DateTime type from the given date part strings and validation package.
	/// It is restricted to the Sql SmallDateTime range of 1/1/1900 up to 6/6/2079.
	/// If allowEmpty is true and each date part string is empty, null will be returned.
	/// Passing an empty string or null for only some date parts will result in ErrorCondition.Invalid.
	/// </summary>
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTimeFromParts(
		ValidationErrorHandler errorHandler, string month, string day, string year, bool allowEmpty ) =>
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
	public ValidationResult<DateTime> GetSqlSmallDateTimeExact( ValidationErrorHandler errorHandler, string input, string pattern ) =>
		ExecuteValidation<DateTime, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateSqlSmallDateTimeExact( valueSetter, trimmedInput, pattern ) );

	/// <summary>
	/// Returns the validated DateTime type from a date string and an exact match pattern.'
	/// Pattern specifies the date format, such as "MM/dd/yyyy".
	/// </summary>
	public ValidationResult<DateTime?> GetNullableSqlSmallDateTimeExact( ValidationErrorHandler errorHandler, string input, string pattern, bool allowEmpty ) =>
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
				return ValidationError.Custom( ErrorCondition.TooEarly, "The {0} is too early." + minMaxMessage );
			if( date >= maxDate )
				return ValidationError.Custom( ErrorCondition.TooLate, "The {0} is too late." + minMaxMessage );
		}
		return null;
	}

	/// <summary>
	/// Validates the date using given allowEmpty, min, and max constraints.
	/// </summary>
	public ValidationResult<DateTime?> GetNullableDateTime(
		ValidationErrorHandler handler, string input, string[]? formats, bool allowEmpty, DateTime minDate, DateTime maxDate ) =>
		ExecuteValidation<DateTime?, string>(
			handler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateDateTime( value => valueSetter( value ), trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the date using given min and max constraints.
	/// </summary>
	public ValidationResult<DateTime> GetDateTime( ValidationErrorHandler handler, string input, string[]? formats, DateTime minDate, DateTime maxDate ) =>
		ExecuteValidation<DateTime, string>(
			handler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDateTime( valueSetter, trimmedInput, formats, minDate, maxDate ) );

	/// <summary>
	/// Validates the given time span.
	/// </summary>
	public ValidationResult<TimeSpan?> GetNullableTimeSpan( ValidationErrorHandler handler, TimeSpan? input, bool allowEmpty ) =>
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
	public ValidationResult<TimeSpan> GetTimeSpan( ValidationErrorHandler handler, TimeSpan? input ) =>
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
	public ValidationResult<TimeSpan?> GetNullableTimeOfDayTimeSpan( ValidationErrorHandler handler, string input, string[]? formats, bool allowEmpty ) =>
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
	public ValidationResult<TimeSpan> GetTimeOfDayTimeSpan( ValidationErrorHandler handler, string input, string[]? formats ) =>
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
	/// <param name="handler"></param>
	/// <param name="input"></param>
	/// <param name="allowEmpty"></param>
	/// <param name="validationMethod"></param>
	/// <param name="emptyValue">The result value that will be used if the input value is empty or if there is a validation error.</param>
	internal ValidationResult<ValType?> ExecuteValidation<ValType, InputType>(
		ValidationErrorHandler handler, InputType input, bool allowEmpty, ValidationMethod<ValType, InputType> validationMethod, ValType? emptyValue = default ) {
		if( isEmpty( input, out var trimmedInput ) ) {
			if( !allowEmpty ) {
				handler.SetValidationResult( ValidationError.Empty() );
				handler.HandleResult( this, !allowEmpty );
			}
			return new ValidationResult<ValType?>( emptyValue, allowEmpty ? null : ValidationError.Empty() );
		}

		var result = emptyValue;
		if( validationMethod( value => result = value, trimmedInput ) is {} error ) {
			handler.SetValidationResult( error );
			handler.HandleResult( this, !allowEmpty );
			return new ValidationResult<ValType?>( emptyValue, error );
		}

		return new ValidationResult<ValType?>( result, null );
	}

	private ValidationResult<string> handleEmptyAndReturnEmptyStringIfInvalid<InputType>(
		ValidationErrorHandler handler, InputType valueAsObject, bool allowEmpty, ValidationMethod<string, InputType> method ) =>
		ExecuteValidation( handler, valueAsObject, allowEmpty, method, "" )!;
}