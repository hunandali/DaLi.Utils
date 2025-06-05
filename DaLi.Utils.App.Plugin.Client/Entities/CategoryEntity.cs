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
 * 	通用分类
 *
 * 	name: CategoryEntity.cs
 * 	create: 2025-6-5 12:45
 *
 * *********************************************************/

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DaLi.Utils.App.Attribute;
using DaLi.Utils.App.Bases;
using DaLi.Utils.App.Interface;
using DaLi.Utils.App.Model;
using DaLi.Utils.Attribute;
using DaLi.Utils.Model;
using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;

namespace DaLi.Utils.App.Entities {
	/// <summary>通用分类</summary>
	[DbTable("App_Categories")]
	[DbIndex(nameof(ParentId))]
	[DbIndex([nameof(ModuleId), nameof(Name)], true)]
	[DbModule(6, "通用分类")]
	public class CategoryEntity : EntityCategoryBase<CategoryEntity>, IEntityExtend {
		/// <summary>说明</summary>
		[BadKeyword(KeywordCheckEnum.REPLACE)]
		public string Content {
			get => Extension["Content"];
			set => Extension["Content"] = value;
		}

		/// <summary>扩展内容</summary>
		[Display(Name = "Extension")]
		[JsonIgnore]
		[JsonMap]
		[DbColumn(Position = -5)]
		[Output(TristateEnum.FALSE)]
		[DbQuery(DynamicFilterOperator.Contains)]
		public NameValueDictionary Extension { get; set; } = [];
	}
}
