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
 * 	文件保存
 * 
 * 	name: Rule.FileSave
 * 	create: 2025-03-14
 * 	memo: 文件保存
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Helper;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>文件保存</summary>
	public class FileSave : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "文件保存";

		/// <summary>原始内容</summary>
		public string Path { get; set; }

		/// <summary>文件内容</summary>
		/// <remarks>，设置为 _Delete 将移除文件</remarks>
		public string Content { get; set; }

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
			bool result;
			if (Content.IsEmpty() || Content.Equals("_Delete", StringComparison.OrdinalIgnoreCase)) {
				result = PathHelper.FileRemove(Path);
				FlowException.ThrowIf(!result, ExceptionEnum.EXECUTE_ERROR, "文件不存在或者删除失败");
			} else {
				result = PathHelper.FileSave(Path, Content);
				FlowException.ThrowIf(!result, ExceptionEnum.EXECUTE_ERROR, "文件保存失败");
			}

			return result;
		}

		#endregion
	}
}