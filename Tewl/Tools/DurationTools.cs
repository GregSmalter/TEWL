using Humanizer;
using Humanizer.Localisation;
using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// Duration extensions.
/// </summary>
[ PublicAPI ]
public static class DurationTools {
	/// <summary>
	/// Returns a phrase representing this duration, such as “52 seconds” or “1 minute, 3 seconds” or “4 days, 6 hours”.
	/// </summary>
	public static string ToConcisePhrase( this Duration duration ) => duration.ToTimeSpan().ToConciseString();

	/// <summary>
	/// Returns a phrase representing this duration in seconds (rounded up), e.g. 93 seconds.
	/// </summary>
	public static string ToSecondsPhrase( this Duration duration ) =>
		duration.Minus( Duration.Epsilon ).Plus( Duration.FromSeconds( 1 ) ).ToTimeSpan().Humanize( minUnit: TimeUnit.Second, maxUnit: TimeUnit.Second );
}