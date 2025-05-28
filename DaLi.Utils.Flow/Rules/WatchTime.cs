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
 * 	计时器
 * 
 * 	name: Rule.FlowTime
 * 	create: 2025-03-14
 * 	memo: 流程执行计时器
 * 	
 * ------------------------------------------------------------
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>流程执行计时器</summary>
	public class WatchTime : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "流程计时器";

		/// <summary>内部执行的规则</summary>
		public List<RuleData> Rules { get; set; }

		/// <inheritdoc/>	
		public override bool OutputAll => true;

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Rules.IsEmpty()) {
				message = "没有用于执行的内部规则";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			Log("计时启动");

			var status = RuleStatus;
			var s = Stopwatch.StartNew();
			var result = FlowHelper.FlowExecute(Rules, ref status, ref context, null, cancel);
			s.Stop();

			RuleStatus = status;

			var desc = s.Elapsed.Show();
			Log($"计时结束，共耗时 {desc}");

			// 检查执行状态
			FlowException.ThrowStatus(status);

			// 全局数据合并
			result ??= [];
			result.Update(new Dictionary<string, object> {
				["ticks"] = s.ElapsedTicks,
				["duration"] = s.ElapsedMilliseconds,
				["durationDisplay"] = desc,
			}, true);

			return result;
		}

		#endregion
	}
}