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
 *  操作状态
 * 
 * 	name: FlowStatus
 * 	create: 2025-03-14
 * 	memo: 操作状态
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Text;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Interface;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow {

	/// <summary>操作状态</summary>
	public class ExecuteStatus {

		/// <summary>规则名称</summary>
		public string Name { get; private set; }

		/// <summary>规则类型</summary>
		public string Type { get; private set; }

		/// <summary>是否成功</summary>
		public bool Success { get; private set; }

		/// <summary>失败状态</summary>
		public ExceptionEnum Exception { get; private set; }

		/// <summary>失败消息</summary>
		public string ExceptionMessage { get; set; }

		/// <summary>开始操作时间</summary>
		public DateTime TimeStart { get; set; }

		/// <summary>结束操作时间</summary>
		public DateTime TimeFinish { get; set; }

		/// <summary>输入参数</summary>
		public SODictionary Input { get; set; }

		/// <summary>日志</summary>
		public Dictionary<DateTime, string> Logs { get; }

		/// <summary>输出结果</summary>
		public object Output { get; set; }

		/// <summary>子消息（循环操作相关消息）</summary>
		public List<ExecuteStatus> Children { get; } = [];

		/// <summary>默认构造</summary>
		public ExecuteStatus() {
			Success = false;
			Exception = ExceptionEnum.NORMAL;
			TimeStart = DateTime.Now;
			Name = "";
			Type = "";
			Input = null;
			Logs = [];
		}

		/// <summary>默认构造</summary>
		public ExecuteStatus(SODictionary input = null) : this() {
			Input = input;
			if (input.IsEmpty()) { return; }

			Name = Input.GetValue("_name", "name");
			Type = Input.GetValue("_type", "type");
		}

		/// <summary>默认构造</summary>
		public ExecuteStatus(IFlowRule rule, SODictionary input = null) : this() {
			Input = input;
			if (rule == null) { return; }

			Name = rule.Name;
			Type = rule.GetType().FullName;
		}

		/// <summary>默认构造</summary>
		public ExecuteStatus(string name, string type, SODictionary input = null) : this() {
			Input = input;
			Name = name;
			Type = type;
		}

		/// <summary>设置消息状态</summary>
		/// <param name="success">是否执行成功</param>
		/// <param name="message">消息内容，失败时如果消息未设置则不处理，成功时如果未设置则清空原始消息</param>
		public ExecuteStatus SetStatus(bool success, string message = null) {
			TimeFinish = DateTime.Now;
			Success = success;
			Exception = ExceptionEnum.NORMAL;
			ExceptionMessage = message;
			Output = null;

			return this;
		}

		/// <summary>设置消息状态</summary>
		/// <param name="status">执行状态</param>
		/// <param name="message">消息内容，失败时如果消息未设置则不处理，成功时如果未设置则清空原始消息</param>
		public ExecuteStatus SetStatus(ExceptionEnum status, string message = null) {
			TimeFinish = DateTime.Now;
			Success = false;
			Exception = status;
			ExceptionMessage = message;
			Output = null;

			return this;
		}

		/// <summary>设置消息状态</summary>
		/// <param name="output">输出结果</param>
		public ExecuteStatus SetStatus(object output) {
			TimeFinish = DateTime.Now;
			Success = true;
			Exception = ExceptionEnum.NORMAL;
			ExceptionMessage = "";
			Output = output;

			return this;
		}

		/// <summary>复制规则</summary>
		public void Add(ExecuteStatus msg) {
			if (msg == null) { return; }

			lock (Children) {
				Children.Add(msg);
			}
		}

		/// <summary>更新类型</summary>
		public void Update(string name, string type) {
			Name = name;
			Type = type;
		}

		/// <summary>更新规则</summary>
		public void Update(ExecuteStatus msg) {
			if (msg == null) {
				return;
			}

			Name = msg.Name;
			Type = msg.Type;
			Success = msg.Success;
			Exception = msg.Exception;
			TimeStart = msg.TimeStart;
			TimeFinish = msg.TimeFinish;
			ExceptionMessage = msg.ExceptionMessage;
			Input = msg.Input;
			Output = msg.Output;

			lock (Children) {
				Children.Clear();
				Children.AddRange(msg.Children);
			}
		}

		/// <summary>获取结果列表</summary>
		public string GetMessage(int level = 0) {
			var sb = new StringBuilder();
			sb.Append($"[{TimeFinish:HH:mm:ss}] ");

			ExceptionMessage ??= "";

			if (ExceptionMessage.StartsWith("调试")) {
				sb.Append("👽 ");
			} else if (Success) {
				sb.Append("😊 ");
			} else {
				// 获取枚举描述
				var description = Exception.Description();
				sb.Append($"😈 [{description}] ");
			}

			if (string.IsNullOrEmpty(Name)) {
				sb.Append(Type);
			} else {
				sb.Append(Name);
				if (!string.IsNullOrEmpty(Type)) {
					sb.Append($"({Type})");
				}
			}

			sb.Append($"：{ExceptionMessage}{Environment.NewLine}");

			lock (Children) {
				level += 1;
				foreach (var msg in Children) {
					sb.Append(new string('\t', level));
					sb.Append(msg.GetMessage(level));
					sb.Append(Environment.NewLine);
				}
			}

			return sb.ToString();
		}

		///// <summary>更新最终流程结果</summary>
		///// <param name="data">流程中用于获取值的参数</param>
		///// <remarks>
		///// 请最终流程执行完成时使用，其他情况无需使用。<para />
		///// 当前操作将原始的 Output 结果作为上下文数据，然后根据获取值的参数来分析最终结果<para />
		///// </remarks>
		//public ExecuteStatus UpdateFinishResult(SODictionary data) {
		//	// 缓存上下文
		//	Context = (SODictionary) Output;

		//	// 获取结果
		//	if (Output.NotEmpty() && data.NotEmpty()) {
		//		data.Update((key, value) => FlowHelper.GetObjectValue(value, Output));
		//	}

		//	// 最终输出结果
		//	Output = data;

		//	return this;
		//}
	}
}