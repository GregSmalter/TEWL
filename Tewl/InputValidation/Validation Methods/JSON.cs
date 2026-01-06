using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// Returns the JSON array represented by the specified input string.
	/// </summary>
	public static ValidationResult<JsonArray?> GetJsonArray( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.ExecuteValidation<JsonArray, string>(
			errorHandler ?? new ValidationErrorHandler( "value(s)" ),
			input,
			true,
			( valueSetter, trimmedInput ) => {
				JsonArray value;
				try {
					value = (JsonArray)JsonNode.Parse( $"[{trimmedInput}]" )!;
				}
				catch( JsonException ) {
					return ValidationError.Custom( ValidationErrorType.Invalid, "The {0} must be valid JSON." );
				}

				valueSetter( value );
				return null;
			} );
}