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
 * 	文本转数组
 * 
 * 	name: Rule.TextArray
 * 	create: 2025-03-17
 * 	memo: 文本转数组
 * 	
 * ------------------------------------------------------------
 */

using System.Linq;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>文本转数组</summary>
	public class TextArray : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "文本转数组";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		/// <summary>字符分隔符</summary>
		public string Separator { get; set; }

		/// <summary>是否过滤重复内容</summary>
		public bool Distinct { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (string.IsNullOrEmpty(Source)) {
				message = "未设置原始内容";
				return false;
			}

			if (string.IsNullOrEmpty(Separator)) {
				message = "未设置分隔符";
				return false;
			}

			return true;
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 分割文本
			var items = Source.SplitEx(Separator);

			// 过滤重复
			if (Distinct) {
				items = [.. items.Distinct()];
			}

			return items;
		}

		#endregion
	}
}