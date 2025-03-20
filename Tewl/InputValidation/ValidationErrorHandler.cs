namespace Tewl.InputValidation;

/// <summary>
/// This class allows you to control what happens when a validation method generates an error. Every validation method takes a ValidationErrorHandler object
/// as the first parameter.
/// </summary>
[ PublicAPI ]
public class ValidationErrorHandler {
	/// <summary>
	/// Method that handles errors instead of the default handling mechanism.
	/// </summary>
	public delegate void CustomHandler( ValidationErrorType errorType );

	/// <summary>
	/// The subject of the error message, if one needs to be generated.
	/// </summary>
	internal string Subject { get; } = "field";

	private readonly CustomHandler? customHandler;
	private readonly Dictionary<ValidationErrorType, string> customMessages = new();

	/// <summary>
	/// Creates an error handler that adds standard error messages, based on the specified subject, to the validator. If the
	/// subject is used to start a
	/// sentence, it will be automatically capitalized.
	/// </summary>
	public ValidationErrorHandler( string subject ) => Subject = subject;

	/// <summary>
	/// Creates an object that invokes code of your choice if an error occurs. The given custom handler will
	/// only be invoked in the case of an error, and it will prevent the default error message from being
	/// added to the validator's error collection.  Even if the handler does not add an error to the validator,
	/// validator.HasErrors will return true because an error has still occurred.
	/// </summary>
	public ValidationErrorHandler( CustomHandler customHandler ) => this.customHandler = customHandler;

	/// <summary>
	/// Modifies this error handler to use a custom message if any errors occur with the specified types. If no error types are passed, the message will be used
	/// for all errors. This method has no effect if a custom handler has been specified.
	/// </summary>
	public void AddCustomErrorMessage( string message, params ValidationErrorType[] errorTypes ) {
		if( errorTypes.Length > 0 )
			foreach( var e in errorTypes )
				customMessages.Add( e, message );
		else
			foreach( var e in EnumTools.GetValues<ValidationErrorType>() )
				customMessages.Add( e, message );
	}

	/// <summary>
	/// Invokes the appropriate behavior according to how this error handler was created.
	/// </summary>
	internal string HandleError( ValidationError error ) {
		// if there is a custom handler, run it and do nothing else
		if( customHandler is not null ) {
			customHandler( error.Type );
			return "";
		}

		// build the error message
		if( !customMessages.TryGetValue( error.Type, out var message ) )
			// NOTE: Do we really need custom message, or can the custom handler manage that?
			message = error.GetMessage( Subject );

		return message;
	}
}