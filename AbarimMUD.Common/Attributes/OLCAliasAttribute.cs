using System;

namespace AbarimMUD.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class OLCAliasAttribute : Attribute
	{
		public string Alias { get; private set; }

		public OLCAliasAttribute(string alias)
		{
			if (string.IsNullOrWhiteSpace(alias))
			{
				throw new ArgumentNullException(nameof(alias));
			}

			Alias = alias;
		}
	}
}
