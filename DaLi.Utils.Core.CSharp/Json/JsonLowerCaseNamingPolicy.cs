/* ------------------------------------------------------------
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
 * 	JSON 小写命名策略
 * 
 * 	name: JsonLowerCaseNamingPolicy
 * 	create: 2024-01-10
 * 	memo: JSON 小写命名策略
 * 
 * ------------------------------------------------------------
 */

using System.Text.Json;

namespace DaLi.Utils.Json {
	/// <summary>JSON 小写命名策略</summary>
	public class JsonLowerCaseNamingPolicy : JsonNamingPolicy {

		/// <summary>转换</summary>
		public override string ConvertName(string name) => name.ToLowerInvariant();
	}
}
