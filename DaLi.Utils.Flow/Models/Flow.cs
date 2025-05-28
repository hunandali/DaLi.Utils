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
 * 	基础流程
 * 
 * 	name: Rule.Flow
 * 	create: 2025-05-26
 * 	memo: 基础流程
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using DaLi.Utils.Flow.Interface;
using DaLi.Utils;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Model {
	/// <summary>流程</summary>
	public class Flow : IFlow {

		/// <summary>构造函数</summary>
		public Flow() { Input = []; Rules = []; CreateTime = Main.DATE_NOW; }

		/// <summary>构造函数</summary>
		/// <param name="name">流程名称</param>
		/// <param name="input">初始化数据</param>
		/// <param name="rules">规则列表</param>
		public Flow(string name, SODictionary input = null, List<RuleData> rules = null) { Name = name; Input = input ?? []; Rules = rules ?? []; CreateTime = Main.DATE_NOW; }

		/// <summary>构造函数</summary>
		/// <param name="name">流程名称</param>
		/// <param name="input">初始化数据</param>
		/// <param name="rules">规则列表 JSON 数据</param>
		public Flow(string name, SODictionary input = null, string rules = null) { Name = name; Input = input ?? []; Rules = rules.FromJson<List<RuleData>>() ?? []; CreateTime = Main.DATE_NOW; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string Description { get; set; }

		/// <inheritdoc/>
		public string Author { get; set; }

		/// <inheritdoc/>
		public DateTime CreateTime { get; set; }

		/// <inheritdoc/>
		public DateTime UpdateTime { get; set; }

		/// <inheritdoc/>
		public bool Debug { get; set; }

		/// <inheritdoc/>
		public SODictionary Input { get; set; }

		/// <inheritdoc/>
		public SODictionary Output { get; set; }

		/// <inheritdoc/>
		public List<RuleData> Rules { get; set; }

		/// <inheritdoc/>
		public object Clone() => this.ToJson(false, false, false).FromJson<Flow>();
	}
}
