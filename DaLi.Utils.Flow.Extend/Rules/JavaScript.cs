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
 * 	脚本执行
 * 
 * 	name: Rule.JavaScript
 * 	create: 2025-05-27
 * 	memo: 脚本执行
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Helper;
using DaLi.Utils.Model;
using Jint;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>JavaScript 脚本</summary>
	public class JavaScript : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "JavaScript 脚本";

		/// <summary>脚本代码</summary>
		public string Source { get; set; }

		/// <summary>引用 dayjs 与 lodash 库</summary>
		public bool Libs { get; set; }

		/// <summary>需要 HTTP 请求操作</summary>
		public bool Http { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "脚本代码未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>共用库 lodash</summary>
		private static readonly string _LODASH = System.Text.Encoding.UTF8.GetString(Resources.lodash);

		/// <summary>共用库 dayjs</summary>
		private static readonly string _DAYJS = System.Text.Encoding.UTF8.GetString(Resources.dayjs);

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var engine = new Engine();

			// 附加组件库
			if (Libs) {
				engine.Modules.Add("lodash", _LODASH);
				engine.Modules.Add("dayjs", _DAYJS);

				engine.Modules.Import("lodash");
				engine.Modules.Import("dayjs");
			}

			// 存在 http 请求
			if (Http) {
				engine.SetValue("fetch", NetHelper.Ajax);
				engine.SetValue("api", NetHelper.Api);
			}

			engine.SetValue("source", context);

			var value = engine.Evaluate(Source).ToObject();
			var result = engine.GetValue("result").ToObject();

			// 尝试从 result 结果变量中取值，获取不到则直接返回输出结果
			return result ?? value;
		}

		#endregion

	}
}