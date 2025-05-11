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
 * 	文本内容替换
 * 
 * 	name: Rule.TextReplace
 * 	create: 2025-03-17
 * 	memo: 文本内容替换
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>文本内容替换</summary>
	public class TextReplace : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "文本内容替换";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		/// <summary>替换部分</summary>
		public SSDictionary Replaces { get; set; }

		/// <summary>替换的 HTML 标签</summary>
		public string[] ClearTags { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (string.IsNullOrEmpty(Source)) {
				message = "未设置原始内容";
				return false;
			}

			if (Replaces.IsEmpty() && ClearTags.IsEmpty()) {
				message = "未设置替换内容";
				return false;
			}

			return true;
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 替换内容
			if (Replaces.NotEmpty()) {
				Replaces.ForEach((k, v) => Source = Source.Replace(k, v));
			}

			// 清理 HTML 标签
			if (ClearTags.NotEmpty()) {
				Source = Source.ClearHtml(ClearTags);
			}

			// 返回结果
			return Source;
		}

		#endregion
	}
}