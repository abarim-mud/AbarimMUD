using AbarimMUD.Data;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;

namespace AbarimMUD.CostsGenerator
{
	internal class Program
	{
		private static readonly Dictionary<int, long> _hardcodedLevels = new Dictionary<int, long>
		{
			[2] = 2000,
			[3] = 4000,
			[4] = 8000,
			[5] = 16000,
			[6] = 20000,
			[7] = 30000,
			[8] = 40000,
			[9] = 70000,
			[10] = 120000,
			[11] = 150000,
			[12] = 200000,
			[13] = 250000,
			[14] = 400000,
			[17] = 1000000,
			[20] = 2500000,
			[40] = 100000000,
		};

		private static readonly Dictionary<int, long> _hardcodedSkillCosts = new Dictionary<int, long>
		{
			[1] = 50,
			[2] = 100,
			[3] = 150,
		};

		static void UpdateValues<T>(SingleFileStorage<int, T> storage, int count,
			long max, int pow,
			Dictionary<int, long> hardcodedValues,
			Action<T, int> idSetter, Action<T, long> valueSetter)
				where T : class, new()
		{
			storage.ClearCache();
			for (var id = 1; id <= count; ++id)
			{
				long value;
				if (hardcodedValues.TryGetValue(id, out value))
				{
				}
				else
				{
					var k = Math.Pow((id - 1) / (float)(count - 1), pow);

					value = (long)(max * k);

					// Make the number pretty
					var str = value.ToString();

					if (str.Length < 3)
					{
						continue;
					}

					var firstDigit = int.Parse(str[0].ToString());

					// Make last digits zeroes
					var f = (long)Math.Pow(10, str.Length - 2);
					if (f > 0)
					{
						var ff = Math.Round(value / (double)f);
						value = (long)(ff * f);
					}
				}

				var l = new T();

				idSetter(l, id);
				valueSetter(l, value);

				storage.Create(l);
			}
		}

		static void Process(string dataFolder)
		{
			StorageUtility.InitializeStorage(Console.WriteLine);

			DataContext.Load(dataFolder);

			UpdateValues(LevelInfo.Storage, 100, 10000000000, 5, _hardcodedLevels,
				(l, id) => l.Level = id, (l, v) => l.Experience = v);
			UpdateValues(SkillCostInfo.Storage, 100, 10000000, 3, _hardcodedSkillCosts,
				(l, id) => l.Order = id, (l, v) => l.Gold = v);

			LevelInfo.Storage.SaveAll();
			SkillCostInfo.Storage.SaveAll();
		}

		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					Console.WriteLine("Usage: AbarimMUD.Launcher <data_folder>");
					return;
				}

				Process(args[0]);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
}
