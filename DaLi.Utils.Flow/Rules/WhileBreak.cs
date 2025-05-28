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
 * 	循环中断
 * 
 * 	name: Rule.WhileBreak
 * 	create: 2025-03-17
 * 	memo: 循环中断
 * 
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>循环中断</summary>
	public class WhileBreak : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "循环中断";

		/// <summary>中断还是退出</summary>
		public bool Stop { get; set; }

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 无返回内容不报错
			ErrorIgnore = TristateEnum.FALSE;
			EmptyIgnore = true;
			Output = null;

			throw new FlowException(Stop ? ExceptionEnum.LOOP_STOP : ExceptionEnum.LOOP_BREAK);
		}

		#endregion
	}
}