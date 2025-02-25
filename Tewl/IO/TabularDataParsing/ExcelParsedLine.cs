using ClosedXML.Excel;

namespace Tewl.IO.TabularDataParsing;

internal class ExcelParsedLine: ParsedLine {
	private readonly List<string> headerFields;
	private readonly IXLRangeRow row;

	public ExcelParsedLine( List<string> headerFields, IXLRangeRow row ) {
		this.headerFields = headerFields;
		this.row = row;
	}

	bool ParsedLine.ContainsData => !row.IsEmpty();

	int ParsedLine.LineNumber => row.RowNumber();

	string ParsedLine.this[ int index ] =>
		// Index is zero-based. The Cell() function on the IXLRangeRow is 1-based. 
		row.Cell( index + 1 ).Value.ToString();


	string ParsedLine.this[ string columnName ] =>
		// Index is zero-based. The Cell() function on the IXLRangeRow is 1-based. 
		row.Cell( headerFields.IndexOf( columnName.ToLower() ) + 1 ).Value.ToString();

	bool ParsedLine.ContainsField( string fieldName ) => headerFields.Contains( fieldName );
}