using System;
using System.Collections.Generic;
using System.IO;
using Tewl.InputValidation;
using Tewl.IO;

namespace TewlTester {
	class Program {
		static void Main( string[] args ) {
			// Unsure about the stream cleanup responsibility here. 
			var parser = new ExcelDataParser( File.OpenRead( @".\TestFiles\TewlTestBook.xlsx" ) );
			var validationErrors = new List<ValidationError>();

			parser.ProcessAllLines( importThing, validationErrors );

			Console.WriteLine( parser.RowsWithoutValidationErrors );
		}

		private static void importThing( Validator validator, ParsedLine line ) {
			Console.WriteLine( line.LineNumber );
			Console.WriteLine( line["Name"] );
		}
	}
}