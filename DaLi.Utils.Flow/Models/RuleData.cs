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
 * 	规则数据
 * 
 * 	name: RuleData
 * 	create: 2025-05-26
 * 	memo: 规则数据
 * 	
 * ------------------------------------------------------------
 */

using DaLi.Utils.Flow.Interface;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Model {
	/// <summary>规则数据</summary>
	public class RuleData : SODictionary, IRuleData {
		/// <summary>构造函数，默认启用该规则</summary>
		public RuleData() => Enabled = true;

		/// <summary>构造函数，默认启用该规则</summary>
		/// <param name="rule">规则数据</param>
		public RuleData(SODictionary rule = null) : base(rule) {
			if (!ContainsKey("Enabled")) { Enabled = true; }
		}

		/// <summary>构造函数，默认启用该规则</summary>
		/// <param name="type">规则类型</param>
		/// <param name="rule">规则 JSON 数据</param>
		public RuleData(string type, string rule = "") : base(rule) {
			Type = type;
			if (!ContainsKey("Enabled")) { Enabled = true; }
		}

		/// <summary>规则</summary>
		public IRule Rule => FlowHelper.RuleItem(this);

		/// <inheritdoc/>
		public string Type { get => GetValue("Type"); set => base["Type"] = value; }

		/// <inheritdoc/>
		public string Output { get => GetValue("Output"); set => base["Output"] = value; }

		/// <inheritdoc/>
		public bool EmptyIgnore { get => GetValue("EmptyIgnore", false); set => base["EmptyIgnore"] = value; }

		/// <inheritdoc/>
		public TristateEnum ErrorIgnore { get => GetValue("ErrorIgnore", TristateEnum.DEFAULT); set => base["ErrorIgnore"] = value; }

		/// <inheritdoc/>
		public string ErrorMessage { get => GetValue("ErrorMessage"); set => base["ErrorMessage"] = value; }

		/// <inheritdoc/>
		public bool Enabled { get => GetValue("Enabled", true); set => base["Enabled"] = value; }
	}
}
