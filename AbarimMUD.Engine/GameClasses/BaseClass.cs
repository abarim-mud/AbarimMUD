using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.GameClasses
{
	public abstract class BaseClass
	{
		private static readonly BaseClass[] _allClasses;

		public static BaseClass[] AllClasses
		{
			get { return _allClasses; }
		}

		public abstract string Name { get; }
		public abstract string Description { get; }

		static BaseClass()
		{
			var allClasses = new List<BaseClass>
			{
				new Warrior(),
				new Physician(),
				new Rogue(),
				new Scholar(),
				new Adept(),
				new Bard()
			};

			_allClasses = allClasses.ToArray();
		}

		public static BaseClass EnsureClassByName(string name)
		{
			var result = (from c in _allClasses where string.Compare(name, c.Name, StringComparison.OrdinalIgnoreCase) == 0 select c).FirstOrDefault();

			if (result == null)
			{
				throw new Exception(string.Format("Could not find a game class {0}.", name));
			}

			return result;
		}
	}
}