/*
 * ------------------------------------------------------------
 * 
 * 	Copyright © 2021 湖南大沥网络科技有限公司.
 * 	Dali.Utils Is licensed under Mulan PSL v2.
 * 
 * 		  author:	木炭(WOODCOAL)
 * 		   email:	a@hndl.vip
 * 		homepage:	http://www.hunandali.com/
 * 
 * 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
 * 
 * ------------------------------------------------------------
 * 
 * 	流程操作
 * 
 * 	name: FlowHelper
 * 	create: 2025-03-14
 * 	memo: 流程操作
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Interface;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow {

	/// <summary>流程操作</summary>
	public static partial class FlowHelper {

		/// <summary>执行流程</summary>
		/// <param name="rules">规则列表</param>
		/// <param name="context">上下文数据</param>
		/// <param name="output">最终输出结果参数，不设置则输出流程中所有数据</param>
		/// <param name="status">消息状态</param>
		/// <param name="cancel">取消Token</param>
		/// <returns>最终执行结果</returns>
		public static SODictionary FlowExecute(
			IEnumerable<IRule> rules,
			ref ExecuteStatus status,
			ref SODictionary context,
			SODictionary output = null,
			CancellationToken cancel = default) {
			status ??= new ExecuteStatus("流程", "FLOW");
			context ??= [];

			// 移除禁用规则
			if (rules.IsEmpty()) {
				status.SetStatus(ExceptionEnum.RULE_INVALID);
				return null;
			}

			// 移除注释项目
			var list = rules.Where(x => !x.GetType().Name.Equals("comment", StringComparison.OrdinalIgnoreCase));
			if (list.IsEmpty()) {
				status.SetStatus(ExceptionEnum.RULE_INVALID);
				return null;
			}

			// 非调试模式移除调试模块
			var debug = IsDebug(context);
			if (!debug) {
				list = [.. list.Where(x => !x.GetType().Name.Equals("debug", StringComparison.OrdinalIgnoreCase))];

				if (list.IsEmpty()) {
					status.SetStatus(ExceptionEnum.RULE_INVALID);
					return null;
				}
			}

			// 输出结果
			var result = new SODictionary();

			// 执行规则，一旦非有效执行结果通过异常跳出
			ExecuteStatus state = null;

			try {
				foreach (var item in list) {
					// 强制终止操作
					cancel.ThrowIfCancellationRequested();

					// 执行规则，一旦非有效执行结果通过异常跳出
					state = null;
					var res = item.Execute(ref state, ref context, cancel);

					// 合并结果，存在 output 的不需要处理
					item.MergeResult(result, res);

					// 记录日志
					status.Add(state);

					// 清除状态数据
					state = null;
				}

				if (output.NotEmpty() && context.NotEmpty()) {
					var data = context;

					// 结果赋值，防止修改 output 原始内容，只输出 output 中设定的数据
					result.Clear();
					output.ForEach((key, value) => result[key] = GetValue(value, data));
					status.SetStatus(result);
				} else {
					status.SetStatus(context);
				}
			} catch (FlowException ex) {
				// 继续触发异常，非完整流程保持异常，以便上级捕获
				status.SetStatus(ex.Status, $"流程中断 {ex.Message}");

			} catch (OperationCanceledException) {
				// 中断操作，不拦截
				status.SetStatus(ExceptionEnum.EXECUTE_ERROR, "流程完成 已经强制结束");

			} catch (Exception ex) {
				// 其他异常，记录标记错误
				status.SetStatus(ExceptionEnum.INNER_EXCEPTION, $"流程异常 {ex.Message}");
			} finally {
				// 记录日志
				status.Add(state);
			}

			// 返回结果
			return result;
		}

		/// <summary>执行流程</summary>
		/// <param name="rules">规则列表</param>
		/// <param name="context">上下文数据</param>
		/// <param name="output">最终输出结果参数，不设置则输出流程中所有数据</param>
		/// <param name="status">消息状态</param>
		/// <param name="cancel">取消Token</param>
		/// <returns>最终执行结果</returns>
		public static SODictionary FlowExecute(
			IEnumerable<RuleData> rules,
			ref ExecuteStatus status,
			ref SODictionary context,
			SODictionary output = null,
			CancellationToken cancel = default) => FlowExecute(RuleList(rules, true, false, true), ref status, ref context, output, cancel);

		/// <summary>执行操作</summary>
		/// <param name="flow">流程规则</param>
		/// <param name="context">初始输入参数，将合并流程中的输入参数；最终将返回整个流程的上下文数据</param>
		/// <param name="cancel">取消Token</param>
		public static ExecuteStatus FlowExecute(IFlow flow, ref SODictionary context, CancellationToken cancel = default) {
			if (flow is null) {
				return new ExecuteStatus().SetStatus(ExceptionEnum.FLOW_INVALID, "流程规则无效");
			}

			// 更新环境变量
			context ??= [];
			context.TryMerge(flow.Input);
			context.Add("_FLOW_", flow);
			UpdateEnvironment(ref context);

			// 调试模式
			SetDebug(context, flow.Debug);

			// 创建流程状态
			var status = new ExecuteStatus(flow.Name, "FLOW", context);

			// 执行
			var s = Stopwatch.StartNew();
			var result = FlowExecute(flow.Rules, ref status, ref context, flow.Output, cancel);
			s.Stop();

			// 全局数据合并
			result ??= [];
			result.Update("_FLOW_", new Dictionary<string, object> {
				["message"] = status.GetMessage(),
				["ticks"] = s.ElapsedTicks,
				["duration"] = s.ElapsedMilliseconds,
				["durationDisplay"] = s.Elapsed.Show(),
			}, true);

			// 更新 flow 结果
			flow.Status = status.Success;
			if (status.Success) {
				var template = flow.Output.GetValue("result");
				flow.Message = GetStringValue(template, result);
			} else {
				flow.Message = status.ExceptionMessage ?? status.Exception.Description();
			}
			flow.Output = result;

			//if (!status.Success) {
			//	return status;
			//}

			status.Output = result;
			status.TimeFinish = Main.DATE_NOW;

			// 不适用用 SetStatus，防止覆盖结果，原始状态数据可能已经记录了异常信息
			//status.SetStatus(result);

			// 返回结果
			return status;
		}

		/// <summary>执行操作</summary>
		/// <param name="flow">流程规则</param>
		/// <param name="context">初始输入参数，将合并流程中的输入参数；最终将返回整个流程的上下文数据</param>
		/// <param name="cancel">取消Token</param>
		public static ExecuteStatus FlowExecute(string flow, ref SODictionary context, CancellationToken cancel = default) => FlowExecute(flow.FromJson<Model.Flow>(), ref context, cancel);

	}
}