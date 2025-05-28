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
 * 	包含内容判断
 * 
 * 	name: Rule.IfInclude
 * 	create: 2025-03-17
 * 	memo: 包含内容判断
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>内容判断</summary>
	public class IfInclude : IfRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "包含内容判断";

		/// <summary>原始数据</summary>
		public string Source { get; set; }

		/// <summary>用于判断包含的值</summary>
		public string Value { get; set; }

		/// <summary>需要包含全部还是包含任意一项</summary>
		/// <remarks>True 全部，False 任意一项</remarks>
		public bool All { get; set; }

		/// <summary>忽略大笑写</summary>
		public bool IngnoreCase { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Value.IsEmpty()) {
				message = "用于判断是否包含的值不能为空";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var targets = Value.SplitDistinct(["\n", "\t"]);
			FlowException.ThrowIf(targets.IsEmpty(), ExceptionEnum.RULE_INPUT_INVALID);

			var flag = Source.IsLike(targets, All, IngnoreCase);
			return Execute(flag, context, cancel);
		}

		#endregion
	}
}