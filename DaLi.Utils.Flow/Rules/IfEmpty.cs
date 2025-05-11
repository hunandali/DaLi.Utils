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
 * 	空值判断
 * 
 * 	name: Rule.IfEmpty
 * 	create: 2025-03-17
 * 	memo: 空值判断，判断原始内容是否为空内容
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>判断操作</summary>
	public class IfValidate : IfRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "空值判断";

		/// <summary>原始数据变量名</summary>
		public object Source { get; set; }

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Execute(ValueExtension.IsEmptyValue(Source), context, cancel);

		#endregion
	}
}