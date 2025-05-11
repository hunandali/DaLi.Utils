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
 * 	当前时间
 * 
 * 	name: Rule.TimeNow
 * 	create: 2025-03-17
 * 	memo: 当前时间
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>当前时间</summary>
	public class TimeNow : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "当前时间";

		/// <summary>是否返回 UTC 时间</summary>
		public bool UTC { get; set; }

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 忽略错误
			ErrorIgnore = TristateEnum.TRUE;

			return UTC ? Main.DATE_NOW.ToUniversalTime() : Main.DATE_NOW;
		}

		#endregion
	}
}