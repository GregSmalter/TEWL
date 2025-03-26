namespace Tewl.InputValidation;

/// <summary>
/// The result of a validation.
/// </summary>
[ PublicAPI ]
public class ValidationResult<T> {
	/// <summary>
	/// Gets the validated value. This is sometimes unusable if there was a validation error, and in that case <see cref="Validator.UnusableValuesReturned"/> will
	/// be true.
	/// </summary>
	public T Value { get; }

	private readonly ValidationError? error;

	/// <summary>
	/// Validator.ExecuteValidation use only.
	/// </summary>
	internal ValidationResult( T value, ValidationError? error ) {
		Value = value;
		this.error = error;
	}

	/// <summary>
	/// Returns the validation error, or null if validation was successful.
	/// </summary>
	/// <param name="value">The validated value.</param>
	public ValidationError? Error( out T value ) {
		value = Value;
		return error;
	}
}