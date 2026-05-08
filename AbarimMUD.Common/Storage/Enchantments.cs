using AbarimMUD.Data;
using System.Collections.Generic;
using Ur;

namespace AbarimMUD.Storage
{
	internal class Enchantments : MultipleFilesStorage<Enchantment>
	{
		public Enchantments() : base("enchantments")
		{
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach (var enchantment in this)
			{
				var newMaterials = new List<ItemMaterial>();
				foreach (var material in enchantment.Materials)
				{
					newMaterials.Add(ItemMaterial.EnsureItemMaterialById(material.Id));
				}

				enchantment.Materials = newMaterials;
			}
		}
	}
}
