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
 * 	表格文本
 * 
 * 	name: Rule.TextTable
 * 	create: 2025-03-17
 * 	memo: 表格文本
 * 	
 * ------------------------------------------------------------
 */

using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>表格文本</summary>
	public class TextTable : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "表格文本";

		/// <summary>原始内容</summary>
		public string Source { get; set; }

		/// <summary>表格开始区域</summary>
		public string Table { get; set; } = "<table[*]</table>";

		/// <summary>行开始区域</summary>
		public string Tr { get; set; } = "<tr[*]</tr>";

		/// <summary>单元格开始区域</summary>
		public string Td { get; set; } = "(<(td|th)(.|\n)*?</(td|th)>)";

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "未设置原始内容";
				return false;
			}

			return true;
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 创建数据表
			var table = new DataTable();

			// 获取表格内容
			var tableContent = Source;
			if (!string.IsNullOrEmpty(Table)) {
				var tablePattern = Table.Replace("[*]", "[^>]*");
				var tableMatch = Regex.Match(Source, tablePattern, RegexOptions.IgnoreCase);

				FlowException.ThrowIf(!tableMatch.Success, ExceptionEnum.EXECUTE_ERROR, "未找到表格内容");
				tableContent = tableMatch.Value;
			}

			// 获取行内容
			var trPattern = Tr.Replace("[*]", "[^>]*");
			var trMatches = Regex.Matches(tableContent, trPattern, RegexOptions.IgnoreCase);

			FlowException.ThrowIf(trMatches.Count == 0, ExceptionEnum.EXECUTE_ERROR, "未找到行内容");

			// 获取单元格内容
			var tdPattern = Td;
			var isFirstRow = true;
			foreach (Match trMatch in trMatches) {
				var tdMatches = Regex.Matches(trMatch.Value, tdPattern, RegexOptions.IgnoreCase);
				if (tdMatches.Count == 0) {
					continue;
				}

				// 第一行创建列
				if (isFirstRow) {
					for (var i = 0; i < tdMatches.Count; i++) {
						table.Columns.Add($"Column{i + 1}");
					}
					isFirstRow = false;
				}

				// 添加行数据
				var row = table.NewRow();
				for (var i = 0; i < tdMatches.Count && i < table.Columns.Count; i++) {
					var tdContent = tdMatches[i].Value;
					tdContent = Regex.Replace(tdContent, "<[^>]+>", string.Empty);
					row[i] = tdContent.Trim();
				}
				table.Rows.Add(row);
			}

			// 返回结果
			return table;
		}

		#endregion
	}
}