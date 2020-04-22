using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Tewl.Tools {
	/// <summary>
	/// XML Tools.
	/// </summary>
	public static class XmlTools {
		/// <summary>
		/// Serializes the given type to XML.
		/// </summary>
		public static string SerializeToXml<T>( this T o ) {
			using( var s = new StringWriter() ) {
				new XmlSerializer( typeof( T ) ).Serialize( s, o );
				return s.ToString();
			}
		}

		/// <summary>
		/// Deserializes the given string to the given type.
		/// </summary>
		public static T Deserialize<T>( string s ) => (T)Deserialize( typeof( T ), s );

		public static object Deserialize( Type t, string s ) => new XmlSerializer( t ).Deserialize( new StringReader( s ) );

		/// <summary>
		/// Converts a string XML payload from UTF16 to UTF8.
		/// </summary>
		public static MemoryStream ConvertXmlStringToUtf8( string utf16Xml ) => new MemoryStream( Encoding.UTF8.GetBytes( utf16Xml.Replace( "utf-16", "utf-8" ) ) );
	}
}