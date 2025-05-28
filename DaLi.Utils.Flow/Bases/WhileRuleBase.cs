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
 * 	循环基类
 * 
 * 	name: WhileRuleBase
 * 	create: 2025-03-15
 * 	memo: 循环基类
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Base {
	/// <summary>循环基类</summary>
	public abstract class WhileRuleBase : RuleBase {

		#region PROPERTY

		/// <summary>循环结果值，如果存在此变量名，则使用变量值，否则使用文本</summary>
		public string Result { get; set; }

		/// <summary>求和字段，循环中用于累积求和的变量或者文本</summary>
		public string Sum { get; set; }

		/// <summary>内部执行的规则</summary>
		public List<RuleData> Rules { get; set; }

		/// <summary>单线程或多线程并行执行，小于 2 单线程，大于 1 异步并行执行线程数</summary>
		public int ParallelNumber { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Rules.IsEmpty()) {
				message = "没有用于循环执行的内部规则";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>执行循环操作</summary>
		/// <param name="min">最小值</param>
		/// <param name="max">最大值</param>
		/// <param name="step">迭代之间的增量</param>
		/// <param name="context">上下文中数据</param>
		/// <param name="action">获取循环参数</param>
		/// <param name="cancel">取消令牌</param>
		protected SODictionary WhileExecute(int min, int max, int step, SODictionary context, Func<int, SODictionary> action, CancellationToken cancel) {
			FlowException.ThrowNull(action, ExceptionEnum.EXECUTE_ERROR, "无效的循环迭代参数");
			FlowException.ThrowIf(min > max, ExceptionEnum.EXECUTE_ERROR, "最大值必须大于等于最小值");
			FlowException.ThrowIf(step == 0, ExceptionEnum.EXECUTE_ERROR, "进度不能为 0");

			// 周期小于 0，倒序
			if (step < 0 && max > min) {
				(max, min) = (min, max);
			}

			// 非安全数组重建
			context ??= [];

			// 所有需要处理的索引值
			var indexs = new List<int>();
			for (var idx = min; idx <= max; idx += step) {
				indexs.Add(idx);
			}

			// 无有效索引列表，返回
			if (indexs.Count == 0) {
				return null;
			}

			return WhileExecute(indexs, context, idx => {
				var loopData = action(idx) ?? new Dictionary<string, object>();
				if (!loopData.ContainsKey("_min")) {
					loopData.Add("_min", min);
				}

				if (!loopData.ContainsKey("_max")) {
					loopData.Add("_max", max);
				}

				if (!loopData.ContainsKey("_count")) {
					loopData.Add("_count", 0);
				}

				if (!loopData.ContainsKey("_interval")) {
					loopData.Add("_interval", step);
				}

				return (SODictionary) loopData;
			}, cancel);
		}

		/// <summary>执行循环操作</summary>
		/// <param name="indexs">所有需要迭代的索引值</param>
		/// <param name="context">上下文中数据</param>
		/// <param name="indexAction">获取循环参数</param>
		/// <param name="cancel">取消令牌</param>
		/// <returns>执行结果</returns>
		protected SODictionary WhileExecute(IEnumerable<int> indexs, SODictionary context, Func<int, SODictionary> indexAction, CancellationToken cancel) {
			FlowException.ThrowIf(indexs.IsEmpty(), ExceptionEnum.EXECUTE_ERROR, "没有循环的索引值");
			FlowException.ThrowNull(indexAction, ExceptionEnum.EXECUTE_ERROR, "无效的循环迭代参数");

			var ret = new ConcurrentBag<object>();
			var retIndex = new ConcurrentBag<object>();
			var retSum = new ConcurrentBag<float>();

			// 执行循环内规则，并返回是否强制中断（True 执行正常，False 强制中断）
			bool Execute(int index) {
				var executeMessage = new ExecuteStatus();
				var success = true;

				try {
					// 获取当前循环变量
					var loopData = indexAction(index) ?? [];

					// 注意此索引不一定是序列 Integer，可能是来自对象的 Key
					if (!loopData.TryGetValue("_index", out var _index)) {
						_index = index;
						loopData.Add("_index", index);
					}

					// 附加参数
					var exchange = new SODictionary(context);
					exchange.TryMerge(loopData);

					// 执行内部规则
					var data = FlowHelper.FlowExecute(Rules, ref executeMessage, ref exchange, null, cancel);

					// 更新结果到全局变量
					if (data.NotEmpty()) {
						// 排除所有下划线变量
						var keys = data.Keys.Where(x => x.StartsWith('_')).ToArray();
						data.Remove(keys);

						context.TryMerge(data);
					}

					// 处理值结果
					if (Result.NotEmpty()) {
						var loopValue = FlowHelper.GetValue(Result, exchange);
						if (loopValue != null) {
							ret.Add(loopValue);
						}
					}

					// 处理求和
					if (Sum.NotEmpty()) {
						var loopSum = FlowHelper.GetStringValue(Sum, exchange, true).ToSingle(true);
						if (loopSum != 0) {
							retSum.Add(loopSum);
						}
					}

					// 记录索引
					retIndex.Add(_index);

				} catch (FlowException ex) {
					executeMessage.SetStatus(ex.Status, ex.Message);

					// 出现强制退出循环，标识执行失败
					success = ex.Status != ExceptionEnum.LOOP_STOP;
				}

				// 执行消息结果记录到循环体
				RuleStatus.Add(executeMessage);

				// 返回是否强制中断
				return success;
			}

			//---------------------
			// 执行循环操作
			//---------------------

			// 小于 2 单线程，否则并行多线程处理循环
			if (ParallelNumber < 2) {
				foreach (var idx in indexs) {
					// LOOP_STOP 强制终止循环
					if (!Execute(idx)) {
						break;
					}
				}
			} else {
				// Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
				var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Math.Min(ParallelNumber, 100)) };
				Parallel.ForEach(indexs, options, (idx, state) => {
					// LOOP_STOP 强制终止循环
					if (!Execute(idx)) {
						state.Stop();
					}
				});
			}

			var result = new SODictionary {
				{ "count", ret.Count },
				{ "run", retIndex.Count },
				{ "context", ret },
				{ "sum", retSum.Sum() },
				{ "index", retIndex }
			};

			RuleStatus.SetStatus(result);
			return result;
		}

		#endregion
	}
}