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
 * 	通用分类控制器基类
 *
 * 	name: CtrCategoryBase.cs
 * 	create: 2025-6-5 14:54
 *
 * *********************************************************/

using System.ComponentModel;
using DaLi.Utils.App.Extension;
using DaLi.Utils.App.Helper;
using DaLi.Utils.App.Interface;
using DaLi.Utils.App.Model;
using FreeSql;
using Microsoft.AspNetCore.Mvc;

namespace DaLi.Utils.App.Base {
	/// <summary>通用分类控制器基类</summary>
	public abstract class CtrCategoryBase<P, E> : CtrDataBase<P>
		where P : EntityCategoryBase<P>, new()
		where E : class, IEntityCategory<P> {

		/// <summary>当前模型</summary>
		protected uint ModuleId => ExtendHelper.GetModuleId<E>();

		#region 事件

		/// <inheritdoc/>
		protected override void ExecuteQuery(EntityActionEnum action, ISelect<P> query, QueryBase<P> queryVM = null) {
			// 强制操作模块
			query.WhereEquals(ModuleId, x => x.ModuleId);
			base.ExecuteQuery(action, query, queryVM);
		}

		/// <inheritdoc/>
		protected override void ExecuteValidate(EntityActionEnum action, P entity, P source = null) {
			switch (action) {
				case EntityActionEnum.ADD:
					// 添加时强制模型类型
					entity.ModuleId = ModuleId;
					AppContext.Fields.Add("ModuleId", entity.ModuleId);
					break;

				case EntityActionEnum.ITEM:
				case EntityActionEnum.EDIT:
				case EntityActionEnum.DELETE:
					if (entity.ModuleId != ModuleId) {
						ErrorMessage.Notification = "模型类型不匹配";
					}
					break;
			}

			base.ExecuteValidate(action, entity, source);
		}

		#endregion

		#region 分类

		/// <summary>分类列表</summary>
		[HttpGet("categories")]
		[Description("分类列表")]
		[ResponseCache(Duration = 30)]
		public IActionResult Categories(long? parentId = null) => ExecuteByList(vm => CategoryHelper.Categories<P, E>(Db, parentId), EntityActionEnum.LIST, "获取分类列表");

		/// <summary>分类列表</summary>
		[NonAction]
		public virtual IActionResult CategoryQuery(QueryBase<P> queryVM = null) => ExecuteByList(vm => CategoryHelper.Categories<P, E>(Db, queryVM), EntityActionEnum.LIST, "获取分类列表");

		#endregion

		#region 资源

		/// <summary>资源列表</summary>
		/// <param name="parentId">分类标识</param>
		/// <param name="queryVM">信息查询</param>
		/// <param name="nameField">信息文本字段</param>
		/// <param name="extField">信息扩展字段</param>
		/// <param name="isTree">是否以树形结构返回数据</param>
		[NonAction]
		public virtual IActionResult ResourcesList(long parentId, QueryBase<E> queryVM = null, string nameField = null, string extField = null, bool isTree = false) => ExecuteById(parentId.ToString(), (entity, vm) => {
			var infos = CategoryHelper.Resources<P, E>(Db, entity.ID, queryVM, nameField, extField);
			return isTree ? infos.ToDataTree(parentId, true) : infos;
		}, EntityActionEnum.LIST, "获取资源列表");

		/// <summary>资源列表</summary>
		/// <param name="queryCateVM">分类查询</param>
		/// <param name="queryInfoVM">信息查询</param>
		/// <param name="nameField">信息文本字段</param>
		/// <param name="extField">信息扩展字段</param>
		[NonAction]
		public virtual IActionResult ResourcesQuery(QueryBase<P> queryCateVM, QueryBase<E> queryInfoVM = null, string nameField = null, string extField = null) => ExecuteByList(vm => CategoryHelper.Resources(Db, queryCateVM, queryInfoVM, nameField, extField), EntityActionEnum.LIST, "获取资源列表");

		/// <summary>资源列表</summary>
		/// <param name="queryVM">信息查询</param>
		/// <param name="nameField">信息文本字段</param>
		/// <param name="extField">信息扩展字段</param>
		/// <param name="isTree">是否以树形结构返回数据</param>
		[NonAction]
		public virtual IActionResult Resources(QueryBase<E> queryVM, string nameField = null, string extField = null, bool isTree = false) {
			if (queryVM == null) {
				return Err();
			}

			// 从查询参数中获取上级标识
			long? parentId = null;
			if (queryVM.ContainsKey("parentId")) {
				parentId = queryVM.GetValue<long>("parentId");
				queryVM.Remove("parentId");
			}

			return ExecuteByList(vm => {
				var infos = CategoryHelper.Resources<P, E>(Db, parentId, queryVM, nameField, extField);
				return isTree ? infos.ToDataTree((long) parentId, true) : infos;
			}, EntityActionEnum.LIST, "获取资源列表");
		}

		#endregion
	}
}
