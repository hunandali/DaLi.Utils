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
 * 	WebHook 通知
 * 
 * 	name: Rule.WebHook
 * 	create: 2025-05-27
 * 	memo: WebHook 通知
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>WebHook 通知</summary>
	public class WebHook : RuleBase {

		#region PROPERTY

		/// <summary>内容模式枚举</summary>
		public enum ContentEnum {
			/// <summary>文本格式</summary>
			Text = 0,

			/// <summary>Markdown 格式</summary>
			Markdown = 1,

			/// <summary>JSON 自定义格式</summary>
			JSON = 2
		}

		/// <summary>规则名称</summary>
		public override string Name => "WebHook 通知";

		/// <summary>WebHook Url</summary>
		public string Url { get; set; }

		/// <summary>发布内容</summary>
		public string Source { get; set; }

		/// <summary>内容模式</summary>
		public ContentEnum Mode { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (!Url.IsUrl()) {
				message = "WebHook 地址无效";
				return false;
			}

			if (Source.IsEmpty()) {
				message = "通知内容不能为空";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			bool ret;
			var message = "";

			var hook = new Utils.Misc.Notifier.WebhookNotifier(Url);
			if (Mode == ContentEnum.JSON) {
				ret = hook.Send(Source, ref message);
			} else {
				ret = hook.Send(Source, "", ref message, new KeyValueDictionary() { { "Markdown", Mode == ContentEnum.Markdown } });
			}

			FlowException.ThrowIf(!ret, ExceptionEnum.EXECUTE_ERROR, message);
			return ret;
		}

		#endregion
	}
}