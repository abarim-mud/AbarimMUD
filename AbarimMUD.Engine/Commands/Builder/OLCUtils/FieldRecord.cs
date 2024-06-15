using System;
using System.Reflection;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	internal class FieldRecord : ReflectionRecord
	{
		private readonly FieldInfo _fieldInfo;

		public override string Name => _fieldInfo.Name;

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
	}
}
