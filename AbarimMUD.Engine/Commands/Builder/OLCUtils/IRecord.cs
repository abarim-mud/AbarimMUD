using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	public interface IRecord
	{
		string Name { get; }
		public string ParamsString { get; }

		bool SetStringValue(ExecutionContext context, object item, IReadOnlyList<string> values);
	}
}
