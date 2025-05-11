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
 *  规则基类
 * 
 * 	name: FlowRuleBase
 * 	create: 2025-03-14
 * 	memo: 规则基类
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Interface;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Base {

	/// <summary>规则基类</summary>
	public abstract class FlowRuleBase : IFlowRule {

		/// <summary>是否已经销毁</summary>
		private bool _IsDisposed;

		/// <inheritdoc/>
		public abstract string Name { get; }

		/// <inheritdoc/>
		public string Output { get; set; }

		/// <inheritdoc/>
		public virtual bool OutputAll => false;

		/// <inheritdoc/>
		public bool EmptyIgnore { get; set; }

		/// <inheritdoc/>
		public TristateEnum ErrorIgnore { get; set; }

		/// <inheritdoc/>
		public string ErrorMessage { get; set; }

		/// <inheritdoc/>
		public bool Enabled { get; set; }

		/// <summary>输入参数</summary>
		public SODictionary Input { get; private set; }

		/// <summary>状态</summary>
		protected ExecuteStatus RuleStatus { get; set; }

		/// <inheritdoc/>
		public void SetInput(SODictionary input) => Input = input;

		/// <inheritdoc/>
		public virtual bool Validate(ref string message) {
			message = null;
			return true;
		}

		/// <summary>执行操作，并返回当前的变量及相关值</summary>
		/// <param name="context">上下文数据，未设置将自动创建，并自动附加最终结果</param>
		/// <param name="cancel">取消令牌</param>
		/// <remarks>如果执行过程存在错误则直接触发异常</remarks>
		/// <returns>执行结果</returns>
		protected abstract object Execute(SODictionary context, CancellationToken cancel);

		/// <inheritdoc/>
		public object Execute(ref ExecuteStatus status, ref SODictionary context, CancellationToken cancel) {
			// status 参数检查
			status ??= new ExecuteStatus(Name, this.GetTypeName());
			status.Input ??= Input;

			// 状态项目
			RuleStatus = status;

			// 参数处理并验证
			context ??= [];
			var message = "";
			var flag = UpdateInput(status.Input, context, ref message);
			FlowException.ThrowIf(flag.HasValue, flag.Value, message);

			// 取消检查
			cancel.ThrowIfCancellationRequested();

			// 输出变量
			object result = null;

			// 执行操作
			try {
				result = Execute(context, cancel);

				// 合并结果
				result = MergeResult(context, result);
			} catch (FlowException ex) {
				// 内部自定义错误
				status.SetStatus(ex.Status, $"规则中断：{ex.Message}");

				// 抛出异常，且不能忽略
				throw;
			} catch (Exception ex) {
				// 忽略错误
				if (ErrorIgnore != TristateEnum.FALSE) {
					status.SetStatus(true, GetError(context, ex, "[错误已忽略]"));
				} else {
					var err = GetError(context, ex);
					status.SetStatus(ExceptionEnum.INNER_EXCEPTION, err);
					FlowException.Throw(ex, err);
				}
			}

			return result;
		}

		/// <summary>获取格式化的错误信息</summary>
		protected string GetError(SODictionary context, Exception ex = null, string prefix = "", string defaultMessage = null) {
			if (!string.IsNullOrEmpty(prefix)) {
				prefix = $"[{prefix}] ";
			}

			if (!string.IsNullOrEmpty(ErrorMessage)) {
				return prefix + ErrorMessage.FormatTemplate(context, true);
			}

			if (ex != null) {
				return prefix + ex.Message;
			}

			return defaultMessage ?? "执行异常";
		}

		/// <summary>通过上下文数据更新输入值</summary>
		/// <param name="context">上下文</param>
		/// <param name="message">错误消息</param>
		/// <returns>null 更新成功，否则返回失败信息</returns>
		public ExceptionEnum? UpdateInput(SODictionary context, ref string message) => UpdateInput(Input, context, ref message);

		/// <summary>通过上下文数据更新输入值</summary>
		/// <param name="input">输入参数</param>
		/// <param name="context">上下文</param>
		/// <param name="message">错误消息</param>
		/// <returns>null 更新成功，否则返回失败信息</returns>
		public ExceptionEnum? UpdateInput(SODictionary input, SODictionary context, ref string message) {
			message = null;

			// 检查是否存在需要更新的属性
			var type = GetType();
			var hasInput = type.GetAllProperties().Any(pro => pro.CanRead && pro.CanWrite && pro.IsPublic());

			// 如果存在需要更新的属性，且没有传入参数则异常，否则忽略
			if (input.IsEmpty()) {
				if (hasInput) {
					message = "缺少输入数据";
					return ExceptionEnum.RULE_INPUT_INVALID;
				}

				return null;
			}

			// 更新输入值
			try {
				foreach (var kv in input) {
					var prop = type.GetSingleProperty(kv.Key);
					if (prop != null && prop.CanWrite) {
						var value = FlowHelper.GetValue(kv.Value, context, prop.PropertyType);
						prop.SetValue(this, value);
					}
				}
			} catch (Exception ex) {
				message = $"输入数据转换异常：{ex.Message}";
				return ExceptionEnum.RULE_INPUT_INVALID;
			}

			if (!Enabled) {
				return ExceptionEnum.RULE_DISABLED;
			}

			// 参数验证
			return Validate(ref message) ? null : ExceptionEnum.RULE_VALIDATE;
		}

		/// <inheritdoc/>
		public object MergeResult(SODictionary context, object result) {
			// 如果需要暴露结果，则将忽略输出变量名
			if (OutputAll) {
				Output = null;

				// 必须为字典数据
				FlowException.ThrowIf(result is not SODictionary, ExceptionEnum.EXECUTE_ERROR, "结果必须为字典数据");

				// 保留子流程结果
				context.TryMerge((SODictionary) result);
			} else if (Output.IsEmpty()) {
				// 不需要输出，直接返回结果
				result = null;
			} else {
				// 需要输出，且不存在结果，且不忽略空值则抛出异常
				FlowException.ThrowIf(result is null && !EmptyIgnore, ExceptionEnum.NO_RESULT);

				// 直接合并结果
				context.TryMerge(new Dictionary<string, object> { { Output, result } });
			}

			return result;
		}

		/// <summary>当前规则无任何值输出，强制忽略错误</summary>
		protected void SkipResult() {
			Output = null;
			EmptyIgnore = true;
		}

		/// <summary>设置日志</summary>
		/// <param name="message">消息</param>
		/// <param name="prefix">前缀</param>
		protected void Log(string message, string prefix = "") {
			if (prefix.NotEmpty()) {
				prefix = $"[{prefix}] ";
			}

			RuleStatus.Logs.TryAdd(Main.DATE_NOW, $"{prefix}{message}");
		}

		/// <inheritdoc/>
		public virtual object Clone() => MemberwiseClone();

		/// <summary>销毁</summary>
		protected virtual void Close() {
		}

		/// <inheritdoc/>
		public void Dispose() {
			if (!_IsDisposed) {
				Close();
				_IsDisposed = true;
			}

			GC.SuppressFinalize(this);
		}
	}
}