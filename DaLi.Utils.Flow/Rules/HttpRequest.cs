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
 * 	网页获取
 * 
 * 	name: Rule.HttpRequest
 * 	create: 2025-03-14
 * 	memo: 网页获取
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>网页获取</summary>
	public class HttpRequest : HttpRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "网页获取";

		/// <summary>Unicode是否需要反编码？用于 json 中汉字进行 Unicode 编码后，进行反转码</summary>
		public bool UnicodeDecode { get; set; }

		/// <summary>是否返回文本内容 1:文本；2:JSON 对象；其他:非文本数据</summary>
		public int ReturnMode { get; set; }

		/// <summary>初始化</summary>
		public HttpRequest() : base() {
			UnicodeDecode = false;
			ReturnMode = 1;
		}

		#endregion

		#region INFORMATION

		/// <summary>克隆</summary>
		public override object Clone() => base.Clone(this);

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => HttpExecute(this, (http, rule) => {
			// 没有变量无需返回数据
			if (rule.Output.IsEmpty()) {
				rule.EmptyIgnore = true;
				return null;
			}

			// 输出结果
			var result = new SODictionary {
				{ "Rule", rule },
				{ "Url", http.Url },
				{ "StatusCode", http.StatusCode },
				{ "StatusDescription", http.StatusDescription },
				{ "Headers",http.ResponseHeaders },
				{ "Cookies", http.GetCookies() }
			};

			// 返回文本内容
			// 如果没有设置文本输出变量则不输出文本内容，防止某些非文本请求异常
			if (rule.ReturnMode is 1 or 2) {
				var encode = rule.Encoding == EncodingEnum.AUTO ? EncodingEnum.AUTO : rule.Encoding;
				var content = http.GetHtml(Convert.ToInt32(encode));
				if (content.IsEmpty()) { return content; }

				// JS Unicode 解码
				if (rule.UnicodeDecode) { content = content.DecodeJsUnicode(); }

				// JSON 转换
				if (rule.ReturnMode is 2) {
					result.Add("Content", content.FromJson());
				} else {
					result.Add("Content", content);
				}
			}

			return result;
		});

		#endregion
	}
}