using System.Text.RegularExpressions;

namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public static ValidationResult<string> GetString( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.GetString( errorHandler, input, allowEmpty, int.MaxValue );

	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public static ValidationResult<string> GetString(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, int maxLength ) =>
		validator.GetString( errorHandler, input, allowEmpty, 0, maxLength );

	/// <summary>
	/// Returns a validated string from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// Automatically trims whitespace from edges of returned string.
	/// </summary>
	public static ValidationResult<string> GetString(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, int minLength, int maxLength ) =>
		validator.executeStringValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				var errorMessage = "The length of the {0} must be between " + minLength + " and " + maxLength + " characters.";
				if( trimmedInput.Length > maxLength )
					return ValidationError.Custom( ValidationErrorType.TooLong, errorMessage );
				if( trimmedInput.Length < minLength )
					return ValidationError.Custom( ValidationErrorType.TooShort, errorMessage );

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
	public static ValidationResult<string> GetEmailAddress(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, int maxLength = 254 ) =>
		validator.executeStringValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				// Validate as a string with same restrictions - if it fails on that, return
				if( new Validator().GetString( null, trimmedInput, allowEmpty, maxLength ).Error( out _ ) is {} error )
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
	public static ValidationResult<string> GetUrl( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.GetUrl( errorHandler, input, allowEmpty, 2048 );

	/// <summary>
	/// Returns a validated URL. Note that you may run into problems with certain browsers if you pass a length longer than
	/// 2048.
	/// </summary>
	public static ValidationResult<string> GetUrl(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, int maxUrlLength ) =>
		validator.executeStringValidation(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => {
				/* If the string is just a number, reject it right out. */
				if( int.TryParse( trimmedInput, out _ ) || double.TryParse( trimmedInput, out _ ) )
					return ValidationError.Invalid();

				/* If it's an email, it's not an URL. */
				if( new Validator().GetEmailAddress( null, trimmedInput, allowEmpty ).Error( out _ ) is null )
					return ValidationError.Invalid();

				/* If it doesn't start with one of our whitelisted schemes, add in the best guess. */
				var validSchemes = new[] { "http", "https", "ftp" };
				if( new Validator().GetString(
						    null,
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
	/// Returns a validated social security number from the given string and restrictions.
	/// If allowEmpty true and an empty string or null is given, the empty string is returned.
	/// </summary>
	public static ValidationResult<string> GetSocialSecurityNumber(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.GetNumber( errorHandler, input, 9, allowEmpty, "-" );

	/// <summary>
	/// Gets a string of the given length whose characters are only numeric values, after throwing out all acceptable garbage
	/// characters.
	/// Example: A social security number (987-65-4321) would be GetNumber( errorHandler, ssn, 9, true, "-" ).
	/// </summary>
	public static ValidationResult<string> GetNumber(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, int numberOfDigits, bool allowEmpty,
		params string[] acceptableGarbageStrings ) =>
		validator.executeStringValidation(
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

	private static ValidationResult<string> executeStringValidation<InputType>(
		this Validator validator, ValidationErrorHandler? handler, InputType input, bool allowEmpty,
		Validator.ValidationMethod<string, InputType> validationMethod ) =>
		validator.ExecuteValidation( handler, input, allowEmpty, validationMethod, emptyValue: "" )!;
}