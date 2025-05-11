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
 * 	JSON 时间类型转换
 * 
 * 	name: JsonDateTimeConverter
 * 	create: 2024-08-09
 * 	memo: JSON 时间类型转换
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaLi.Utils.Json {

	/// <summary>JSON 时间类型转换</summary>
	public class JsonDateTimeConverter : JsonConverter<DateTime> {
		/// <summary>读取</summary>
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TokenType == JsonTokenType.Null) {
				return default;
			}

			if (reader.TokenType == JsonTokenType.String) {
				var stringValue = reader.GetString();
				if (string.IsNullOrEmpty(stringValue)) {
					return default;
				}

				if (DateTime.TryParse(stringValue, out var result)) {
					return result;
				}

				return default;
			}

			return reader.GetDateTime();
		}

		/// <summary>写入</summary>
		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));//writer.WriteStringValue(value.ToString("O"));
	}
}

