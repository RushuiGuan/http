using System.Text.Json;
using System.Text.Json.Serialization;

namespace Albatross.Http {
	public static class DefaultJsonSerializationOptions {
		public static readonly JsonSerializerOptions Value = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
	}
}