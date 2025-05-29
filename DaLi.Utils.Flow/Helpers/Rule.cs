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
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Helper;

namespace DaLi.Utils.Flow {

	/// <summary>流程操作</summary>
	public static partial class FlowHelper {

		/// <summary>当前系统中的规则类型</summary>
		private static ImmutableDictionary<string, Type> _Rules = null;

		/// <summary>当前系统中的规则类型</summary>
		public static ImmutableDictionary<string, Type> Rules {
			get {
				_Rules ??= ReflectionHeler
					.CurrentTypes(true, null, true, typeof(IRule))
					.Where(x => x.IsPublic && x.IsClass && !x.IsAbstract)
					.ToImmutableDictionary(
						x => x.Name,
						x => x,
						StringComparer.OrdinalIgnoreCase
					);

				return _Rules;
			}
			set => _Rules = value ?? ImmutableDictionary<string, Type>.Empty;
		}

		/// <summary>从输入参数中获取规则</summary>
		public static IRule RuleItem(RuleData input) {
			if (input.IsEmpty()) {
				return null;
			}

			var typeName = input.Type;
			if (typeName.IsEmpty()) {
				return null;
			}

			// 获取实际规则类型
			if (!Rules.TryGetValue(typeName, out var type)) {
				// 尝试使用短名称匹配（即：规则类型不包含命名空间的类型）
				if (typeName.Contains('.')) {
					type = Rules
						.Where(x => x.Value.FullName.EndsWith(typeName, StringComparison.OrdinalIgnoreCase))
						.Select(x => x.Value)
						.FirstOrDefault();
				}
			}

			// 没有分析到类型
			if (type == null) {
				// 代理运行模式，返回代理规则
				if (ProxyMode) {
					return new FlowProxy { Source = input };
				}
				return null;
			}

			// 反序列为实际规则
			var rule = (IRule) Activator.CreateInstance(type);
			if (rule == null) {
				return null;
			}

			// 序列化后再保存
			rule.SetInput(input);
			return rule;
		}

		/// <summary>获取规则</summary>
		public static IRule RuleItem(string rule)
			=> RuleItem(new RuleData(rule));

		/// <summary>从流程规则数据中获取规则</summary>
		/// <param name="rules">规则列表</param>
		public static List<IRule> RuleList(IEnumerable<RuleData> rules) {
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
	}
}