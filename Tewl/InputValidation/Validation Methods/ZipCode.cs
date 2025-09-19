namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// Gets a validated United States zip code object given the complete zip code with optional +4 digits.
	/// </summary>
	public static ValidationResult<ZipCode> GetZipCode( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsZipCode, emptyValue: new ZipCode() )!;

	/// <summary>
	/// Gets a validated US or Canadian zip code.
	/// </summary>
	public static ValidationResult<ZipCode> GetUsOrCanadianZipCode(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.ExecuteValidation( errorHandler, input, allowEmpty, ZipCode.CreateUsOrCanadianZipCode, emptyValue: new ZipCode() )!;
}