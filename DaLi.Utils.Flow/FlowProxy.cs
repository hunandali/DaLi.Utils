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
 * 	代理运行
 * 
 * 	name: Rule.FlowProxy
 * 	create: 2025-03-13
 * 	memo: 代理运行
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Text.Json.Serialization;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow {

	/// <summary>代理运行</summary>
	public class FlowProxy : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "代理规则执行";

		///// <summary>原始规则</summary>
		//public string Rule { get; set; }

		/// <summary>规则数据</summary>
		public RuleData Source { get; set; }

		///// <summary>获取 API 客户端的方法</summary>
		//[JsonIgnore]
		//public static Func<ApiClient> GetApiClient { get; set; }

		/// <summary>代理操作</summary>
		[JsonIgnore]
		public static Func<RuleData, SODictionary, ExecuteStatus> ProxyExecute { get; set; }

		/// <summary>构造</summary>
		public FlowProxy() => Enabled = true;

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			//message = "未设置有效的设置 API 客户端的函数";
			//if (GetApiClient == null) {
			//	return false;
			//}

			//message = "原始规则无效";
			//if (!Rule.IsJson()) {
			//	return false;
			//}

			//var dic = Rule.ToJsonDictionary();
			//if (dic.IsEmpty() || !dic.ContainsKey("type")) {
			//	return false;
			//}

			if (Source.IsEmpty()) {
				message = "未设置有效的原始规则";
				return false;
			}

			if (ProxyExecute is null) {
				message = "未设置有效的代理操作方法";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var status = ProxyExecute(Source, context);
			FlowException.ThrowNull(status, ExceptionEnum.EXECUTE_ERROR, "代理执行错误，未返回有效数据");
			FlowException.ThrowStatus(status);

			return status.Output;

			////////////////////////////////////////////////////////////////
			//var client = GetApiClient();
			//FlowException.ThrowNull(client, ExceptionEnum.RULE_INVALID, "未设置有效的设置 API 客户端的函数");

			//// {"success":true,"data":{"success":true,"time":"2023-01-16T11:49:03.2730039","rule":{"type":"TableData","name":"\u8868\u683C\u6570\u636E\u5F55\u5165","output":"db","errorIgnore":false,"enabled":true},"result":false,"children":[]},"traceId":"0HMNNEHM7OB94:00000002","host":"localhost:10000"}
			//var ret = client.ExecuteApi<ExecuteStatus>(Utils.Http.Model.HttpMethodEnum.POST, "flow/rule", new { Rule = Rule.EncodeBase64(), context }.ToJson(false, false, false));
			//FlowException.ThrowNull(ret, ExceptionEnum.EXECUTE_ERROR, $"代理错误，接口异常未返回有效数据");
			//FlowException.ThrowStatus(ret.Data);

			//// 返回结果
			//return ret.Data.Output;
		}

		#endregion
	}
}