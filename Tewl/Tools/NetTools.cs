using System;
using System.Linq;
using System.Net;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Tools supporting DNS, URIs, encoding, and so forth.
	/// </summary>
	[ PublicAPI ]
	public static class NetTools {
		/// <summary>
		/// Returns the host name of the local computer.
		/// </summary>
		public static string GetLocalHostName() => Dns.GetHostEntry( "" ).HostName;


		/// <summary>
		/// Combines the given URLs into a single URL with no trailing slash.
		/// </summary>
		/// <param name="one">A URL, not null.</param>
		/// <param name="two">A URL, not null.</param>
		/// <param name="urls">Array or list of URLs to combine in addition to one and two.</param>
		/// <returns>Combined URL string.</returns>
		public static string CombineUrls( string one, string two, params string[] urls ) {
			if( one == null || two == null )
				throw new ArgumentException( "String cannot be null." );

			var combinedUrl = one.Trim( '/' ) + "/" + two.Trim( '/' ) + "/"; // NOTE: Make this more like CombinePaths next time this is changed

			foreach( var url in urls )
				combinedUrl += url.Trim( '/' ) + "/";

			return combinedUrl.TrimEnd( '/' );
		}

		/// <summary>
		/// Given a type, generates a Url based on the namespace and type.
		/// Every level of the namespace up to and including the first one that
		/// ends in with the string "site" (case insensitive) is dropped.
		/// The type "MyControl" in namespace "one.two.three.MyWebSite.Stuff.MyControls"
		/// will return "Stuff/MyControls/MyControl.ascx".
		/// </summary>
		public static string GetRelativeUrlToAscxFromType( Type type ) {
			if( type.FullName.ToLower().IndexOf( "site" ) == -1 )
				throw new ArgumentException( "Type's namespace does not contain the string \"site\" anywhere in its name." );
			var url = type.FullName.Replace( '.', '/' ) + ".ascx";
			while( true ) {
				var parts = url.DissectByChar( '/' );
				var firstLevel = parts[ 0 ];
				url = parts[ 1 ];
				if( firstLevel.ToLower().EndsWith( "site" ) )
					return url;
			}
		}

		/// <summary>
		/// Returns an anchor tag with the specified parameters. Use this only for status messages that are being built after LoadData. In all other cases, use
		/// EwfLink.
		/// </summary>
		public static string BuildBasicLink( string text, string url, bool navigatesInNewWindow ) {
			return "<a href=\"" + url + "\"" + ( navigatesInNewWindow ? " target=\"_blank\"" : "" ) + ">" + text + "</a>";
		}

		/// <summary>
		/// Gets whether a request for the specified URL returns an HTTP status code other than 200 OK.
		/// </summary>
		public static bool LinkIsBroken( string url ) {
			bool? broken = null;
			ExecuteHttpHeadRequest( url, response => broken = response == null || response.StatusCode != HttpStatusCode.OK );
			return broken.Value;
		}

		/// <summary>
		/// Performs an HTTP HEAD request for the specified URL and executes the specified method with the response. The response will be null if the server is
		/// unavailable.
		/// </summary>
		public static void ExecuteHttpHeadRequest( string url, Action<HttpWebResponse> responseHandler, bool disableCertificateValidation = false ) {
			var request = WebRequest.CreateHttp( url );

			request.Method = "HEAD";
			if( disableCertificateValidation )
				request.ServerCertificateValidationCallback += ( sender, certificate, chain, errors ) => true;
			using( var response = request.getResponseIfPossible() )
				responseHandler( response );
		}

		private static HttpWebResponse getResponseIfPossible( this HttpWebRequest request ) {
			try {
				return getResponseForAnyStatusCode( request );
			}
			catch( WebException e ) {
				if( new[] { WebExceptionStatus.ConnectFailure, WebExceptionStatus.Timeout }.Contains( e.Status ) )
					return null;
				throw;
			}
		}

		/// <summary>
		/// Gets a response and prevents exceptions caused by HTTP status codes. From http://stackoverflow.com/a/1366869/35349.
		/// </summary>
		private static HttpWebResponse getResponseForAnyStatusCode( this HttpWebRequest request ) {
			try {
				return (HttpWebResponse)request.GetResponse();
			}
			catch( WebException e ) {
				if( e.Response == null || e.Status != WebExceptionStatus.ProtocolError )
					throw;
				return (HttpWebResponse)e.Response;
			}
		}
	}
}