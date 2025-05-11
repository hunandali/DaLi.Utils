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
 *  程序集反射操作
 * 
 * 	name: ReflectionHeler
 * 	create: 2025-03-13
 * 	memo: 程序集反射操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using DaLi.Utils.Extension;

namespace DaLi.Utils.Helper {
	/// <summary>反射操作</summary>
	public sealed class ReflectionHeler {

		#region 插件加载上下文

		/// <summary>插件加载上下文对象，以便隔离与主程序的进程，直接加载对于存在外部引用的插件可能导致无法正常使用。具体参考：https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/creating-app-with-plugin-support</summary>
		private class PluginLoadContext : AssemblyLoadContext {
			/// <summary>依赖解析器</summary>
			private readonly AssemblyDependencyResolver _Resolver;

			/// <summary>默认路径</summary>
			private readonly string _Path;

			public PluginLoadContext(string path) : base(isCollectible: true) {
				if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
					_Path = path;
					_Resolver = new AssemblyDependencyResolver(path);
				}
			}

			/// <summary>根据 AssemblyName 解析并加载程序集</summary>
			protected override Assembly Load(AssemblyName assemblyName) {
				var assemblyPath = _Resolver.ResolveAssemblyToPath(assemblyName);
				if (string.IsNullOrEmpty(assemblyPath)) { return null; }
				return LoadFromAssemblyPath(assemblyPath);
			}

			/// <summary>允许派生的类按名称加载非托管库</summary>
			protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
				var libraryPath = _Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
				if (string.IsNullOrEmpty(libraryPath)) { return IntPtr.Zero; }
				return LoadUnmanagedDllFromPath(libraryPath);
			}

