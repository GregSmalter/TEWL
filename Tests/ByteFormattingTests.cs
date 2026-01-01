namespace Tests;

class ByteFormattingTests {
	[ Test ]
	public void Bytes() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64, false ), Is.EqualTo( "64 bytes" ) );
	}

	[ Test ]
	public void Kilo() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000, false ), Is.EqualTo( "64 kB" ) );
	}

	[ Test ]
	public void Mega() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000_000, false ), Is.EqualTo( "64 MB" ) );
	}

	[ Test ]
	public void Giga() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_500_000_000, false ), Is.EqualTo( "64.5 GB" ) );
	}

	[ Test ]
	public void Kibi() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000, true ), Is.EqualTo( "62 KiB" ) );
	}

	[ Test ]
	public void Mebi() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_000_000, true ), Is.EqualTo( "61 MiB" ) );
	}

	[ Test ]
	public void Gibi() {
		Assert.That( FormattingMethods.GetFormattedBytes( 64_500_000_000, true ), Is.EqualTo( "60.1 GiB" ) );
	}
}