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
 *  流程操作
 * 
 * 	name: FlowHelper
 * 	create: 2025-03-14
 * 	memo: 流程操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Interface;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow {

	/// <summary>流程操作</summary>
	public static partial class FlowHelper {

		/// <summary>当前系统中的规则类型</summary>
		private static ImmutableDictionary<string, Type> _Rules = ImmutableDictionary<string, Type>.Empty;

		/// <summary>当前系统中的规则类型</summary>
		public static ImmutableDictionary<string, Type> Rules {
			get {
				_Rules ??= ReflectionHeler
						.CurrentTypes(true, null, true, typeof(IFlowRule))
						.Where(x => x.IsPublic && x.IsClass && !x.IsAbstract)
						.ToImmutableDictionary(
							x => x.FullName,
							x => x,
							StringComparer.OrdinalIgnoreCase
						);
				return _Rules;
			}
			set => _Rules = value ?? ImmutableDictionary<string, Type>.Empty;
		}

		/// <summary>从输入参数中获取规则</summary>
		public static IFlowRule RuleItem(SODictionary input) {
			if (input.IsEmpty()) {
				return null;
			}

			var typeName = input.GetValue("_type", "type");
			if (typeName.IsEmpty()) {
				return null;
			}

			// 获取实际规则类型
			if (!Rules.TryGetValue(typeName, out var type)) {
				// 尝试使用短名称匹配（即：规则类型不包含命名空间的类型）
				if (!typeName.Contains('.')) {
					typeName = $".{typeName}";
					type = Rules
						.Where(x => x.Key.EndsWith(typeName, StringComparison.OrdinalIgnoreCase))
						.Select(x => x.Value)
						.FirstOrDefault();
				}
			}

			// 没有分析到类型
			if (type == null) {
				// 代理运行模式，返回代理规则
				if (ProxyMode) {
					return new FlowProxy { Rule = input.ToJson(false, false, false) };
				}
				return null;
			}

			// 反序列为实际规则
			var rule = (IFlowRule) Activator.CreateInstance(type);
			if (rule == null) {
				return null;
			}

			rule.SetInput(input);
			return rule;
		}

		/// <summary>获取规则</summary>
		public static IFlowRule RuleItem(string rule)
			=> RuleItem(new SODictionary(rule));

		/// <summary>从流程规则数据中获取规则</summary>
		/// <param name="rules">规则列表</param>
		public static List<IFlowRule> RuleList(IEnumerable<SODictionary> rules) {
			if (rules.IsEmpty()) {
				return null;
			}

			// 获取规则，如果存在任一无效规则，则此规则列表无效
			var ret = rules.Select(RuleItem).ToList();
			if (ret.Any(x => x == null)) {
				return null;
			}

			return ret;
		}

		///// <summary>执行操作</summary>
		//public static ExecuteStatus RuleExecute(SODictionary input, ref SODictionary context, CancellationToken cancel = default) {
		//	var item = RuleItem(input);
		//	FlowException.ThrowIf(item is null, ExceptionEnum.RULE_INVALID);

		//	var result = new ExecuteStatus(item, input);
		//	item.Execute(result, ref context, cancel);
		//	return result;
		//}
	}
}