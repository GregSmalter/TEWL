using Tewl.InputValidation;

namespace Tewl;

/// <summary>
/// Contains methods that prepare data for display. Data is assumed to be valid.
/// </summary>
[ PublicAPI ]
public static class FormattingMethods {
	/// <summary>
	/// Formats the specified phone number in the 555-555-5555 x1234 style. Does not accept null.
	/// </summary>
	public static string GetPhoneWithDashes( string standardPhoneString ) {
		var phoneNumber = PhoneNumber.CreateFromStandardPhoneString( standardPhoneString );
		return GetPhoneWithDashesFromObject( phoneNumber );
	}

	internal static string GetPhoneWithDashesFromObject( PhoneNumber phoneNumber ) {
		// If anyone ever asks for leading ones to be shown (1-800-555-5555), do it by using a known list of area codes (800, 888).
		var formattedPhoneString = "";
		if( !phoneNumber.Empty ) {
			if( phoneNumber.IsInternational )
				formattedPhoneString = phoneNumber.InternationalNumber;
			else {
				formattedPhoneString += phoneNumber.AreaCode + "-";
				formattedPhoneString += phoneNumber.Number.Insert( 3, "-" );
				formattedPhoneString += phoneNumber.Extension.Length == 0 ? "" : " x" + phoneNumber.Extension;
			}
		}

		return formattedPhoneString;
	}

	/// <summary>
	/// Formats the specified phone number in the (555) 555-5555 x1234 style. Does not accept null.
	/// </summary>
	public static string GetPhoneWithAcParens( string standardPhoneString ) {
		var phoneNumber = PhoneNumber.CreateFromStandardPhoneString( standardPhoneString );
		var formattedPhoneString = "";
		if( phoneNumber.IsInternational )
			formattedPhoneString = phoneNumber.InternationalNumber;
		else {
			if( phoneNumber.AreaCode.Length > 0 )
				formattedPhoneString += "(" + phoneNumber.AreaCode + ") ";
			if( phoneNumber.Number.Length > 0 )
				formattedPhoneString += phoneNumber.Number.Insert( 3, "-" );
			if( phoneNumber.Extension.Length > 0 )
				formattedPhoneString += " x" + phoneNumber.Extension;
		}

		return formattedPhoneString;
	}

	/// <summary>
	/// Formats the specified phone number in the 555.555.5555 x1234 style. Does not accept null.
	/// </summary>
	public static string GetPhoneWithDots( string standardPhoneString ) {
		var phoneNumber = PhoneNumber.CreateFromStandardPhoneString( standardPhoneString );
		var formattedPhoneString = "";
		if( !phoneNumber.Empty ) {
			if( phoneNumber.IsInternational )
				formattedPhoneString = phoneNumber.InternationalNumber;
			else {
				formattedPhoneString += phoneNumber.AreaCode + "." + phoneNumber.Number.Insert( 3, "." );
				formattedPhoneString += phoneNumber.Extension.Length == 0 ? "" : " x" + phoneNumber.Extension;
			}
		}

		return formattedPhoneString;
	}

	/// <summary>
	/// Extracts the extension digits from the specified phone number. Does not accept null.
	/// </summary>
	public static string GetPhoneExtension( string standardPhoneString ) => PhoneNumber.CreateFromStandardPhoneString( standardPhoneString ).Extension;

	/// <summary>
	/// Formats the specified social security number with dashes. Accepts the empty string, but does not accept null.
	/// </summary>
	public static string GetSocialSecurityNumberWithDashes( string ssn ) {
		if( ssn.Length > 0 )
			return ssn.Substring( 0, 3 ) + "-" + ssn.Substring( 3, 2 ) + "-" + ssn.Substring( 5, 4 );
		return "";
	}

	/// <summary>
	/// Formats the specified address in multi line format. Do not pass null for any parameters.
	/// </summary>
	public static string GetAddressWithNewLines( string deliveryAddress, string city, string stateAbbreviation, string zipCode, string addOnCode ) {
		// The add on code means nothing without the ZIP Code.
		if( zipCode.Length == 0 )
			addOnCode = "";

		return StringTools.ConcatenateWithDelimiter(
			Environment.NewLine,
			deliveryAddress,
			StringTools.ConcatenateWithDelimiter(
				" ",
				StringTools.ConcatenateWithDelimiter( ", ", city, stateAbbreviation ),
				StringTools.ConcatenateWithDelimiter( "-", zipCode, addOnCode ) ) );
	}

	/// <summary>
	/// Formats the specified address in a single-line format. Do not pass null for any parameters.
	/// </summary>
	public static string GetAddressOneLine( string deliveryAddress, string city, string stateAbbreviation, string zipCode, string addOnCode ) =>
		GetAddressWithNewLines( deliveryAddress, city, stateAbbreviation, zipCode, addOnCode ).Replace( Environment.NewLine, ", " );

	/// <summary>
	/// Uses GetFormattedBytes to return a string in the form "60.1 GiB/s".
	/// </summary>
	public static string GetFormattedBytesPerSecond( long numberOfBytes, TimeSpan elapsedTime ) =>
		GetFormattedBytes( (ulong)( numberOfBytes / elapsedTime.TotalSeconds ), true ) + "/s";

	/// <summary>
	/// Returns the given number of bytes in the most useful way possible. For example, 64 will return 64 bytes. 64,000 will return 64 kB or 62 KiB.
	/// 64,500,000,000 will return 64.5 GB or 60.1 GiB. Maximum precision is 3 significant digits. GB and GiB are the largest units returned.
	/// </summary>
	/// <param name="numberOfBytes"></param>
	/// <param name="useBinaryPrefix">Pass true for 1,024-based (binary) units such as MiB or GiB instead of MB and GB.</param>
	public static string GetFormattedBytes( long numberOfBytes, bool useBinaryPrefix ) => GetFormattedBytes( (ulong)numberOfBytes, useBinaryPrefix );

	/// <summary>
	/// Returns the given number of bytes in the most useful way possible. For example, 64 will return 64 bytes. 64,000 will return 64 kB or 62 KiB.
	/// 64,500,000,000 will return 64.5 GB or 60.1 GiB. Maximum precision is 3 significant digits. GB and GiB are the largest units returned.
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="useBinaryPrefix">Pass true for 1,024-based (binary) units such as MiB or GiB instead of MB and GB.</param>
	public static string GetFormattedBytes( ulong bytes, bool useBinaryPrefix ) {
		var stepMultiplier = useBinaryPrefix ? 1024u : 1000u;
		const string doubleFormattingString = "G3";

		if( bytes < stepMultiplier )
			return bytes + " bytes";
		if( bytes < Math.Pow( stepMultiplier, 2 ) )
			return ( bytes / stepMultiplier ).ToString( doubleFormattingString ) + ( useBinaryPrefix ? " KiB" : " kB" );
		if( bytes < Math.Pow( stepMultiplier, 3 ) )
			return ( bytes / Math.Pow( stepMultiplier, 2 ) ).ToString( doubleFormattingString ) + ( useBinaryPrefix ? " MiB" : " MB" );
		return ( bytes / Math.Pow( stepMultiplier, 3 ) ).ToString( doubleFormattingString ) + ( useBinaryPrefix ? " GiB" : " GB" );
	}
}