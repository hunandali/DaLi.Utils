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
 * 	应用控制器基类
 *
 * 	name: CtrDataBase.cs
 * 	create: 2025-6-5 14:51
 *
 * *********************************************************/

using DaLi.Utils.App.Interface;
using DaLi.Utils.App.Model;

namespace DaLi.Utils.App.Base {
	/// <summary>应用控制器基类</summary>
	public abstract class CtrDataBase<T> : CtrEntityBase<T, VMEntityItem<T>, VMEntityList<T>, QueryBase<T>>
	where T : class, IEntity {
	}
}
