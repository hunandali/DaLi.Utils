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
 * 	输出异常
 * 
 * 	name: Rule.Exception
 * 	create: 2025-03-14
 * 	memo: 输出异常
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>输出异常</summary>
	public class Exception : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "输出异常";

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 无结果输出
			SkipResult();

			// 强制输出错误
			ErrorIgnore = TristateEnum.FALSE;

			FlowException.Throw(ExceptionEnum.EXECUTE_ERROR, ErrorMessage);

			return null;
		}

		#endregion
	}
}