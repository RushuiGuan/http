using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Albatross.Http {
	/// <summary>
	/// Fluent builder for constructing <see cref="HttpRequestMessage"/> instances with support for
	/// JSON, text, stream, form URL-encoded, and multipart form data content types.
	/// Includes typed query string helpers for date/time types.
	/// The builder resets its state after each call to <see cref="Build"/>, allowing reuse.
	/// </summary>
	public class RequestBuilder {
		private HttpMethod _method = HttpMethod.Get;
		private string? _relativeUrl;
		private JsonSerializerOptions _serializerOptions = DefaultJsonSerializerOptions.Value;
		private HttpContent? _content;
		readonly NameValueCollection _queryString = new NameValueCollection();
		
		public RequestBuilder UseSerializationOptions(JsonSerializerOptions options) {
			this._serializerOptions = options;
			return this;
		}
		public RequestBuilder WithMethod(HttpMethod method) {
			this._method = method;
			return this;
		}
		public RequestBuilder WithRelativeUrl(string relativeUrl) {
			this._relativeUrl = relativeUrl;
			return this;
		}
		public RequestBuilder AddQueryString(string name, string value) {
			this._queryString.Add(name, value);
			return this;
		}
		public RequestBuilder AddDateTimeQueryString(string name, DateTime value) {
			this._queryString.Add(name, value.ToString("o"));
			return this;
		}
		public RequestBuilder AddDateOnlyQueryString(string name, DateOnly value) {
			this._queryString.Add(name, value.ToString("yyyy-MM-dd"));
			return this;
		}
		public RequestBuilder AddTimeOnlyQueryString(string name, TimeOnly value) {
			this._queryString.Add(name, value.ToString("HH:mm:ss.fffffff").TrimEnd('0').TrimEnd('.'));
			return this;
		}
		public RequestBuilder AddDateTimeOffsetQueryString(string name, DateTimeOffset value) {
			this._queryString.Add(name, value.ToString("o"));
			return this;
		}
		public RequestBuilder CreateJsonRequest<T>(T? t) {
			if (t != null) {
				var text = JsonSerializer.Serialize<T>(t, this._serializerOptions);
				_content = new StringContent(text, Encoding.UTF8, ContentTypes.Json);
			}else {
				_content = null;
			}
			return this;
		}
		public RequestBuilder CreateStringRequest(string? text) {
			if (!string.IsNullOrEmpty(text)) {
				_content = new StringContent(text, Encoding.UTF8, ContentTypes.Text);
			} else {
				_content = null;
			}
			return this;
		}
		public RequestBuilder CreateStreamRequest(Stream stream) {
			_content = new StreamContent(stream);
			return this;
		}
		public RequestBuilder CreateFormUrlEncodedRequest(IDictionary<string, string> formUrlEncodedValues) {
			_content = new FormUrlEncodedContent(formUrlEncodedValues);
			return this;
		}

		/// <summary>
		/// Adds a file to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple files.
		/// </summary>
		/// <param name="fieldName">The form field name for the file.</param>
		/// <param name="fileName">The file name to send to the server.</param>
		/// <param name="fileContent">The file content as a byte array.</param>
		/// <param name="contentType">The MIME type of the file (e.g., "image/png").</param>
		/// <returns>The request with file content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public RequestBuilder AddFileToMultipartFormData(string fieldName, string fileName, byte[] fileContent, string contentType) {
			var content = GetOrCreateContent<MultipartFormDataContent>();
			var filePartContent = new ByteArrayContent(fileContent);
			filePartContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
			content.Add(filePartContent, fieldName, fileName);
			return this;
		}

		/// <summary>
		/// Adds a file to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple files.
		/// </summary>
		/// <param name="fieldName">The form field name for the file.</param>
		/// <param name="fileName">The file name to send to the server.</param>
		/// <param name="fileStream">The file content as a stream.</param>
		/// <param name="contentType">The MIME type of the file (e.g., "image/png").</param>
		/// <returns>The request with file content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public RequestBuilder AddFileToMultipartFormData(string fieldName, string fileName, Stream fileStream, string contentType) {
			var content = GetOrCreateContent<MultipartFormDataContent>();
			var filePartContent = new StreamContent(fileStream);
			filePartContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
			content.Add(filePartContent, fieldName, fileName);
			return this;
		}

		/// <summary>
		/// Adds a string form field to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple fields.
		/// </summary>
		/// <param name="fieldName">The form field name.</param>
		/// <param name="value">The string value.</param>
		/// <returns>The request with form field added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public RequestBuilder AddStringToMultipartFormData(string fieldName, string value) {
			var content = GetOrCreateContent<MultipartFormDataContent>();
			content.Add(new StringContent(value), fieldName);
			return this;
		}

		/// <summary>
		/// Adds a JSON-serialized object to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple fields.
		/// </summary>
		/// <typeparam name="T">The type of the object to serialize.</typeparam>
		/// <param name="fieldName">The form field name.</param>
		/// <param name="value">The object to serialize as JSON.</param>
		/// <returns>The request with JSON content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public RequestBuilder AddJsonToMultipartFormData<T>(string fieldName, T value) {
			var content = GetOrCreateContent<MultipartFormDataContent>();
			var json = JsonSerializer.Serialize(value, this._serializerOptions);
			var jsonContent = new StringContent(json, Encoding.UTF8, ContentTypes.Json);
			content.Add(jsonContent, fieldName);
			return this;
		}

		T GetOrCreateContent<T>() where T : HttpContent, new() {
			if (_content == null) {
				var t = new T();
				this._content = t;
				return t;
			} else if (_content is T t) {
				return t;
			} else {
				throw new InvalidOperationException($"Request content is of type {_content.GetType().Name}, cannot be used as {typeof(T).Name}.");
			}
		}

		public RequestBuilder Reset() {
			_method = HttpMethod.Get;
			_relativeUrl = null;
			_serializerOptions = DefaultJsonSerializerOptions.Value;
			_content = null;
			_queryString.Clear();
			return this;
		}

		/// <summary>
		/// Builds the <see cref="HttpRequestMessage"/> from the current builder state and resets the builder for reuse.
		/// </summary>
		public HttpRequestMessage Build() {
			var url = _relativeUrl.CreateUrl(this._queryString);
			if(url.Length > 0 && url[^1] == '&') {
				url.Length -= 1; // remove trailing '&'
			}
			var request = new HttpRequestMessage(_method, url.ToString()) {
				Content = this._content
			};
			Reset();
			return request;
		}
	}
}

