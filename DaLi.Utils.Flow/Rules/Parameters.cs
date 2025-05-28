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
 * 	自定义参数组
 * 
 * 	name: Rule.Parameters
 * 	create: 2025-03-17
 * 	memo: 自定义参数组
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>自定义参数组</summary>
	public class Parameters : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "自定义参数组";

		/// <summary>参数列表</summary>
		public SODictionary Source { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "参数未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		/// <summary>克隆</summary>
		public override object Clone() {
			var r = (Parameters) MemberwiseClone();
			r.Source = (SODictionary) (Source?.Clone());
			return r;
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => Source;

		#endregion
	}
}