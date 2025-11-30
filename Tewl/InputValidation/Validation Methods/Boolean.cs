namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// Accepts either true/false (case-sensitive) or 1/0.
	/// Returns the validated boolean type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<bool> GetBoolean( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.ExecuteValidation<bool, string>( errorHandler, input, false, validateBoolean );

	/// <summary>
	/// Accepts either true/false (case-sensitive) or 1/0.
	/// Returns the validated boolean type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<bool?> GetNullableBoolean( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.ExecuteValidation<bool?, string>(
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
}