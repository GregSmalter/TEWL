namespace Tewl.IO {
	/// <summary>
	/// Represents a line/row that been parsed into fields that
	/// are accessible through the indexers of this object.
	/// </summary>
	public interface TabularDataParsedLine {
		/// <summary>
		/// Returns true if any field on this line has a non-empty, non-whitespace value.
		/// </summary>
		bool ContainsData { get; }

		/// <summary>
		/// Returns the line number from the source document that this parsed line was created from.
		/// </summary>
		int LineNumber { get; }

		/// <summary>
		/// Returns the value of the field with the given zero-based column index.
		/// </summary>
		string this[ int index ] {get; }

		/// <summary>
		/// Returns the value of the field with the given column name.
		/// </summary>
		string this[ string columnName ] {get; }

		/// <summary>
		/// Returns true if the line contains the given field.
		/// </summary>
		bool ContainsField( string fieldName );
	}
}