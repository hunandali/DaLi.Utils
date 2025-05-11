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
 * 	验证基类
 * 
 * 	name: ValidateRuleBase
 * 	create: 2025-03-17
 * 	memo: 验证基类
 * 
 * ------------------------------------------------------------
 */

using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Base {
	/// <summary>验证基类</summary>
	public abstract class ValidateRuleBase : FlowRuleBase {

		#region PROPERTY

		/// <summary>为匹配成功时输出的内容</summary>
		public string Success { get; set; }

		/// <summary>为匹配失败时输出的内容</summary>
		public string Fail { get; set; }

		/// <summary>触发异常。</summary>
		/// <remarks>
		/// True：匹配成功异常；<para />
		/// False：匹配失败异常；<para />
		/// Default：忽略，成功或失败都不触发</remarks>
		public TristateEnum Throw { get; set; }
		#endregion

		#region EXECUTE

		/// <summary>执行验证结果</summary>
		/// <param name="result">验证结果</param>
		/// <param name="context">上下文</param>
		/// <returns>返回结果</returns>
		protected object Execute(bool result, SODictionary context) {
			// 验证成功且触发成功异常
			FlowException.ThrowIf(result && Throw == TristateEnum.TRUE, ExceptionEnum.EXECUTE_ERROR, GetError(context, null, null, "值不能存在内容"));

			// 验证失败且触发失败异常
			FlowException.ThrowIf(!result && Throw == TristateEnum.FALSE, ExceptionEnum.EXECUTE_ERROR, GetError(context, null, null, "值必须存在内容"));

			// 输出结果
			if (result) {
				return Success.IsEmpty() ? result : FlowHelper.GetObjectValue(Success, context);
			} else {
				return Fail.IsEmpty() ? result : FlowHelper.GetObjectValue(Fail, context);
			}
		}

		#endregion
	}
}