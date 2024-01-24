using ClosedXML.Excel;
using JetBrains.Annotations;
using Tewl.InputValidation;

namespace Tewl.IO;

/// <summary>
/// Represents a worksheet inside an Excel workbook (file).
/// </summary>
[ PublicAPI ]
public class ExcelWorksheet {
	private readonly IXLWorksheet worksheet;
	private int rowIndex = 1;

	/// <summary>
	/// The name of the worksheet that appears in the tab at the bottom of the workbook.
	/// </summary>
	public string Name { get => worksheet.Name; set => worksheet.Name = value; }

	/// <summary>
	/// Creates a new worksheet.
	/// </summary>
	internal ExcelWorksheet( IXLWorksheet worksheet ) => this.worksheet = worksheet;

	/// <summary>
	/// Freezes the first row of this worksheet so when you scroll vertically, it does not scroll.
	/// </summary>
	public void FreezeHeaderRow() => worksheet.SheetView.FreezeRows( 1 );

	/// <summary>
	/// Assigns the given cell to the given value. Cell names correspond to their name in Excel, e.g. D10.
	/// Using the most specific overload for the datatype passed will be recognized by Excel.
	/// </summary>
	public void PutCellValue( string cellName, string cellValue ) => worksheet.Cell( cellName ).Value = cellValue;

	/// <summary>
	/// Assigns the given cell to the given value. Cell names correspond to their name in Excel, e.g. D10
	/// Using the most specific overload for the datatype passed will be recognized by Excel.
	/// </summary>
	public void PutCellValue( string cellName, double cellValue ) => worksheet.Cell( cellName ).Value = cellValue;

	/// <summary>
	/// Assigns the given cell to the given value. Cell names correspond to their name in Excel, e.g. D10
	/// Using the most specific overload for the datatype passed will be recognized by Excel.
	/// </summary>
	public void PutCellValue( string cellName, DateTime cellValue ) => worksheet.Cell( cellName ).Value = cellValue;

	/// <summary>
	/// Assigns a formula to a cell. Cell names correspond to their name in Excel, e.g. D10.
	/// The formula should include the equals sign prefix.
	/// </summary>
	public void PutFormula( string cellName, string formula ) {
		var cell = worksheet.Cell( cellName );
		cell.FormulaA1 = formula;
		setOrAddCellStyle( cell, bold: true );
	}

	/// <summary>
	/// Adds a header (bold appearance) row with the given column values to the worksheet.
	/// </summary>
	public void AddHeaderToWorksheet( params string[] headerValues ) => addRowToWorksheet( true, headerValues );

	/// <summary>
	/// Adds a data row with the given column values to the worksheet.
	/// If a date is detected, it will be inserted in mm/dd/yyyy format.
	/// </summary>
	public void AddRowToWorksheet( params string[] cellValues ) => addRowToWorksheet( false, cellValues );

	private void addRowToWorksheet( bool bold, params string[] cellValues ) {
		var columnIndex = 1;
		foreach( var cellValue in cellValues ) {
			var cell = worksheet.Cell( rowIndex, columnIndex++ );
			setOrAddCellStyle( cell, bold: bold, textWrapped: true );

			try {
				putRowValueInCell( cell, cellValue );
			}
			catch( Exception e ) {
				throw new Exception( "Failed to put the value \"{0}\" into a cell.".FormatWith( cellValue ), e );
			}
		}

		++rowIndex;
	}

	private void putRowValueInCell( IXLCell cell, string value ) {
		var v = new Validator();
		var detectedDate = v.GetNullableDateTime(
			new ValidationErrorHandler( "" ),
			value,
			DateTimeTools.DayMonthYearFormats.Concat( DateTimeTools.MonthDayYearFormats ).ToArray(),
			false,
			DateTime.MinValue,
			DateTime.MaxValue );
		if( !v.ErrorsOccurred ) {
			setOrAddCellStyle( cell, date: true );
			cell.Value = detectedDate;
			return;
		}

		v = new Validator();
		v.GetEmailAddress( new ValidationErrorHandler( "" ), value, false );
		if( !v.ErrorsOccurred ) {
			cell.Value = value;
			cell.SetHyperlink( new XLHyperlink( "mailto:" + value ) );
			return;
		}

		v = new Validator();
		var validatedUrl = v.GetUrl( new ValidationErrorHandler( "" ), value, false );
		if( !v.ErrorsOccurred ) {
			cell.Value = value;
			cell.SetHyperlink( new XLHyperlink( validatedUrl ) );
			return;
		}

		cell.Value = value;
	}

	/// <summary>
	/// Adds simplicity to adding cell styles
	/// </summary>
	private void setOrAddCellStyle( IXLCell cell, bool bold = false, bool textWrapped = false, bool date = false ) {
		if( bold )
			cell.Style.Font.Bold = true;
		if( textWrapped )
			cell.Style.Alignment.WrapText = true;
		if( date )
			cell.Style.DateFormat.SetFormat( "mm/dd/yyyy" );
	}
}