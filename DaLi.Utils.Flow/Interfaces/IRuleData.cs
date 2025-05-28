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
 *  流程规则接口参数
 * 
 * 	name: IRule
 * 	create: 2025-03-13
 * 	memo: 流程规则接口参数
 * 
 * ------------------------------------------------------------
 */

namespace DaLi.Utils.Flow.Interface {

	/// <summary>流程规则接口</summary>
	public interface IRuleData : IRuleBase {

		/// <summary>模块类型</summary>
		string Type { get; }
	}
}