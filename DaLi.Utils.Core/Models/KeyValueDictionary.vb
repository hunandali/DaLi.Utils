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
' 	键值字典集合
'
' 	name: Model.KeyValueDictionary
' 	create: 2022-04-08
' 	memo: 键值字典集合，忽略键名大小写
' 	
' ------------------------------------------------------------

Imports DaLi.Utils.Json

Namespace Model

	''' <summary>键值字典集合，忽略键名大小写</summary>
	Public Class KeyValueDictionary
		Inherits Dictionary(Of String, Object)
		Implements ICloneable

		''' <summary>线程锁定对象</summary>
		Private ReadOnly _Lock As New Object

#Region "初始化"

		Public Sub New()
			MyBase.New(StringComparer.OrdinalIgnoreCase)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, Object)))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, Object))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
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
				MyBase.Add(key, value)
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
						MyBase.Add(key, value)
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
						MyBase.Item(key) = value
						R = True
					ElseIf force Then
						MyBase.Add(key, value)
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

		' 不能移除，否则会执行内置移除参数
		''' <summary>移除项目</summary>
		Public Overloads Sub Remove(key1 As String, key2 As String)
			Remove({key1, key2})
		End Sub

		''' <summary>仅保留指定键的值</summary>
		Public Sub Keep(ParamArray keys() As String)
			If keys.IsEmpty Then Return

			' 获取需要移除的键
			keys = Me.Keys.Where(Function(x) Not keys.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			Remove(keys)
		End Sub

		''' <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		Default Public Overloads Property Item(key As String) As Object
			Get
				Dim R = Nothing

				SyncLock _Lock
					If key.NotNull AndAlso ContainsKey(key) Then
						R = MyBase.Item(key)
					End If
				End SyncLock

				Return R
			End Get
			Set(value As Object)
				If key.NotNull Then
					SyncLock _Lock
						If ContainsKey(key) Then
							MyBase.Item(key) = value
						Else
							MyBase.Add(key, value)
						End If
					End SyncLock
				End If
			End Set
		End Property

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Default Public Overloads ReadOnly Property Item(key As String, defaultValue As Object) As Object
			Get
				Return If(Item(key), defaultValue)
			End Get
		End Property

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(key As String) As String
			Return GetValue(key, "")
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(ParamArray keys As String()) As String
			Dim value = ""

			For Each key In keys
				value = GetValue(key)
				If value.NotEmpty Then Exit For
			Next

			Return value
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(key As String, baseType As Type) As Object
			Dim value = Item(key)
			If value IsNot Nothing Then value = JsonExtension.ToJson(value).FromJson(baseType)

			Return value
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(keys As IEnumerable(Of String), baseType As Type) As Object
			If keys.IsEmpty Then Return Nothing

			Dim value = Nothing

			For Each key In keys
				value = GetValue(key, baseType)
				If Not ValueExtension.IsEmptyValue(value) Then Exit For
			Next

			Return value
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(Of T)(key As String, Optional defaultValue As T = Nothing) As T
			Dim value = Item(key)
			If value IsNot Nothing Then value = TypeExtension.ToObjectString(value).ToValue(Of T)

			Return If(value, defaultValue)
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetValue(Of T)(keys As IEnumerable(Of String), Optional defaultValue As T = Nothing) As T
			If keys.IsEmpty Then Return Nothing

			Dim value = Nothing

			For Each key In keys
				value = GetValue(Of T)(key)
				If Not ValueExtension.IsEmptyValue(Of T)(value) Then Exit For
			Next

			Return If(value, defaultValue)
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(key As String) As List(Of String)
			Return GetListValue(Of String)(key)
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(ParamArray keys As String()) As List(Of String)
			Dim value As List(Of String) = Nothing

			For Each key In keys
				value = GetListValue(key)
				If value.NotEmpty Then Exit For
			Next

			Return value
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(key As String, baseType As Type) As List(Of Object)
			Dim data = ChangeType(Of IEnumerable(Of Object))(Item(key))
			If data Is Nothing Then Return Nothing

			Return data.Select(Function(x) ToValue(x, baseType)).Where(Function(x) x IsNot Nothing).ToList
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(keys As IEnumerable(Of String), baseType As Type) As List(Of Object)
			If keys.IsEmpty Then Return Nothing

			Dim value As List(Of Object) = Nothing

			For Each key In keys
				value = GetListValue(key, baseType)
				If value.NotEmpty Then Exit For
			Next

			Return value
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(Of T)(key As String) As List(Of T)
			Dim data = ChangeType(Of IEnumerable(Of Object))(Item(key))
			If data Is Nothing Then Return Nothing

			Return data.Select(Function(x) ToValue(Of T)(x)).Where(Function(x) x IsNot Nothing).ToList
		End Function

		''' <summary>获取项目，设置时如果不存在则返回默认值</summary>
		Public Function GetListValue(Of T)(keys As IEnumerable(Of String)) As List(Of T)
			If keys.IsEmpty Then Return Nothing

			Dim value As List(Of T) = Nothing

			For Each key In keys
				value = GetListValue(Of T)(key)
				If value.NotEmpty Then Exit For
			Next

			Return value
		End Function

#End Region

#Region "序列化"

		''' <summary>Json 转换成对象</summary>
		Public Shared Function FromJson(source As String) As KeyValueDictionary
			If source.IsEmpty Then
				Return New KeyValueDictionary
			Else
				Return New KeyValueDictionary(source.ToJsonDictionary)
			End If
		End Function

		''' <summary>Xml 转换成对象</summary>
		Public Shared Function FromXml(source As String) As KeyValueDictionary
			Return If(source.ToXmlObject(Of KeyValueDictionary), New KeyValueDictionary)
		End Function

		''' <summary>转换成XML</summary>
		Public Function ToXml() As String
			' 请勿使用 Extension 简写，如：Me.ToXml 这样会出现死循环
			Return XmlExtension.ToXml(Me)
		End Function

		''' <summary>转换成XML</summary>
		Public Function ToJson() As String
			' 请勿使用 Extension 简写，如：Me.ToJson 这样会出现死循环
			Return JsonExtension.ToJson(Of KeyValueDictionary)(Me)
		End Function

		''' <summary>转换成 NameValueDictionary</summary>
		Public Function ToNameValueDictionary() As NameValueDictionary
			Dim ret As New NameValueDictionary

			For Each kv In Me
				ret(kv.Key) = kv.Value?.ToString
			Next

			Return ret
		End Function

		''' <summary>生成 Hash</summary>
		Public Function GetHash() As String
			If Count < 1 Then Return ""
			Return MyBase.GetHash(True)
		End Function

#End Region

#Region "加解密"

		''' <summary>加密</summary>
		Public Function Encode(Optional key As String = "", Optional clutter As Integer = 0) As String
			Return ToJson.EncodeString(clutter, key)
		End Function

		''' <summary>解密</summary>
		Public Shared Function Decode(code As String, Optional key As String = "", Optional clutter As Integer = 0) As KeyValueDictionary
			Return FromJson(code.DecodeString(clutter, key))
		End Function

#End Region

		''' <summary>克隆</summary>
		''' <remarks>注意：如果值为对象，则在克隆的时候可能不会深度克隆。</remarks>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return New KeyValueDictionary(Me)
		End Function

		''' <summary>遍历项目</summary>
		Public Sub ForEach(action As Action(Of String, Object))
			If action IsNot Nothing AndAlso Count > 0 Then
				For Each KV In New KeyValueDictionary(Me)
					action.Invoke(KV.Key, KV.Value)
				Next
			End If
		End Sub

		''' <summary>替换值中标签数据，使用 JSON 序列化深度修改，注意返回结果的结构可能会发生变化！</summary>
		Public Function FormatTemplate(replaceDatas As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As KeyValueDictionary
			Dim Ret As New KeyValueDictionary

			For Each Kv In Me
				If Kv.Value IsNot Nothing AndAlso Kv.Value.GetType.IsString Then
					Ret.Add(Kv.Key, Kv.Value.ToString.FormatTemplate(replaceDatas, clearTag))
				Else
					Ret.Add(Kv.Key, Kv.Value)
				End If
			Next

			Return Ret
		End Function

		''' <summary>替换值中标签数据</summary>
		Public Function FormatAction(act As Func(Of Object, Object)) As KeyValueDictionary
			Dim Ret = New KeyValueDictionary(Me)
			If Ret.Count < 1 OrElse act Is Nothing Then Return Ret

			For Each key In Ret.Keys
				Ret(key) = act(Ret(key))
			Next

			Return Ret
		End Function
	End Class

End Namespace