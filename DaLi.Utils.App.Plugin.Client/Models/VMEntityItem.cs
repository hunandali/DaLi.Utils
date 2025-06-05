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
 * 	单项操作视图
 *
 * 	name: VMEntityItem.cs
 * 	create: 2025-6-5 13:30
 *
 * *********************************************************/

using System;
using DaLi.Utils.App.Base;
using DaLi.Utils.App.Interface;

namespace DaLi.Utils.App.Model {
	/// <summary>单项操作视图</summary>
	public class VMEntityItem<T> : VMEntityItemBase<T> where T : class, IEntity {

		/// <summary>当前操作用户</summary>
		public override string User =>
				$"{Environment.UserName}({Environment.MachineName})";
	}
}
