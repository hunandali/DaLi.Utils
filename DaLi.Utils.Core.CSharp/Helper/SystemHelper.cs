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
 *  系统操作
 * 
 * 	name: SystemHelper
 * 	create: 2025-03-13
 * 	memo: 系统操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.IO;

namespace DaLi.Utils.Helper {
	/// <summary>系统操作</summary>
	public sealed class SystemHelper {

		#region 路径处理

		/// <summary>系统启动的目录</summary>
		private static string _RootFolder;

		/// <summary>系统启动的目录</summary>
		public static string RootFolder {
			get {
				if (string.IsNullOrEmpty(_RootFolder)) {
					// 获取程序路径
					_RootFolder = typeof(SystemHelper).Assembly.Location;
					if (string.IsNullOrEmpty(_RootFolder)) {
						// 获取进程路径
						_RootFolder = Environment.ProcessPath;
						if (string.IsNullOrEmpty(_RootFolder)) {
							// 获取当前目录
							_RootFolder = AppDomain.CurrentDomain.BaseDirectory;
						} else {
							_RootFolder = Path.GetDirectoryName(_RootFolder);
						}
					} else {
						_RootFolder = Path.GetDirectoryName(_RootFolder);
					}
				}

				return _RootFolder;
			}
		}

		/// <summary>获取完整路径</summary>
		/// <param name="path">相对路径</param>
		/// <param name="tryCreate">是否尝试创建此路径的上级目录，如：d:\a\b\c True 则自动创建 d:\a\b 的目录</param>
		/// <param name="isFolder">当前获取的是目录还是文件地址，以便建立对应的目录</param>
		/// <remarks>如果给定的相对路径非绝对路径，则将自动以当前启动目录为基函路径</remarks>
		public static string FullPath(string path, bool tryCreate = false, bool isFolder = false) {
			if (string.IsNullOrEmpty(path)) {
				return RootFolder;
			}

			// 获取绝对路径
			var ret = Path.GetFullPath(path, RootFolder);

			if (tryCreate) {
				var dirPath = isFolder ? ret : Path.GetDirectoryName(ret);
				try {
					if (!Directory.Exists(dirPath)) {
						Directory.CreateDirectory(dirPath);
					}
				} catch (Exception) {
				}
			}

			return ret;
		}

		#endregion
	}
}
