using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tewl.Tools {
	/// <summary>
	/// Provides helpful string methods.
	/// </summary>
	public static class StringTools {
		/// <summary>
		/// Returns a two-element string array containing
		/// the strings on either side of the given word (neither
		/// including the word).  Whole word (word surrounded by
		/// spaces) is searched, only.  Returns null if the word is
		/// not found in the given string, or if the given
		/// string is null.
		/// </summary>
		public static string[] DissectByWholeWord( this string line, string word ) {
			if( line == null )
				return null;

			word = " " + word.Trim() + " ";
			var wordIndex = line.IndexOf( word );
			if( wordIndex == -1 )
				return null;

			var elements = new string[ 2 ];

			elements[ 0 ] = line.Substring( 0, wordIndex ).Trim();
			elements[ 1 ] = line.Substring( wordIndex + word.Length ).Trim();
			return elements;
		}

		/// <summary>
		/// Returns a two-element string array containing the strings on either
		/// side of the given character (neither including the character).  Returns
		/// null if the character is not found in the given string, or if the given
		/// string is null.
		/// </summary>
		public static string[] DissectByChar( this string line, char character ) {
			if( line == null )
				return null;

			var wordIndex = line.IndexOf( character );
			if( wordIndex == -1 )
				return null;

			var elements = new string[ 2 ];

			elements[ 0 ] = line.Substring( 0, wordIndex ).Trim();
			elements[ 1 ] = line.Substring( wordIndex + 1 ).Trim();
			return elements;
		}

		/// <summary>
		/// Returns the given string with its first letter-or-digit character capitalized.
		/// </summary>
		public static string CapitalizeString( this string text ) {
			if( text == null )
				return null;

			return new string( text.ToCharArray().Select( ( c, index ) => index == getIndexOfFirstLetterOrDigit( text ) ? char.ToUpper( c ) : c ).ToArray() );
		}

		/// <summary>
		/// Returns the given string with its first letter-or-digit character lowercased. Do not pass null.
		/// </summary>
		public static string LowercaseString( this string text ) =>
			new string( text.ToCharArray().Select( ( c, index ) => index == getIndexOfFirstLetterOrDigit( text ) ? char.ToLower( c ) : c ).ToArray() );

		private static int getIndexOfFirstLetterOrDigit( string text ) => text.IndexOfAny( text.ToCharArray().Where( char.IsLetterOrDigit ).ToArray() );

		/// <summary>
		/// Removes all of the non-alphanumeric characters from this string.
		/// </summary>
		public static string RemoveNonAlphanumericCharacters( this string text, bool preserveWhiteSpace = false ) =>
			// See http://stackoverflow.com/a/8779277/35349.
			Regex.Replace( text, preserveWhiteSpace ? @"[^A-Za-z0-9\s]+" : @"[^A-Za-z0-9]+", "" );

		/// <summary>
		/// Removes all of the non-alpha characters, including white space, from this string.
		/// </summary>
		public static string RemoveNonAlphaCharacters( this string text ) =>
			// See http://stackoverflow.com/a/8779277/35349.
			Regex.Replace( text, @"[^A-Za-z]+", "" );

		/// <summary>
		/// Replaces each of the non-alphanumeric characters in this string with a space.
		/// </summary>
		public static string ReplaceNonAlphanumericCharactersWithWhiteSpace( this string text ) => Regex.Replace( text, @"[^A-Za-z0-9]", " " );

		/// <summary>
		/// Replaces each of the non-alpha characters in this string with a space.
		/// </summary>
		public static string ReplaceNonAlphaCharactersWithWhiteSpace( this string text ) => Regex.Replace( text, @"[^A-Za-z]", " " );

		/// <summary>
		/// Removes all instances of all given characters from the
		/// given string, and returns this new string.
		/// </summary>
		public static string RemoveCharacters( this string text, params char[] characters ) => replaceCharactersWithString( text, "", characters );

		/// <summary>
		/// Returns a string with all of the given characters replaced with whitespace.
		/// </summary>
		public static string ReplaceCharactersWithWhitespace( this string text, params char[] characters ) => replaceCharactersWithString( text, " ", characters );

		private static string replaceCharactersWithString( string text, string replacementString, params char[] characters ) {
			if( text is null )
				return null;

			foreach( var character in characters )
				text = text.Replace( character.ToString(), replacementString );
			return text;
		}

		/// <summary>
		/// Returns true if the string is "true" (ignoring case) and false otherwise.
		/// Do not call this method on a null string.
		/// </summary>
		public static bool StringToBool( this string word ) => word.ToLower() == "true";

		/// <summary>
		/// Returns null if the string is the empty string, otherwise returns the same as StringToBool.
		/// Do not call this method on a null string.
		/// </summary>
		public static bool? StringToNullableBool( this string word ) {
			if( word.Length == 0 )
				return null;

			return StringToBool( word );
		}

		/// <summary>
		/// Returns the given string with every instance of "xY" where x is a lowercase
		/// letter and Y is a capital letter with "x Y".  Therefore, "LeftLeg" becomes "Left Leg".
		/// Also handles digits and converts a string such as "Reference1Name" to "Reference 1 Name".
		/// </summary>
		public static string CamelToEnglish( this string text ) {
			// Don't do anything with null
			// Skip empty string since we'll get out of range errors
			if( string.IsNullOrEmpty( text ) )
				return text;

			// When a space should be inserted directly before the current character onto the new string:
			// Y/N insert space
			//													text[i]
			//										lower		upper		digit
			//							lower   N				Y				Y
			//	text[i-1]		upper		N				N				Y
			//							digit		Y				Y				N

			var newText = "";
			for( var i = 1; i < text.Length; i++ ) {
				newText += text[ i - 1 ];

				var previousChar = new { IsUpper = char.IsUpper( text[ i - 1 ] ), IsLower = char.IsLower( text[ i - 1 ] ), IsDigit = char.IsDigit( text[ i - 1 ] ) };
				var currentChar = new { IsUpper = char.IsUpper( text[ i ] ), IsLower = char.IsLower( text[ i ] ), IsDigit = char.IsDigit( text[ i ] ) };

				if( currentChar.IsUpper && ( previousChar.IsLower || previousChar.IsDigit ) || currentChar.IsDigit && ( previousChar.IsLower || previousChar.IsUpper ) ||
				    currentChar.IsLower && previousChar.IsDigit )
					newText += " ";
			}

			return newText + text[ text.Length - 1 ];
		}

		/// <summary>
		/// Returns the given string with underscores replaced by spaces and capitalization at the beginning of every word.
		/// Example: "FIRST_NAME" becomes "First Name".
		/// </summary>
		public static string OracleToEnglish( this string text ) => ConcatenateWithDelimiter( " ", text.Separate( "_", true ).Select( s => s.ToLower().CapitalizeString() ).ToArray() );

		/// <summary>
		/// Removes whitespace from between words, capitalizes the first letter of each word, lowercases the remainder of each
		/// word,
		/// and lowercases the first letter of the whole string (ex: "One two" becomes "oneTwo"). Trims the resulting string.
		/// Do not call this on the null string.
		/// </summary>
		public static string EnglishToCamel( this string text ) => text.EnglishToPascal().LowercaseString();

		/// <summary>
		/// Removes whitespace from between words, capitalizes the first letter of each word, and lowercases the remainder of each
		/// word (ex: "one two" becomes "OneTwo").
		/// Trims the resulting string.
		/// Do not call this on the null string.
		/// </summary>
		public static string EnglishToPascal( this string text ) => ConcatenateWithDelimiter( "", text.Separate().Select( t => t.ToLower().CapitalizeString() ).ToArray() );

		/// <summary>
		/// Lowercases the specified string and replaces spaces with underscores.
		/// </summary>
		public static string EnglishToOracle( this string text ) => ConcatenateWithDelimiter( "_", text.Separate().Select( i => i.ToLower() ).ToArray() );

		/// <summary>
		/// Removes all invalid file name characters from the string, and then returns ToPascalCase for the resulting string.
		/// Also guarantees the string will not end in a period or space.
		/// Do not call this on the null string.
		/// </summary>
		public static string ToSafeFileName( this string text ) => text.RemoveCharacters( Path.GetInvalidFileNameChars() ).TrimEnd( '.' ).EnglishToPascal();

		/// <summary>
		/// Returns true if the given string is null or contains only whitespace (is empty after being trimmed). Do not call this
		/// unless you understand its
		/// appropriate and inappropriate uses as documented in coding standards.
		/// </summary>
		public static bool IsNullOrWhiteSpace( this string text ) => text == null || text.IsWhitespace();

		/// <summary>
		/// Returns true if the string is empty or made up entirely of whitespace characters (as defined by the Trim method).
		/// The string must not be null.
		/// </summary>
		public static bool IsWhitespace( this string text ) => text.Trim().Length == 0;

		/// <summary>
		/// Concatenates two strings together with a space between them. If either string is empty or if both strings are empty,
		/// there will be no space added.
		/// Null strings are treated as empty strings.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// </summary>
		public static string ConcatenateWithSpace( this string s1, string s2 ) => ConcatenateWithSpace( new[] { s1, s2 } );

		/// <summary>
		/// Concatenates two strings together with a space between them. If either string is empty or if both strings are empty,
		/// there will be no space added.
		/// Null strings are treated as empty strings.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// </summary>
		public static string ConcatenateWithSpace( params string[] strings ) => ConcatenateWithSpace( (IEnumerable<string>)strings );

		/// <summary>
		/// Concatenates two strings together with a space between them. If either string is empty or if both strings are empty,
		/// there will be no space added.
		/// Null strings are treated as empty strings.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// </summary>
		public static string ConcatenateWithSpace( IEnumerable<string> strings ) => ConcatenateWithDelimiter( " ", strings );

		/// <summary>
		/// Given a collection, returns a comma-delimited list of the ToStrings of the elements. Null objects are converted to
		/// empty strings.
		/// Empty strings are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of
		/// the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// </summary>
		public static string GetCommaDelimitedStringFromCollection<T>( this IEnumerable<T> collection ) =>
			ConcatenateWithDelimiter( ", ", collection.Select( o => o.ObjectToString( true ) ) );

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// </summary>
		public static string ConcatenateWithDelimiter( string delimiter, IEnumerable<string> strings ) {
			var tokens = strings.Select( i => ( i ?? "" ).Trim() ).Where( i => i.Length > 0 ).ToList();
			if( !tokens.Any() )
				return "";
			var result = new StringBuilder( tokens.First() );
			foreach( var token in tokens.Skip( 1 ) )
				result.Append( delimiter + token );
			return result.ToString();
		}

		/// <summary>
		/// Creates a single string consisting of each string in the given list, delimited by the given delimiter.  Empty strings
		/// are handled intelligently in that you will not get two delimiters in a row, or a delimiter at the end of the string.
		/// Whitespace is trimmed from the given strings before concatenation.
		/// Null strings are treated as empty strings.
		/// </summary>
		public static string ConcatenateWithDelimiter( string delimiter, params string[] strings ) => ConcatenateWithDelimiter( delimiter, (IEnumerable<string>)strings );

		/// <summary>
		/// Performs ConcatenateWithDelimiter with characters instead of strings.
		/// Null strings are treated as empty strings.
		/// </summary>
		public static string ConcatenateWithDelimiter( string delimiter, params char[] chars ) {
			var strings = new string[ chars.Length ];
			var cnt = 0;
			foreach( var character in chars )
				strings[ cnt++ ] = character.ToString();
			return ConcatenateWithDelimiter( delimiter, strings );
		}

		/// <summary>
		/// Returns the given string truncated to the given max length (if necessary).
		/// </summary>
		public static string Truncate( this string s, int maxLength ) {
			if( s is null )
				return null;

			return s.Substring( 0, Math.Min( maxLength, s.Length ) );
		}

		/// <summary>
		/// Returns the given string truncated from the front to the given max length (if necessary).
		/// </summary>
		public static string TruncateStart( this string s, int maxLength ) {
			if( s is null )
				return null;

			return s.Substring( Math.Max( 0, s.Length - maxLength ), Math.Min( maxLength, s.Length ) );
		}

		/// <summary>
		/// Removes all characters that are between the begin string and the end string, not including the begin and end strings.
		/// For example, "This 'quoted text'.".RemoveTextBetweenStrings( "'", "'" ) returns "This ''.";
		/// </summary>
		public static string RemoveTextBetweenStrings( this string s, string beginString, string endString ) =>
			Regex.Replace( s, getRegexSafeString( beginString ) + @"(.*?\s*)*" + getRegexSafeString( endString ), beginString + endString, RegexOptions.Multiline );

		private static string getRegexSafeString( string s ) => @"\" + ConcatenateWithDelimiter( @"\", s.ToCharArray() );

		/// <summary>
		/// Splits this non null string into a list of non null substrings using white space characters as separators. Empty
		/// substrings will be excluded from the
		/// list, and therefore, if this string is empty or contains only white space characters, the list will be empty.
		/// All strings in the resulting list are trimmed, not explicitly, but by definition because any surrounding whitespace
		/// would have counted as part of the delimiter.
		/// </summary>
		public static List<string> Separate( this string s ) {
			// Impossible to respond to R# warning because if you replace inline separators, the compiler can't figure out what method to call.
			string[] separators = null;
			return s.Split( separators, StringSplitOptions.RemoveEmptyEntries ).ToList();
		}

		/// <summary>
		/// Splits this non null string into a list of non null substrings using the specified separator. If substrings are trimmed
		/// and empties excluded, and this
		/// string is empty or contains only separators and white space characters, the list will be empty.
		/// </summary>
		public static List<string> Separate( this string s, string separator, bool trimSubStringsAndExcludeEmpties ) {
			var strings = s.Split( new[] { separator }, StringSplitOptions.None ).AsEnumerable();
			if( trimSubStringsAndExcludeEmpties )
				strings = strings.Select( str => str.Trim() ).Where( str => str.Length > 0 );
			return strings.ToList();
		}

		/// <summary>
		/// If s is not empty, appends the given delimiter to s.
		/// Otherwise, returns s.
		/// Do not pass null for s or delimiter.
		/// </summary>
		public static string AppendDelimiter( this string s, string delimiter ) => s.Length > 0 ? s + delimiter : s;

		/// <summary>
		/// If s is not empty, prepends the given delimiter to s.
		/// Otherwise, returns s.
		/// Do not pass null for s or delimiter.
		/// </summary>
		public static string PrependDelimiter( this string s, string delimiter ) => s.Length > 0 ? delimiter + s : s;

		/// <summary>
		/// If s is not empty, returns s surrounded by stringOnLeft and stringOnRight.
		/// Otherwise, returns s.
		/// Do not pass null for s.
		/// </summary>
		public static string Surround( this string s, string stringOnLeft, string stringOnRight ) => s.Length > 0 ? stringOnLeft + s + stringOnRight : s;

		/// <summary>
		/// Returns a string representing the list of items in the form "one, two, three and four".
		/// </summary>
		public static string GetEnglishListPhrase( IEnumerable<string> items, bool useSerialComma ) {
			items = items.Where( i => i.Any() ).ToArray();
			switch( items.Count() ) {
				case 0:
					return "";
				case 1:
					return items.First();
				case 2:
					return items.First() + " and " + items.ElementAt( 1 );
				default:
					return ConcatenateWithDelimiter( ", ", items.Take( items.Count() - 1 ).ToArray() ) + ( useSerialComma ? ", and " : " and " ) + items.Last();
			}
		}

		/// <summary>
		/// Returns true if this string matches the given pattern. The smaller string should be the pattern. Automatically ignores
		/// case.
		/// Passing the empty string for the pattern will always return true.
		/// Passing whitespace for the pattern when ignoreSurroundingWhitespace is true will always return true, regardless of the
		/// value of allowPartialMatches.
		/// Null string will be handled as empty string. The pattern should not be null.
		/// E.g. "Example".IsLike("ex") and "Example".IsLike("example") are true.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="pattern"></param>
		/// <param name="ignoreSurroundingWhitespace">If true, trims both the string and the pattern before comparing.</param>
		/// <param name="allowPartialMatches">
		/// If true, the pattern "ge" will match "General Mills". Otherwise, only "general mills"
		/// will match.
		/// </param>
		public static bool IsLike( this string s, string pattern, bool ignoreSurroundingWhitespace = true, bool allowPartialMatches = true ) {
			s = s ?? "";
			if( ignoreSurroundingWhitespace ) {
				s = s.Trim();
				pattern = pattern.Trim();
			}

			if( pattern.Length == 0 )
				return true;

			pattern = Regex.Escape( pattern );
			if( !allowPartialMatches )
				pattern = "^" + pattern + "$";
			return Regex.IsMatch( s, pattern, RegexOptions.IgnoreCase );
		}

		/// <summary>
		/// Returns true if strings starts with otherString, ignoring case.
		/// </summary>
		public static bool StartsWithIgnoreCase( this string s, string otherString ) => s.StartsWith( otherString, StringComparison.OrdinalIgnoreCase );

		/// <summary>
		/// Returns true if strings ends with otherString, ignoring case.
		/// </summary>
		public static bool EndsWithIgnoreCase( this string s, string otherString ) => s.EndsWith( otherString, StringComparison.OrdinalIgnoreCase );

		/// <summary>
		/// Returns true if the two strings are equal, ignoring case.
		/// </summary>
		public static bool EqualsIgnoreCase( this string s, string otherString ) => s.Equals( otherString, StringComparison.OrdinalIgnoreCase );

		/// <summary>
		/// Trims each string and compares them with EqualsIgnoreCase.
		/// </summary>
		public static bool EqualsIgnoreCaseAndWhitespace( this string s, string otherString ) => s.Trim().EqualsIgnoreCase( otherString.Trim() );

		/// <summary>
		/// Returns a new string in which the last instance of the specified string in the current instance is replaced by another
		/// specified string.
		/// </summary>
		public static string ReplaceLast( this string s, string oldValue, string newValue ) {
			var pos = s.LastIndexOf( oldValue );
			if( pos < 0 )
				return s;
			return s.Substring( 0, pos ) + newValue + s.Substring( pos + oldValue.Length );
		}

		/// <summary>
		/// Removes all leading occurrences of non uppercase characters from the current string object.
		/// </summary>
		public static string TrimLowerStart( this string s ) => s.Substring( s.indexOfFirstUpper() );

		private static int indexOfFirstUpper( this string s ) {
			for( var i = 0; i < s.Length; i += 1 ) {
				if( char.IsUpper( s, i ) )
					return i;
			}

			return s.Length;
		}

		/// <summary>
		/// Returns a single word comprised of the first character of each word in this non null string.
		/// </summary>
		public static string ToAcronym( this string s ) {
			return new string( s.Separate().Select( i => i.First() ).ToArray() );
		}

		/// <summary>
		/// An implementation of SoundEx roughly equivalent to SQL server's implementation. Returns the SoundEx code.
		/// </summary>
		public static string SoundEx( string word ) {
			// The length of the returned code.
			const int length = 4;

			// Value to return.
			var value = "";

			// The size of the word to process.
			var size = word.Length;

			// The word must be at least two characters in length.
			if( size > 1 ) {
				// Convert the word to uppercase characters.
				word = word.ToUpper( CultureInfo.InvariantCulture );

				// Convert the word to a character array.
				var chars = word.ToCharArray();

				// Buffer to hold the character codes.
				var buffer = new StringBuilder { Length = 0 };

				// The current and previous character codes.
				var prevCode = 0;
				var currCode = 0;

				// Add the first character to the buffer.
				buffer.Append( chars[ 0 ] );

				// Loop through all the characters and convert them to the proper character code.
				for( var i = 1; i < size; i++ ) {
					switch( chars[ i ] ) {
						case 'A':
						case 'E':
						case 'I':
						case 'O':
						case 'U':
						case 'H':
						case 'W':
						case 'Y':
							currCode = 0;
							break;
						case 'B':
						case 'F':
						case 'P':
						case 'V':
							currCode = 1;
							break;
						case 'C':
						case 'G':
						case 'J':
						case 'K':
						case 'Q':
						case 'S':
						case 'X':
						case 'Z':
							currCode = 2;
							break;
						case 'D':
						case 'T':
							currCode = 3;
							break;
						case 'L':
							currCode = 4;
							break;
						case 'M':
						case 'N':
							currCode = 5;
							break;
						case 'R':
							currCode = 6;
							break;
					}

					// Check if the current code is the same as the previous code.
					if( currCode != prevCode )
						// Check to see if the current code is 0 (a vowel); do not process vowels.
					{
						if( currCode != 0 )
							buffer.Append( currCode );
					}

					// Set the previous character code.
					prevCode = currCode;

					// If the buffer size meets the length limit, exit the loop.
					if( buffer.Length == length )
						break;
				}

				// Pad the buffer, if required.
				size = buffer.Length;
				if( size < length )
					buffer.Append( '0', length - size );

				// Set the value to return.
				value = buffer.ToString();
			}

			// Return the value.
			return value;
		}

		/// <summary>
		/// Converts this string to a given Enum value. Case sensitive.
		/// This method does not enforce valid Enum values.
		/// </summary>
		/// C# doesn't allow constraining the value to an Enum
		public static T ToEnum<T>( this string s ) => (T)Enum.Parse( typeof( T ), s );

		/// <summary>
		/// Returns a slug for this string, which will contain only lowercase alphanumeric characters and hyphens.
		/// </summary>
		// From https://stackoverflow.com/a/2921135/35349.
		public static string ToUrlSlug( this string s ) {
			var str = s.removeDiacritics().ToLower();

			// invalid chars
			str = Regex.Replace( str, @"[^a-z0-9\s-]", "" );

			// convert multiple spaces into one space
			str = Regex.Replace( str, @"\s+", " " ).Trim();

			// cut and trim
			str = str.Substring( 0, str.Length <= 45 ? str.Length : 45 ).Trim();

			// hyphens
			str = Regex.Replace( str, @"\s", "-" );

			return str;
		}

		// From https://stackoverflow.com/a/249126/35349.
		private static string removeDiacritics( this string s ) =>
			new string( s.Normalize( NormalizationForm.FormD ).Where( c => CharUnicodeInfo.GetUnicodeCategory( c ) != UnicodeCategory.NonSpacingMark ).ToArray() ).Normalize(
				NormalizationForm.FormC );
	}
}