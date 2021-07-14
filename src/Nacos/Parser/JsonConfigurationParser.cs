using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Nacos
{
    /// <summary>
    /// Json <inheritdoc cref="IConfigurationParser"/>
    /// </summary>
    public class JsonConfigurationParser : IConfigurationParser
    {
        #region Private 字段

        private static readonly JsonReaderOptions s_jsonReaderOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        #endregion Private 字段

        #region Public 方法

        /// <inheritdoc/>
        public bool CanParse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return true;
            }

            return TryParseToJsonDocument(content, out _);
        }

        /// <inheritdoc/>
        public IDictionary<string, string?> Parse(string content)
        {
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (TryParseToJsonDocument(content, out var document)
                && document is not null)
            {
                VisitJsonElement(document.RootElement, string.Empty, data);
            }

            return data;
        }

        #endregion Public 方法

        #region Private 方法

        private static bool TryParseToJsonDocument(string content, out JsonDocument? document)
        {
            var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(content).AsSpan(), s_jsonReaderOptions);

            return JsonDocument.TryParseValue(ref utf8JsonReader, out document);
        }

        #region Visit

        private static string CombinePath(string path, string section)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return section;
            }
            return $"{path}:{section}";
        }

        private void VisitJsonElement(JsonElement element, string path, Dictionary<string, string?> container)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var childElement in element.EnumerateObject())
                    {
                        VisitJsonElement(childElement.Value, CombinePath(path, childElement.Name), container);
                    }
                    break;

                case JsonValueKind.Array:
                    int arrayIndex = 0;
                    foreach (var arrayItem in element.EnumerateArray())
                    {
                        VisitJsonElement(arrayItem, CombinePath(path, arrayIndex.ToString()), container);
                        arrayIndex++;
                    }
                    break;

                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    if (container.ContainsKey(path))
                    {
                        throw new FormatException($"There has duplicated key - {path} in a json document.");
                    }
                    container[path] = element.ToString();
                    break;

                case JsonValueKind.Undefined:
                default:
                    throw new FormatException($"Invalid JsonElement {path}");
            }
        }

        #endregion Visit

        #endregion Private 方法
    }
}