/*
 * ------------------------------------------------------------
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
 * 	HTML 解析
 * 
 * 	name: Rule.HtmlParser
 * 	create: 2025-05-27
 * 	memo: HTML 解析
 * 	
 * ------------------------------------------------------------
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>HTML 解析</summary>
	public class HtmlParser : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "HTML 解析";

		/// <summary>Html 内容</summary>
		public string Source { get; set; }

		/// <summary>查询路径，如果需要连续查询多个则用回车间隔，不存在则不处理，使用整个 Source 内容</summary>
		public string MainPath { get; set; }

		/// <summary>是否返回多条记录</summary>
		public bool Multi { get; set; }

		/// <summary>返回对象的名称路径集合，使用查询获取内部 html，不存在则不处理</summary>
		public SSDictionary DataPath { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "要解析的内容不能为空";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>解析主文档</summary>
		private List<IElement> ParseMain() {
			// 1. 解析 HTML
			var parser = new AngleSharp.Html.Parser.HtmlParser();
			var doc = parser.ParseDocument(Source);
			FlowException.ThrowNull(doc, ExceptionEnum.VALUE_VALIDATE, "无法解析原始数据");

			// 2. 获取主路径
			// 未设置则使用整个区域
			if (MainPath.IsEmpty()) {
				return [doc.DocumentElement];
			}

			var els = doc.DocumentElement.QuerySelectorAll(MainPath);
			return Multi ? [.. els] : [.. els.Take(1)];
		}

		/// <summary>获取元素属性</summary>
		private static Dictionary<string, string> ElementAttributes(IElement el) {
			if (el is null) {
				return null;
			}

			var attrs = el.Attributes.ToDictionary(attr => attr.Name, attr => attr.Value);
			attrs.TryAdd("_tag", el.TagName);
			attrs.TryAdd("_text", el.TextContent);
			attrs.TryAdd("_html", el.OuterHtml);
			attrs.TryAdd("_content", el.InnerHtml);

			return attrs;
		}

		/// <summary>解析元素</summary>
		/// <param name="el">元素</param>
		private object ParseElement(IElement el) {
			// 无数据路径获取
			if (DataPath.IsEmpty()) {
				return ElementAttributes(el);
			}

			// 存在数据获取
			var ret = new SODictionary();

			DataPath.ForEach((key, value) => {
				el = el.QuerySelector(value);
				if (el != null) {
					ret.Add(key, ElementAttributes(el));
				}
			});

			return ret;
		}

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var els = ParseMain();
			FlowException.ThrowIf(els.IsEmpty(), ExceptionEnum.VALUE_VALIDATE, "无法解析指定内容");

			var rets = els.Select(ParseElement).Where(item => item != null);
			return Multi ? rets : rets?.FirstOrDefault();
		}

		#endregion
	}
}