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
 * 	值判断操作
 * 
 * 	name: Rule.IfValue
 * 	create: 2025-03-14
 * 	memo: 值判断操作
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>值判断操作</summary>
	public class IfValue : IfRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "值比较判断";

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
			ValueOperateEnum.NOT_EQUAL => Execute(!Source.Equals(Value), context, cancel),
			ValueOperateEnum.GREATER => Execute(Source.ToNumber() > Value.ToNumber(), context, cancel),
			ValueOperateEnum.GREATER_EQUAL => Execute(Source.ToNumber() >= Value.ToNumber(), context, cancel),
			ValueOperateEnum.LESS => Execute(Source.ToNumber() < Value.ToNumber(), context, cancel),
			ValueOperateEnum.LESS_EQUAL => Execute(Source.ToNumber() <= Value.ToNumber(), context, cancel),
			ValueOperateEnum.CONTAINS => Execute(Source.IsLike(Value), context, cancel),
			ValueOperateEnum.NOT_CONTAINS => Execute(!Source.IsLike(Value), context, cancel),
			_ => Execute(object.Equals(Source, Value), context, cancel),
		};

		#endregion
	}
}