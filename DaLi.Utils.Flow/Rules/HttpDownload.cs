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
 * 	网络下载
 * 
 * 	name: Rule.HttpDownload
 * 	create: 2025-03-14
 * 	memo: 网络下载
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>网络下载</summary>
	public class HttpDownload : HttpRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "网络下载";

		/// <summary>下载目录</summary>
		public string Folder { get; set; }

		/// <summary>文件名</summary>
		public string FileName { get; set; }

		/// <summary>文件存在处理</summary>
		public ExistsActionEnum Exists { get; set; }

		/// <summary>初始化</summary>
		public HttpDownload() : base() => Exists = ExistsActionEnum.RENAME;

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Folder.IsEmpty()) {
				message = "下载目录未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		/// <summary>克隆</summary>
		public override object Clone() => base.Clone(this);

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => HttpExecute(this, (http, rule) => {
			// 保存
			var (Path, Flag) = http.SaveFile(rule.Folder, rule.Exists, rule.FileName);

			// 没有变量无需返回数据
			if (rule.Output.IsEmpty()) {
				rule.EmptyIgnore = true;
				return null;
			}

			return new SODictionary {
				{ "Path", Path },
				{ "Flag", Flag },
				{ "Rule", rule },
				{ "Url", http.Url },
				{ "StatusCode", http.StatusCode },
				{ "StatusDescription", http.StatusDescription },
				{ "Headers",http.ResponseHeaders },
				{ "Cookies", http.GetCookies() }
			};
		});

		#endregion
	}
}