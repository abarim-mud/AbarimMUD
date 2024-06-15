using AbarimMUD.Data;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	public abstract class ReflectionRecord : IRecord
	{
		public abstract string Name { get; }
		public abstract Type Type { get; }
		public string TypeName => Type.Name.ToLower();

		public string ParamsString
		{
			get
			{
				var p = "_params_";

				if (Type.IsEnum || Type.IsNullableEnum())
				{
					p = Type.BuildEnumString();
				}

				return p;
			}
		}

		public abstract object GetValue(object obj);
		public abstract void SetValue(object obj, object value);

		public virtual bool SetStringValue(ExecutionContext context, object item, IReadOnlyList<string> values)
		{
			var s = values[0];
			if (s.EqualsToIgnoreCase("null"))
			{
				if (Type.IsClass || Type.IsNullable())
				{
					SetValue(item, null);
				}
				else
				{
					context.Send($"Property {Name} of type '{TypeName}' can't be set to null.");
					return false;
				}
			}
			else if (Type == typeof(string))
			{
				SetValue(item, s);
			}
			else if (Type == typeof(bool) || Type == typeof(bool?))
			{
				bool b;
				if (!context.EnsureBool(s, out b))
				{
					return false;
				}

				SetValue(item, b);
			}
			else if (Type == typeof(int) || Type == typeof(int?))
			{
				int i;
				if (!context.EnsureInt(s, out i))
				{
					return false;
				}

				SetValue(item, i);
			}
			else if (Type.IsEnum || Type.IsNullableEnum())
			{
				var enumType = Type;
				if (enumType.IsNullableEnum())
				{
					enumType = enumType.GetNullableType();
				}

				object v;
				if (!context.EnsureEnum(s, enumType, out v))
				{
					return false;
				}

				SetValue(item, v);
			}
			else if (Type == typeof(ValueRange) || Type == typeof(ValueRange?))
			{
				int level1Value;
				if (!context.EnsureInt(values[0], out level1Value))
				{
					return false;
				}

				int level100Value;
				if (!context.EnsureInt(values[1], out level100Value))
				{
					return false;
				}

				SetValue(item, new ValueRange(level1Value, level100Value));
			}
			else if (Type == typeof(GameClass))
			{
				var cls = context.EnsureClassById(s);
				if (cls == null)
				{
					return false;
				}

				if (ReferenceEquals(cls, item))
				{
					context.Send("Object can't reference itself.");
					return false;
				}

				SetValue(item, cls);
			}
			else
			{
				context.Send($"Setting propertes of type '{TypeName}' isn't implemented.");
				return false;
			}

			return true;
		}
	}
}
