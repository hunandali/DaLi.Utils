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
 * 	JSON 布尔类型转换
 * 
 * 	name: JsonBooleanConverter
 * 	create: 2024-07-30
 * 	memo: JSON 布尔类型转换，当传递的值为 null 时转换为 false
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaLi.Utils.Json {

	/// <summary>JSON 布尔类型转换</summary>
	public class JsonBooleanConverter : JsonConverter<bool> {
		/// <summary>读取</summary>
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TokenType == JsonTokenType.Null) {
				return false;
			}

			return reader.GetBoolean();
		}

		/// <summary>写入</summary>
		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
	}
}

