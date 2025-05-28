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
 * 	文件操作
 * 
 * 	name: Rule.File
 * 	create: 2025-03-14
 * 	memo: 文件读取、写入、删除等操作
 * 	
 * ------------------------------------------------------------
 */

using System.IO;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>文件操作</summary>
	public class File : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "文件操作";

		/// <summary>文件路径</summary>
		public string Path { get; set; }

		/// <summary>操作：0. 信息; 1. 读取; 2. 保存; 3. 删除</summary>
		public int Action { get; set; } = 0;

		/// <summary>保存时的文件内容，非文本将使用 JSON 格式保存</summary>
		public object Source { get; set; }

		/// <summary>保存文件存在处理</summary>
		public ExistsActionEnum Exists { get; set; }

		/// <summary>
		/// 读取时是否忽略文件不存在的情况。
		/// true 忽略文件不存在的情况，false 抛出异常
		/// </summary>
		public bool Skip { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Path.IsEmpty()) {
				message = "文件路径未设置";
				return false;
			}

			if (Action is < 0 or > 3) {
				message = "文件操作类型未设置";
				return false;
			}

			if (Action == 2 && Source is null) {
				message = "文件操作类型为保存时，保存文件的内容必须设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var filePath = Path;

			// 检查文件是否存在
			var isExist = PathHelper.FileExist(ref filePath);

			// 对于非保存文件，文件需要存在
			if (Action != 2 && !isExist) {
				if (Skip) {
					EmptyIgnore = true;
					return null;
				}

				FlowException.Throw(ExceptionEnum.EXECUTE_ERROR, "文件不存在");
			}

			// 读取文件内容
			if (Action == 1) {
				return PathHelper.FileRead(filePath);
			}

			// 保存文件
			if (Action == 2) {
				var content = Source is string str ? str : Source.ToJson();

				// 检查并创建目录
				PathHelper.Root(filePath, true, false);

				// 存在则追加
				if (!isExist) {
					// 文件不存在，则创建
					System.IO.File.WriteAllText(filePath, content);
				} else if (Exists == ExistsActionEnum.APPEND) {
					// 追加
					System.IO.File.AppendAllText(filePath, content);
				} else if (Exists == ExistsActionEnum.OVERWRITE) {
					// 覆盖
					System.IO.File.WriteAllText(filePath, content);
				} else if (Exists == ExistsActionEnum.RENAME) {
					// 重命名后保存
					filePath = PathHelper.FileRename(filePath);
					System.IO.File.WriteAllText(filePath, content);
				}

				return filePath;
			}

			// 删除文件
			if (Action == 3) {
				System.IO.File.Delete(filePath);
				return filePath;
			}

			// 读取文件信息
			var file = new FileInfo(filePath);
			return new SODictionary {
					{ "Name", file.Name },
					{ "FullName", file.FullName },
					{ "Extension", file.Extension },
					{ "Directory", file.DirectoryName },
					{ "Exists", file.Exists },
					{ "Length", file.Length },
					{ "LastWriteTime", file.LastWriteTime },
					{ "LastWriteTimeUtc", file.LastWriteTimeUtc },
					{ "LastAccessTime", file.LastAccessTime },
					{ "LastAccessTimeUtc", file.LastAccessTimeUtc },
					{ "CreationTime", file.CreationTime },
				};
		}

		#endregion
	}
}
