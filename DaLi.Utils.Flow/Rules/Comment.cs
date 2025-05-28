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
 * 	注释
 * 
 * 	name: Rule.Comment
 * 	create: 2025-03-14
 * 	memo: 注释，用于分割项目，注释项目，无其他作用
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>注释</summary>
	public class Comment : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "备注";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "备注内容不能为空";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			SkipResult();
			return null;
		}

		#endregion
	}
}