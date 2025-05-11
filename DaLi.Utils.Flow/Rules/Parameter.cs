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
 * 	自定义单项参数
 * 
 * 	name: Rule.Parameter
 * 	create: 2025-03-17
 * 	memo: 自定义单项参数，尝试 JSON 反序列化，序列化不成功则返回原值
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>自定义单项参数</summary>
	public class Parameter : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "自定义单项参数";

		/// <summary>参数列表</summary>
		public object Source { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Source is null) {
				message = "参数未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Source;

		#endregion
	}
}