using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
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
	/// <summary>
	/// The result of a method that makes a request.
	/// </summary>
	[ PublicAPI ]
	public class Result {
		/// <summary>
		/// Returns the failure, or null if the request was successful.
		/// </summary>
		public Failure? Failure { get; }

		/// <summary>
		/// HttpClientTools.ExecuteRequest use only.
		/// </summary>
		internal Result( Failure? failure ) {
			Failure = failure;
		}
	}

	/// <summary>
	/// The result of a method that makes a request.
	/// </summary>
	[ PublicAPI ]
	public class Result<T> {
		private readonly T value;
		private readonly Failure? failure;

		/// <summary>
		/// HttpClientTools.ExecuteRequest use only.
		/// </summary>
		internal Result( T value, Failure? failure ) {
			this.value = value;
			this.failure = failure;
		}

		/// <summary>
		/// Returns the failure, or null if the request was successful.
		/// </summary>
		/// <param name="value">The value.</param>
		public Failure? Failure( out T value ) {
			value = this.value;
			return failure;
		}
	}

	/// <summary>
	/// A request failure.
	/// </summary>
	[ PublicAPI ]
	public class Failure {
		/// <summary>
		/// Gets the error message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Gets whether the server may have begun processing the request, which is important when deciding whether to retry a non-idempotent request.
		/// </summary>
		public bool RequestProcessingMayHaveBegun { get; }

		/// <summary>
		/// HttpClientTools.ExecuteRequest use only.
		/// </summary>
		internal Failure( string message, bool requestProcessingMayHaveBegun ) {
			Message = message;
			RequestProcessingMayHaveBegun = requestProcessingMayHaveBegun;
		}
	}

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
	/// Makes a GET request for a text-based resource and returns its representation.
	/// </summary>
	public static Result<string?> GetText( this HttpClient client, string url, bool returnNullIfNotFound = false ) =>
		ExecuteRequest( () => getText( client, url, returnNullIfNotFound ) );

	/// <summary>
	/// Makes a GET request for a text-based resource and returns its representation, retrying several times with exponential back-off in the event of network
	/// problems or transient failures on the server. Use only from a background process that can tolerate a long delay.
	/// </summary>
	public static string? GetTextWithRetry( this HttpClient client, string url, bool returnNullIfNotFound = false, string additionalHandledMessage = "" ) {
		try {
			return ExecuteRequestWithRetry( true, () => getText( client, url, returnNullIfNotFound ), additionalHandledMessage: additionalHandledMessage );
		}
		catch( Exception e ) {
			throw new Exception( $"A GET request for {url} failed.", e );
		}
	}

	private static async Task<string?> getText( this HttpClient client, string url, bool returnNullIfNotFound ) {
		using var response = await client.GetAsync( url, HttpCompletionOption.ResponseHeadersRead );
		if( returnNullIfNotFound && response.StatusCode == HttpStatusCode.NotFound )
			return null;
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	/// <summary>
	/// Makes a GET request for a resource and writes its representation to a file at the specified path. Overwrites the destination file if it already exists.
	/// </summary>
	public static Result DownloadFile( this HttpClient client, string url, string destinationPath ) =>
		ExecuteRequest( () => downloadFile( client, url, destinationPath ) );

	/// <summary>
	/// Makes a GET request for a resource and writes its representation to a file at the specified path, retrying several times with exponential back-off in the
	/// event of network problems or transient failures on the server. Use only from a background process that can tolerate a long delay. Overwrites the
	/// destination file if it already exists.
	/// </summary>
	public static void DownloadFileWithRetry( this HttpClient client, string url, string destinationPath, string additionalHandledMessage = "" ) =>
		ExecuteRequestWithRetry( true, () => downloadFile( client, url, destinationPath ), additionalHandledMessage: additionalHandledMessage );

	private static async Task downloadFile( this HttpClient client, string url, string destinationPath ) {
		using var response = await client.GetAsync( url, HttpCompletionOption.ResponseHeadersRead );
		response.EnsureSuccessStatusCode();

		IoMethods.DeleteFile( destinationPath );
		await using var fileStream = IoMethods.GetFileStreamForWrite( destinationPath );
		await response.Content.CopyToAsync( fileStream );
	}

	/// <summary>
	/// Executes a method that makes a request using <see cref="HttpClient"/>.
	/// </summary>
	public static Result ExecuteRequest( Func<Task> method ) {
		try {
			using( MiniProfiler.Current.Step( "TEWL - Execute HTTP request" ) )
				Task.Run( method ).Wait();
		}
		catch( Exception exception ) {
			if( exception.InnerException is {} e ) {
				if( preRequestProcessingFailureOccurred( e ) )
					return new Result( new Failure( e.Message, false ) );
				if( possibleRequestProcessingFailureOccurred( e ) || e is HttpRequestException
					   {
						   StatusCode: HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable
					   } )
					return new Result( new Failure( e.Message, true ) );

				// Remove the AggregateException from the Task.Run line above.
				if( exception is AggregateException )
					ExceptionDispatchInfo.Capture( e ).Throw();
			}
			throw;
		}
		return new Result( null );
	}

	/// <summary>
	/// Executes a method that makes a request using <see cref="HttpClient"/>.
	/// </summary>
	public static Result<T> ExecuteRequest<T>( Func<Task<T>> method ) {
		T? value = default;
		var result = ExecuteRequest( async () => { value = await method(); } );
		return new Result<T>( value!, result.Failure );
	}

	/// <summary>
	/// Executes a method that makes a request using <see cref="HttpClient"/>, retrying several times with exponential back-off in the event of network problems
	/// or transient failures on the server. Use only from a background process that can tolerate a long delay.
	/// </summary>
	public static void ExecuteRequestWithRetry(
		bool requestIsIdempotent, Func<Task> method, string additionalHandledMessage = "", Action? persistentFailureHandler = null ) {
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
		catch( Exception exception ) {
			if( persistentFailureHandler is null || exception.InnerException is not {} e || !isHandled( e ) )
				throw;
			persistentFailureHandler();
		}
		return;

		bool isHandled( Exception e ) {
			if( preRequestProcessingFailureOccurred( e ) )
				return true;
			if( !requestIsIdempotent )
				return false;

			if( possibleRequestProcessingFailureOccurred( e ) )
				return true;
			return additionalHandledMessage.Length > 0 && e is HttpRequestException && e.Message.Contains( additionalHandledMessage );
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

	private static bool preRequestProcessingFailureOccurred( Exception e ) {
		return e is HttpRequestException { InnerException: SocketException { SocketErrorCode: SocketError.HostNotFound or SocketError.NoData } };
	}

	private static bool possibleRequestProcessingFailureOccurred( Exception e ) {
		if( e is TaskCanceledException ) // timeout
			return true;
		if( e is HttpRequestException { InnerException: SocketException { SocketErrorCode: SocketError.ConnectionRefused } } )
			return true;
		if( e is HttpRequestException { StatusCode: HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway } )
			return true;
		return false;
	}

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