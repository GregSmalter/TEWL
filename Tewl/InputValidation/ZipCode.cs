using System.Text.RegularExpressions;

namespace Tewl.InputValidation {
	/// <summary>
	/// A zip code.
	/// </summary>
	[ PublicAPI ]
	public class ZipCode {
		private const string usPattern = @"(?<zip>^\d{5})(-?(?<plus4>\d{4}))?$";
		private const string canadianPattern = @"(?<caZip>^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1} *\d{1}[A-Z]{1}\d{1}$)";

		/// <summary>
		/// The normal 5-digit zip code.
		/// </summary>
		public string Zip { get; private set; } = string.Empty;

		/// <summary>
		/// The optional 4-digit extension.
		/// </summary>
		public string Plus4 { get; private set; } = string.Empty;

		/// <summary>
		/// The full zip code with 4-digit extension (12345-6789).
		/// </summary>
		public string FullZipCode => StringTools.ConcatenateWithDelimiter( "-", Zip, Plus4 );

		internal ZipCode() {}

		internal static ValidationError? CreateUsZipCode( Action<ZipCode> valueSetter, string trimmedInput ) {
			var match = Regex.Match( trimmedInput, usPattern );
			if( match.Success ) {
				valueSetter( getZipCodeFromValidUsMatch( match ) );
				return null;
			}

			return ValidationError.Invalid();
		}

		internal static ValidationError? CreateUsOrCanadianZipCode( Action<ZipCode> valueSetter, string trimmedInput ) {
			var match = Regex.Match( trimmedInput, usPattern );
			if( match.Success ) {
				valueSetter( getZipCodeFromValidUsMatch( match ) );
				return null;
			}
			if( ( match = Regex.Match( trimmedInput, canadianPattern, RegexOptions.IgnoreCase ) ).Success ) {
				valueSetter( new ZipCode { Zip = match.Groups[ "caZip" ].Value } );
				return null;
			}

			return ValidationError.Invalid();
		}

		private static ZipCode getZipCodeFromValidUsMatch( Match match ) => new() { Zip = match.Groups[ "zip" ].Value, Plus4 = match.Groups[ "plus4" ].Value };
	}
}