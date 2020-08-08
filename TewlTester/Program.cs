using System;
using System.Collections.Generic;
using Tewl.InputValidation;
using Tewl.IO;
using Tewl.IO.TabularDataParsing;

namespace TewlTester {
	class Program {
		static void Main( string[] args ) {
			testCsv();
			testXls();
		}

		private static void testCsv() {
			var csvParser = TabularDataParser.CreateForCsvFile( @"..\..\..\TestFiles\TewlTestBook.csv", true );
			var validationErrors = new List<ValidationError>();

			csvParser.ParseAndProcessAllLines( importThing, validationErrors );

			Console.WriteLine( $"CSV test: {csvParser.RowsWithoutValidationErrors} rows imported without error." );
		}

		private static void testXls() {
			var xlsParser = TabularDataParser.CreateForExcelFile( @"..\..\..\TestFiles\TewlTestBook.xlsx" );
			var validationErrors = new List<ValidationError>();

			xlsParser.ParseAndProcessAllLines( importThing, validationErrors );

			Console.WriteLine( $"Excel test: {xlsParser.RowsWithoutValidationErrors} rows imported without error." );
		}

		private static void importThing( Validator validator, ParsedLine line ) {
			var value = line["dATe"];
			Console.WriteLine( line.LineNumber + ": " + value );
		}
	}
}