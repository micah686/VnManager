using System;
using Newtonsoft.Json;

namespace VndbSharp.Models.Release
{
	public class VisualNovelMetadata
	{
		public UInt32 Id { get; private set; }
		[JsonProperty("title")]
		public String Name { get; private set; }
		[JsonProperty("original")]
		public String OriginalName { get; private set; }
	}
}
