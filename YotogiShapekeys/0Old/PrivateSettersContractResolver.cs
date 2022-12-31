using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ShapekeyMaster
{
	class PrivateSettersContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var prop = base.CreateProperty(member, memberSerialization);
			if (!prop.Writable)
			{
				var property = member as PropertyInfo;
				var hasPrivateSetter = property?.GetSetMethod(true) != null;
				prop.Writable = hasPrivateSetter;
			}
			return prop;
		}
	}
}