﻿using JetBrains.Annotations;

namespace Tewl.Oracle;

/// <summary>
/// Static methods pertaining to Oracle Database.
/// </summary>
[ PublicAPI ]
public static class OracleTools {
	/// <summary>
	/// Converts a boolean into a decimal for storage in Oracle.
	/// </summary>
	public static decimal BooleanToDecimal( this bool b ) => b ? 1 : 0;

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