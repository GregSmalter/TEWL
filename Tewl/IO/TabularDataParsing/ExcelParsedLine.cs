using ClosedXML.Excel;

namespace Tewl.IO.TabularDataParsing;

internal class ExcelParsedLine: ParsedLine {
	private readonly IReadOnlyDictionary<string, int> columnIndicesByName;
	private readonly IXLRangeRow row;

	public ExcelParsedLine( IReadOnlyDictionary<string, int> columnIndicesByName, IXLRangeRow row ) {
		this.columnIndicesByName = columnIndicesByName;
		this.row = row;
	}

	bool ParsedLine.ContainsData => !row.IsEmpty();

	int ParsedLine.LineNumber => row.RowNumber();

	public string this[ int index ] =>
		// Index is zero-based. The Cell() function on the IXLRangeRow is 1-based.
		row.Cell( index + 1 ).Value.ToString();


	string ParsedLine.this[ string columnName ] =>
		columnIndicesByName.TryGetValue( columnName, out var index )
			? this[ index ]
			: throw new ArgumentException(
				  $"Column “{columnName}” does not exist. The columns are {StringTools.GetEnglishListPhrase( columnIndicesByName.Keys.Select( i => $"“{i}”" ), true )}." );

	bool ParsedLine.ContainsField( string fieldName ) => columnIndicesByName.ContainsKey( fieldName );
}