namespace Tewl.InputValidation;

partial class ValidatorExtensions {
	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<byte> GetByte( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.GetByte( errorHandler, input, byte.MinValue, byte.MaxValue );

	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<byte> GetByte( this Validator validator, ValidationErrorHandler? errorHandler, string input, byte min, byte max ) =>
		validator.ExecuteValidation<byte, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated byte type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<byte?> GetNullableByte( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.ExecuteValidation<byte?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<byte>( value => valueSetter( value ), trimmedInput, byte.MinValue, byte.MaxValue ) );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<short> GetShort( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.GetShort( errorHandler, input, short.MinValue, short.MaxValue );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<short> GetShort( this Validator validator, ValidationErrorHandler? errorHandler, string input, short min, short max ) =>
		validator.ExecuteValidation<short, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<short?> GetNullableShort( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.GetNullableShort( errorHandler, input, allowEmpty, short.MinValue, short.MaxValue );

	/// <summary>
	/// Returns the validated short type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<short?> GetNullableShort(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, short min, short max ) =>
		validator.ExecuteValidation<short?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<short>( value => valueSetter( value ), trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<int> GetInt( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.GetInt( errorHandler, input, int.MinValue, int.MaxValue );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// <paramref name="min" /> and <paramref name="max" /> are inclusive.
	/// </summary>
	public static ValidationResult<int> GetInt( this Validator validator, ValidationErrorHandler? errorHandler, string input, int min, int max ) =>
		validator.ExecuteValidation<int, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated int type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<int?> GetNullableInt(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, int min = int.MinValue, int max = int.MaxValue ) =>
		validator.ExecuteValidation<int?, string>(
			errorHandler,
			input,
			allowEmpty,
			( valueSetter, trimmedInput ) => validateGenericIntegerType<int>( value => valueSetter( value ), trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated long type from the given string and validation package.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// <paramref name="min" /> and <paramref name="max" /> are inclusive.
	/// </summary>
	public static ValidationResult<long> GetLong(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, long min = long.MinValue, long max = long.MaxValue ) =>
		validator.ExecuteValidation<long, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateGenericIntegerType( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns the validated long type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<long?> GetNullableLong(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, long min = long.MinValue, long max = long.MaxValue ) =>
		validator.ExecuteValidation<long?, string>(
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
	public static ValidationResult<float> GetFloat( this Validator validator, ValidationErrorHandler? errorHandler, string input, float min, float max ) =>
		validator.ExecuteValidation<float, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateFloat( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns a validated float type from the given string, validation package, and min/max restrictions.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<float?> GetNullableFloat(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, float min, float max ) =>
		validator.ExecuteValidation<float?, string>(
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
	public static ValidationResult<decimal> GetDecimal( this Validator validator, ValidationErrorHandler? errorHandler, string input ) =>
		validator.GetDecimal( errorHandler, input, decimal.MinValue, decimal.MaxValue );

	/// <summary>
	/// Returns a validated decimal type from the given string, validation package, and min/max restrictions.
	/// Passing an empty string or null will result in ErrorCondition.Empty.
	/// </summary>
	public static ValidationResult<decimal>
		GetDecimal( this Validator validator, ValidationErrorHandler? errorHandler, string input, decimal min, decimal max ) =>
		validator.ExecuteValidation<decimal, string>(
			errorHandler,
			input,
			false,
			( valueSetter, trimmedInput ) => validateDecimal( valueSetter, trimmedInput, min, max ) );

	/// <summary>
	/// Returns a validated decimal type from the given string and validation package.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<decimal?>
		GetNullableDecimal( this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty ) =>
		validator.GetNullableDecimal( errorHandler, input, allowEmpty, decimal.MinValue, decimal.MaxValue );

	/// <summary>
	/// Returns a validated decimal type from the given string, validation package, and min/max restrictions.
	/// If allowEmpty is true and the given string is empty, null will be returned.
	/// </summary>
	public static ValidationResult<decimal?> GetNullableDecimal(
		this Validator validator, ValidationErrorHandler? errorHandler, string input, bool allowEmpty, decimal min, decimal max ) =>
		validator.ExecuteValidation<decimal?, string>(
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
}