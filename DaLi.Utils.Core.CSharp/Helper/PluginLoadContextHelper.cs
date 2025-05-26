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
 *  在隔离环境中加载插件
 * 
 * 	name: PluginLoadContextHelper
 * 	create: 2025-05-25
 * 	memo: 在隔离环境中加载插件
 * 
 * ------------------------------------------------------------
 */

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DaLi.Utils.Helper {
	/// <summary>
	/// 自定义的 AssemblyLoadContext，用于在隔离环境中加载插件。
	/// 这使得插件可以拥有自己的依赖项，而不会与宿主应用程序或其他插件发生冲突。
	/// </summary>
	/// <remarks>
	/// 此上下文将首先尝试从插件目录加载程序集，
	/// 然后在必要时回退到默认加载上下文以加载共享依赖项。
	/// 确保每个组件独立运作，同时又为整体做出贡献。
	/// </remarks>
	public class PluginLoadContextHelper : AssemblyLoadContext {
		/// <summary>依赖解析器</summary>
		private readonly AssemblyDependencyResolver _Resolver;

		/// <summary>默认路径</summary>
		private readonly string _Path;

		/// <summary>
		/// 初始化 <see cref="PluginLoadContextHelper"/> 类的新实例。
		/// </summary>
		/// <exception cref="ArgumentNullException">如果 pluginPath 为 null 或为空，则抛出此异常。</exception>
		/// <exception cref="FileNotFoundException">如果 pluginPath 指定的文件不存在，则抛出此异常。</exception>
		public PluginLoadContextHelper(string path) {
			ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));

			if (!File.Exists(path)) {
				throw new FileNotFoundException($"在 {path} 未找到插件 DLL");
			}

			_Path = path;

			// 解析器将在插件 DLL 所在的同一目录以及任何特定于运行时的或 NuGet 包的子目录中查找依赖项。
			_Resolver = new AssemblyDependencyResolver(path);
		}

		/// <summary>
		/// 按其 <see cref="AssemblyName"/> 加载程序集。
		/// </summary>
		/// <param name="assemblyName">要加载的程序集的 <see cref="AssemblyName"/>。</param>
		/// <returns>已加载的 <see cref="Assembly"/>；如果找不到程序集，则为 null。</returns>
		/// <remarks>
		/// 当需要解析程序集时，运行时会调用此方法。
		/// 它首先尝试使用 AssemblyDependencyResolver 解析程序集，该解析器知道
		/// 如何查找插件的依赖项（包括来自 NuGet 包的依赖项）。
		/// 如果解析器找不到它，它会回退到 Default 上下文，允许插件使用共享的框架程序集。
		/// </remarks>
		protected override Assembly Load(AssemblyName assemblyName) {
			var assemblyPath = _Resolver.ResolveAssemblyToPath(assemblyName);
			if (string.IsNullOrEmpty(assemblyPath)) { return null; }
			return LoadFromAssemblyPath(assemblyPath);
		}

		/// <summary>
		/// 按名称加载非托管库（本机 DLL）。
		/// </summary>
		/// <param name="unmanagedDllName">要加载的非托管库的名称。</param>
		/// <returns>已加载的非托管库的句柄；如果找不到库，则为 <see cref="IntPtr.Zero"/>。</returns>
		/// <remarks>
		/// 此方法允许插件加载本机依赖项。
		/// AssemblyDependencyResolver 也有助于定位这些本机库。
		/// </remarks>
		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
			var libraryPath = _Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
			if (string.IsNullOrEmpty(libraryPath)) { return IntPtr.Zero; }
			return LoadUnmanagedDllFromPath(libraryPath);
		}

		/// <summary>加载默认的程序集</summary>
		public Assembly Load() => string.IsNullOrEmpty(_Path) ? null : LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(_Path)));
	}
}
