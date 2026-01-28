using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;

namespace Albatross.Http {
	public static class UrlExtensions {
		/// <summary>
		/// Creates one or more relative URL strings with array values joined by a delimiter into a single query parameter.
		/// When the URL would exceed <paramref name="maxUrlLength"/>, values are split across multiple URLs.
		/// </summary>
		/// <remarks>
		/// This method accommodates non-standard APIs that expect delimited array values in a single query parameter
		/// (e.g., <c>id=0,1,2</c>) rather than the standard HTTP approach of repeated parameters.
		/// For the standard approach (e.g., <c>id=0&amp;id=1&amp;id=2</c>), use <see cref="CreateUrlArray"/> instead.
		/// </remarks>
		/// <param name="baseUri">The base URI used to calculate available length for the relative URL portion.</param>
		/// <param name="relativeUrl">The relative URL path (e.g., "api/items").</param>
		/// <param name="queryStrings">Additional query string parameters to include in each URL.</param>
		/// <param name="maxUrlLength">The maximum allowed total URL length (base URI + relative URL + query string).</param>
		/// <param name="queryStringKey">The query string key for the array values (e.g., "id").</param>
		/// <param name="delimiter">The delimiter used to join array values (e.g., "," or "|").</param>
		/// <param name="arrayValues">The array values to be included across the generated URLs.</param>
		/// <returns>One or more relative URL strings, each within the specified max length.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="maxUrlLength"/> is too small to fit even a single array value.
		/// </exception>
		public static IEnumerable<string> CreateUrlArrayByDelimitedValue(Uri baseUri, string relativeUrl, NameValueCollection queryStrings, int maxUrlLength, string queryStringKey, string delimiter, params string[] arrayValues) {
			var urls = new List<string>();
			int offset = 0;
			do {
				var sb = relativeUrl.CreateUrl(queryStrings);
				sb.Append(Uri.EscapeDataString(queryStringKey)).Append('=');
				int index;
				for (index = offset; index < arrayValues.Length; index++) {
					int current = sb.Length;
					if (index > offset) {
						sb.Append(Uri.EscapeDataString(delimiter));
					}
					sb.Append(Uri.EscapeDataString(arrayValues[index]));
					if (sb.Length > maxUrlLength - baseUri.AbsoluteUri.Length) {
						sb.Length = current;
						if (index == offset) {
							throw new ArgumentException("Cannot create requests because url max length is smaller than the minimum length required for a single request");
						}
						break;
					}
				}
				urls.Add(sb.ToString());
				offset = index;
			} while (offset < arrayValues.Length);
			return urls;
		}

		/// <summary>
		/// Creates one or more relative URL strings with array values as repeated query parameters.
		/// When the URL would exceed <paramref name="maxUrlLength"/>, values are split across multiple URLs.
		/// </summary>
		/// <remarks>
		/// This method uses the standard HTTP approach of repeating query parameters for array values
		/// (e.g., <c>id=0&amp;id=1&amp;id=2</c>). For APIs that expect delimited values (e.g., <c>id=0,1,2</c>),
		/// use <see cref="CreateUrlArrayByDelimitedValue"/> instead.
		/// </remarks>
		/// <param name="baseUri">The base URI used to calculate available length for the relative URL portion.</param>
		/// <param name="relativeUrl">The relative URL path (e.g., "api/items").</param>
		/// <param name="queryStrings">Additional query string parameters to include in each URL.</param>
		/// <param name="maxUrlLength">The maximum allowed total URL length (base URI + relative URL + query string).</param>
		/// <param name="arrayQueryStringKey">The query string key for the array values (e.g., "id").</param>
		/// <param name="arrayQueryStringValues">The array values to be included across the generated URLs.</param>
		/// <returns>One or more relative URL strings, each within the specified max length.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="maxUrlLength"/> is too small to fit even a single array value.
		/// </exception>
		public static IEnumerable<string> CreateUrlArray(Uri baseUri, string relativeUrl, NameValueCollection queryStrings, int maxUrlLength, string arrayQueryStringKey, params string[] arrayQueryStringValues) {
			var urls = new List<string>();
			int offset = 0;
			do {
				var sb = relativeUrl.CreateUrl(queryStrings);
				int index;
				for (index = offset; index < arrayQueryStringValues.Length; index++) {
					int current = sb.Length;
					sb.AddQueryParam(arrayQueryStringKey, arrayQueryStringValues[index]!);
					if (sb.Length > maxUrlLength - baseUri.AbsoluteUri.Length) {
						sb.Length = current;
						if (index == offset) {
							throw new ArgumentException("Cannot create requests because url max length is smaller than the minimum length required for a single request");
						}
						break;
					}
				}
				urls.Add(sb.ToString());
				offset = index;
			} while (offset < arrayQueryStringValues.Length);
			return urls;
		}

