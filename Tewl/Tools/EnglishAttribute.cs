﻿using System;

namespace Tewl.Tools {
	/// <summary>
	/// Apply this attribute to the value of an Enum.
	/// </summary>
	public class EnglishAttribute: Attribute {
		public readonly string English;

		public EnglishAttribute( string english ) {
			English = english;
		}
	}
}