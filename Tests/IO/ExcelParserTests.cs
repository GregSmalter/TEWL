using ClosedXML.Excel;
using Tewl.IO;
using Tewl.IO.TabularDataParsing;

namespace Tests.IO;

class ExcelParserTests {
	[ Test ]
	public void BlankHeaderColumns() {
		IoMethods.ExecuteWithTempFolder( folderPath => {
			var filePath = Path.Combine( folderPath, "test.xlsx" );
			using( var workbook = new XLWorkbook() ) {
				var worksheet = workbook.AddWorksheet();
				worksheet.Cell( "A1" ).Value = "Stabbr";
				worksheet.Cell( "B1" ).Value = "SystemAgency";
				worksheet.Cell( "C1" ).Value = "StrategyType";
				worksheet.Cell( "D1" ).Value = "CompletionStrategy";
				worksheet.Cell( "E1" ).Value = "Example";
				worksheet.Cell( "F1" ).Value = "Included in Briefs";
				worksheet.Cell( "I1" ).Value = "Completion Strategy - old";
				worksheet.Cell( "J1" ).Value = "Completion Strategy Example";
				worksheet.Cell( "K1" ).Value = "Additional Information";
				worksheet.Cell( "Q1" ).Value = "NOTES:";
				worksheet.Cell( "A2" ).Value = "UT";
				worksheet.Cell( "B2" ).Value = "System";
				worksheet.Cell( "C2" ).Value = "Policy";
				worksheet.Cell( "D2" ).Value = "Targeted completion initiative";
				worksheet.Cell( "E2" ).Value = "Example text";
				workbook.SaveAs( filePath );
			}

			var parser = TabularDataParser.CreateForExcelFile( filePath, [ ] );
			var validationErrors = new List<DataValidationError>();
			var completionStrategies = new List<string>();
			parser.ParseAndProcessAllLines( ( line, _ ) => completionStrategies.Add( line[ "CompletionStrategy" ] ), validationErrors );

			Assert.That( validationErrors, Is.Empty );
			Assert.That( completionStrategies.Single(), Is.EqualTo( "Targeted completion initiative" ) );
		} );
	}

	[ Test ]
	public void DuplicateNamedHeaders() {
		IoMethods.ExecuteWithTempFolder( folderPath => {
			var filePath = Path.Combine( folderPath, "test.xlsx" );
			using( var workbook = new XLWorkbook() ) {
				var worksheet = workbook.AddWorksheet();
				worksheet.Cell( "A1" ).Value = "Stabbr";
				worksheet.Cell( "B1" ).Value = "Stabbr";
				worksheet.Cell( "A2" ).Value = "UT";
				worksheet.Cell( "B2" ).Value = "WA";
				workbook.SaveAs( filePath );
			}

			var parser = TabularDataParser.CreateForExcelFile( filePath, [ ] );
			var validationErrors = new List<DataValidationError>();
			var lineCount = 0;
			parser.ParseAndProcessAllLines(
				( line, _ ) => {
					lineCount++;
					Assert.That( () => line[ "Stabbr" ], Throws.ArgumentException );
				},
				validationErrors );

			Assert.That( validationErrors, Is.Empty );
			Assert.That( lineCount, Is.EqualTo( 1 ) );
		} );
	}
}