		/// <summary>
		/// Creates a <see cref="StringBuilder"/> containing the URL with query string parameters appended.
		/// </summary>
		/// <remarks>
		/// All keys and values are URL-encoded using <see cref="Uri.EscapeDataString"/>.
		/// If a key has multiple values, each is added as a separate query parameter.
		/// The resulting string ends with a trailing <c>&amp;</c> if any query parameters are present.
		/// </remarks>
		/// <param name="url">The base URL or relative path (e.g., "api/items").</param>
		/// <param name="queryStringValues">The query string parameters to append. Can be null or empty.</param>
		/// <returns>A <see cref="StringBuilder"/> containing the URL with query parameters, suitable for further modification.</returns>
		public static StringBuilder CreateUrl(this string? url, NameValueCollection? queryStringValues) {
			url = url ?? string.Empty;
			var sb = new StringBuilder(url);
			if (queryStringValues?.Count > 0) {

				// if url does not already contain '?', append it
				if (!url.Contains("?")) {
					sb.Append("?");
				} else if (!(url.EndsWith("?") || url.EndsWith("&"))) {
					// if the url doesn't end with '?' or '&', append '&'
					sb.Append("&");
				}

				for (int i = 0; i < queryStringValues.Count; i++) {
					string[] values = queryStringValues.GetValues(i) ?? Array.Empty<string>();
					string key = queryStringValues.GetKey(i) ?? string.Empty;
					foreach (string value in values) {
						sb.AddQueryParam(key, value);
					}
				}
			}
			return sb;
		}
		internal static void AddQueryParam(this StringBuilder sb, string key, string value) {
			sb.Append(Uri.EscapeDataString(key));
			sb.Append("=");
			sb.Append(Uri.EscapeDataString(value));
			sb.Append("&");
		}

		/// <summary>
		/// Gets the full URI from the request, resolving relative URIs against the provided base address.
		/// </summary>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="baseAddress">The base address to use for resolving relative URIs.</param>
		/// <returns>The full absolute URI.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request URI is null and no base address is provided,
		/// or when the request URI is relative and no base address is provided.
		/// </exception>
		public static Uri GetFullUri(this HttpRequestMessage request, Uri? baseAddress) {
			if (request.RequestUri == null) {
				if (baseAddress == null) {
					throw new InvalidOperationException("RequestUri is null and no base address is provided");
				}
				return baseAddress;
			}
			if (request.RequestUri.IsAbsoluteUri) {
				return request.RequestUri;
			}
			if (baseAddress == null) {
				throw new InvalidOperationException("RequestUri is relative and no base address is provided");
			}
			return new Uri(baseAddress, request.RequestUri);
		}
		
		public const string ISO8601DateOnlyFormat = "yyyy-MM-dd";
		public const string ISO8601TimeOnlyFormat = "HH:mm:ss.fffffff";
		public const string ISO8601Format = "yyyy-MM-ddTHH:mm:ssK";
		
		public static string ISO8601(this DateOnly value) => value.ToString(ISO8601DateOnlyFormat);
		public static string ISO8601(this TimeOnly value) => value.ToString(ISO8601TimeOnlyFormat).TrimEnd('0').TrimEnd('.');
		public static string ISO8601(this DateTime value) => value.ToString(ISO8601Format);
		public static string ISO8601(this DateTimeOffset value) => value.ToString(ISO8601Format);
	}
}