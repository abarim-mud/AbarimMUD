﻿using System;

namespace AbarimMUD.Commands.AreaBuilder.OLCUtils
{
	public abstract class Record
	{
		public bool HasSetter { get; set; }

		public string Name => InternalName.ToLower();
		protected abstract string InternalName { get; }

		public abstract Type Type { get; }
		public string Category { get; set; }

		public abstract object GetValue(object obj);
		public abstract void SetValue(object obj, object value);

		public abstract T FindAttribute<T>() where T : Attribute;
	}
}
