using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Product;

namespace ShapekeyMaster
{
	internal class TranslationResource
	{
		public readonly string filePath;
		private Dictionary<string, string> Map;
		public TranslationResource(string filePath) 
		{
			Map = new Dictionary<string, string>();
			var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));
			this.filePath = filePath;

			foreach (var tran in translation) 
			{
				Map[tran.Key.ToLower()] = tran.Value;
			}
		}

		public string this[string name]
		{
			get 
			{
				name = name.ToLower();

				if (Map.TryGetValue(name, out var transString)) 
				{
					return transString;
				}

				Main.logger.LogWarning($"The translation resource is missing key {name}! Will fallback to using the key as the result!");

				Map[name] = name;
				return name;
			}
		}
	}
}
