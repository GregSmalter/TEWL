using System;
using System.Collections.Generic;
using Tewl.InputValidation;
using Tewl.IO;

namespace TewlTester {
	class Program {
		static void Main( string[] args ) {
			testCsv();
			testXls();
		}

		private static void testCsv() {
			var csvParser = TabularDataParser.CreateForCsvFile( @"C:\EveryoneFullControl\Knocking2020StateNotes.csv", true );
			var validationErrors = new List<ValidationError>();

			csvParser.ParseAndProcessAllLines( importThing, validationErrors );

			Console.WriteLine( $"CSV test: {csvParser.RowsWithoutValidationErrors} rows imported without error." );
		}

		private static void testXls() {
			var xlsParser = TabularDataParser.CreateForExcelFile( @"C:\EveryoneFullControl\Knocking2020StateNotes.xlsx" );
			var validationErrors = new List<ValidationError>();

			xlsParser.ParseAndProcessAllLines( importThing, validationErrors );

			Console.WriteLine( $"Excel test: {xlsParser.RowsWithoutValidationErrors} rows imported without error." );
		}

		private static void importThing( Validator validator, TabularDataParsedLine line ) {
			Console.WriteLine( line.LineNumber + ": " + line[0] );
		}
	}
}