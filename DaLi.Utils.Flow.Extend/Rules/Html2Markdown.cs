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
 * 	Source 转 Markdown
 * 
 * 	name: Rule.Html2Markdown
 * 	create: 2025-05-27
 * 	memo: Source 转 Markdown
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>缓存数据读取</summary>
	public class Html2Markdown : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "Html 转 Markdown";

		/// <summary>要转换的 Source 内容</summary>
		public string Source { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "要转换的 Html 内容未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>转换器</summary>
		private static ReverseMarkdown.Converter _Converter;

		/// <summary>转换器</summary>
		protected static ReverseMarkdown.Converter Converter {
			get {
				_Converter ??= new ReverseMarkdown.Converter();
				return _Converter;
			}
		}

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			Source = Source.ClearHtml("comment", "style", "script", "doctype");
			return Converter.Convert(Source);
		}

		#endregion

	}
}