﻿using System.Linq;
using System.Net.Mime;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// String constants for the content types for common file types.
	/// </summary>
	[ PublicAPI ]
	public static class ContentTypes {
		/// <summary>
		/// Content type to be used for Android application packages.
		/// </summary>
		public const string ApplicationPackageFile = "application/vnd.android.package-archive";

		/// <summary>
		/// Content type to be used by .zip files.
		/// </summary>
		public const string ApplicationZip = MediaTypeNames.Application.Zip;

		/// <summary>
		/// Content type to be used by .htm and .html files.
		/// </summary>
		public const string Html = MediaTypeNames.Text.Html;

		/// <summary>
		/// Content type to be used for JavaScript files.
		/// </summary>
		public const string JavaScript = "application/javascript";

		/// <summary>
		/// Content type to be used for JSON files.
		/// </summary>
		public const string Json = "application/json";

		/// <summary>
		/// CSS.
		/// </summary>
		public const string Css = "text/css";

		/// <summary>
		/// CSV
		/// </summary>
		public const string Csv = "text/csv";

		/// <summary>
		/// Content type to be used by PDF files.
		/// </summary>
		public const string Pdf = MediaTypeNames.Application.Pdf;

		/// <summary>
		/// Content type to be used by plain text files (.txt, etc.).
		/// </summary>
		public const string PlainText = MediaTypeNames.Text.Plain;

		/// <summary>
		/// Content type to be used by XML files.
		/// </summary>
		public const string Xml = MediaTypeNames.Text.Xml;

		/// <summary>
		/// Content type to be used by Microsoft Word files.
		/// </summary>
		public const string WordDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

		/// <summary>
		/// Content type to be used by pre-docx Microsoft Word files.
		/// </summary>
		public const string WordDoc = "application/msword";

		/// <summary>
		/// Content type to be used by Microsoft Excel files.
		/// </summary>
		public const string ExcelXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

		/// <summary>
		/// Content type to be used by pre-xlsx Microsoft Excel files.
		/// </summary>
		public const string ExcelXls = "application/msexcel";

		/// <summary>
		/// Content type to be used by JPEG images.
		/// </summary>
		public const string Jpeg = MediaTypeNames.Image.Jpeg;

		/// <summary>
		/// Content type to be used by GIF images.
		/// </summary>
		public const string Gif = MediaTypeNames.Image.Gif;

		/// <summary>
		/// Content type to be used by PNG images.
		/// </summary>
		public const string Png = "image/png";

		/// <summary>
		/// Content type to be used by Scalable Vector Graphics images.
		/// </summary>
		public const string Svg = "image/svg+xml";

		/// <summary>
		/// Returns true if the given content type is any type of image.
		/// </summary>
		public static bool IsImageType( string contentType ) => contentType.StartsWith( "image/" );

		/// <summary>
		/// Returns the content type that matches the extension on the given fileName. String.Empty is returned on failure.
		/// </summary>
		public static string GetContentType( string fileName ) {
			var beginExtensionIndex = fileName.LastIndexOf( '.' );
			if( beginExtensionIndex == -1 )
				return string.Empty;

			var fileExtension = fileName.Substring( beginExtensionIndex );
			switch( fileExtension ) {
				case FileExtensions.Apk:
					return ApplicationPackageFile;
				case FileExtensions.Css:
					return Css;
				case FileExtensions.Csv:
					return Csv;
				case FileExtensions.ExcelXls:
					return ExcelXls;
				case FileExtensions.ExcelXlsx:
					return ExcelXlsx;
				case FileExtensions.Gif:
					return Gif;
				case FileExtensions.JavaScript:
					return JavaScript;
				case FileExtensions.Pdf:
					return Pdf;
				case FileExtensions.Png:
					return Png;
				case FileExtensions.Svg:
					return Svg;
				case FileExtensions.Txt:
					return PlainText;
				case FileExtensions.WordDocx:
					return WordDocx;
				case FileExtensions.WordDoc:
					return WordDoc;
				case FileExtensions.Xml:
					return Xml;
				case FileExtensions.Zip:
					return ApplicationZip;
				default:
					if( FileExtensions.Jpeg.Any( jpg => jpg == fileExtension ) )
						return Jpeg;
					return string.Empty;
			}
		}
	}
}