/*
 * ------------------------------------------------------------
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
 *  流程接口
 * 
 * 	name: IFlow
 * 	create: 2025-03-14
 * 	memo: 流程接口，定义一个完整的流程结构
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using DaLi.Utils.Flow.Model;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Interface {

	/// <summary>流程接口</summary>
	public interface IFlow : ICloneable {

		/// <summary>流程名称</summary>
		string Name { get; set; }

		/// <summary>流程描述</summary>
		string Description { get; set; }

		/// <summary>流程作者</summary>
		string Author { get; set; }

		/// <summary>流程创建时间</summary>
		DateTime CreateTime { get; set; }

		/// <summary>流程最后更新时间</summary>
		DateTime UpdateTime { get; set; }

		/// <summary>调试模式</summary>
		bool Debug { get; set; }

		/// <summary>输入参数</summary>
		SODictionary Input { get; set; }

		/// <summary>输出结果参数规则，设置则 Result 则只输出 Output 中限定的内容，否则将输出整个流程的信息</summary>
		SODictionary Output { get; set; }

		/// <summary>最终执行结果，此值根据 Output 参数调整而变化</summary>
		SODictionary Result { get; set; }

		/// <summary>运行结果消息，正常返回 Output[result] 的值，否则返回错误信息</summary>
		string Message { get; set; }

		/// <summary>运行状态，成功或者失败</summary>
		bool Status { get; set; }

		/// <summary>流程规则</summary>
		List<RuleData> Rules { get; set; }
	}
}