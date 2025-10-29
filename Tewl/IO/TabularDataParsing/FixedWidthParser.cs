using System.Text;

namespace Tewl.IO.TabularDataParsing;

internal class FixedWidthParser: TextBasedTabularDataParser {
	/// <summary>
	/// Creates a parser designed to parse a file with fixed data column widths. Specify the starting position of each column (using one-based column index).
	/// Characters that take up more than 1 unit of width, such as tabs, can cause problems here.
	/// </summary>
	public static TabularDataParser CreateWithFilePath( string filePath, int headerRowsToSkip, params int[] columnStartPositions ) =>
		new FixedWidthParser( columnStartPositions ) { headerRowsToSkip = headerRowsToSkip, fileReader = new FileReader( filePath ) };

	private readonly int charactersToSkip;
	private readonly int[] columnWidths; // Maps column indices to column widths

	public FixedWidthParser( int[] columnStartPositions ) {
		if( columnStartPositions.Length == 0 )
			throw new ArgumentException( "Must have at least one column. " );

		charactersToSkip = columnStartPositions[ 0 ] - 1;
		if( charactersToSkip < 0 )
			throw new ArgumentException( "The first column position must be positive.  Column positions are 1-based." );

		columnWidths = new int[ columnStartPositions.Length ];
		for( var i = 1; i < columnStartPositions.Length; i++ ) {
			var width = columnStartPositions[ i ] - columnStartPositions[ i - 1 ];
			if( width < 1 )
				throw new ArgumentException( "Column with zero or negative width detected.  Column positions must be in ascending order." );
			columnWidths[ i - 1 ] = width;
		}

		columnWidths[ columnWidths.Length - 1 ] = int.MaxValue;
		// We don't know how wide the last column is, but we don't need to since we will just read to the end of the line
	}

	protected override IReadOnlyList<string> parseLine( string? line ) {
		var fields = new List<string>();
		if( !line.IsNullOrWhiteSpace() )
			using( TextReader tr = new StringReader( line ) ) {
				for( var i = 0; i < charactersToSkip; i++ )
					tr.Read();

				for( var i = 0; i < columnWidths.Length; i++ )
					fields.Add( parseFixedWidthField( tr, columnWidths[ i ] ).Trim() );
			}
		return fields;
	}

	private string parseFixedWidthField( TextReader tr, int width ) {
		var sb = new StringBuilder();

		for( var i = 0; i < width && tr.Peek() != -1; i++ )
			sb.Append( (char)tr.Read() );

		return sb.ToString();
	}
}