using System;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	public delegate bool SetStringValuesDelegate(ExecutionContext context, object item, IReadOnlyList<string> values);

	public class DelegateRecord : IRecord
	{
		private SetStringValuesDelegate _setter;

		public string Name { get; private set; }
		public string ParamsString { get; private set; }

		public DelegateRecord(string name, string paramsString, SetStringValuesDelegate setter)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(paramsString))
			{
				throw new ArgumentNullException(nameof(paramsString));
			}

			Name = name;
			ParamsString = paramsString;
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}

		public bool SetStringValue(ExecutionContext context, object item, IReadOnlyList<string> values)
		{
			return _setter(context, item, values);
		}
	}
}