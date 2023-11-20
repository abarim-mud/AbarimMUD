using System;
using System.Linq;
using System.Reflection;

namespace AbarimMUD.Utils
{
	public static class ReflectionExtension
	{
		public static MethodInfo FindMethodByAttribute<T>(this Type type) where T : Attribute
		{
			var attributeType = typeof (T);
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach (var m in methods)
			{
				var attrs = m.GetCustomAttributes(true);
				foreach (object a in attrs)
				{
					if (a.GetType() == attributeType)
					{
						// Found
						return m;
					}
				}
			}

			return null;
		}

		public static PropertyInfo FindPropertyByAttribute<T>(this Type type) where T : Attribute
		{
			var attributeType = typeof (T);
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach (var m in properties)
			{
				var attrs = m.GetCustomAttributes(true);
				foreach (object a in attrs)
				{
					if (a.GetType() == attributeType)
					{
						// Found
						return m;
					}
				}
			}

			return null;
		}

		public static T FindAttribute<T>(this PropertyInfo property) where T : Attribute
		{
			var result =
				(from T a in property.GetCustomAttributes(typeof (T), true) select a).FirstOrDefault();

			return result;
		}

		public static T FindAttribute<T>(this MethodInfo method) where T : Attribute
		{
			var result =
				(from T a in method.GetCustomAttributes(typeof (T), true) select a).FirstOrDefault();

			return result;
		}

		public static T FindAttribute<T>(this Type type) where T : Attribute
		{
			var result =
				(from T a in type.GetCustomAttributes(typeof (T), true) select a).FirstOrDefault();

			return result;
		}
	}
}