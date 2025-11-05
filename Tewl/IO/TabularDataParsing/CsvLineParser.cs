using System.Text;

namespace Tewl.IO.TabularDataParsing;

/// <summary>
/// Parses a line of a Microsoft Excel CSV file using the definition of CSV at
/// http://en.wikipedia.org/wiki/Comma-separated_values.
/// </summary>
[ PublicAPI ]
internal class CsvLineParser: TextBasedTabularDataParser {
	/// <summary>
	/// Creates a parser designed to parse a CSV file.  Passing true for hasHeaderRow will result in the first row being used to map
	/// header names to column indices.  This will allow you to access fields using the header name in addition to the column index.
	/// </summary>
	public CsvLineParser( string filePath ) {
		fileReader = new FileReader( filePath );
	}

	/// <summary>
	/// Creates a parser designed to parse a CSV file.  Passing true for hasHeaderRow will result in the first row being used to map
	/// header names to column indices.  This will allow you to access fields using the header name in addition to the column index.
	/// </summary>
	public CsvLineParser( Stream stream ) {
		fileReader = new FileReader( stream );
	}

	/// <summary>
	/// Parses a line of a Microsoft Excel CSV file and returns a collection of string fields.
	/// </summary>
	protected override IReadOnlyList<string> parseLine( string? line ) {
		var fields = new List<string>();
		if( !line.IsNullOrWhiteSpace() )
			using( TextReader tr = new StringReader( line ) )
				parseCommaSeparatedFields( tr, fields );
		return fields;
	}

	private void parseCommaSeparatedFields( TextReader tr, List<string> fields ) {
		parseCommaSeparatedField( tr, fields );
		while( tr.Peek() == ',' ) {
			tr.Read();
			parseCommaSeparatedFields( tr, fields );
		}
	}

	private void parseCommaSeparatedField( TextReader tr, List<string> fields ) {
		if( tr.Peek() != -1 ) {
			string field;
			if( tr.Peek() != '"' )
				field = parseSimpleField( tr );
			else
				field = parseQuotedField( tr );
			fields.Add( field.Trim() );
		}
	}

	private string parseSimpleField( TextReader tr ) {
		var sb = new StringBuilder();

		var ch = tr.Peek();
		while( ch != -1 && ch != ',' ) {
			sb.Append( (char)tr.Read() );
			ch = tr.Peek();
		}

		return sb.ToString();
	}

	private string parseQuotedField( TextReader tr ) {
		var sb = new StringBuilder();

		// Skip the opening quote
		tr.Read();

		var ch = tr.Read();
		// Continue until the end of the file or until we reach an unescaped quote.
		while( ch != -1 && !( ch == '"' && tr.Peek() != '"' ) ) {
			// If we encounter an escaped double quote, skip one of the double quotes.
			if( ch == '"' && tr.Peek() == '"' )
				tr.Read();

			sb.Append( (char)ch );
			ch = tr.Read();
		}

		return sb.ToString();
	}
}