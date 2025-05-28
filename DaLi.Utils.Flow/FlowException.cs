/* ------------------------------------------------------------
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
 *  流程异常
 * 
 * 	name: FlowException
 * 	create: 2025-03-14
 * 	memo: 流程异常
 * 
 * ------------------------------------------------------------
 */

using System;
using DaLi.Utils.Extension;

namespace DaLi.Utils.Flow {

	/// <summary>流程异常</summary>
	public class FlowException : Exception {

		/// <summary>异常状态</summary>
		public ExceptionEnum Status { get; }

		/// <summary>初始化异常</summary>
		/// <param name="status">异常类型</param>
		/// <param name="message">异常消息</param>
		public FlowException(ExceptionEnum status, string message = null) : base(message ?? status.Description()) => Status = status;

		/// <summary>初始化异常</summary>
		/// <param name="exception">异常</param>
		/// <param name="message">异常消息</param>
		public FlowException(Exception exception, string message) : base(message, exception) => Status = ExceptionEnum.INNER_EXCEPTION;

		/// <summary>抛出异常</summary>
		/// <param name="status">异常类型</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void Throw(ExceptionEnum status, string message = null) => throw new FlowException(status, message);

		/// <summary>抛出异常</summary>
		/// <param name="exception">异常</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void Throw(Exception exception, string message = null) => throw new FlowException(exception, message);

		/// <summary>根据执行状态抛出异常，不成功则抛出</summary>
		/// <param name="status">执行状态</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void ThrowStatus(ExecuteStatus status) {
			if (!status.Success) {
				throw new FlowException(status.Exception, status.ExceptionMessage);
			}
		}

		/// <summary>根据条件抛出异常</summary>
		/// <param name="yes">true 将抛出异常</param>
		/// <param name="status">异常类型</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void ThrowIf(bool yes, ExceptionEnum status, string message = null) {
			if (yes) {
				throw new FlowException(status, message);
			}
		}

		/// <summary>根据条件抛出异常</summary>
		/// <param name="yes">true 将抛出异常</param>
		/// <param name="exception">异常</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void ThrowIf(bool yes, Exception exception, string message = null) {
			if (yes) {
				throw new FlowException(exception, message);
			}
		}
		/// <summary>数据为 null 抛出异常</summary>
		/// <param name="source">用于校验的数据</param>
		/// <param name="status">异常类型</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void ThrowNull(object source, ExceptionEnum status, string message = null) {
			if (source is null) {
				throw new FlowException(status, message);
			}
		}

		/// <summary>数据为 null 抛出异常</summary>
		/// <param name="source">用于校验的数据</param>
		/// <param name="exception">异常</param>
		/// <param name="message">异常消息</param>
		/// <exception cref="FlowException">流程异常</exception>
		public static void ThrowNull(object source, Exception exception, string message = null) {
			if (source is null) {
				throw new FlowException(exception, message);
			}
		}
	}
}