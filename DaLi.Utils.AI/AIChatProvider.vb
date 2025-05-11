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
' 	AI 对话统一接口
'
' 	name: AIChatProvider
' 	create: 2024-12-20
' 	memo: AI 对话统一接口
'
' ------------------------------------------------------------

Imports System.ClientModel
Imports System.Text
Imports System.Threading
Imports DaLi.Utils.AI.Model
Imports DaLi.Utils.Json.JsonExtension
Imports Microsoft.Extensions.AI
Imports OpenAI

''' <summary>AI 对话统一接口</summary>
Public Class AIChatProvider

	''' <summary>系统角色，具体的提示内容</summary>
	Public Property SystemPrompt As String

	''' <summary>是否返回 JSON 格式数据</summary>
	Public Property JsonResult As Boolean

	''' <summary>最多保留对话上下问数量</summary>
	Public Property Rounds As Integer = 20

	''' <summary>聊天历史记录</summary>
	Public ReadOnly History As List(Of ChatMessage)

	''' <summary>聊天客户端</summary>
	Private ReadOnly _ChatClient As IChatClient

	''' <summary>构造</summary>
	''' <param name="type">AI 接口类型</param>
	''' <param name="url">Api 地址</param>
	''' <param name="model">模型</param>
	Public Sub New(type As AIClientEnum, Optional url As String = "", Optional key As String = "", Optional model As String = "")
		Select Case type
			Case AIClientEnum.OLLAMA
				_ChatClient = New OllamaChatClient(url, model)

			Case AIClientEnum.OPENAI
				_ChatClient = New OpenAIChatClient(New OpenAIClient(New ApiKeyCredential(key),
												   New OpenAIClientOptions With {.Endpoint = New Uri(url), .NetworkTimeout = TimeSpan.FromMinutes(5)}),
												   model)
			Case Else
				Throw New Exception("暂无此 AI 接口")
		End Select

		History = New List(Of ChatMessage)
	End Sub

	''' <summary>更新历史信息，移除多余的历史数据</summary>
	''' <param name="renew">是否清空所有历史，重新开始对话</param>
	Public Sub UpdateMessage(Optional renew As Boolean = False)
		If renew Then History.Clear()

		' 检查尺寸
		Dim Max = Rounds * 2
		If Max < 1 Then Max = 10

		If Max = 0 Then
			History.Clear()
		ElseIf History.Count >= Max Then
			History.RemoveRange(0, History.Count - Max + 1)
		End If

		' 检查是否存在角色提示
		If SystemPrompt.NotEmpty Then
			Dim role = History.Where(Function(x) x.Role = ChatRole.System).LastOrDefault

			' 角色不存在或者角色内容有变化
			If role Is Nothing Then
				History.Insert(0, New ChatMessage With {.Role = ChatRole.System, .Text = SystemPrompt})
			ElseIf role.Text <> SystemPrompt Then
				role.Text = SystemPrompt
			End If
		End If
	End Sub

	''' <summary>将信息带入更新信息的上下文</summary>
	Private Sub UpdateMessage(message As ChatMessage)
		If message IsNot Nothing Then History.Add(message)

		Call UpdateMessage(False)
	End Sub

	''' <summary>更新历史信息</summary>
	Private Sub UpdateMessage(ParamArray messages As ChatMessage())
		' 清空历史
		History.Clear()

		' 附加数据
		History.AddRange(messages)

		' 整理
		Call UpdateMessage(False)
	End Sub

	''' <summary>更新历史记录</summary>
	''' <remarks>S: 系统, A: 机器人, 其他: 用户。不设置则直接为用户信息</remarks>
	Private Sub UpdateMessage(ParamArray messages As String())
		If messages.IsEmpty Then Return

		' 清空历史
		History.Clear()

		' 处理历史记录
		For Each item In messages
			If item.IsEmpty Then Continue For

			' 创建消息，默认为用户信息
			Dim message As New ChatMessage With {.Role = ChatRole.User, .Text = item}

			Dim path = item.IndexOf(":"c)
			If path > 0 Then
				message.Text = item.Substring(path + 1)
				Select Case item.Substring(0, path).Trim.ToLowerInvariant
					Case "s", "sys", "system"
						message.Role = ChatRole.System

					Case "a", "ass", "assistant"
						message.Role = ChatRole.Assistant

						'Case "q", "user"
						'	message.Role = ChatRole.User
				End Select
			End If

			History.Add(message)
		Next

		' 整理
		Call UpdateMessage(False)
	End Sub

	''' <summary>更新选项</summary>
	Private Function UpdateOptions(opts As ChatOptions) As ChatOptions
		If JsonResult Then
			opts = If(opts, New ChatOptions)
			opts.ResponseFormat = ChatResponseFormat.Json
		End If

		Return opts
	End Function

	''' <summary>对话</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <param name="historyMessage">历史对话数据；一行一条；分别以 A: 或 Q: 开头代表问题与回答；无此开头的数据将被忽略；Dify 无需此参数</param>
	''' <returns>同步方式，一次性返回所有结果</returns>
	Public Function Chat(prompt As String, Optional historyMessage As IEnumerable(Of String) = Nothing, Optional opts As ChatOptions = Nothing) As ChatCompletion
		If historyMessage.NotEmpty Then UpdateMessage(historyMessage.ToArray)
		UpdateMessage(New ChatMessage With {.Role = ChatRole.User, .Text = prompt})

		Return _ChatClient.CompleteAsync(History, UpdateOptions(opts), CancellationToken.None).Result
	End Function

	''' <summary>对话</summary>
	''' <param name="message">对话消息</param>
	''' <returns>同步方式，一次性返回所有结果</returns>
	Public Function Chat(message As ChatMessage, Optional opts As ChatOptions = Nothing) As ChatCompletion
		ArgumentNullException.ThrowIfNull(message)

		UpdateMessage(message)
		Return _ChatClient.CompleteAsync(History, UpdateOptions(opts), CancellationToken.None).Result
	End Function

	''' <summary>对话</summary>
	''' <param name="messages">历史对话消息</param>
	''' <returns>同步方式，一次性返回所有结果</returns>
	Public Function Chat(messages As IList(Of ChatMessage), Optional opts As ChatOptions = Nothing) As ChatCompletion
		ArgumentNullException.ThrowIfNull(messages)

		UpdateMessage(messages.ToArray)
		Return _ChatClient.CompleteAsync(History, UpdateOptions(opts), CancellationToken.None).Result
	End Function

	''' <summary>对话</summary>
	''' <returns>异步方式，流式返回结果</returns>
	Private Async Function ChatAsync(Optional opts As ChatOptions = Nothing, Optional callback As Action(Of StreamingChatCompletionUpdate) = Nothing) As Task(Of ChatCompletion)
		Dim data = _ChatClient.CompleteStreamingAsync(History, UpdateOptions(opts), CancellationToken.None)

		Dim text As New StringBuilder
		Dim last As StreamingChatCompletionUpdate = Nothing

		Await data.ForEachAsync(Sub(x)
									last = x
									text.Append(x)
									callback(x)
								End Sub)
		'Await FunctionHelper.ForEachAsync(data, Sub(x)
		'											last = x
		'											text.Append(x)
		'											callback(x)
		'										End Sub)

		If last Is Nothing Then Return Nothing

		Dim message As New ChatMessage With {.Role = ChatRole.Assistant, .Text = text.ToString}
		Return New ChatCompletion(message) With {
			.Choices = History,
			.AdditionalProperties = last.AdditionalProperties,
			.CompletionId = last.CompletionId,
			.CreatedAt = last.CreatedAt,
			.FinishReason = last.FinishReason,
			.ModelId = last.ModelId,
			.RawRepresentation = last.RawRepresentation
		}
	End Function

	''' <summary>对话</summary>
	''' <param name="prompt">生成响应的提示</param>
	''' <param name="historyMessage">历史对话数据；一行一条；分别以 A: 或 Q: 开头代表问题与回答；无此开头的数据将被忽略；Dify 无需此参数</param>
	''' <returns>异步方式，流式返回结果</returns>
	Public Async Function ChatAsync(prompt As String, Optional historyMessage As IEnumerable(Of String) = Nothing, Optional opts As ChatOptions = Nothing, Optional callback As Action(Of StreamingChatCompletionUpdate) = Nothing) As Task(Of ChatCompletion)
		If historyMessage.NotEmpty Then UpdateMessage(historyMessage.ToArray)
		UpdateMessage(New ChatMessage With {.Role = ChatRole.User, .Text = prompt})

		Return Await ChatAsync(opts, callback)
	End Function

	''' <summary>对话</summary>
	''' <param name="message">对话消息</param>
	''' <returns>异步方式，流式返回结果</returns>
	Public Async Function ChatAsync(message As ChatMessage, Optional opts As ChatOptions = Nothing, Optional callback As Action(Of StreamingChatCompletionUpdate) = Nothing) As Task(Of ChatCompletion)
		ArgumentNullException.ThrowIfNull(message)

		UpdateMessage(message)
		Return Await ChatAsync(opts, callback)
	End Function

	''' <summary>对话</summary>
	''' <param name="messages">历史对话消息</param>
	''' <returns>异步方式，流式返回结果</returns>
	Public Async Function ChatAsync(messages As IList(Of ChatMessage), Optional opts As ChatOptions = Nothing, Optional callback As Action(Of StreamingChatCompletionUpdate) = Nothing) As Task(Of ChatCompletion)
		ArgumentNullException.ThrowIfNull(messages)

		UpdateMessage(messages.ToArray)
		Return Await ChatAsync(opts, callback)
	End Function
	''' <summary>获取聊天属性</summary>
	Public Shared Function GetOptions(data As IDictionary(Of String, Object), Optional valueUpdate As Func(Of Object, Object) = Nothing) As ChatOptions
		If data.IsEmpty Then Return Nothing

		If valueUpdate IsNot Nothing Then data = New KeyValueDictionary(data).FormatAction(valueUpdate)

		Return data.ToJson.fromjson(Of ChatOptions)
	End Function

End Class
