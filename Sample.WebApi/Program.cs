using Albatross.Hosting;
using System;
using System.Threading.Tasks;

namespace Sample.WebApi {
	public class Program {
		public static Task Main(string[] args) {
			return new Setup(args, AppContext.BaseDirectory)
				.ConfigureWebHost<MyStartup>()
				.RunAsync();
		}
	}
}