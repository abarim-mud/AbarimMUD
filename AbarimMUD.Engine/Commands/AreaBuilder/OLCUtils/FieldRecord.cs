using System;
using System.Reflection;

namespace AbarimMUD.Commands.AreaBuilder.OLCUtils
{
	internal class FieldRecord : Record
	{
		private readonly FieldInfo _fieldInfo;

		protected override string InternalName => _fieldInfo.Name;

		public override Type Type => _fieldInfo.FieldType;

		public FieldRecord(FieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
		}

		public override object GetValue(object obj)
		{
			return _fieldInfo.GetValue(obj);
		}

		public override void SetValue(object obj, object value)
		{
			_fieldInfo.SetValue(obj, value);
		}

		public override T FindAttribute<T>()
		{
			return _fieldInfo.FindAttribute<T>();
		}
	}
}
