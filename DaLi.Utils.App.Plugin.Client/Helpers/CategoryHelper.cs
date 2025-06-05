/* **********************************************************
 *
 * 	Copyright © 2025 湖南大沥网络科技有限公司.
 *
 * 	  author:	木炭(WOODCOAL)
 * 	   email:	i@woodcoal.cn
 * 	homepage:	http://www.hunandali.com/
 *
 * 	Dali.Utils libs(Packages) Is licensed under Mulan PSL v2.
 * 	请依据 Mulan PSL v2 的条款使用本项目。
 * 	获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
 *
 * ------------------------------------------------------------
 *
 * 	分类相关操作
 *
 * 	name: CategoryHelper.cs
 * 	create: 2025-6-5 14:55
 *
 * *********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using DaLi.Utils.App.Base;
using DaLi.Utils.App.Bases;
using DaLi.Utils.App.Entities;
using DaLi.Utils.App.Extension;
using DaLi.Utils.App.Helper;
using DaLi.Utils.App.Interface;
using DaLi.Utils.App.Model;

namespace DaLi.Utils.App.Helpers {
	/// <summary>分类相关操作</summary>
	public static class CategoryHelper {
		/// <summary>缓存时间(秒)</summary>
		/// <remarks>信息缓存时间，如果用于列表缓存将是此值的 5 倍；默认调试模式为 10 秒，正式模式为 60 秒</remarks>
		public static int CACHE_TIME { get; set; } = 10;

		/// <summary>缓存列表</summary>
		private static readonly LRU<string, (DateTime Last, List<DataList<long>> Data)> _Cache = new(200);

		#region 分类

		/// <summary>获取分类列表</summary>
		/// <param name="db">数据库上下文</param>
		/// <param name="moduleId">数据模型标识</param>
		/// <param name="queryVM">上级查询条件</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Categories<P>(IFreeSql db, uint moduleId, QueryBase<P> queryVM, string extField = null) where P : EntityCategoryBase<P> {
			if (queryVM == null || moduleId < 1) {
				return null;
			}

			// 强制使用当前模块
			queryVM.Update("moduleId", moduleId, true);

			var cacheKey = $"{typeof(P).FullName}_{queryVM.GetHash}_{extField}";
			var (Last, Data) = _Cache.Get(cacheKey);
			if (Last > DateTime.Now) {
				return Data;
			}

			// 从查询数据中分析头部信息
			var id = queryVM.GetValue<long?>("id");
			if (id.HasValue) {
				queryVM.Remove("id");
			}

			// 无数据或过期，重新加载
			var data = db.SelectTree<P>(id, q => queryVM.QueryExecute(q, false), false, true)
				.ToDataList(null, null, "Name", null, extField)?
				.Select(x => new DataList<long>(x))
				.ToList();

			// 分类缓存时间 3 倍
			_Cache.Put(cacheKey, (DateTime.Now.AddSeconds(CACHE_TIME * 3), data));

			return data;
		}

		/// <summary>获取分类列表</summary>
		/// <param name="db">数据库上下文</param>
		/// <param name="queryVM">上级查询条件</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Categories<P, E>(IFreeSql db, QueryBase<P> queryVM, string extField = null)
			where P : EntityCategoryBase<P>
			where E : IEntity {
			var moduleId = ExtendHelper.GetModuleId<E>();
			if (moduleId < 1) {
				return null;
			}

			return Categories(db, moduleId, queryVM, extField);
		}

		/// <summary>获取分类列表</summary>
		/// <param name="db">数据库上下文</param>
		/// <param name="id">当前标识</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Categories<P, E>(IFreeSql db, long? id, string extField = null)
			where P : EntityCategoryBase<P>
			where E : IEntity {
			var queryVM = new QueryBase<P>();
			if (id.HasValue) {
				queryVM.Update("id", id.Value, true);
			}

			return Categories<P, E>(db, queryVM, extField);
		}

		/// <summary>获取通用分类列表</summary>
		/// <param name="db"></param>
		/// <param name="id">当前标识</param>
		/// <param name="extField"></param>
		public static List<DataList<long>> Categories<T>(IFreeSql db, long? id, string extField = null)
			where T : class, IEntityCategory<CategoryEntity> => Categories<CategoryEntity, T>(db, id, extField);

		#endregion

		#region 资源

		/// <summary>获取指定分类下资源的数据，并以列表结构展示</summary>
		/// <param name="db"></param>
		/// <param name="categories">分类数据列表</param>
		/// <param name="queryVM">资源查询条件</param>
		/// <param name="nameField">显示名称字段</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Resources<P, E>(IFreeSql db, List<DataList<long>> categories, QueryBase<E> queryVM = null, string nameField = "Name", string extField = null)
			where P : EntityCategoryBase<P>
			where E : class, IEntityCategory<P> {
			if (categories == null || categories.Count == 0) {
				return null;
			}

			// 存在栏目则分析与栏目相关的信息
			var cateIds = categories.Select(x => x.Value).ToList();

			// 查询
			var query = db.Select<E>();
			queryVM?.QueryExecute(query);

			var infos = query
				.WhereContain(cateIds, x => x.ParentId.Value)
				.ToList()
				.ToDataList(null, null, nameField, null, extField)?
				.Select(x => new DataList<long>(x))
				.ToList();

			if (infos == null || infos.Count == 0) {
				return null;
			}

			// 资源列表
			var list = new List<DataList<long>>(infos);

			// 反推，移除无内容的父级分类
			void findParents(List<DataList<long>> datas) {
				var ids = datas.Select(x => x.Parent).Distinct().ToList();
				var parents = categories.Where(x => ids.Contains(x.Value)).ToList();
				if (parents != null && parents.Count > 0) {
					list.AddRange(parents);

					// 继续迭代
					findParents(parents);
				}
			}

			findParents(infos);

			return list;
		}

		/// <summary>获取指定分类下资源的数据，并以列表结构展示</summary>
		/// <param name="db"></param>
		/// <param name="queryCateVM">上级查询条件</param>
		/// <param name="queryInfoVM">资源查询条件</param>
		/// <param name="nameField">显示名称字段</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Resources<P, E>(IFreeSql db, QueryBase<P> queryCateVM, QueryBase<E> queryInfoVM = null, string nameField = "Name", string extField = null)
			where P : EntityCategoryBase<P>
			where E : class, IEntityCategory<P> {
			if (queryCateVM == null) {
				return null;
			}

			var moduleId = ExtendHelper.GetModuleId<E>();
			if (moduleId < 1) {
				return null;
			}

			// 强制使用当前模块
			queryCateVM.Update("moduleId", moduleId, true);

			// 注意缓存键，需要与分类的缓存键区别
			var cacheKey = $"_{typeof(P).FullName}_{queryCateVM.GetHash}_{queryInfoVM?.GetHash()}_{nameField}_{extField}";
			var (Last, Data) = _Cache.Get(cacheKey);
			if (Last > DateTime.Now) {
				return Data;
			}

			// 无数据或过期，重新加载
			var cates = Categories<P, E>(db, queryCateVM);
			var infos = Resources<P, E>(db, cates, queryInfoVM, nameField, extField);

			// 缓存
			_Cache.Put(cacheKey, (DateTime.Now.AddSeconds(CACHE_TIME), infos));

			return infos;
		}

		/// <summary>获取指定分类下资源的数据，并以列表结构展示</summary>
		/// <param name="db"></param>
		/// <param name="parentId">上级标识</param>
		/// <param name="queryVM">资源查询条件</param>
		/// <param name="nameField">显示名称字段</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Resources<P, E>(IFreeSql db, long? parentId, QueryBase<E> queryVM = null, string nameField = "Name", string extField = null)
			where P : EntityCategoryBase<P>
			where E : class, IEntityCategory<P> {
			var queryParentVM = new QueryBase<P>();
			if (parentId.HasValue) {
				queryParentVM.Update("parentId", parentId.Value, true);
			}

			return Resources(db, queryParentVM, queryVM, nameField, extField);
		}

		/// <summary>获取通用分类下资源的数据，并以列表结构展示</summary>
		/// <param name="db"></param>
		/// <param name="parentId">上级标识</param>
		/// <param name="queryVM">资源查询条件</param>
		/// <param name="nameField">显示名称字段</param>
		/// <param name="extField">扩展字段名</param>
		public static List<DataList<long>> Resources<T>(IFreeSql db, long? parentId, QueryBase<T> queryVM = null, string nameField = "Name", string extField = null)
			where T : class, IEntityCategory<CategoryEntity> => Resources<CategoryEntity, T>(db, parentId, queryVM, nameField, extField);

		#endregion
	}
}
