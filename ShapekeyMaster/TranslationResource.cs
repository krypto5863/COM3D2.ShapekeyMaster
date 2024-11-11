using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ShapeKeyMaster
{
	internal class TranslationResource
	{
		public readonly string FilePath;
		private readonly Dictionary<string, string> map;

		public TranslationResource(string filePath)
		{
			map = new Dictionary<string, string>();
			var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));
			FilePath = filePath;

			foreach (var tran in translation)
			{
				map[tran.Key.ToLower()] = tran.Value;
			}
		}

		public string this[string name]
		{
			get
			{
				name = name.ToLower();

				if (map.TryGetValue(name, out var transString))
				{
					return transString;
				}

				ShapeKeyMaster.PluginLogger.LogWarning($"The translation resource is missing key {name}! Will fallback to using the key as the result!");

				map[name] = name;
				return name;
			}
		}
	}
}