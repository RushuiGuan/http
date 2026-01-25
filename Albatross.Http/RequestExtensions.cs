using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Albatross.Http {
	public static class RequestExtensions {
		public static HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, NameValueCollection queryStringValues) {
			var sb = relativeUrl.CreateUrl(queryStringValues);
			if (sb.Length > 0 && sb[^1] == '&') { sb.Length--; }
			var request = new HttpRequestMessage(method, sb.ToString());
			return request;
		}
		public static HttpRequestMessage CreateJsonRequest<T>(this HttpRequestMessage request, T? t, JsonSerializerOptions jsonSerializerOptions) {
			if (t != null) {
				var content = JsonSerializer.Serialize<T>(t, jsonSerializerOptions);
				request.Content = new StringContent(content, Encoding.UTF8, ContentTypes.Json);
			}
			return request;
		}
		public static HttpRequestMessage CreateStringRequest(this HttpRequestMessage request, string? content) {
			if (content != null) {
				request.Content = new StringContent(content, Encoding.UTF8, ContentTypes.Text);
			}
			return request;
		}
		public static HttpRequestMessage CreateStreamRequest(this HttpRequestMessage request, Stream stream) {
			request.Content = new StreamContent(stream);
			return request;
		}
		public static HttpRequestMessage CreateFormUrlEncodedRequest(this HttpRequestMessage request, IDictionary<string, string> formUrlEncodedValues) {
			var content = new FormUrlEncodedContent(formUrlEncodedValues);
			request.Content = content;
			return request;
		}

		/// <summary>
		/// Adds a file to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple files.
		/// </summary>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="fieldName">The form field name for the file.</param>
		/// <param name="fileName">The file name to send to the server.</param>
		/// <param name="fileContent">The file content as a byte array.</param>
		/// <param name="contentType">The MIME type of the file (e.g., "image/png").</param>
		/// <returns>The request with file content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public static HttpRequestMessage AddFileToMultipartFormData(this HttpRequestMessage request, string fieldName, string fileName, byte[] fileContent, string contentType) {
			var content = request.Content as MultipartFormDataContent;
			if (content == null) {
				if (request.Content != null) {
					throw new InvalidOperationException("Request content is not MultipartFormDataContent");
				} else {
					content = new MultipartFormDataContent();
					request.Content = content;
				}
			}
			var filePartContent = new ByteArrayContent(fileContent);
			filePartContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
			content.Add(filePartContent, fieldName, fileName);
			return request;
		}

		/// <summary>
		/// Adds a file to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple files.
		/// </summary>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="fieldName">The form field name for the file.</param>
		/// <param name="fileName">The file name to send to the server.</param>
		/// <param name="fileStream">The file content as a stream.</param>
		/// <param name="contentType">The MIME type of the file (e.g., "image/png").</param>
		/// <returns>The request with file content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public static HttpRequestMessage AddFileToMultipartFormData(this HttpRequestMessage request, string fieldName, string fileName, Stream fileStream, string contentType) {
			var content = request.Content as MultipartFormDataContent;
			if (content == null) {
				if (request.Content != null) {
					throw new InvalidOperationException("Request content is not MultipartFormDataContent");
				} else {
					content = new MultipartFormDataContent();
					request.Content = content;
				}
			}
			var filePartContent = new StreamContent(fileStream);
			filePartContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
			content.Add(filePartContent, fieldName, fileName);
			return request;
		}

		/// <summary>
		/// Adds a string form field to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple fields.
		/// </summary>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="fieldName">The form field name.</param>
		/// <param name="value">The string value.</param>
		/// <returns>The request with form field added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public static HttpRequestMessage AddStringToMultipartFormData(this HttpRequestMessage request, string fieldName, string value) {
			var content = request.Content as MultipartFormDataContent;
			if (content == null) {
				if (request.Content != null) {
					throw new InvalidOperationException("Request content is not MultipartFormDataContent");
				} else {
					content = new MultipartFormDataContent();
					request.Content = content;
				}
			}
			content.Add(new StringContent(value), fieldName);
			return request;
		}

		/// <summary>
		/// Adds a JSON-serialized object to the request as multipart/form-data content.
		/// Creates a new <see cref="MultipartFormDataContent"/> if the request has no content,
		/// or adds to the existing one if present. Can be called multiple times to add multiple fields.
		/// </summary>
		/// <typeparam name="T">The type of the object to serialize.</typeparam>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="fieldName">The form field name.</param>
		/// <param name="value">The object to serialize as JSON.</param>
		/// <param name="jsonSerializerOptions">The JSON serializer options.</param>
		/// <returns>The request with JSON content added.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the request already has content that is not <see cref="MultipartFormDataContent"/>.
		/// </exception>
		public static HttpRequestMessage AddJsonToMultipartFormData<T>(this HttpRequestMessage request, string fieldName, T value, JsonSerializerOptions jsonSerializerOptions) {
			var content = request.Content as MultipartFormDataContent;
			if (content == null) {
				if (request.Content != null) {
					throw new InvalidOperationException("Request content is not MultipartFormDataContent");
				} else {
					content = new MultipartFormDataContent();
					request.Content = content;
				}
			}
			var json = JsonSerializer.Serialize(value, jsonSerializerOptions);
			var jsonContent = new StringContent(json, Encoding.UTF8, ContentTypes.Json);
			content.Add(jsonContent, fieldName);
			return request;
		}
	}
}
