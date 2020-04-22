using System;

namespace Tewl {
	public static class FuzzyStringMatching {
		/// <summary>
		/// Calculates the Levenshtein Distance between two strings.
		/// It is minimum of single character insert/delete/update operations needed to transfrom first string into the second
		/// string.
		/// </summary>
		/// <param name="firstString">First string to calculate the distance</param>
		/// <param name="secondString">Second string to calculate the distance</param>
		/// <param name="ignoreCase">Specifies whether to ignore case in comparison</param>
		/// <returns>int represending the Levenshtein Distance</returns>
		public static int LevenshteinDistance( string firstString, string secondString, bool ignoreCase ) {
			var strF = ignoreCase ? firstString.ToLower() : firstString;
			var strS = ignoreCase ? secondString.ToLower() : secondString;
			var lenF = strF.Length;
			var lenS = strS.Length;
			var d = new int[ lenF + 1, lenS + 1 ];

			for( var i = 0; i <= lenF; i++ )
				d[ i, 0 ] = i;
			for( var j = 0; j <= lenS; j++ )
				d[ 0, j ] = j;

			for( var j = 1; j <= lenS; j++ ) {
				for( var i = 1; i <= lenF; i++ ) {
					if( strF[ i - 1 ] == strS[ j - 1 ] )
						d[ i, j ] = d[ i - 1, j - 1 ];
					else {
						d[ i, j ] = Math.Min(
							Math.Min(
								d[ i - 1, j ] + 1,
								// a deletion
								d[ i, j - 1 ] + 1 ),
							//an Insertion
							d[ i - 1, j - 1 ] + 1 ); // a substitution
					}
				}
			}

			return d[ lenF, lenS ];
		}

		/// <summary>
		/// Returns a score between 0.0-1.0 indicating how closely two strings match.  1.0 is a 100%
		/// T-SQL equality match, and the score goes down from there towards 0.0 for less similar strings.
		/// </summary>
		public static double? GetSimilarityScore( string string1, string string2 ) {
			// GMS NOTE: Isnullorwhitespace?
			if( string1 == null || string2 == null )
				return null;

			var s1 = string1.ToUpper().TrimEnd( ' ' );
			var s2 = string2.ToUpper().TrimEnd( ' ' );
			if( s1 == s2 )
				return 1.0F; // At this point, T-SQL would consider them the same, so I will too

			var flatLevScore = internalGetSimilarityScore( s1, s2 );

			var letterS1 = getLetterSimilarityString( s1 );
			var letterS2 = getLetterSimilarityString( s2 );
			var letterScore = internalGetSimilarityScore( letterS1, letterS2 );

			if( flatLevScore == 1.0F && letterScore == 1.0F )
				return 1.0F;

			if( flatLevScore == 0.0F && letterScore == 0.0F )
				return 0.0F;

			// Return weighted result
			return ( flatLevScore * 0.2F ) + ( letterScore * 0.8F );
		}

		private static double internalGetSimilarityScore( string s1, string s2 ) {
			var dist = LevenshteinDistance( s1, s2, true );
			var maxLen = s1.Length > s2.Length ? s1.Length : s2.Length;
			if( maxLen == 0 )
				return 1.0F;
			return 1.0F - Convert.ToDouble( dist ) / Convert.ToDouble( maxLen );
		}

		private static string getLetterSimilarityString( string s1 ) {
			if( s1 == null )
				return "";
			var newStr = new char[ s1.Length ];
			var newStrIndex = 0;
			for( var i = 0; i < s1.Length; i++ ) {
				var c = s1[ i ];
				if( !char.IsLetterOrDigit( c ) )
					continue;
				newStr[ newStrIndex++ ] = char.ToUpper( c );
			}

			Array.Sort( newStr, 0, newStrIndex );
			return new string( newStr, 0, newStrIndex );
		}
	}
}