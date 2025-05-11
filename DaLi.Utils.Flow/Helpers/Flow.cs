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
			IEnumerable<SODictionary> rules,
			ref ExecuteStatus status,
			ref SODictionary context,
			SODictionary output = null,
			CancellationToken cancel = default) {
			status ??= new ExecuteStatus("流程", "FLOW");
			context ??= [];

			// 移除禁用规则
			var list = RuleList(rules);
			if (list.IsEmpty()) {
				status.SetStatus(ExceptionEnum.RULE_INVALID);
				return null;
			}

			// 移除注释项目
			list = [.. list.Where(x => !x.GetType().Name.Equals("comment", StringComparison.OrdinalIgnoreCase))];
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

			try {
				foreach (var item in list) {
					// 强制终止操作
					cancel.ThrowIfCancellationRequested();

					// 执行规则，一旦非有效执行结果通过异常跳出
					ExecuteStatus state = null;
					var res = item.Execute(ref state, ref context, cancel);

					// 记录日志
					status.Add(state);

					// 合并结果，存在 output 的不需要处理
					item.MergeResult(result, res);
				}

				if (output.NotEmpty() && context.NotEmpty()) {
					var data = context;
					output.Update((key, value) => GetValue(value, data));

					status.SetStatus(output);
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
			}

			// 返回结果
			return output ?? result;
		}

		/// <summary>执行操作</summary>
		/// <param name="flow">流程规则</param>
		/// <param name="context">初始输入参数，将合并流程中的输入参数；最终将返回整个流程的上下文数据</param>
		/// <param name="cancel">取消Token</param>
		public static ExecuteStatus FlowExecute(IFlow flow, ref SODictionary context, CancellationToken cancel = default) {
			if (flow == null) {
				return new ExecuteStatus().SetStatus(ExceptionEnum.FLOW_INVALID, "流程规则无效");
			}

			// 更新环境变量
			context ??= [];
			context.TryMerge(flow.Input);
			context.Add("_FLOW_", flow);
			UpdateEnvironment(context);

			// 创建状态
			var status = new ExecuteStatus(flow.Name, "FLOW", context);

			// 执行
			var s = Stopwatch.StartNew();
			var result = FlowExecute(flow.Rules, ref status, ref context, flow.Output, cancel);
			s.Stop();

			if (!status.Success) {
				return status;
			}

			// 全局数据合并
			result ??= [];
			result.Update("_FLOW_", new Dictionary<string, object> {
				["message"] = status.GetMessage(),
				["ticks"] = s.ElapsedTicks,
				["duration"] = s.ElapsedMilliseconds,
				["durationDisplay"] = s.Elapsed.Show(),
			}, true);

			status.SetStatus(result);

			// 返回结果
			return status;
		}

		/// <summary>执行操作</summary>
		/// <param name="flow">流程规则</param>
		/// <param name="context">初始输入参数，将合并流程中的输入参数；最终将返回整个流程的上下文数据</param>
		/// <param name="cancel">取消Token</param>
		public static ExecuteStatus FlowExecute(string flow, ref SODictionary context, CancellationToken cancel = default) => FlowExecute(flow.FromJson<IFlow>(), ref context, cancel);

	}
}