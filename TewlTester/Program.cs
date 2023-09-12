using System;
using System.Collections.Generic;
using System.IO;
using Tewl.InputValidation;
using Tewl.IO;
using Tewl.IO.TabularDataParsing;

namespace TewlTester {
	class Program {
		static void Main( string[] args ) {
			testExcelWriting();
			testCsvWriting();
			testCsv();
			testTabDelimitedWriting();
			testXls();
		}

		private static void testExcelWriting() {
			var excelFile = new ExcelFileWriter();
			excelFile.DefaultWorksheet.AddHeaderToWorksheet( "ID", "Name", "Date", "Email", "Website" );
			excelFile.DefaultWorksheet.AddRowToWorksheet( "123", "Greg", "1/1/2012", "greg.smalter@gmail.com", "https://www.google.com" );
			excelFile.DefaultWorksheet.AddRowToWorksheet( "321", "", "12/19/2020", "", "https://microsoft.com" );
			
			using( var stream = File.OpenWrite( "tewlTestTabularWrite.xlsx" ) )
				excelFile.SaveToStream( stream );
		}

		private static void testCsvWriting() {
			var csvFile = new CsvFileWriter();

			using( var stream = new StreamWriter( File.OpenWrite( "tewlTestTabularWrite.csv" ) ) )
				writeData( csvFile, stream );
		}

		private static void testTabDelimitedWriting() {
			var csvFile = new TabDelimitedFileWriter();

			using( var stream = new StreamWriter( File.OpenWrite( "tewlTestTabularWrite.txt" ) ) )
				writeData( csvFile, stream );
		}

		// It's sort of a failure that the Excel writer cannot be passed here. But between there being more than one worksheet and other problems, it's hard to
		// have it implement the same interface.
		private static void writeData( TextBasedTabularDataFileWriter writer, TextWriter stream ) {
			writer.AddValuesToLine( "ID", "Name", "Date", "Email", "Website" );
			writer.WriteCurrentLineToFile( stream );
			writer.AddValuesToLine("123", "Greg", "1/1/2012", "greg.smalter@gmail.com", "https://www.google.com" );
			writer.WriteCurrentLineToFile( stream );
			writer.AddValuesToLine("123", "Greg", "1/1/2012", "greg.smalter@gmail.com", "https://www.google.com" );
			writer.WriteCurrentLineToFile( stream );
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
			var email = line[ "email" ];
			var website = line[ "website" ];
			Console.WriteLine( line.LineNumber + ": Date: " + value + $", Email: {email}, Website: {website}" );
		}
	}
}