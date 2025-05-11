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
 * 	JSON Object 数据转换
 * 
 * 	name: JsonConverter
 * 	create: 2024-01-10
 * 	memo: JSON 自定义转换，对于 Object 类型数据，比如用户提交的数据类型为 Object 时如果不做自定义解析有可能无法识别
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Text.Json;

namespace DaLi.Utils.Json {

	/// <summary>JSON Object 数据转换</summary>
	public class JsonObjectConverter : JsonConverterBase {

		/// <summary>解析节点</summary>
		private readonly Func<JsonElement, object> _JsonElementParse;

		/// <summary>构造</summary>
		public JsonObjectConverter() => _JsonElementParse = null;

		/// <summary>构造</summary>
		public JsonObjectConverter(Func<JsonElement, object> jsonElementParse) => _JsonElementParse = jsonElementParse;

		/// <summary>写入</summary>
		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) {
			if (value is not null and double val) {
				if (double.IsNaN(val)) {
					writer.WriteStringValue("NaN");
					return;
				}

				if (double.IsInfinity(val)) {
					writer.WriteStringValue(val > 0 ? "Infinity" : "-Infinity");
					return;
				}
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}

		/// <inheritdoc/>
		protected override object Read(JsonElement value) => _JsonElementParse is null ? value.Parse() : _JsonElementParse(value);
	}
}
