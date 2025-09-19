using System.Text.RegularExpressions;

namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// The same as GetPhoneNumber, except the given default area code will be prepended on the phone number if necessary.
	/// This is useful when working with data that had the area code omitted because the number was local.
	/// </summary>
	public static ValidationResult<string> GetPhoneNumberWithDefaultAreaCode(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage,
		string defaultAreaCode ) {
		if( new Validator().GetPhoneNumber( null, input, allowExtension, allowEmpty, allowSurroundingGarbage ).Error( out _ ) is not null )
			// If the phone number was invalid without the area code, but is valid with the area code, we really validate using the default
			// area code and then return.  In all other cases, we return what would have happened without tacking on the default area code.
			if( new Validator().GetPhoneNumber( null, defaultAreaCode + input, allowExtension, allowEmpty, allowSurroundingGarbage ).Error( out _ ) is null )
				return validator.GetPhoneNumber( errorHandler, defaultAreaCode + input, allowExtension, allowEmpty, allowSurroundingGarbage );

		return validator.GetPhoneNumber( errorHandler, input, allowExtension, allowEmpty, allowSurroundingGarbage );
	}

	/// <summary>
	/// Returns a validated phone number as a standard phone number string given the complete phone number with optional
	/// extension as a string. If allow empty is true and an empty string or null is given, the empty string is returned.
	/// Pass true for allow surrounding garbage if you want to allow "The phone number is 585-455-6476yadayada." to be parsed
	/// into 585-455-6476
	/// and count as a valid phone number.
	/// </summary>
	public static ValidationResult<string> GetPhoneNumber(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage ) =>
		validator.GetPhoneWithLastFiveMapping( errorHandler, input, allowExtension, allowEmpty, allowSurroundingGarbage, null );

	/// <summary>
	/// Returns a validated phone number as a standard phone number string given the complete phone number with optional
	/// extension or the last five digits of the number and a dictionary of single
	/// digits to five-digit groups that become the first five digits of the full number.  If allow empty is true and an empty
	/// string or null is given, the empty string is returned.
	/// </summary>
	public static ValidationResult<string> GetPhoneWithLastFiveMapping(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage,
		Dictionary<string, string>? firstFives ) {
		return validator.executeStringValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				if( new Validator().GetPhoneNumberAsObject( null, trimmedInput, allowExtension, allowEmpty, allowSurroundingGarbage, firstFives )
					    .Error( out var value ) is {} error )
					return error;

				valueSetter( value.StandardPhoneString );
				return null;
			} );
	}

	internal static ValidationResult<PhoneNumber> GetPhoneNumberAsObject(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowExtension, bool allowEmpty, bool allowSurroundingGarbage,
		Dictionary<string, string>? firstFives ) {
		return validator.ExecuteValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				var invalidPrefix = "The {0} (" + trimmedInput + ") is invalid.";
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
						return ValidationError.Custom( ValidationErrorType.Invalid, "The five digit phone number you entered isn't recognized." );
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
								ValidationErrorType.Invalid,
								invalidPrefix + " Extensions are not permitted in this field. Use the separate extension field." );
					}
					else
						return ValidationError.Custom( ValidationErrorType.Invalid, invalidMessage );
				}

				valueSetter( phoneNumber );
				return null;
			},
			emptyValue: PhoneNumber.CreateFromParts( "", "", "" ) )!;
	}

	/// <summary>
	/// Returns a validated phone number extension as a string.
	/// If allow empty is true and the empty string or null is given, the empty string is returned.
	/// </summary>
	public static ValidationResult<string> GetPhoneNumberExtension(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.executeStringValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				if( !Regex.IsMatch( trimmedInput, @"^ *(?<ext>\d{1,5}) *$" ) )
					return ValidationError.Invalid();

				valueSetter( trimmedInput );
				return null;
			} );
}