using System;
using System.Collections.Generic;
using Tewl.InputValidation;
using Tewl.IO;

namespace TewlTester {
	class Program {
		static void Main( string[] args ) {
			var csvParser = TabularDataParser.CreateForCsvFile( @"", true );
			var validationErrors = new List<ValidationError>();

			csvParser.ParseAndProcessAllLines( importThing, validationErrors );

			Console.WriteLine( csvParser.RowsWithoutValidationErrors );
		}

		private static void importThing( Validator validator, ParsedLine line ) { }
	}
}