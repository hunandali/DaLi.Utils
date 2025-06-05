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
 * 	数据分类基类
 *
 * 	name: EntityCategoryBase.cs
 * 	create: 2025-6-5 12:30
 *
 * *********************************************************/

using System.ComponentModel.DataAnnotations;
using DaLi.Utils.App.Attribute;
using DaLi.Utils.App.Entity;
using DaLi.Utils.App.Model;
using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;

namespace DaLi.Utils.App.Base {
	/// <summary>数据分类基类</summary>
	public abstract class EntityCategoryBase<T> : EntityTreeDateBase<T> where T : EntityCategoryBase<T> {
		/// <summary>资源类型</summary>
		[DbQuery]
		public uint ModuleId { get; set; }

		/// <summary>资源类型</summary>
		[Navigate(nameof(ModuleId))]
		public ModuleEntity Module { get; set; }

		/// <summary>名称</summary>
		[Required]
		[MaxLength(100)]
		[DbQuery(DynamicFilterOperator.Contains)]
		[BadKeyword(KeywordCheckEnum.REPLACE)]
		public string Name { get; set; }

		/// <summary>备注</summary>
		[Required]
		[MaxLength(250)]
		[DbQuery(DynamicFilterOperator.Contains)]
		[BadKeyword(KeywordCheckEnum.REPLACE)]
		public string Memo { get; set; }

		/// <summary>启用</summary>
		[DbQuery]
		public bool Enabled { get; set; }
	}
}
