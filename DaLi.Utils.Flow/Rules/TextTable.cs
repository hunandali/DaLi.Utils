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
using System.Linq;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>表格文本</summary>
	public class TextTable : RuleBase {

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

		/// <summary>是否多个表格区域</summary>
		public bool Multi { get; set; }

		/// <summary>清除单元格中的标签</summary>
		public string[] ClearTags { get; set; } = ["td"];

		/// <summary>忽略表格的前几行（防止表头），默认：1</summary>
		public int IgnoreIndex { get; set; } = 1;

		/// <summary>
		/// 表格值操作记录，如果设置此指，则结果将用字典列表返回，而不再是数组列表。<para />
		/// 名称为字段名，值为字段处理字符串。<para />
		/// 每行数据可以用 _data[index] 来代替第 N 列，从 0 开始
		/// </summary>
		public SSDictionary ValueAction { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "未设置原始内容";
				return false;
			}

			if (Table.IsEmpty() && Tr.IsEmpty() && Td.IsEmpty()) {
				message = "未设置有效的表格规则";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 1. 获取表格区域
			var tables = Source.Cut(Table, 0, Multi);
			FlowException.ThrowNull(tables, ExceptionEnum.EXECUTE_ERROR, "未找到表格内容");

			var content = Multi ? string.Join('\n', tables) : (string) tables;
			var rows = (string[]) content.Cut(Tr, 0, true);
			rows = [.. rows.Skip(IgnoreIndex)];
			FlowException.ThrowIf(rows.IsEmpty(), ExceptionEnum.EXECUTE_ERROR, "未获取到表格行数据");

			var cells = rows.Select(td => {
				var tds = (string[]) td.Cut(Td, 0, true);
				if (tds.IsEmpty()) {
					return null;
				}

				if (ClearTags.IsEmpty()) {
					return tds;
				}

				return [.. tds.Select(td => td.ClearHtml(ClearTags))];
			}).Where(x => x != null).ToList();
			FlowException.ThrowIf(cells.IsEmpty(), ExceptionEnum.EXECUTE_ERROR, "未获取到表格单元格数据");

			// 2. 调整为相同维度
			var max = cells.Max(x => x.Length);
			for (var i = 0; i < cells.Count; i++) {
				if (cells[i].Length < max) {
					var cell = cells[i];
					System.Array.Resize(ref cell, max);
					cells[i] = cell;
				}
			}

			// 3. 处理数据
			if (ValueAction.IsEmpty()) {
				return cells;
			}

			// 新的上下文
			var data = new SODictionary(context);

			return cells.Select(row => {
				data["_data"] = row;

				// 返回类型
				var ret = new SODictionary();

				ValueAction.ForEach((key, value) => {
					var obj = FlowHelper.GetObjectValue(value, data);
					ret.Add(key, obj);
				});

				return ret;
			}).Where(row => row.NotEmpty()).ToList();

			#endregion
		}
	}
}