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
 * 	调试判断
 * 
 * 	name: Rule.IfDebug
 * 	create: 2025-03-14
 * 	memo: 调试判断，判断是否调试环境
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>调试判断</summary>
	public class IfDebug : IfRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "调试判断";

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Execute(FlowHelper.IsDebug(context), context, cancel);

		#endregion
	}
}