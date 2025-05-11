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
 *  枚举
 * 
 * 	name: Enums
 * 	create: 2025-03-13
 * 	memo: 枚举
 * 
 * ------------------------------------------------------------
 */

using System.ComponentModel;

namespace DaLi.Utils.Flow {

	/// <summary>错误类型枚举</summary>
	public enum ExceptionEnum {

		/// <summary>通用错误</summary>
		[Description("通用错误")]
		NORMAL,

		/// <summary>流程无效，如：流程错误</summary>
		[Description("流程无效")]
		FLOW_INVALID,

		/// <summary>规则无效，如：规则错误</summary>
		[Description("规则无效")]
		RULE_INVALID,

		/// <summary>输入参数无效，如：将输出参数转换成规则属性时出错</summary>
		[Description("输入参数无效")]
		RULE_INPUT_INVALID,

		/// <summary>规则已经禁用</summary>
		[Description("规则已经禁用")]
		RULE_DISABLED,

		/// <summary>规则验证失败</summary>
		[Description("规则验证失败")]
		RULE_VALIDATE,

		/// <summary>执行错误</summary>
		[Description("执行错误")]
		EXECUTE_ERROR,

		/// <summary>内部错误</summary>
		[Description("内部错误")]
		INNER_EXCEPTION,

		/// <summary>无结果</summary>
		[Description("无结果")]
		NO_RESULT,

		/// <summary>循环停止</summary>
		[Description("循环停止")]
		LOOP_STOP,

		/// <summary>循环中断</summary>
		[Description("循环中断")]
		LOOP_BREAK,

		/// <summary>值验证失败</summary>
		[Description("值验证失败")]
		VALUE_VALIDATE
	}

	/// <summary>值操作枚举</summary>
	public enum ValueOperateEnum {
		/// <summary>等于</summary>
		[Description("等于")]
		EQUAL,

		/// <summary>不等于</summary>
		[Description("不等于")]
		NOT_EQUAL,

		/// <summary>大于</summary>
		[Description("大于")]
		GREATER,

		/// <summary>大于等于</summary>
		[Description("大于等于")]
		GREATER_EQUAL,

		/// <summary>小于</summary>
		[Description("小于")]
		LESS,

		/// <summary>小于等于</summary>
		[Description("小于等于")]
		LESS_EQUAL,

		/// <summary>包含</summary>
		[Description("包含")]
		CONTAINS,

		/// <summary>不包含</summary>
		[Description("不包含")]
		NOT_CONTAINS
	}
}