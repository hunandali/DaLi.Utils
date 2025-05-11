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
 * 	验证当前是否调试环境
 * 
 * 	name: Rule.ValidateDebug
 * 	create: 2025-03-17
 * 	memo: 验证当前是否调试环境
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>验证当前是否调试环境</summary>
	public class ValidateDebug : ValidateRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "验证当前是否调试环境";

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Execute(FlowHelper.IsDebug(context), context);

		#endregion
	}
}