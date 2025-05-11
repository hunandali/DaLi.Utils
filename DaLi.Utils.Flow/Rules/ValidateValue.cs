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
 * 	验证值操作
 * 
 * 	name: Rule.ValidateValue
 * 	create: 2025-03-17
 * 	memo: 验证值是否存在内容，字典、列表、文本长度大于 1，时间大于 2000年，数字大于 0，布尔为 True
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>验证值操作</summary>
	public class ValidateValue : ValidateRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "验证值操作";

		/// <summary>原始数据</summary>
		public string Source { get; set; }

		/// <summary>用于判断的值</summary>
		public string Value { get; set; }

		/// <summary>判断操作，先转换成文本后再比较</summary>
		public ValueOperateEnum Operate { get; set; }

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Operate switch {
			ValueOperateEnum.NOT_EQUAL => Execute(!Source.Equals(Value), context),
			ValueOperateEnum.GREATER => Execute(Source.ToNumber() > Value.ToNumber(), context),
			ValueOperateEnum.GREATER_EQUAL => Execute(Source.ToNumber() >= Value.ToNumber(), context),
			ValueOperateEnum.LESS => Execute(Source.ToNumber() < Value.ToNumber(), context),
			ValueOperateEnum.LESS_EQUAL => Execute(Source.ToNumber() <= Value.ToNumber(), context),
			ValueOperateEnum.CONTAINS => Execute(Source.IsLike(Value), context),
			ValueOperateEnum.NOT_CONTAINS => Execute(!Source.IsLike(Value), context),
			_ => Execute(object.Equals(Source, Value), context),
		};

		#endregion
	}
}