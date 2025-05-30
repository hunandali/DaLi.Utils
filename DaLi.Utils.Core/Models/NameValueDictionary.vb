﻿' ------------------------------------------------------------
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
' 	文本字典集合
'
' 	name: Model.NameValueDictionary
' 	create: 2019-03-14
' 	memo: 文本字典集合
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Json

Namespace Model

	''' <summary>文本字典集合</summary>
	Public Class NameValueDictionary
		Inherits Dictionary(Of String, String)
		Implements ICloneable

		''' <summary>线程锁定对象</summary>
		Private ReadOnly _Lock As New Object

#Region "初始化"

		Public Sub New()
			MyBase.New(StringComparer.OrdinalIgnoreCase)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, String)))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, String))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
		End Sub

		Public Sub New(namevalues As Specialized.NameValueCollection)
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(namevalues)
		End Sub

#End Region

#Region "常用函数"

		''' <summary>添加键值，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Overloads Sub AddFast(key As String, Optional value As Object = Nothing)
			AddFast(Of Object)(key, value)
		End Sub

		''' <summary>添加键值，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Overloads Sub AddFast(Of T)(key As String, value As T)
			If key.IsNull Then Return

			SyncLock _Lock
				MyBase.Add(key, value?.ToObjectString)
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(Of T)(collection As IEnumerable(Of KeyValuePair(Of String, T)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(Of T)(dictionary As IDictionary(Of String, T))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(nameValues As Specialized.NameValueCollection)
			If nameValues Is Nothing OrElse nameValues.Count < 1 Then Return

			SyncLock nameValues
				For Each key In nameValues.AllKeys
					AddFast(key, nameValues(key))
				Next
			End SyncLock
		End Sub

		''' <summary>添加键值，但是存在则不添加</summary>
		Public Overloads Function Add(key As String, Optional value As Object = Nothing) As Boolean
			Return Add(Of Object)(key, value)
		End Function

		''' <summary>添加键值，但是存在则不添加</summary>
		Public Overloads Function Add(Of T)(key As String, value As T) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If Not MyBase.ContainsKey(key) Then
						MyBase.Add(key, value?.ToObjectString)
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(Of T)(collection As IEnumerable(Of KeyValuePair(Of String, T)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(Of T)(dictionary As IDictionary(Of String, T))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(nameValues As Specialized.NameValueCollection)
			If nameValues Is Nothing OrElse nameValues.Count < 1 Then Return

			SyncLock nameValues
				For Each key In nameValues.AllKeys
					Add(key, nameValues(key))
				Next
			End SyncLock
		End Sub

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="value">值</param>
		''' <param name="force">不存在则强制添加</param>
		Public Function Update(key As String, Optional value As Object = Nothing, Optional force As Boolean = False) As Boolean
			Return Update(Of Object)(key, value, force)
		End Function

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="value">值</param>
		''' <param name="force">不存在则强制添加</param>
		Public Function Update(Of T)(key As String, value As T, Optional force As Boolean = False) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						MyBase.Item(key) = value.ToObjectString
						R = True
					ElseIf force Then
						MyBase.Add(key, value.ToObjectString)
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(Of T)(collection As IEnumerable(Of KeyValuePair(Of String, T)), Optional force As Boolean = False)
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(Of T)(dictionary As IDictionary(Of String, T), Optional force As Boolean = False)
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>移除项目</summary>
		Public Overloads Sub Clear()
			If MyBase.Count > 0 Then
				SyncLock _Lock
					MyBase.Clear()
				End SyncLock
			End If
		End Sub

		''' <summary>移除项目</summary>
		Public Overloads Function Remove(key As String) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						R = MyBase.Remove(key)
					End If
				End SyncLock
			End If

			Return R
		End Function

		' 不能移除，否则会执行内置移除参数
		''' <summary>移除项目</summary>
		Public Overloads Sub Remove(key1 As String, key2 As String)
			Remove({key1, key2})
		End Sub

		''' <summary>移除项目</summary>
		Public Overloads Function Remove(ParamArray keys() As String) As Long
			Dim ret = 0

			If keys?.Length > 0 Then
				SyncLock _Lock
					For Each Key In keys
						If Key.NotNull AndAlso ContainsKey(Key) Then
							If MyBase.Remove(Key) Then ret += 1
						End If
					Next
				End SyncLock
			End If

			Return ret
		End Function

		''' <summary>仅保留指定键的值</summary>
		Public Sub Keep(ParamArray keys() As String)
			If keys.IsEmpty Then Return

			' 获取需要移除的键
			keys = Me.Keys.Where(Function(x) Not keys.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			Remove(keys)
		End Sub

		''' <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		Default Public Overloads Property Item(key As String) As String
			Get
				Dim R = ""

				SyncLock _Lock
					If key.NotNull AndAlso MyBase.ContainsKey(key) Then
						R = MyBase.Item(key)
					End If
				End SyncLock

				Return R
			End Get
			Set(value As String)
				If key.NotNull Then
					SyncLock _Lock
						If MyBase.ContainsKey(key) Then
							MyBase.Item(key) = value
						Else
							MyBase.Add(key, value)
						End If
					End SyncLock
				End If
			End Set
		End Property

#End Region

#Region "序列化"

		''' <summary>类似网址请求格式：a=b＆c=d</summary>
		Public Shared Function FromQueryString(source As String, Optional encode As Text.Encoding = Nothing) As NameValueDictionary
			Dim R As NameValueDictionary = Nothing

			If source.NotEmpty Then
				R = New NameValueDictionary(Web.HttpUtility.ParseQueryString(source, If(encode, Text.Encoding.UTF8)))
			End If

			Return If(R, New NameValueDictionary)
		End Function

		''' <summary>数组转换成对象，每条格式：名称=值</summary>
		Public Shared Function FromArray(ParamArray source As String()) As NameValueDictionary
			Dim Ret As New NameValueDictionary

			If source?.Length > 0 Then
				For Each L In source
					If L.NotEmpty AndAlso L.Contains("="c) Then
						Dim P = L.IndexOf("="c)
						If P > 0 Then
							Dim Key = L.Substring(0, P).Trim
							Dim Value = L.Substring(P + 1).Trim

							If Key.NotNull Then
								Ret(Key) = Value.DecodeLine
							End If
						End If
					End If
				Next
			End If

			Return Ret
		End Function

		''' <summary>转换成类似网址请求格式：a=b＆c=d</summary>
		Public Function ToQueryString(Optional encoding As Text.Encoding = Nothing) As String
			If Count < 1 Then
				Return ""
			Else
				With New Text.StringBuilder
					For Each key As String In MyBase.Keys
						.AppendFormat("{0}={1}&", key.EncodeUrl(encoding), MyBase.Item(key).EncodeUrl(encoding))
					Next

					If .Length > 0 Then .Length -= 1

					Return .ToString
				End With
			End If
		End Function

		''' <summary>Json 转换成对象</summary>
		Public Shared Function FromJson(source As String) As NameValueDictionary
			Return If(source.ToJsonNameValues, New NameValueDictionary)
		End Function

		''' <summary>Xml 转换成对象</summary>
		Public Shared Function FromXml(source As String) As NameValueDictionary
			Return If(source.ToXmlObject(Of NameValueDictionary), New NameValueDictionary)
		End Function

		''' <summary>文本转换成对象，自动分析，如果包含换行或者回车，使用字符模式，否则使用网址请求模式</summary>
		Public Shared Function FromString(source As String) As NameValueDictionary
			If source.NotEmpty Then
				If source.Contains(vbCr) OrElse source.Contains(vbLf) Then
					Return FromArray(source.SplitDistinct(vbCrLf))
				Else
					Return FromQueryString(source)
				End If
			Else
				Return New NameValueDictionary
			End If
		End Function

		''' <summary>文本转换成对象</summary>
		Public Shared Function FromString(source As String, Optional isQueryString As Boolean = True) As NameValueDictionary
			If source.NotEmpty Then
				If isQueryString Then
					Return FromQueryString(source)
				Else
					Return FromArray(source.SplitDistinct(vbCrLf))
				End If
			Else
				Return New NameValueDictionary
			End If
		End Function

		''' <summary>转换成数组</summary>
		Public Function ToArray() As String()
			If Count < 1 Then
				Return System.Array.Empty(Of String)
			Else
				With New Generic.List(Of String)
					For Each key As String In MyBase.Keys
						.Add(key & "=" & MyBase.Item(key).EncodeLine)
					Next

					Return .ToArray
				End With
			End If
		End Function

		''' <summary>转换成字符串</summary>
		Public Overloads Function ToString(Optional isQueryString As Boolean = True) As String
			If isQueryString Then
				Return ToQueryString()
			Else
				Return String.Join(vbCrLf, ToArray)
			End If
		End Function

		''' <summary>转换成XML</summary>
		Public Function ToXml() As String
			' 请勿使用 Extension 简写，如：Me.ToXml 这样会出现死循环
			Return XmlExtension.ToXml(Me)
		End Function

		''' <summary>转换成 Json</summary>
		Public Function ToJson() As String
			' 请勿使用 Extension 简写，如：Me.ToJson 这样会出现死循环
			Return JsonExtension.ToJson(Me, False, False)
		End Function

		''' <summary>生成 Hash</summary>
		Public Function GetHash() As String
			If Count < 1 Then Return ""
			Return [Select](Function(x) $"[{x.Key.ToLowerInvariant}]:{x.Value}").OrderBy(Function(x) x).JoinString(vbCrLf).MD5
		End Function

#End Region

#Region "加解密"

		''' <summary>加密</summary>
		Public Function Encode(Optional key As String = "", Optional clutter As Integer = 0) As String
			Return ToJson.EncodeString(clutter, key)
		End Function

		''' <summary>解密</summary>
		Public Shared Function Decode(code As String, Optional key As String = "", Optional clutter As Integer = 0) As NameValueDictionary
			Return FromJson(code.DecodeString(clutter, key))
		End Function

#End Region

		''' <summary>克隆</summary>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return New NameValueDictionary(Me)
		End Function

		''' <summary>遍历项目</summary>
		Public Sub ForEach(action As Action(Of String, String))
			If action IsNot Nothing AndAlso Count > 0 Then
				For Each KV In Clone()
					action.Invoke(KV.Key, KV.Value)
				Next
			End If
		End Sub

		''' <summary>替换值中标签数据</summary>
		Public Function FormatTemplate(replaceDatas As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As NameValueDictionary
			Dim Ret = New NameValueDictionary(Me)
			If Ret.Count < 1 OrElse replaceDatas.IsEmpty Then Return Ret

			For Each key In Ret.Keys
				Ret(key) = Ret(key).FormatTemplate(replaceDatas, clearTag)
			Next

			Return Ret
		End Function

		''' <summary>替换值中标签数据</summary>
		Public Function FormatAction(act As Func(Of String, String)) As NameValueDictionary
			Dim Ret = New NameValueDictionary(Me)
			If Ret.Count < 1 OrElse act Is Nothing Then Return Ret

			For Each key In Ret.Keys
				Ret(key) = act(Ret(key))
			Next

			Return Ret
		End Function

	End Class

End Namespace