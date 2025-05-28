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
 *  流程规则接口基类
 * 
 * 	name: IRuleBase
 * 	create: 2025-05-26
 * 	memo: 流程规则接口基类
 * 
 * ------------------------------------------------------------
 */

using System;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Interface {

	/// <summary>流程规则接口基类</summary>
	public interface IRuleBase : ICloneable {

		/// <summary>输出变量名</summary>
		string Output { get; set; }

		/// <summary>是否忽略无结果</summary>
		/// <remarks>False：当未分析到任何内容时报错；True：当未分析到任何内容时忽略此问题</remarks>
		bool EmptyIgnore { get; set; }

		/// <summary>忽略错误</summary>
		/// <remarks>
		/// TRUE：忽略所有错误<para />
		/// DEFAULT: 仅忽略执行错误，规则错误不忽略<para />
		/// FALSE: 不忽略
		/// </remarks>
		TristateEnum ErrorIgnore { get; set; }

		/// <summary>友好错误消息</summary>
		/// <remarks>仅针对未忽略错误时执行中的错误</remarks>
		string ErrorMessage { get; set; }

		/// <summary>启用</summary>
		bool Enabled { get; set; }
	}
}