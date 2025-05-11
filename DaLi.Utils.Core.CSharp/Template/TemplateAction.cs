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
 *  模板操作
 * 
 * 	name: TemplateAction
 * 	create: 2025-03-07
 * 	memo: 模板操作，可扩展的模板操作系统，支持动态注册和执行各种模板转换操作。默认 UTF-8 字符集
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DaLi.Utils.Extension;
using DaLi.Utils.Helper;
using DaLi.Utils.Model;

namespace DaLi.Utils.Template {

	/// <summary>模板操作事件处理</summary>
	public class TemplateAction(Encoding charset = null) {

		/// <summary>默认字符编码，用于文本处理</summary>
		public readonly Encoding Charset = charset ?? Encoding.UTF8;

		/// <summary>存储所有注册的模板操作</summary>
		private readonly StringDictionary<Func<object, SSDictionary, object>> _Instance = [];

		/// <summary>注册一个新的模板操作</summary>
		/// <param name="command">操作命令名称</param>
		/// <param name="execute">执行该操作的委托函数</param>
		/// <param name="replace">是否替换已存在的同名操作</param>
		/// <returns>注册是否成功</returns>
		/// <remarks>
		/// 操作命令会被自动转换为小写以确保大小写不敏感
		/// 如果操作已存在且 replace 为 false，则注册失败
		/// </remarks>
		public bool Register(string command, Func<object, SSDictionary, object> execute, bool replace = false) {
			if (string.IsNullOrEmpty(command) || execute == null) {
				return false;
			}

			// 尝试强制添加
			if (_Instance.Add(command, execute)) { return true; }

			// 添加失败表示已经存在，如果不强制替换则返回失败
			if (!replace) { return false; }

			// 更新数据
			return _Instance.Update(command, execute);
		}

		/// <summary>注册一个新的模板操作</summary>
		/// <param name="callerName"></param>
		/// <param name="execute">执行该操作的委托函数</param>
		/// <param name="replace">是否替换已存在的同名操作</param>
		/// <returns>注册是否成功</returns>
		/// <remarks>
		/// 操作命令会被自动转换为小写以确保大小写不敏感
		/// 如果操作已存在且 replace 为 false，则注册失败
		/// </remarks>
		public bool Register(Func<object, SSDictionary, object> execute, bool replace = false, [CallerArgumentExpression(nameof(execute))] string callerName = null) {
			if (string.IsNullOrEmpty(callerName) || execute == null) { return false; }

			var path = callerName.LastIndexOf('.');
			callerName = path >= 0 ? callerName[(path + 1)..] : callerName;

			// 一般不会出现函数名前后有 . 的情况
			//callerName = path >= 0 && path < callerName.Length ? callerName[(path + 1)..] : callerName.Replace('.', '-');

			return Register(callerName, execute, replace);
		}

		/// <summary>注销一个模板操作</summary>
		/// <param name="command">要注销的操作命令名称</param>
		/// <returns>注销是否成功</returns>
		public bool Unregister(string command) {
			if (string.IsNullOrEmpty(command)) {
				return false;
			}

			return _Instance.Remove(command);
		}

		/// <summary>执行模板操作</summary>
		/// <param name="source">原始内容</param>
		/// <param name="attributes">属性元组序列</param>
		/// <returns>处理后的内容</returns>
		/// <remarks>
		/// 该方法支持层级化的属性处理，例如：
		/// command.subkey=value 将被解析为command操作的子属性
		/// </remarks>
		public object Execute(object source, IEnumerable<KeyValuePair<string, string>> attributes) {
			if (attributes is null || !attributes.Any()) { return source; }

			// 处理属性
			var attrs = attributes.ToList();
			for (var i = 0; i < attrs.Count; i++) {
				var (key, value) = attrs[i];

				// 键为空跳过
				if (string.IsNullOrEmpty(key)) { continue; }

				// 跳过包含点号的键（子属性将在后续处理）
				if (key.Contains('.')) { continue; }

				// 未注册的操作，跳过
				if (!_Instance.TryGet(key, out var command)) { continue; }

				// 创建属性字典并添加主属性
				var dict = new SSDictionary() { { key, value } };

				// 处理子属性
				var prefix = $"{key}.";
				var prefixLength = prefix.Length;

				// 收集相关的子属性
				for (var j = i + 1; j < attrs.Count; j++) {
					var (nextKey, nextValue) = attrs[j];
					if (nextKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
						dict.Add(nextKey[prefixLength..], nextValue);
						i++;
					} else {
						break;
					}
				}

				// 执行操作
				source = command.Invoke(source, dict);
			}

