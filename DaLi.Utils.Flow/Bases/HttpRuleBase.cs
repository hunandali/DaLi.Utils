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
 * 	Http 请求规则基类
 * 
 * 	name: HttpRuleBase
 * 	create: 2025-03-14
 * 	memo: Http 请求规则基类
 * 	
 * ------------------------------------------------------------
 */

using System;
using DaLi.Utils.Extension;
using DaLi.Utils.Http;
using DaLi.Utils.Http.Model;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Base {

	/// <summary>Http 请求规则基类</summary>
	public abstract class HttpRuleBase : FlowRuleBase {

		#region PROPERTY

		/// <summary>网址</summary>
		public string URL { get; set; }

		/// <summary>自动跳转</summary>
		public bool AutoRedirect { get; set; }

		/// <summary>超时，最少 5 秒</summary>
		public int Timeout { get; set; } = 60;

		/// <summary>Http上传表单编码类型</summary>
		public HttpPostEnum PostType { get; set; }

		/// <summary>请求方式</summary>
		public string Method { get; set; }

		/// <summary>来源地址</summary>
		public string Referer { get; set; }

		/// <summary>浏览器头</summary>
		public string UserAgent { get; set; }

		/// <summary>Headers</summary>
		public SSDictionary Headers { get; set; }

		/// <summary>提交 Cookiese 信息</summary>
		public SSDictionary Cookies { get; set; }

		/// <summary>Post 提交数据</summary>
		public SODictionary SendDatas { get; set; }

		/// <summary>编码</summary>
		public EncodingEnum Encoding { get; set; }

		/// <summary>初始化</summary>
		public HttpRuleBase() {
			Encoding = EncodingEnum.AUTO;
			Method = "GET";
		}

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (!URL.IsUrl()) {
				message = "请求网址无效";
				return false;
			}

			return base.Validate(ref message);
		}

		/// <summary>克隆</summary>
		protected T Clone<T>(T source) where T : HttpRuleBase {
			var clone = (T) source.MemberwiseClone();
			clone.Headers = (SSDictionary) (Headers?.Clone());
			clone.Cookies = (SSDictionary) (Cookies?.Clone());
			clone.SendDatas = (SODictionary) (SendDatas?.Clone());
			return clone;
		}

		#endregion

		#region EXECUTE

		/// <summary>执行操作</summary>
		public static object HttpExecute<T>(T rule, Func<HttpClient, T, object> func) where T : HttpRuleBase {
			FlowException.ThrowNull(rule, ExceptionEnum.RULE_INVALID, "请求规则未设置");
			FlowException.ThrowNull(func, ExceptionEnum.RULE_INVALID, "Http 结果请求处理函数异常");

			// 注册编码
			Main.EncodingRegister();

			// 创建请求对象
			var http = new HttpClient {
				Url = rule.URL,
				Timeout = rule.Timeout.Range(5) * 1000,
				AllowAutoRedirect = rule.AutoRedirect,
				PostType = rule.PostType
			};

			http.SetMethod(rule.Method);

			if (rule.UserAgent.NotEmpty()) {
				http.UserAgent = rule.UserAgent;
			}

			if (rule.Referer.NotEmpty()) {
				http.Referer = rule.Referer;
			}

			if (rule.Cookies.NotEmpty()) {
				rule.Cookies.ForEach(http.SetCookie);
			}

			if (rule.Headers.NotEmpty()) {
				rule.Headers.ForEach(http.SetHeader);
			}

			if (rule.SendDatas.NotEmpty()) {
				if (rule.PostType is HttpPostEnum.JSON or HttpPostEnum.RAW) {
					http.SetRawContent(rule.SendDatas.ToJson());
				} else {
					rule.SendDatas.ForEach((k, v) => http.SetPostContent(k, v?.ToString()));
				}
			}

			// 执行操作
			http.Execute();

			// 结果处理
			var result = func.Invoke(http, rule);

			// 注销 http 对象
			http.Dispose();

			return result;
		}

		#endregion
	}
}