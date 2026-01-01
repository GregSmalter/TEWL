namespace Tests;

class ByteFormattingTests {
	[ Test ]
	public void Bytes() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64 ), Is.EqualTo( "64 bytes" ) );
	}

	[ Test ]
	public void Kilo() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000 ), Is.EqualTo( "62 KiB" ) );
	}

	[ Test ]
	public void Mega() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000_000 ), Is.EqualTo( "61 MiB" ) );
	}

	[ Test ]
	public void Giga() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_500_000_000 ), Is.EqualTo( "60.1 GiB" ) );
	}
}