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
 * 	判断基类
 * 
 * 	name: IfRuleBase
 * 	create: 2025-03-17
 * 	memo: 判断基类
 * 
 * ------------------------------------------------------------
 */

using System.Collections.Generic;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Base {
	/// <summary>判断基类</summary>
	public abstract class IfRuleBase : RuleBase {

		#region PROPERTY

		/// <summary>为匹配成功时输出的内容</summary>
		public string Success { get; set; }

		/// <summary>为匹配失败时输出的内容</summary>
		public string Fail { get; set; }

		/// <summary>存在生效还是不存在生效（True 不存在值是执行子规则，False 存在值时执行子规则）</summary>
		public bool Reverse { get; set; }

		/// <summary>内部执行的规则</summary>
		public List<RuleData> Rules { get; set; }
		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Rules.IsEmpty()) {
				message = "没有用于判断后执行的内部规则";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>执行判断结果</summary>
		/// <param name="result">判断结果</param>
		/// <param name="context">上下文</param>
		/// <param name="cancel">取消</param>
		/// <returns>返回结果</returns>
		protected object Execute(bool result, SODictionary context, CancellationToken cancel) {
			if (result == Reverse) {
				return Fail.IsEmpty() ? result : FlowHelper.GetObjectValue(Fail, context);
			}

			var status = RuleStatus;
			var data = new SODictionary(context);
			FlowHelper.FlowExecute(Rules, ref status, ref data, null, cancel);
			RuleStatus = status;

			return Success.IsEmpty() ? result : FlowHelper.GetObjectValue(Success, data);
		}

		#endregion
	}
}