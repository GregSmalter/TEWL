using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Reflection-based utilities.
	/// </summary>
	[ PublicAPI ]
	public static class ReflectionTools {
		/// <summary>
		/// Returns the name of the property in the provided expression.
		/// </summary>
		public static string GetPropertyName<T>( Expression<Func<T>> propertyExpression ) => ( (MemberExpression)propertyExpression.Body ).Member.Name;

		/// <summary>
		/// Returns the attribute for the given object if it's available. Otherwise, returns null.
		/// Improve performance by declaring your attribute as sealed.
		/// </summary>
		public static T GetAttribute<T>( this object e ) where T: Attribute {
			var memberInfo = e.GetType().GetMember( e.ToString() );
			if( memberInfo.Any() ) {
				var attributes = memberInfo[ 0 ].GetCustomAttributes( typeof( T ), false );
				if( attributes.Any() )
					return attributes[ 0 ] as T;
			}

			return null;
		}
	}
}