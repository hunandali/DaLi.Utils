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
 *  流程规则接口
 * 
 * 	name: IRule
 * 	create: 2025-03-13
 * 	memo: 基础流程规则，类似黑盒子，无需知道如何实现，仅需要提供输出参数，分析出结果即可。至于最终结果是什么，由调用它的项目决定
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Interface {

	/// <summary>流程规则接口</summary>
	public interface IRule : IRuleBase, IDisposable {

		/// <summary>模块名称</summary>
		string Name { get; }

		/// <summary>对于存在子流程的项目是否将子流程的执行结果直接输出到主流程</summary>
		/// <remarks>
		/// - 对于不存在子流程的项目不用设置此值；<para />
		/// - 如果需要暴露结果则将忽略输出变量名；<para />
		/// - 用于暴露的子流程结果一定要是字典类型数据；<para />
		/// - true 则将子流程的结果输出到主流程；<para />
		/// - false 不输出，流程内的数据无法被外部使用。<para />
		/// 例如：计时器如果不将内部数据暴露到主流程，外部将无法获取到计时器的内流程的执行结果。
		/// </remarks>
		bool OutputAll { get; }

		/// <summary>验证规则项目是否异常</summary>
		bool Validate(ref string message);

		/// <summary>设置输入参数</summary>
		void SetInput(SODictionary input);

		/// <summary>执行操作，并返回当前的变量及相关值</summary>
		/// <param name="status">输入状态及参数，不能为空，必须传入</param>
		/// <param name="context">上下文数据，未设置将自动创建，并自动附加最终结果</param>
		/// <param name="cancel">取消令牌</param>
		/// <remarks>如果执行过程存在错误则直接触发异常</remarks>
		/// <returns>执行结果</returns>
		object Execute(ref ExecuteStatus status, ref SODictionary context, CancellationToken cancel);

		/// <summary>合并结果到上下文</summary>
		/// <param name="context">上下文</param>
		/// <param name="result">输出结果</param>
		object MergeResult(SODictionary context, object result);
	}
}