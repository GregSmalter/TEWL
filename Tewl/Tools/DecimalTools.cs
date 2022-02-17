﻿using System;
using System.Numerics;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Extension methods for decimals.
	/// </summary>
	[ PublicAPI ]
	public static class DecimalTools {
		// Avoiding implicit conversions just for clarity.
		private static readonly BigInteger ten = new BigInteger( 10 );
		private static readonly BigInteger uInt32Mask = new BigInteger( 0xffffffffU );

		/// <summary>
		/// Gets a decimal that is equal to this decimal and does not include trailing zeros in its internal representation.
		/// From http://stackoverflow.com/questions/4298719.
		/// </summary>
		public static decimal Normalize( this decimal input ) {
			unchecked {
				var bits = decimal.GetBits( input );
				var mantissa = new BigInteger( (uint)bits[ 0 ] ) + ( new BigInteger( (uint)bits[ 1 ] ) << 32 ) + ( new BigInteger( (uint)bits[ 2 ] ) << 64 );

				var sign = bits[ 3 ] & int.MinValue;
				var exponent = ( bits[ 3 ] & 0xff0000 ) >> 16;

				// The loop condition here is ugly, because we want to do both the DivRem part and the exponent check :(
				while( exponent > 0 ) {
					var divided = BigInteger.DivRem( mantissa, ten, out var remainder );
					if( remainder != BigInteger.Zero )
						break;
					exponent--;
					mantissa = divided;
				}

				// Okay, now put it all back together again...
				bits[ 3 ] = ( exponent << 16 ) | sign;

				// For each 32 bits, convert the bottom 32 bits into a uint (which won't
				// overflow) and then cast to int (which will respect the bits, which
				// is what we want)
				bits[ 0 ] = (int)(uint)( mantissa & uInt32Mask );
				mantissa >>= 32;
				bits[ 1 ] = (int)(uint)( mantissa & uInt32Mask );
				mantissa >>= 32;
				bits[ 2 ] = (int)(uint)( mantissa & uInt32Mask );

				return new decimal( bits );
			}
		}

		/// <summary>
		/// Rounds this value to the nearest hundred.
		/// </summary>
		public static int RoundToHundred( this decimal value, MidpointRounding m ) => (int)Math.Round( value / 100m, m ) * 100;

		/// <summary>
		/// Returns the dollar amount to two decimal places prefixed with $. e.g. $8.99
		/// </summary>
		public static string ToMoneyString( this decimal d ) => d.ToString( "c2" );

		/// <summary>
		/// Returns true if this dollar value includes fractional cents.
		/// </summary>
		public static bool DollarValueHasFractionalCents( this decimal dollarValue ) {
			var centValue = dollarValue * 100;
			return centValue != (int)centValue;
		}

		/// <summary>
		/// Converts a decimal (presumably from Oracle) into a boolean.
		/// </summary>
		public static bool DecimalToBoolean( this decimal d ) {
			if( d == 1 )
				return true;
			if( d == 0 )
				return false;
			throw new ApplicationException( "Unknown decimal value encountered when converting to boolean." );
		}
	}
}