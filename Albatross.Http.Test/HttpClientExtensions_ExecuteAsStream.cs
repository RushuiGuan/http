using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Albatross.Http;
using Albatross.Http.Exceptions;
using Xunit;

namespace Albatross.Http.Test {
	public class HttpClientExtensions_ExecuteAsStream {
		static async Task<List<T>> Collect<T>(IAsyncEnumerable<T> source) {
			var list = new List<T>();
			await foreach (var item in source) {
				list.Add(item);
			}
			return list;
		}

		[Fact]
		public async Task SuccessWithJsonArray_YieldsEachItem() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "[1,2,3]");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var items = await Collect(client.ExecuteAsStream<int>(request, TestHttp.Options, CancellationToken.None));

			Assert.Equal(new[] { 1, 2, 3 }, items);
		}

		[Fact]
		public async Task EmptyJsonArray_YieldsNoItems() {
			using var client = TestHttp.Client(HttpStatusCode.OK, "[]");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			var items = await Collect(client.ExecuteAsStream<int>(request, TestHttp.Options, CancellationToken.None));

			Assert.Empty(items);
		}

		// the status code is checked before streaming begins, so an error status throws on enumeration
		[Fact]
		public async Task ErrorStatus_ThrowsSemanticException() {
			using var client = TestHttp.Client(HttpStatusCode.NotFound, "error");
			using var request = new HttpRequestMessage(HttpMethod.Get, "api/test");

			await Assert.ThrowsAsync<HttpNotFoundException>(
				() => Collect(client.ExecuteAsStream<int>(request, TestHttp.Options, CancellationToken.None)));
		}
	}
}
