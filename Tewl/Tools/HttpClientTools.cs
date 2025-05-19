using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using StackExchange.Profiling;
using Tewl.IO;

namespace Tewl.Tools;

/// <summary>
/// Static methods pertaining to HttpClient.
/// </summary>
[ PublicAPI ]
public static class HttpClientTools {
	private class WriterContent( Action<Stream> bodyWriter ): HttpContent {
		protected override Task SerializeToStreamAsync( Stream stream, TransportContext? context ) {
			bodyWriter( stream );
			return Task.CompletedTask;
		}

		protected override Task SerializeToStreamAsync( Stream stream, TransportContext? context, CancellationToken cancellationToken ) =>
			SerializeToStreamAsync( stream, context );

		protected override void SerializeToStream( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
			bodyWriter( stream );
		}

		protected override bool TryComputeLength( out long length ) {
			length = 0;
			return false;
		}
	}

	/// <summary>
	/// Makes a GET request for a text-based resource and returns its representation, retrying several times with exponential back-off in the event of network
	/// problems or transient failures on the server. Use only from a background process that can tolerate a long delay.
	/// </summary>
	public static string? GetTextWithRetry( this HttpClient client, string url, bool returnNullIfNotFound = false, string additionalHandledMessage = "" ) {
		try {
			return ExecuteRequestWithRetry(
				true,
				async () => {
					using var response = await client.GetAsync( url, HttpCompletionOption.ResponseHeadersRead );
					if( returnNullIfNotFound && response.StatusCode == HttpStatusCode.NotFound )
						return null;
					response.EnsureSuccessStatusCode();
					return await response.Content.ReadAsStringAsync();
				},
				additionalHandledMessage: additionalHandledMessage );
		}
		catch( Exception e ) {
			throw new Exception( $"A GET request for {url} failed.", e );
		}
	}

	/// <summary>
	/// Makes a GET request for a resource and writes its representation to a file at the specified path, retrying several times with exponential back-off in the
	/// event of network problems or transient failures on the server. Use only from a background process that can tolerate a long delay. Overwrites the
	/// destination file if it already exists.
	/// </summary>
	public static void DownloadFileWithRetry( this HttpClient client, string url, string destinationPath, string additionalHandledMessage = "" ) =>
		ExecuteRequestWithRetry(
			true,
			async () => {
				using var response = await client.GetAsync( url, HttpCompletionOption.ResponseHeadersRead );
				response.EnsureSuccessStatusCode();

				IoMethods.DeleteFile( destinationPath );
				await using var fileStream = IoMethods.GetFileStreamForWrite( destinationPath );
				await response.Content.CopyToAsync( fileStream );
			},
			additionalHandledMessage: additionalHandledMessage );

	/// <summary>
	/// Executes a method that makes a request using <see cref="HttpClient"/>, retrying several times with exponential back-off in the event of network problems
	/// or transient failures on the server. Use only from a background process that can tolerate a long delay.
	/// </summary>
	public static void ExecuteRequestWithRetry(
		bool requestIsIdempotent, Func<Task> method, string additionalHandledMessage = "", Action? persistentFailureHandler = null ) {
		bool isHandled( Exception e ) {
			if( e is HttpRequestException { InnerException: SocketException { SocketErrorCode: SocketError.HostNotFound or SocketError.NoData } } )
				return true;
			if( !requestIsIdempotent )
				return false;

			if( e is TaskCanceledException ) // timeout
				return true;
			if( e is HttpRequestException { InnerException: SocketException { SocketErrorCode: SocketError.ConnectionRefused } } )
				return true;
			if( e is HttpRequestException { StatusCode: HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway } )
				return true;

			return additionalHandledMessage.Length > 0 && e is HttpRequestException && e.Message.Contains( additionalHandledMessage );
		}

		try {
			using( MiniProfiler.Current.Step( "TEWL - Execute HTTP request with retry" ) )
				new ResiliencePipelineBuilder().AddRetry( getRetryOptions( e => isHandled( e.InnerException! ), 7 ) )
					.Build()
					.Execute(
						() => new ResiliencePipelineBuilder()
							.AddRetry( getRetryOptions( e => e.InnerException is HttpRequestException { StatusCode: HttpStatusCode.ServiceUnavailable }, 11 ) )
							.Build()
							.Execute( () => Task.Run( method ).Wait() ) );
		}
		catch( Exception e ) {
			if( persistentFailureHandler is null || !isHandled( e.InnerException! ) )
				throw;
			persistentFailureHandler();
		}
	}

	private static RetryStrategyOptions getRetryOptions( Func<Exception, bool> predicate, int attemptCount ) =>
		new()
			{
				ShouldHandle = predicateArguments => ValueTask.FromResult( predicateArguments.Outcome.Exception is {} exception && predicate( exception ) ),
				BackoffType = DelayBackoffType.Exponential,
				Delay = TimeSpan.FromSeconds( 2 ),
				MaxRetryAttempts = attemptCount
			};

	/// <summary>
	/// Executes a method that makes a request using <see cref="HttpClient"/>, retrying several times with exponential back-off in the event of network problems
	/// or transient failures on the server. Use only from a background process that can tolerate a long delay.
	/// </summary>
	public static T ExecuteRequestWithRetry<T>(
		bool requestIsIdempotent, Func<Task<T>> method, string additionalHandledMessage = "", Action? persistentFailureHandler = null ) {
		T? result = default;
		ExecuteRequestWithRetry(
			requestIsIdempotent,
			async () => { result = await method(); },
			additionalHandledMessage: additionalHandledMessage,
			persistentFailureHandler: persistentFailureHandler );
		return result!;
	}

	public static HttpContent GetRequestContentFromWriter( Action<Stream> bodyWriter ) => new WriterContent( bodyWriter );
}