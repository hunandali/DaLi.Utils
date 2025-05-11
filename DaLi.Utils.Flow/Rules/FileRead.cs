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
 * 	文件读取
 * 
 * 	name: Rule.FileRead
 * 	create: 2025-03-14
 * 	memo: 文件读取
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>文件读取</summary>
	public class FileRead : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "文件读取";

		/// <summary>文件路径</summary>
		public string Path { get; set; }

		/// <summary>是否 JSON 格式</summary>
		public bool IsJson { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Path.IsEmpty()) {
				message = "文件路径未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var filePath = Path;
			if (!PathHelper.FileExist(ref filePath)) {
				FlowException.Throw(ExceptionEnum.EXECUTE_ERROR, "文件不存在");
			}

			var content = PathHelper.FileRead(filePath);
			var result = IsJson ? content.FromJson() : content;

			return result;
		}

		#endregion
	}
}