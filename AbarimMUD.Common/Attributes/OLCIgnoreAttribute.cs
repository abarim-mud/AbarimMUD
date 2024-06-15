using System;

namespace AbarimMUD.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class OLCIgnoreAttribute: Attribute
	{
	}
}
