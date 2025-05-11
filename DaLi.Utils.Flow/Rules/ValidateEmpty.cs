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
 * 	验证值是否空内容
 * 
 * 	name: Rule.ValidateEmpty
 * 	create: 2025-03-17
 * 	memo: 验证值是否空内容，字典、列表、文本长度大于 1，时间大于 2000年，数字大于 0，布尔为 True
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>验证值是否空内容</summary>
	public class ValidateEmpty : ValidateRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "验证值是否存在内容";

		/// <summary>原始日期数据，不设置为当前时间</summary>
		public object Source { get; set; }

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Execute(ValueExtension.IsEmptyValue(Source), context);

		#endregion
	}
}