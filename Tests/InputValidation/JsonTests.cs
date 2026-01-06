using Tewl.InputValidation;

namespace Tests.InputValidation;

class JsonTests {
	[ Test ]
	public void ArrayEmpty() {
		Assert.That( new Validator().GetJsonArray( null, "" ).Error( out _ ), Is.Null );
	}

	[ Test ]
	public void ArraySingleValue() {
		Assert.That( new Validator().GetJsonArray( null, "1" ).Error( out _ ), Is.Null );
	}

	[ Test ]
	public void ArrayTwoValues() {
		Assert.That( new Validator().GetJsonArray( null, "1,2" ).Error( out _ ), Is.Null );
	}

	[ Test ]
	public void ArrayTwoValuesSpaced() {
		Assert.That( new Validator().GetJsonArray( null, "  1 ,   2 " ).Error( out _ ), Is.Null );
	}

	[ Test ]
	public void ArrayTwoValuesTrailingComma() {
		Assert.That( new Validator().GetJsonArray( null, "1,2," ).Error( out _ ), Is.Not.Null );
	}
}