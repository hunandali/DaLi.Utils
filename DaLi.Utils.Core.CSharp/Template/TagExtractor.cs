/* ------------------------------------------------------------
 * 
 * 	Copyright © 2021 湖南大沥网络科技有限公司.
 * 	Dali.Utils Is licensed under Mulan PSL v2.
 * 
 * 		  author:	木炭(WOODCOAL)
 * 		   email:	i@woodcoal.cn
 * 		homepage:	http://www.hunandali.com/
 * 
 * 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
 * 
 * ------------------------------------------------------------
 * 
 *  标签提取器，用于从文本中提取和解析标签
 * 
 * 	name: TagExtractor
 * 	create: 2025-03-08
 * 	memo: 标签提取器，用于从文本中提取和解析标签。能够正确提取标签、解析属性、处理转义字符和嵌套标签。
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DaLi.Utils.Template {

	/// <summary>标签提取器，用于从文本中提取和解析标签</summary>
	/// <param name="prefix">标签前缀，默认为'{' 字符</param>
	/// <param name="suffix">标签后缀，默认为'}' 字符</param>
	/// <param name="escapeChar">转义字符，默认为'\' 字符</param>
	public class TagExtractor(char prefix = '{', char suffix = '}', char escapeChar = '\\') {

		/// <summary>标签前缀，默认为'{' 字符</summary>
		private readonly char _Prefix = prefix;

		/// <summary>标签后缀，默认为'}' 字符</summary>
		private readonly char _Suffix = suffix;

		/// <summary>转义字符，默认为'\' 字符</summary>
		private readonly char _EscapeChar = escapeChar;

		/// <summary>从文本中提取所有符合条件的标签</summary>
		/// <param name="text">要分析的文本</param>
		/// <returns>提取的标签列表</returns>
		public List<TagInfo> ExtractTags(string text) {
			if (string.IsNullOrEmpty(text)) {
				return [];
			}

			var tags = new List<TagInfo>();
			var uniqueTags = new HashSet<string>();
			var startIndex = 0;

			// 用于存储嵌套标签的栈，确保内部标签先被处理
			var nestedTagsStack = new Stack<Tuple<string, int>>(); // 内容和优先级
			var currentDepth = 0;

			while (startIndex < text.Length) {
				// 查找标签开始位置
				var tagStart = -1;

				for (var i = startIndex; i < text.Length; i++) {
					// 检查是否为转义字符
					if (i < text.Length - 1 && text[i] == _EscapeChar && (text[i + 1] == _Prefix || text[i + 1] == _Suffix)) {
						i++;

						// 跳过转义字符和被转义的字符
						continue;
					}

					if (text[i] == _Prefix) {
						tagStart = i;
						break;
					}
				}

				if (tagStart == -1) {
					break;
				}

				// 查找标签结束位置
				var tagEnd = -1;
				var nestedLevel = 0;
				for (var i = tagStart + 1; i < text.Length; i++) {
					// 检查是否为转义字符
					if (i < text.Length - 1 && text[i] == _EscapeChar && (text[i + 1] == _Prefix || text[i + 1] == _Suffix)) {
						i++;

						// 跳过转义字符和被转义的字符
						continue;
					}

					// 检测嵌套的开始标签
					if (text[i] == _Prefix) {
						nestedLevel++;
					}

					// 检测嵌套的结束标签
					else if (text[i] == _Suffix) {
						if (nestedLevel > 0) {
							nestedLevel--;
						} else {
							tagEnd = i;
							break;
						}
					}
				}

				if (tagEnd == -1) {
					break;
				}

				// 提取标签内容
				var contentStart = tagStart + 1;
				var contentLength = tagEnd - contentStart;
				if (contentLength <= 0) {
					startIndex = tagEnd + 1;
					continue;
				}

				// 获取原始内容用于解析当前标签
				var originalContent = text.Substring(contentStart, contentLength);
				var tagContent = RemoveEscapeChars(originalContent).Trim();
				var originalTag = text.Substring(tagStart, tagEnd - tagStart + 1);

				// 解析当前标签
				var tagInfo = ParseTag(tagContent);
				if (tagInfo != null) {
					tagInfo.OriginalContent = originalTag;
					tagInfo.Hash = CalculateHash(originalTag);
					currentDepth++;

					// 将当前标签加入栈中，以便后续按正确顺序添加
					nestedTagsStack.Push(new Tuple<string, int>(originalContent, currentDepth));
				}

				startIndex = tagEnd + 1;
			}

			// 处理嵌套标签栈，确保内部标签先被处理
			// 将栈中的元素按照深度排序，确保内部标签先于外部标签处理
			var orderedTags = nestedTagsStack.OrderByDescending(t => t.Item2).ToList();
			foreach (var tagData in orderedTags) {
				var content = tagData.Item1;

				// 解析标签内容
				var tagContent = RemoveEscapeChars(content).Trim();
				var tagInfo = ParseTag(tagContent);
				if (tagInfo != null) {
					var originalTag = _Prefix + content + _Suffix;
					tagInfo.OriginalContent = originalTag;
					tagInfo.Hash = CalculateHash(originalTag);

					// 去重处理
					if (uniqueTags.Add(tagInfo.Hash)) {
						tags.Add(tagInfo);
					}

					// 递归处理可能的嵌套标签
					var innerTags = ExtractTags(content);
					foreach (var innerTag in innerTags) {
						if (uniqueTags.Add(innerTag.Hash)) {
							tags.Add(innerTag);
						}
					}
				}
			}

			return tags;
		}

		/// <summary>移除字符串中的转义字符</summary>
		/// <param name="text">包含转义字符的文本</param>
		/// <param name="removeAll">是否处理所有转义字符，不仅限于前缀、后缀和转义字符本身</param>
		/// <returns>移除转义字符后的文本</returns>
		public string RemoveEscapeChars(string text, bool removeAll = true) {
			if (string.IsNullOrEmpty(text)) {
				return text;
			}

			// 如果字符串中不包含转义字符，直接返回
			if (!text.Contains($"{_EscapeChar}{_Prefix}")) { return text; }

			var sb = new StringBuilder(text.Length);
			for (var i = 0; i < text.Length; i++) {
				// 处理转义字符
				if (i < text.Length - 1 && text[i] == _EscapeChar) {
					if (removeAll || text[i + 1] == _Prefix || text[i + 1] == _Suffix || text[i + 1] == _EscapeChar) {
						// 处理所有转义字符，不仅限于前缀、后缀和转义字符本身
						sb.Append(text[i + 1]);
						i++;

						continue;
					}
				}

				sb.Append(text[i]);
			}
			return sb.ToString();
		}

		/// <summary>从文本中提取指定名称的标签</summary>
		/// <param name="text">要分析的文本</param>
		/// <param name="tagName">标签名称</param>
		/// <returns>提取的标签列表</returns>
		public List<TagInfo> ExtractTagsByName(string text, string tagName) {
			if (string.IsNullOrEmpty(tagName)) {
				throw new ArgumentException("标签名称不能为空", nameof(tagName));
			}

			return [.. ExtractTags(text).Where(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase))];
		}

		/// <summary>计算内容的哈希值</summary>
		private static string CalculateHash(string content) {
			if (string.IsNullOrEmpty(content)) {
				return string.Empty;
			}
			var bytes = Encoding.UTF8.GetBytes(content);
			var hash = SHA256.HashData(bytes);
			return Convert.ToBase64String(hash);
		}

		/// <summary>解析标签内容，提取标签名和属性</summary>
		private TagInfo ParseTag(string tagContent) {
			if (string.IsNullOrEmpty(tagContent)) {
				return null;
			}

			var tagInfo = new TagInfo();
			var index = 0;

			// 跳过前导空白
			while (index < tagContent.Length && char.IsWhiteSpace(tagContent[index])) {
				index++;
			}

			// 提取标签名
			var nameStart = index;
			while (index < tagContent.Length && IsValidNameChar(tagContent[index])) {
				index++;
			}

			// 没有有效的标签名
			if (index == nameStart) {
				return null;
			}

			tagInfo.Name = tagContent[nameStart..index];

			// 解析属性
			while (index < tagContent.Length) {
				// 跳过空白
				while (index < tagContent.Length && char.IsWhiteSpace(tagContent[index])) {
					index++;
				}

				if (index >= tagContent.Length) {
					break;
				}

				// 提取属性名
				var attrNameStart = index;
				while (index < tagContent.Length && IsValidNameChar(tagContent[index])) {
					index++;
				}

				// 无效的属性名或已到达字符串末尾
				if (index == attrNameStart || index >= tagContent.Length) {
					break;
				}

				var attrName = tagContent[attrNameStart..index];

				// 跳过空白和等号
				while (index < tagContent.Length && (char.IsWhiteSpace(tagContent[index]) || tagContent[index] == '=')) {
					index++;
				}

				if (index >= tagContent.Length || (char.IsWhiteSpace(tagContent[index - 1]) && tagContent[index - 1] != '=')) {
					// 属性没有值，添加空值
					tagInfo.Attributes.Add(new KeyValuePair<string, string>(attrName, string.Empty));
					continue;
				}

				// 提取属性值
				string attrValue;
				var quoteChar = tagContent[index];

				// 引号包围的值
				if (quoteChar is '"' or '\'') {
					index++;

					// 跳过开始引号
					var valueStart = index;
					var escaped = false;

					while (index < tagContent.Length) {
						if (!escaped && tagContent[index] == quoteChar) {
							break;
						}

						// 处理转义字符
						if (!escaped && tagContent[index] == _EscapeChar) {
							escaped = true;
						} else {
							escaped = false;
						}

						index++;
					}

					// 没有找到结束引号
					if (index >= tagContent.Length) {
						attrValue = tagContent[valueStart..];
					} else {
						attrValue = tagContent[valueStart..index];

						// 跳过结束引号
						index++;
					}

					// 处理属性值中的转义字符
					attrValue = RemoveEscapeChars(attrValue);
				} else {
					// 无引号的值
					var valueStart = index;

					while (index < tagContent.Length && !char.IsWhiteSpace(tagContent[index])) {
						index++;
					}

					attrValue = tagContent[valueStart..index];
				}

				tagInfo.Attributes.Add(new KeyValuePair<string, string>(attrName, attrValue));
			}

			return tagInfo;
		}

		/// <summary>检查字符是否为有效的标签名或属性名字符，支持点号，移除冒号，以及方括号</summary>
		private static bool IsValidNameChar(char c) => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == '[' || c == ']';

		/// <summary>将标签信息转换为标签字符串</summary>
		/// <param name="tagInfo">标签信息</param>
		/// <returns>标签字符串</returns>
		public string TagInfoToString(TagInfo tagInfo) {
			if (tagInfo == null || string.IsNullOrEmpty(tagInfo.Name)) {
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.Append(_Prefix).Append(tagInfo.Name);

			foreach (var attr in tagInfo.Attributes) {
				sb.Append(' ').Append(attr.Key);

				if (!string.IsNullOrEmpty(attr.Value)) {
					sb.Append('=').Append('"').Append(attr.Value).Append('"');
				}
			}

			sb.Append(_Suffix);
			return sb.ToString();
		}

		/// <summary>对文本中的特殊字符进行转义</summary>
		/// <param name="text">需要转义的文本</param>
		/// <returns>转义后的文本</returns>
		public string EscapeText(string text) {
			if (string.IsNullOrEmpty(text)) {
				return text;
			}

			var sb = new StringBuilder(text.Length * 2);
			for (var i = 0; i < text.Length; i++) {
				if (text[i] == _Prefix || text[i] == _Suffix || text[i] == _EscapeChar) {
					sb.Append(_EscapeChar);
				}
				sb.Append(text[i]);
			}
			return sb.ToString();
		}
	}
}
