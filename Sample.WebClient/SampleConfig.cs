using Albatross.Config;
using Microsoft.Extensions.Configuration;

namespace Sample.WebClient {
	public class SampleConfig : ConfigBase{
		public SampleConfig(IConfiguration configuration) : base(configuration, string.Empty) {
			this.EndPoint = configuration.GetRequiredEndPoint("sample");
		}

		public string EndPoint { get;  }
	}
}