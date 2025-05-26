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
 *  流程操作
 * 
 * 	name: FlowHelper
 * 	create: 2025-03-14
 * 	memo: 流程操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Data;
using System.Reflection;
using System.Text;
using DaLi.Utils.Auto;
using DaLi.Utils.Extension;
using DaLi.Utils.Helper;
using DaLi.Utils.Http;
using DaLi.Utils.Json;
using DaLi.Utils.Misc.SnowFlake;
using DaLi.Utils.Model;
using DaLi.Utils.Template;
using static DaLi.Utils.Extension.TemplateExtension;

namespace DaLi.Utils.Flow {

	/// <summary>流程操作</summary>
	public static partial class FlowHelper {

		#region 默认模板处理实例

		/// <summary>默认模板处理实例</summary>
		private static readonly Lazy<TemplateAction> _Default = new(() => {
			var instance = new TemplateAction(Encoding.UTF8);
			TemplateCommands.Register(instance);
			ExtensionCommands.Register(instance);
			return instance;
		});

		/// <summary>默认模板处理实例</summary>
		public static TemplateAction Default => _Default.Value;

		#endregion

		#region 环境变量

		/// <summary>获取基础数据</summary>
		/// <remarks>
		/// 返回系统环境变量和基础配置数据
		/// 可通过重写此属性来扩展或自定义环境数据
		/// </remarks>
		public static SODictionary Environment {
			get {
				var ass = Assembly.GetEntryAssembly();
				var company = ass.Company().EmptyValue("大沥网络");

				return new SODictionary {
					{ "powerby", $"<a href=\"https://www.hunandali.com/\">{company}</a>" },
					{"url", "https://www.hunandali.com/"},
					{"company", company},
					{"copyright", ass.Copyright().EmptyValue($"©{Main.DATE_NOW.Year} 大沥网络")},
					{"software", ass.Product().EmptyValue(ass.Title(), ass.Name())},
					{"description", ass.Description()},
					{"version", ass.Version()},
					{"start", Main.SYS_START},
					{"date", Main.DATE_NOW},
					{"guid", Guid.NewGuid().ToString()},
					{"rnd", SnowFlakeHelper.JsID()}
				};
			}
		}

		/// <summary>更新上下文中的环境变量</summary>
		/// <param name="context">上下文数据</param>
		public static SODictionary UpdateEnvironment(SODictionary context) {
			context ??= [];
			context.Add("_SYS_", Environment);
			return context;
		}

		/// <summary>获取当前是否为调试模式</summary>
		public static bool IsDebug(SODictionary context) => context.NotEmpty() && context.GetValue("_DEBUG_", false);

		/// <summary>设置调试模式</summary>
		public static void SetDebug(SODictionary context, bool value) => context?.Update("_DEBUG_", value, true);

		/// <summary>代理模式，当本机无此规则时，使用远程代理运行方式</summary>
		public static bool ProxyMode { get; set; }

		/// <summary>代理模式，当本机无此规则时，使用远程代理运行方式</summary>
		public static Func<ApiClient> ProxyClient { get => RuleProxy.GetApiClient; set => RuleProxy.GetApiClient = value; }

		#endregion

		#region 变量数据处理

		/// <summary>获取变量数据，主要用于值转换</summary>
		/// <param name="source"></param>
		/// <param name="context">上下文数据</param>
		/// <remarks>
		/// 变量使用 {} 包含，原始内容如果存在 {} 则使用 \{ \} 转义。
		/// 1. 原始值不包含 {} 则直接返回原始值；
		/// 2. 原始值以 { 开头 } 结尾则包含内容为键名，直接从字典中获取值；取不到则返回原始内容；
		/// 3. 如果内容中存在 {} 包含内容，则查询后替换
		/// </remarks>
		/// <returns>格式化后的结果</returns>
		public static object GetValue(object source, SODictionary context) {
			if (source is string template) {
				return Default.FormatTemplate(template, context, true);
			}

			return source;
		}

		/// <summary>获取变量数据并转换成指定类型</summary>
		/// <param name="source">原始值（变量名、模板）</param>
		/// <param name="context">数据</param>
		/// <param name="baseType">要转换的类型</param>
		public static object GetValue(object source, SODictionary context, Type baseType) {
			var value = GetValue(source, context);
			if (value is null) {
				return null;
			}

			// 文本类型直接返回
			if (baseType == typeof(string) && value is string) {
				return value;
			}

			// 对于非字典或者集合数据，直接转换
			if (!baseType.IsDictionary() && !baseType.IsCollection() && !baseType.IsEnumerable()) {
				return ConvertHelper.ChangeObject(value, baseType);
			}

			// 转换字典或者集合，并处理每项值
			var ret = value.ToString().FromJson(true, x => Default.FormatTemplate(x, context, true));
			return ConvertHelper.ChangeObject(ret, baseType);
		}

		/// <summary>获取变量数据并转换成指定类型</summary>
		/// <param name="source">原始值（变量名、模板）</param>
		/// <param name="context">数据</param>
		public static T GetValue<T>(object source, SODictionary context) =>
			(T) GetValue(source, context, typeof(T));

		/// <summary>获取变量数据并转换成文本</summary>
		/// <param name="source">原始值（变量名、模板）</param>
		/// <param name="context">数据</param>
		/// <param name="math">是否将获取的值进行一次计算，计算失败则返回原值</param>
		public static string GetStringValue(object source, SODictionary context, bool math = false) {
			var value = GetValue(source, context);
			if (value is null) {
				return string.Empty;
			}

			var ret = value.ToString();
			if (math) {
				try {
					ret = new DataTable().Compute(ret, null).ToString();
				} catch { }
			}

			return ret;
		}

		/// <summary>获取变量数据并转换成对象，主要用类型转换</summary>
		/// <param name="source">原始值（变量名、模板）</param>
		/// <param name="context">数据</param>
		/// <remarks>
		/// 与 GetValue 相比，当前如果获取到的结果非 Object，则尝试转换成 JSON 对象或者集合后再一次处理
		/// 使用 JSON 反序列时仍然会对每一个文本项目做一次 GetValue 处理
		/// </remarks>
		public static object GetObjectValue(object source, SODictionary context) {
			var value = GetValue(source, context);
			if (value is null) {
				return null;
			}

			if (value is not string) {
				return value;
			}

			// 对于文本尝试转换成 JSON
			var ret = value.ToString().FromJson(true, x => Default.FormatTemplate(x, context, true));
			return ret ?? value;
		}

		#endregion

		#region 返回值处理

		/// <summary>简单结果，仅一个返回值</summary>
		public static SODictionary SimpleResult(object value) => value is null ? null : new() { { "result", value } };

		#endregion

	}
}