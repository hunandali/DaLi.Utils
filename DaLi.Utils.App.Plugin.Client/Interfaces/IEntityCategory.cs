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
 * 	含分类属性的资源实体接口
 *
 * 	name: IEntityCategory.cs
 * 	create: 2025-6-5 12:26
 *
 * *********************************************************/

using DaLi.Utils.App.Base;

namespace DaLi.Utils.App.Interface {

	/// <summary>含分类属性的资源实体接口</summary>
	public interface IEntityCategory<T> : IEntityParent where T : EntityCategoryBase<T> {
		/// <summary>上级</summary>
		T Parent { get; set; }
	}
}
