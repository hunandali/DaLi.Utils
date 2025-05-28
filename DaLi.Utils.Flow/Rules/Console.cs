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
 * 	控制台打印
 * 
 * 	name: Rule.Console
 * 	create: 2025-03-14
 * 	memo: 控制台打印
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>控制台打印</summary>
	public class Console : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "控制台打印";

		/// <summary>原始内容，非文本将使用 JSON 序列化</summary>
		public object Source { get; set; }

		/// <summary>打印类型（info/succ/err/warning/normal）</summary>
		public string Mode { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source is null) {
				message = "控制台打印内容不能为空";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 强制忽略错误
			SkipResult();

			switch (Mode.EmptyValue("normal").ToLowerInvariant()[0]) {
				case 'i':
					System.Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case 'e':
					System.Console.ForegroundColor = ConsoleColor.Red;
					break;
				case 'w':
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case 's':
					System.Console.ForegroundColor = ConsoleColor.Green;
					break;
			}

			var content = Source is string str ? str : Source.ToJson();

			System.Console.WriteLine(content);
			System.Console.ResetColor();

			return null;
		}

		#endregion
	}
}