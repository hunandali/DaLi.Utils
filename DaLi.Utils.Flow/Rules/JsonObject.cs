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
 * 	JSON 数据转换
 * 
 * 	name: Rule.JsonObject
 * 	create: 2025-03-17
 * 	memo: JSON 数据转换
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>JSON 数据转换</summary>
	public class JsonObject : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "JSON 数据转换";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		/// <summary>获取路径，按顺序往下获取</summary>
		public string Path { get; set; }

		/// <summary>是否忽略大小写</summary>
		public bool IgnoreCase { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (!Source.IsJson()) {
				message = "无效 JSON 数据";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var ret = Source.FromJson();
			FlowException.ThrowIf(ret == null, ExceptionEnum.EXECUTE_ERROR, "JSON 数据转换失败");

			// 存在 path 需要获取值
			if (Path.IsEmpty()) {
				return ret;
			}

			return ObjectAccessorHelper.GetValue(ret, Path, IgnoreCase);
		}

		#endregion
	}
}