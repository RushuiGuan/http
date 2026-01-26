using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Http {
	public static class ResponseExtensions {
		public const string GZipEncoding = "gzip";

		/// <summary>
		/// The response content stream should be be disposed by the caller because it is a single use stream. often LoggingMessageHandler might want to read
		/// the content stream for logging purposes.  The stream will be disposed when the HttpResponseMessage is disposed.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static async Task<Stream> GetContentStream(this HttpResponseMessage response, CancellationToken cancellationToken) {
			var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			if (response.Content.Headers.ContentEncoding.Contains(GZipEncoding)) {
				var gzip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
				return gzip;
			} else {
				return stream;
			}
		}

		public static async Task<string> ReadResponseAsText(this HttpResponseMessage response, bool resetBaseStream, CancellationToken cancellationToken) {
			if (response.Content.Headers.ContentLength != 0) {
				var stream = await response.GetContentStream(cancellationToken);
				try {
					using var reader = new StreamReader(stream, leaveOpen: true);
					return await reader.ReadToEndAsync(cancellationToken);
				} finally {
					if (stream is GZipStream gzip) {
						gzip.Dispose();
						stream = gzip.BaseStream;
					}
				}
			} else {
				return string.Empty;
			}
		}

		public static async Task<ResultType?> ReadResponseAsJson<ResultType>(this HttpResponseMessage response, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			var stream = await response.GetContentStream(cancellationToken);
			try {
				return await JsonSerializer.DeserializeAsync<ResultType>(stream, serializerOptions, cancellationToken);
			} finally {
				if (stream is GZipStream gzip) {
					gzip.Dispose();
				}
			}
		}

		public static async Task<ResultType?> ReadResponse<ResultType>(this HttpResponseMessage response, JsonSerializerOptions serializerOptions, CancellationToken cancellationToken) {
			if (typeof(ResultType) == typeof(string)) {
				var content = await response.ReadResponseAsText(false, cancellationToken);
				return (ResultType?)(object)content;
			} else {
				return await response.ReadResponseAsJson<ResultType>(serializerOptions, cancellationToken);
			}
		}

		public static async Task ReadResponseWithOutputStream(this HttpResponseMessage response, Stream outputStream, CancellationToken cancellationToken) {
			if (response.Content.Headers.ContentEncoding.Contains(GZipEncoding)) {
				var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
				using var gzip = new GZipStream(responseStream, CompressionMode.Decompress, true);
				await gzip.CopyToAsync(outputStream, cancellationToken);
			} else {
				await response.Content.CopyToAsync(outputStream, cancellationToken);
			}
		}
	}
}