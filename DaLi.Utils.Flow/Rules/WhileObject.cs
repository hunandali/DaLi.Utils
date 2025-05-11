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
 * 	按值循环
 * 
 * 	name: DaLi.Utils.Flow.Rules.WhileObject
 * 	create: 2025-03-17
 * 	memo: 按值循环（对象或者集合）
 * 
 * ------------------------------------------------------------
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>按值循环</summary>
	public class WhileObject : WhileRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "按值循环";

		/// <summary>原始 JSON 数据（对应变量值必须为数组 或者 对象）</summary>
		public object Source { get; set; }

		/// <summary>忽略前几条</summary>
		public int Skip { get; set; }

		/// <summary>处理多少条，默认处理全部</summary>
		public int Count { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Source is null) {
				message = "原始数据无效";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 先将数据转换成有效的类型
			var data = Source is string str ? str : Source.ToJson();
			var obj = data.FromJson();

			if (Skip < 1) { Skip = 0; }
			if (Count < 1) { Count = int.MaxValue; }
			if (Count > int.MaxValue - Skip) { Count = int.MaxValue - Skip; }

			var min = Skip;
			var max = Skip + Count - 1;

			// 按字典处理
			if (obj is Dictionary<string, object> dict) {
				var keys = dict.Keys.ToArray();
				if (max >= dict.Count) { max = dict.Count - 1; }

				return WhileExecute(min, max, 1, context, idx => {
					var key = keys[idx];
					var item = dict[key];

					return new SODictionary { { "_index", key }, { "_item", item } };
				}, cancel);
			}

			// 按列表处理
			if (obj is List<object> list) {
				if (max >= list.Count) { max = list.Count - 1; }

				return WhileExecute(min, max, 1, context, idx => new SODictionary { { "_item", list[idx] } }, cancel);
			}

			// 无效值
			FlowException.Throw(ExceptionEnum.RULE_INPUT_INVALID);
			return null;
		}

		#endregion
	}
}