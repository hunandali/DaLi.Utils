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
 * 	序列化数据为 JSON
 * 
 * 	name: Rule.JsonString
 * 	create: 2025-03-17
 * 	memo: 序列化数据为 JSON
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>序列化数据为 JSON</summary>
	public class JsonString : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "序列化数据为 JSON";

		/// <summary>原始数据</summary>
		public object Source { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Source is null) {
				message = "原始数据未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Source.ToJson();

		#endregion
	}
}