			/// <summary>加载默认的程序集</summary>
			public Assembly LoadAssembly() => string.IsNullOrEmpty(_Path) ? null : LoadFromAssemblyPath(_Path);
		}

		#endregion

		#region 过滤器操作

		/// <summary>系统 Assembly 名，以便过滤掉系统 Assembly</summary>
		private static readonly HashSet<string> _InvalidAssemblies = new(StringComparer.OrdinalIgnoreCase) { "system.*", "microsoft.*", "mscorlib*", "netstandard*", "vshost.*", "interop.*", "google.*", "icsharpcode.*", "newtonsoft.*", "windowsbase*", "swashbuckle.*", "csrediscore*", "mysql*", "npgsql.*", "npoi.*", "sqlite*", "serilog*", "freeredis*", "freesql*", "pomelo.*", "oracle.*" };

		/// <summary>系统 Type 名，以便过滤掉系统 Type</summary>
		/// <remarks>可以使用通配符 *</remarks>
		private static readonly HashSet<string> _InvalidTypes = new(_InvalidAssemblies, StringComparer.OrdinalIgnoreCase);

		/// <summary>添加更多需要过滤的 Assembly 名称</summary>
		public static void InvalidAssemblyInsert(params string[] names) {
			if (names.IsEmpty()) { return; }

			foreach (var name in names) {
				if (!string.IsNullOrEmpty(name)) { _InvalidAssemblies.Add(name.Trim()); }
			}
		}

		/// <summary>添加更多需要过滤的 Type 名称</summary>
		/// <remarks>可以使用通配符 *</remarks>
		public static void InvalidTypeInsert(params string[] names) {
			if (names.IsEmpty()) { return; }

			foreach (var name in names) {
				if (!string.IsNullOrEmpty(name)) { _InvalidTypes.Add(name.Trim()); }
			}
		}

		/// <summary>滤器无效名称</summary>
		/// <returns>首尾都包含或者不含点则只要存在此值即可；如果只有末尾有点则比较开头，如果只有开始有点则比较末尾</returns>
		public static bool IsInvalidName(string checkName, bool isAssembly) {
			if (string.IsNullOrEmpty(checkName)) { return true; }

			var filter = isAssembly ? _InvalidAssemblies : _InvalidTypes;

			// 实现Like方法的功能
			foreach (var name in filter) {
				if (checkName.IsLike(name)) { return true; }
			}

			return false;
		}

		#endregion

		#region 程序集加载

		/// <summary>获取当前系统所有程序集</summary>
		/// <param name="includeBin">是否包含 Bin 目录</param>
		/// <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		/// <param name="skipSystemAssembly">是否过滤系统程序集</param>
		/// <returns>返回所有程序集列表</returns>
		public static HashSet<Assembly> CurrentAssemblies(bool includeBin = false, string pluginFolder = "plugins", bool skipSystemAssembly = true) {
			var assemblies = new HashSet<Assembly>();

			// 添加程序集（过滤无效名称）
			void AddAssembly(Assembly assembly) {
				if (assembly is null) { return; }

				// 不过滤系统程序集 或者 非系统程序集则直接添加
				if (!skipSystemAssembly || !IsInvalidName(assembly.GetName().Name, true)) { assemblies.Add(assembly); }
			}

			// 加载当前程序集
			foreach (var assembly in AssemblyLoadContext.Default.Assemblies) {
				AddAssembly(assembly);

				// 加载引用的程序集
				try {
					foreach (var refName in assembly.GetReferencedAssemblies()) {
						AddAssembly(Assembly.Load(refName));
					}
				} catch (Exception ex) {
					Console.Error.WriteLine($"{assembly.GetName().Name} 引用程序集加载异常：{ex.Message}");
				}
			}

			// 加载Bin目录
			if (includeBin) {
				var binPath = SystemHelper.FullPath("bin", true, true);
				if (Directory.Exists(binPath)) {
					foreach (var file in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories)) {
						try {
							AddAssembly(AssemblyLoadContext.Default.LoadFromAssemblyPath(file));
						} catch (Exception ex) {
							Console.Error.WriteLine($"Bin目录程序集 {file} 加载异常：{ex.Message}");
						}
					}
				}
			}

			// 加载插件目录
			if (!string.IsNullOrEmpty(pluginFolder)) {
				// 如果 plugins 目录下存在下级目录，则使用下级目录的目录名做为插件名，获取下级目录下同名 dll；更好的隔离划分插件
				// 注意加载方式，plugin 需要使用 PluginLoadContext 加载，bin 目录直接加载

				var pluginPath = SystemHelper.FullPath(pluginFolder, true, true);
				if (Directory.Exists(pluginPath)) {
					var pluginFiles = Directory.GetDirectories(pluginPath)
						.Select(dir => Path.Combine(dir, $"{Path.GetFileName(dir)}.dll"));

					foreach (var file in pluginFiles) {
						try {
							AddAssembly(new PluginLoadContext(file).LoadAssembly());
						} catch (Exception ex) {
							Console.Error.WriteLine($"插件程序集 {file} 加载异常：{ex.Message}");
						}
					}
				}
			}

			return assemblies;
		}

		/// <summary>加载指定文件的程序集</summary>
		/// <param name="filepath">指定程序集的路径</param>
		/// <param name="includeNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		public static Assembly LoadAssembly(string filepath, params string[] includeNames) {
			if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath)) {
				return null;
			}

			var fileName = Path.GetFileNameWithoutExtension(filepath);
			if (IsInvalidName(fileName, true)) {
				return null;
			}

			Assembly assembly = null;
			try {
				// 加载程序集 (.net core 使用 AssemblyLoadContext)
				assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filepath);
			} catch { }

			if (assembly == null) { return null; }

			// 过滤无效名称
			var assemblyName = assembly.GetName().Name;
			if (IsInvalidName(assemblyName, true)) { return null; }

			// 检查是否必须包含
			if (includeNames?.Length > 0 && !includeNames.Any(n => assemblyName.IsLike(n))) {
				return null;
			}

			return assembly;
		}

		/// <summary>加载指定目录下的程序集</summary>
		/// <param name="folder">获取目录，未指定则当前启动目录</param>
		/// <param name="includeNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		public static HashSet<Assembly> LoadAssemblies(string folder = null, params string[] includeNames) {
			folder ??= SystemHelper.RootFolder;
			if (!Directory.Exists(folder)) { return []; }

			var assemblies = new HashSet<Assembly>();

			foreach (var file in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories)) {
				try {
					var assembly = LoadAssembly(file, includeNames);
					if (assembly != null) { assemblies.Add(assembly); }
				} catch (Exception ex) {
					Console.Error.WriteLine($"程序集 {file} 加载异常：{ex.Message}");
				}
			}

			return assemblies;
		}

		#endregion

		#region 类型加载

		/// <summary>获取程序集中的类型</summary>
		/// <param name="assembly">程序集</param>
		/// <param name="baseTypes">必须包含的基类，用于过滤无效的类型</param>
		public static HashSet<Type> GetTypes(Assembly assembly, params Type[] baseTypes) {
			if (assembly == null) { return []; }

			var types = new HashSet<Type>();

			try {
				foreach (var type in assembly.GetTypes()) {
					if (type == null) { continue; }

					// 过滤无效名称
					var typeName = type.FullName;
					if (IsInvalidName(type.FullName, false)) { continue; }

					// 检查是否必须包含
					if (baseTypes.NotEmpty() && !baseTypes.Any(n => type.IsComeFrom(n))) { continue; }

					types.Add(type);
				}
			} catch { }

			return types;
		}

		/// <summary>获取指定目录下所有程序集中的类型</summary>
		/// <param name="folder">获取目录，未指定则当前启动目录</param>
		/// <param name="baseTypes">必须包含的基类，用于过滤无效的类型</param>
		public static HashSet<Type> GetTypes(string folder = null, params Type[] baseTypes) => [.. LoadAssemblies(folder).SelectMany(assembly => GetTypes(assembly, baseTypes))];

		/// <summary>获取当前系统所有程序集</summary>
		/// <param name="includeBin">是否包含 Bin 目录</param>
		/// <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		/// <param name="skipSystemAssembly">是否过滤系统程序集</param>
		/// <param name="baseTypes">必须包含的基类，用于过滤无效的类型</param>
		/// <returns>返回所有程序集列表</returns>
		public static HashSet<Type> CurrentTypes(bool includeBin = false, string pluginFolder = "plugins", bool skipSystemAssembly = true, params Type[] baseTypes) => [.. CurrentAssemblies(includeBin, pluginFolder, skipSystemAssembly).SelectMany(assembly => GetTypes(assembly, baseTypes))];

		/// <summary>获取程序集中的类型</summary>
		/// <param name="assembly">程序集</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="includeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<Type> GetTypes(Assembly assembly, bool publicOnly = false, params string[] includeNames) {
			if (assembly == null) { return []; }

			var types = new HashSet<Type>();

			try {
				var list = publicOnly ? assembly.GetExportedTypes() : assembly.GetTypes();
				foreach (var type in list) {
					if (type == null) { continue; }

					// 过滤无效名称
					var typeName = type.FullName;
					if (IsInvalidName(type.FullName, false)) { continue; }

					// 检查是否必须包含
					if (includeNames?.Length > 0 && !includeNames.Any(n => typeName.IsLike(n))) { continue; }

					types.Add(type);
				}
			} catch { }

			return types;
		}

		/// <summary>获取指定目录下所有程序集中的类型</summary>
		/// <param name="folder">获取目录，未指定则当前启动目录</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="includeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<Type> GetTypes(string folder = null, bool publicOnly = false, params string[] includeNames) => [.. LoadAssemblies(folder).SelectMany(assembly => GetTypes(assembly, publicOnly, includeNames))];

		/// <summary>获取当前系统所有程序集</summary>
		/// <param name="includeBin">是否包含 Bin 目录</param>
		/// <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		/// <param name="skipSystemAssembly">是否过滤系统程序集</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="includeNames">必须包含的类型名称，用于过滤无效的类型</param>
		/// <returns>返回所有程序集列表</returns>
		public static HashSet<Type> CurrentTypes(bool includeBin = false, string pluginFolder = "plugins", bool skipSystemAssembly = true, bool publicOnly = false, params string[] includeNames) => [.. CurrentAssemblies(includeBin, pluginFolder, skipSystemAssembly).SelectMany(assembly => GetTypes(assembly, publicOnly, includeNames))];

		#endregion

		#region 创建实例

		/// <summary>根据类型创建实例</summary>
		/// <param name="filepath">指定程序集的路径</param>
		/// <param name="typeName">类型全名称，忽略大小写</param>
		/// <returns>创建的实例</returns>
		public static object CreateInstance(string filepath, string typeName) {
			if (string.IsNullOrEmpty(filepath) || string.IsNullOrEmpty(typeName)) {
				return null;
			}

			filepath = SystemHelper.FullPath(filepath);
			if (!File.Exists(filepath)) {
				return null;
			}

			try {
				return Assembly.LoadFrom(filepath)?.CreateInstance(typeName, true);
			} catch {
				return null;
			}
		}

		/// <summary>从程序集加载实例</summary>
		/// <param name="assembly">程序集</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="includeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<object> CreateInstances(Assembly assembly, bool publicOnly = false, params string[] includeNames) {
			var instances = new HashSet<object>();

			var types = GetTypes(assembly, publicOnly, includeNames);
			foreach (var type in types) {
				try {
					var instance = Activator.CreateInstance(type);
					if (instance != null) {
						instances.Add(instance);
					}
				} catch { }
			}

			return instances;
		}

		/// <summary>从程序集加载实例</summary>
		/// <param name="assembly">程序集</param>
		/// <param name="baseTypes">必须包含的基类，用于过滤无效的类型</param>
		public static HashSet<object> CreateInstances(Assembly assembly, params Type[] baseTypes) {
			var instances = new HashSet<object>();

			var types = GetTypes(assembly, baseTypes);
			foreach (var type in types) {
				try {
					var instance = Activator.CreateInstance(type);
					if (instance != null) {
						instances.Add(instance);
					}
				} catch { }
			}

			return instances;
		}

		/// <summary>从程序集加载实例</summary>
		/// <param name="assembly">程序集</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="includeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<T> CreateInstances<T>(Assembly assembly, bool publicOnly = false, params string[] includeNames) => [.. CreateInstances(assembly, publicOnly, includeNames).OfType<T>()];

		/// <summary>从程序集加载实例</summary>
		/// <param name="assembly">程序集</param>
		public static HashSet<T> CreateInstances<T>(Assembly assembly) => [.. CreateInstances(assembly, typeof(T)).OfType<T>()];

		/// <summary>创建指定目录下的指定类型的实例</summary>
		/// <param name="folder">指定目录，未指定则当前启动目录</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="assemblyNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		/// <param name="typeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<object> CreateInstances(string folder = null, bool publicOnly = false, IEnumerable<string> assemblyNames = null, IEnumerable<string> typeNames = null) {
			var assNames = assemblyNames?.ToArray() ?? [];
			var tpNames = typeNames?.ToArray() ?? [];
			var assemblies = LoadAssemblies(folder, assNames);

			return [.. assemblies.SelectMany(assembly => CreateInstances(assembly, publicOnly, tpNames))];
		}

		/// <summary>创建指定目录下的指定类型的实例</summary>
		/// <param name="folder">指定目录，未指定则当前启动目录</param>
		/// <param name="assemblyNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		/// <param name="baseTypes">必须包含的基类，用于过滤无效的类型</param>
		public static HashSet<object> CreateInstances(string folder = null, IEnumerable<string> assemblyNames = null, params Type[] baseTypes) {
			var assNames = assemblyNames?.ToArray() ?? [];
			var assemblies = LoadAssemblies(folder, assNames);

			return [.. assemblies.SelectMany(assembly => CreateInstances(assembly, baseTypes))];
		}

		/// <summary>创建指定目录下的指定类型的实例</summary>
		/// <param name="folder">指定目录，未指定则当前启动目录</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="assemblyNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		/// <param name="typeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<T> CreateInstances<T>(string folder = null, bool publicOnly = false, IEnumerable<string> assemblyNames = null, IEnumerable<string> typeNames = null) => [.. CreateInstances(folder, publicOnly, assemblyNames, typeNames).OfType<T>()];

		/// <summary>创建指定目录下的指定类型的实例</summary>
		/// <param name="includeBin">是否包含 Bin 目录</param>
		/// <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		/// <param name="skipSystemAssembly">是否过滤系统程序集</param>
		/// <param name="publicOnly">是否获取此程序集中定义的公共类型的集合，这些公共类型在程序集外可见。</param>
		/// <param name="assemblyNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		/// <param name="typeNames">必须包含的类型名称，用于过滤无效的类型</param>
		public static HashSet<object> CurrentInstances(bool includeBin = false, string pluginFolder = "plugins", bool skipSystemAssembly = true, bool publicOnly = false, IEnumerable<string> assemblyNames = null, IEnumerable<string> typeNames = null) {
			var assNames = assemblyNames?.ToArray() ?? [];
			var tpNames = typeNames?.ToArray() ?? [];
			var assemblies = CurrentAssemblies(includeBin, pluginFolder, skipSystemAssembly);

			return [.. assemblies.SelectMany(assembly => CreateInstances(assembly, publicOnly, tpNames))];
		}

		/// <summary>创建指定目录下的指定类型的实例</summary>
		/// <param name="includeBin">是否包含 Bin 目录</param>
		/// <param name="pluginFolder">是否需要加载插件目录，需要则设置插件目录的名称。系统将扫描查询目录下与子目录名相同的程序集。如：plugins 则将扫描 /plugins/xxx/xxx.dll</param>
		/// <param name="skipSystemAssembly">是否过滤系统程序集</param>
		/// <param name="assemblyNames">必须包含的程序集名称，用于过滤无效的程序集</param>
		public static HashSet<T> CurrentInstances<T>(bool includeBin = false, string pluginFolder = "plugins", bool skipSystemAssembly = true, IEnumerable<string> assemblyNames = null) {
			var assNames = assemblyNames?.ToArray() ?? [];
			var assemblies = CurrentAssemblies(includeBin, pluginFolder, skipSystemAssembly);

			return [.. assemblies.SelectMany(CreateInstances<T>)];
		}

		#endregion

	}
}