			return source;
		}

		/// <summary>获取所有已注册的操作命令</summary>
		/// <returns>操作命令集合</returns>
		public IEnumerable<string> Keys() => _Instance.Keys;

		#region 模板实例

		/// <summary>默认实例</summary>
		private static readonly Lazy<TemplateAction> _Default = new(() => {
			var instance = new TemplateAction(Encoding.UTF8);
			TemplateCommands.Register(instance);
			return instance;
		});

		/// <summary>默认实例</summary>
		public static TemplateAction Default => _Default.Value;

		#endregion

		#region 模板分析

		/// <summary>字典数据缓存，用于存储扁平化对象的结果</summary>
		private static readonly ConditionalWeakTable<IDictionary<string, object>, IDictionary<string, object>> _Cache = [];

		/// <summary>使用数据字典格式化模板，默认使用 {} 前后缀</summary>
		/// <param name="template">模板文本</param>
		/// <param name="data">数据字典</param>
		/// <param name="clearTag">对于字典中不存在的标签是否清除</param>
		/// <returns>格式化后的结果</returns>
		public object FormatTemplate(string template, IDictionary<string, object> data, bool clearTag = false) => FormatTemplate(template, data, '{', '}', clearTag, false);

		/// <summary>使用数据字典格式化模板</summary>
		/// <param name="template">模板文本</param>
		/// <param name="data">数据字典</param>
		/// <param name="prefix">前缀字符</param>
		/// <param name="suffix">后缀字符</param>
		/// <param name="clearTag">对于字典中不存在的标签是否清除</param>
		/// <param name="skipAttribute">是否跳过标签属性的处理</param>
		/// <returns>格式化后的结果</returns>
		public object FormatTemplate(string template, IDictionary<string, object> data, char prefix = '{', char suffix = '}', bool clearTag = false, bool skipAttribute = false) {
			// 模板或数据为空不处理
			if (string.IsNullOrEmpty(template) || data.IsEmpty()) { return template; }

			// 如果不存在前后缀，则返回原始内容
			if (!template.Contains(prefix) || !template.Contains(suffix)) { return template; }

			// 提取模板中的所有标签
			var extractor = new TagExtractor(prefix, suffix);
			var tags = extractor.ExtractTags(template);
			if (tags.IsEmpty()) { return template; }

			// 获取扁平化的数据字典
			if (!_Cache.TryGetValue(data, out var dict)) {
				dict = data.ToFlatDictionary(StringComparer.OrdinalIgnoreCase);
				_Cache.Add(data, dict);
			}

			// 对于只有一个标签的模板，且模板由前后缀包围，则直接处理并返回
			if (template.StartsWith(prefix) && template.EndsWith(suffix) && tags.Count == 1) {
				var tag = tags[0];

				// 获取标签名对应的数据值，返回原始内容
				if (!dict.TryGetValue(tag.Name, out var value)) { return clearTag ? null : template; }

				// 应用格式化函数
				value = skipAttribute || tag.Attributes.IsEmpty() ? value : Execute(value, tag.Attributes);

				// 直接返回
				return value;
			}

			// 创建结果字符串构建器
			var result = new StringBuilder(template);

			// 获取标签排序，从后向前排序，避免位置偏移问题
			var tagOrder = tags.OrderByDescending(t => template.IndexOf(t.OriginalContent));

			// 从后向前替换，避免位置偏移问题
			foreach (var tag in tagOrder) {
				// 获取标签名对应的数据值
				// 如果数据字典中没有对应的值，保留原标签
				if (!dict.TryGetValue(tag.Name, out var value)) {
					// 不清除不存在的标签，则直接下一轮
					if (!clearTag) { continue; }
				} else {
					// 应用格式化函数
					value = skipAttribute || tag.Attributes.IsEmpty() ? value : Execute(value, tag.Attributes);
				}

				// 在结果中替换标签
				while (true) {
					var startIndex = result.ToString().IndexOf(tag.OriginalContent);
					if (startIndex < 0) { break; }

					result.Remove(startIndex, tag.OriginalContent.Length);
					result.Insert(startIndex, value?.ToString());
				}
			}

			// 反转义后输出
			return extractor.RemoveEscapeChars(result.ToString(), false);
		}

		#endregion

	}
}
