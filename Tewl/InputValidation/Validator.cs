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