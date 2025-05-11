' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	循环基类
'
' 	name: Auto.Rule.WhileBase
' 	create: 2023-01-23
' 	memo: 循环基类
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Threading

Namespace Auto.Rule
	''' <summary>循环基类</summary>
	Public MustInherit Class WhileBase
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>循环结果值，如果存在此变量名，则使用变量值，否则使用文本</summary>
		Public Property Result As String

		''' <summary>求和字段，循环中用于累积求和的变量或者文本</summary>
		Public Property Sum As String

		''' <summary>循环体中执行的规则</summary>
		Public Property Rules As String

		''' <summary>单线程或多线程并行执行，小于 2 单线程，大于 1 异步并行执行线程数</summary>
		Public Property ParallelNumber As Integer

#End Region

#Region "INFORMATION"

		''' <summary>验证规则是否存在异常</summary>
		Public Overrides Function Validate(ByRef Optional message As String = Nothing) As Boolean
			message = "循环执行规则未设置或者异常"
			Dim ruleList = AutoHelper.RuleList(Rules, True)
			If ruleList.IsEmpty Then Return False

			Return MyBase.Validate(message)
		End Function

#End Region

#Region "EXECUTE"

		''' <summary>执行循环操作</summary>
		''' <param name="min">最小值</param>
		''' <param name="max">最大值</param>
		''' <param name="[step]">迭代之间的增量</param>
		''' <param name="action">获取循环参数</param>
		''' <param name="message">循环的主体的消息数据</param>
		''' <param name="data">上下文中数据</param>
		Protected Function WhileExecute(min As Integer, max As Integer, [step] As Integer,
										data As IDictionary(Of String, Object),
										action As Func(Of Integer, IDictionary(Of String, Object)),
										message As AutoMessage, cancel As CancellationToken) As IDictionary(Of String, Object)
			message.Message = "无效循环迭代参数"
			If action Is Nothing Then Return Nothing

			message.Message = "最大值必须大于等于最小值"
			If min > max Then Return Nothing

			message.Message = "进度不能为 0"
			If [step] = 0 Then Return Nothing

			' 周期小于 0，倒序
			If [step] < 0 AndAlso max > min Then Swap(min, max)

			' 非安全数组重建
			If data Is Nothing OrElse TypeOf data IsNot SafeKeyValueDictionary Then data = New SafeKeyValueDictionary(data)

			' 所有需要处理的索引值
			Dim indexs As New List(Of Integer)
			For idx = min To max Step [step]
				indexs.Add(idx)
			Next

			' 无有效索引列表，返回
			If indexs.Count = 0 Then Return Nothing

			Return WhileExecute(indexs, data, Function(x)
												  Dim loopData = If(action(x), New Dictionary(Of String, Object))
												  If Not loopData.ContainsKey("_min") Then loopData.Add("_min", min)
												  If Not loopData.ContainsKey("_max") Then loopData.Add("_max", max)
												  If Not loopData.ContainsKey("_count") Then loopData.Add("_count", 0)
												  If Not loopData.ContainsKey("_interval") Then loopData.Add("_interval", [step])
												  Return loopData
											  End Function, message, cancel)
		End Function

		''' <summary>执行循环操作</summary>
		''' <param name="indexs">所有需要迭代的索引值</param>
		''' <param name="action">获取循环参数</param>
		''' <param name="message">循环的主体的消息数据</param>
		''' <param name="data">上下文中数据</param>
		''' <returns>执行结果</returns>
		Protected Function WhileExecute(indexs As IEnumerable(Of Integer),
										data As SafeKeyValueDictionary,
										action As Func(Of Integer, IDictionary(Of String, Object)),
										message As AutoMessage, cancel As CancellationToken) As IDictionary(Of String, Object)
			message.Message = "无效循环迭代参数"
			If indexs.IsEmpty OrElse action Is Nothing Then Return Nothing

			Dim ret As New ConcurrentBag(Of Object)
			Dim retIndex As New ConcurrentBag(Of Object)
			Dim retSum As New ConcurrentBag(Of Single)

			' 规则列表
			Dim ruleList = AutoHelper.RuleList(Rules, True)

			' 执行循环内规则，并返回是否强制中断（True 执行正常，False 强制中断）
			Dim execute = Function(index As Integer) As Boolean
							  Dim executeMessage As New AutoMessage
							  Dim success = True

							  Try
								  ' 获取当前循环变量
								  Dim loopData = If(action(index), New Dictionary(Of String, Object))

								  ' 注意此索引不一定是序列 Interger，可能是来自对象的 Key
								  Dim _index = Nothing
								  If Not loopData.TryGetValue("_index", _index) Then
									  _index = index
									  loopData.Add("_index", index)
								  End If

								  ' 附加参数
								  Dim exchange As New SafeKeyValueDictionary(data)
								  exchange.Update(loopData)

								  ' 执行内部规则
								  Dim dic = AutoHelper.FlowExecute(ruleList, False, False, exchange, executeMessage, cancel)

								  ' 处理值结果
								  If Result.NotEmpty AndAlso ret IsNot Nothing Then
									  Dim loopValue = AutoHelper.GetVar(Result, exchange)
									  If loopValue IsNot Nothing Then ret.Add(loopValue)
								  End If

								  ' 处理求和
								  If Sum.NotEmpty AndAlso retSum IsNot Nothing Then
									  Dim loopSum = AutoHelper.GetVarString(Sum, exchange, True).ToSingle(True)
									  If loopSum <> 0 Then retSum.Add(loopSum)
								  End If

								  ' 记录索引
								  retIndex.Add(_index)

								  ' 更新结果到全局变量
								  If dic.NotEmpty Then
									  ' 排除所有下划线变量
									  dic = dic.Where(Function(x) Not x.Key.StartsWith("_"c)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
									  AutoHelper.UpdateData(data, dic)
								  End If
							  Catch ex As AutoException
								  executeMessage.Message = ex.Message

								  ' 出现强制退出循环，标识执行失败
								  success = ex.AutoType <> ExceptionEnum.LOOP_STOP
							  End Try

							  ' 执行消息结果记录到循环体
							  message.Add(executeMessage)

							  ' 返回是否强制中断
							  Return success
						  End Function

			'---------------------
			' 执行循环操作
			'---------------------

			' 小于 2 单线程，否则并行多线程处理循环
			If ParallelNumber < 2 Then
				For Each idx In indexs
					' LOOP_STOP 强制终止循环
					If Not execute(idx) Then Exit For
				Next
			Else
				' Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
				Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
				Parallel.ForEach(indexs, Sub(idx, state)
											 ' LOOP_STOP 强制终止循环
											 If Not execute(idx) Then state.Stop()
										 End Sub)
			End If

			message.SetSuccess()
			Return New Dictionary(Of String, Object) From {{"count", ret.Count}, {"run", retIndex.Count}, {"data", ret}, {"sum", retSum.Sum}, {"index", retIndex}}
		End Function

#End Region

		'#Region "EXECUTE"

		'''' <summary>执行循环内部规则操作</summary>
		'''' <param name="rules">内部规则列表</param>
		'''' <param name="loopData">循环中数据</param>
		'''' <param name="contextData">上下文中数据</param>
		'''' <param name="varResult">返回值模板</param>
		'''' <param name="result">返回值列表</param>
		'''' <param name="varSum">求和字段</param>
		'''' <param name="resultSUM">求和值</param>
		'''' <remarks>执行的消息结果</remarks>
		'Protected Shared Function WhileExecute(rules As IList(Of IRule), contextData As SafeKeyValueDictionary, loopData As IDictionary(Of String, Object), varResult As String, result As ConcurrentBag(Of Object), varSum As String, resultSum As ConcurrentBag(Of Single)) As AutoMessage
		'	Dim message As New AutoMessage

		'	Dim exchange As New SafeKeyValueDictionary(contextData)
		'	exchange.Update(loopData)

		'	Dim dic = AutoHelper.FlowExecute(rules, False, False, exchange, message)

		'	If varResult.NotEmpty AndAlso result IsNot Nothing Then
		'		Dim loopValue = AutoHelper.GetVar(varResult, exchange)
		'		If loopValue IsNot Nothing Then result.Add(loopValue)
		'	End If

		'	If varSum.NotEmpty AndAlso resultSum IsNot Nothing Then
		'		Dim loopSum = AutoHelper.GetVarString(varSum, exchange, True).ToSingle(True)
		'		If loopSum <> 0 Then resultSum.Add(loopSum)
		'	End If

		'	' 异常退出
		'	If dic.NotEmpty Then
		'		' 排除所有下划线变量
		'		dic = dic.Where(Function(x) Not x.Key.StartsWith("_")).ToDictionary(Function(x) x.Key, Function(x) x.Value)
		'		AutoHelper.UpdateData(contextData, dic)
		'	End If

		'	Return message
		'End Function

		'''' <summary>执行循环操作</summary>
		'''' <param name="indexs">所有需要迭代的索引值</param>
		'''' <param name="action">执行的操作，返回消息数据</param>
		'''' <param name="message">循环的主体的消息数据</param>
		'Protected Sub WhileExecute(indexs As IEnumerable(Of Integer), action As Func(Of Integer, AutoMessage), message As AutoMessage)
		'	message.Message = "无效循环迭代参数"
		'	If indexs.IsEmpty OrElse action Is Nothing Then Return

		'	' 处理迭代操作
		'	Dim execute As Func(Of Integer, Boolean) =
		'		Function(index)
		'			Try
		'				Dim ret = action(index)
		'				message.Add(ret)
		'				Return ret.Success
		'			Catch ex As AutoException
		'				' 强制终止循环
		'				If ex.AutoType = ExceptionEnum.LOOP_STOP Then Return False

		'				'' 跳过当前循环
		'				'If ex.AutoType = ExceptionEnum.LOOP_BREAK Then Continue For
		'			End Try

		'			Return True
		'		End Function

		'	' 小于 2 单线程，否则并行多线程处理循环
		'	If ParallelNumber < 2 Then
		'		For Each idx In indexs
		'			' LOOP_STOP 强制终止循环
		'			If Not execute(idx) Then Exit For
		'		Next
		'	Else
		'		' Parallel 循环最大值没有包含值本身，所以需要 + 1，否则少一项
		'		Dim options As New ParallelOptions With {.MaxDegreeOfParallelism = ParallelNumber.Range(1, 100)}
		'		Parallel.ForEach(indexs, Sub(idx, state)
		'									 ' LOOP_STOP 强制终止循环
		'									 If Not execute(idx) Then state.Stop()
		'								 End Sub)
		'	End If
		'End Sub

		'''' <summary>执行循环操作</summary>
		'''' <param name="min">最小值</param>
		'''' <param name="max">最大值</param>
		'''' <param name="[step]">迭代之间的增量</param>
		'''' <param name="action">执行的操作，返回消息数据</param>
		'Protected Sub WhileExecute(min As Integer, max As Integer, [step] As Integer, action As Func(Of Integer, AutoMessage), message As AutoMessage)
		'	Dim indexs As New List(Of Integer)

		'	' 所有需要处理的索引值
		'	For idx = min To max Step [step]
		'		indexs.Add(idx)
		'	Next

		'	WhileExecute(indexs, action, message)
		'End Sub

		'		''' <summary>循环内部规则操作</summary>
		'		''' <param name="rules">内部规则列表</param>
		'		''' <param name="loopData">循环中数据</param>
		'		''' <param name="contextData">上下文中数据</param>
		'		''' <param name="message">消息状态</param>
		'		''' <param name="varResult">返回值模板</param>
		'		''' <param name="result">返回值列表</param>
		'		''' <param name="varSum">求和字段</param>
		'		''' <param name="resultSUM">求和值</param>
		'		Protected Shared Function WhileExecute(rules As IList(Of IRule), contextData As SafeKeyValueDictionary, loopData As IDictionary(Of String, Object), varResult As String, result As ConcurrentBag(Of Object), varSum As String, resultSum As ConcurrentBag(Of Single), message As AutoMessage) As Boolean
		'			Try
		'				Dim exchange As New SafeKeyValueDictionary(contextData)
		'				exchange.Update(loopData)

		'				Dim dic = AutoHelper.FlowExecute(rules, False, False, exchange, message)

		'				If varResult.NotEmpty AndAlso result IsNot Nothing Then
		'					Dim loopValue = AutoHelper.GetVar(varResult, exchange)
		'					If loopValue IsNot Nothing Then result.Add(loopValue)
		'				End If

		'				If varSum.NotEmpty AndAlso resultSum IsNot Nothing Then
		'					Dim loopSum = AutoHelper.GetVarString(varSum, exchange, True).ToSingle(True)
		'					If loopSum <> 0 Then resultSum.Add(loopSum)
		'				End If

		'				' 异常退出
		'				If dic Is Nothing Then
		'					Return False
		'				Else
		'					' 排除所有下划线变量
		'					dic = dic.Where(Function(x) Not x.Key.StartsWith("_")).ToDictionary(Function(x) x.Key, Function(x) x.Value)
		'					AutoHelper.UpdateData(contextData, dic)
		'				End If
		'			Catch ex As AutoException
		'				message.Message = ex.Message

		'				' 退出循环
		'				If ex.AutoType = ExceptionEnum.LOOP_STOP Then Return False

		'				' 其他异常无需处理
		'			End Try

		'			Return True
		'		End Function

		'#End Region

	End Class
End Namespace