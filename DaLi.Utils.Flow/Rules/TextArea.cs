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
 * 	区域文本截取
 * 
 * 	name: Rule.TextArea
 * 	create: 2025-03-17
 * 	memo: 区域文本截取
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Linq;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>区域文本截取</summary>
	public class TextArea : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "截取文本区域";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		/// <summary>开始区域</summary>
		public string AreaBegin { get; set; }

		/// <summary>结束区域</summary>
		public string AreaEnd { get; set; }

		/// <summary>是否包含起始区域</summary>
		public bool IncBegin { get; set; }

		/// <summary>是否包含结束区域</summary>
		public bool IncEnd { get; set; }

		/// <summary>如果需要返回多段内容，则此处为多段内容的间隔符号</summary>
		public string Separator { get; set; }

		/// <summary>清除单元格中的标签</summary>
		public string[] ClearTags { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (string.IsNullOrEmpty(Source)) {
				message = "未设置原始内容";
				return false;
			}

			if (string.IsNullOrEmpty(AreaBegin) && string.IsNullOrEmpty(AreaEnd)) {
				message = "未设置区域范围";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 区域切割
			var isMuti = !string.IsNullOrEmpty(Separator);
			var area = Source.Cut(AreaBegin, AreaEnd, isMuti, true);
			FlowException.ThrowIf(area is null, ExceptionEnum.EXECUTE_ERROR, "未分析到任何有效内容");

			// 处理内容
			var result = "";
			if (isMuti) {
				result = ((string[]) area).Select(x => {
					if (!IncBegin && x.StartsWith(AreaBegin, StringComparison.OrdinalIgnoreCase)) { x = x[AreaBegin.Length..]; }
					if (!IncEnd && x.EndsWith(AreaEnd, StringComparison.OrdinalIgnoreCase)) { x = x[..^AreaEnd.Length]; }
					return x;
				}).JoinString(Separator);
			} else {
				result = (string) area;
				if (!IncBegin && result.StartsWith(AreaBegin, StringComparison.OrdinalIgnoreCase)) { result = result[AreaBegin.Length..]; }
				if (!IncEnd && result.EndsWith(AreaEnd, StringComparison.OrdinalIgnoreCase)) { result = result[..^AreaEnd.Length]; }
			}

			// 清理 Html
			if (ClearTags.NotEmpty()) {
				result = result.ClearHtml(ClearTags);
			}

			return result;
		}

		#endregion
	}
}