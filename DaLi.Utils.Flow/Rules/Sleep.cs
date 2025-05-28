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
 * 	延时
 * 
 * 	name: Rule.Sleep
 * 	create: 2025-03-17
 * 	memo: 延时
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>延时</summary>
	public class Sleep : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "延时";

		/// <summary>延时时长，单位：秒</summary>
		public int Length { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Length < 0) {
				message = "延时时长不能小于 0 ";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 强制忽略错误
			SkipResult();

			for (var i = 1; i <= Length; i++) {
				cancel.ThrowIfCancellationRequested();
				Thread.Sleep(1000);
			}

			return null;
		}

		#endregion
	}
}