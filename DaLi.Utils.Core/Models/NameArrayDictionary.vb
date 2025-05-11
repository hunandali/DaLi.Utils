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
' 	文本集合字典集合
'
' 	name: Model.NameArrayDictionary
' 	create: 2024-08-18
' 	memo: 文本集合字典集合
' 	
' ------------------------------------------------------------

Namespace Model

	''' <summary>文本集合字典集合</summary>
	''' <remarks>注意与 NameValueDictionary 的区别</remarks>
	Public Class NameArrayDictionary(Of T)
		Inherits Dictionary(Of String, ObjectArray(Of T))
		Implements ICloneable

		''' <summary>线程锁定对象</summary>
		Private ReadOnly _Lock As New Object

#Region "初始化"

		Public Sub New()
			MyBase.New(StringComparer.OrdinalIgnoreCase)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, T)))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, T())))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(collection As IEnumerable(Of KeyValuePair(Of String, ObjectArray(Of T))))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(collection)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, T))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, T()))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
		End Sub

		Public Sub New(dictionary As IDictionary(Of String, ObjectArray(Of T)))
			MyBase.New(StringComparer.OrdinalIgnoreCase)
			AddRangeFast(dictionary)
		End Sub

#End Region

#Region "常用函数"

		''' <summary>添加键值，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Overloads Sub AddFast(key As String, ParamArray values As T())
			If key.IsNull Then Return

			SyncLock _Lock
				MyBase.Add(key, New ObjectArray(Of T)(values))
			End SyncLock
		End Sub

		''' <summary>添加键值，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Overloads Sub AddFast(key As String, value As ObjectArray(Of T))
			If key.IsNull Then Return

			SyncLock _Lock
				MyBase.Add(key, value)
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(collection As IEnumerable(Of KeyValuePair(Of String, T)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(collection As IEnumerable(Of KeyValuePair(Of String, T())))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(collection As IEnumerable(Of KeyValuePair(Of String, ObjectArray(Of T))))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(dictionary As IDictionary(Of String, T))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(dictionary As IDictionary(Of String, T()))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，不校验是否存在，注意：如果存在相同键则会抛出异常</summary>
		Public Sub AddRangeFast(dictionary As IDictionary(Of String, ObjectArray(Of T)))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					AddFast(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

		''' <summary>添加键值，但是存在则不添加</summary>
		Public Overloads Function Add(key As String, ParamArray values As T()) As Boolean
			If key.IsEmpty OrElse ContainsKey(key) Then Return False

			SyncLock _Lock
				MyBase.Add(key, New ObjectArray(Of T)(values))
			End SyncLock

			Return True
		End Function

		''' <summary>添加键值，但是存在则不添加</summary>
		Public Overloads Function Add(key As String, value As ObjectArray(Of T)) As Boolean
			If key.IsEmpty OrElse ContainsKey(key) Then Return False

			SyncLock _Lock
				MyBase.Add(key, value)
			End SyncLock

			Return True
		End Function

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(collection As IEnumerable(Of KeyValuePair(Of String, T)))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(collection As IEnumerable(Of KeyValuePair(Of String, T())))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(collection As IEnumerable(Of KeyValuePair(Of String, ObjectArray(Of T))))
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(dictionary As IDictionary(Of String, T))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(dictionary As IDictionary(Of String, T()))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		''' <summary>添加一组数据，如果存在则不添加</summary>
		Public Sub AddRange(dictionary As IDictionary(Of String, ObjectArray(Of T)))
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Add(c.Key, c.Value)
				Next
			End SyncLock
		End Sub

		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="value">值</param>
		''' <param name="force">不存在则强制添加</param>
		Public Function Update(key As String, value As T, Optional force As Boolean = False) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						MyBase.Item(key) = New ObjectArray(Of T)(value)
						R = True
					ElseIf force Then
						MyBase.Add(key, New ObjectArray(Of T)(value))
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="values">值</param>
		''' <param name="force">不存在则强制添加</param>
		Public Function Update(key As String, values As T(), Optional force As Boolean = False) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						MyBase.Item(key) = New ObjectArray(Of T)(values)
						R = True
					ElseIf force Then
						MyBase.Add(key, New ObjectArray(Of T)(values))
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="value">值</param>
		''' <param name="force">不存在则强制添加</param>
		Public Function Update(key As String, value As ObjectArray(Of T), Optional force As Boolean = False) As Boolean
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
		Public Sub UpdateRange(collection As IEnumerable(Of KeyValuePair(Of String, T)), Optional force As Boolean = False)
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(collection As IEnumerable(Of KeyValuePair(Of String, T())), Optional force As Boolean = False)
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(collection As IEnumerable(Of KeyValuePair(Of String, ObjectArray(Of T))), Optional force As Boolean = False)
			If collection.IsEmpty Then Return

			SyncLock collection
				For Each c In collection
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(dictionary As IDictionary(Of String, T), Optional force As Boolean = False)
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(dictionary As IDictionary(Of String, T()), Optional force As Boolean = False)
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		''' <summary>更新一组数据，默认不存在则不修改</summary>
		''' <param name="force">不存在则强制添加</param>
		Public Sub UpdateRange(dictionary As IDictionary(Of String, ObjectArray(Of T)), Optional force As Boolean = False)
			If dictionary.IsEmpty Then Return

			SyncLock dictionary
				For Each c In dictionary
					Update(c.Key, c.Value, force)
				Next
			End SyncLock
		End Sub

		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

		''' <summary>添加或者更新数据，不存在则添加，存在则附加到最后</summary>
		''' <param name="key">键</param>
		''' <param name="values">值</param>
		Public Function AddOrUpdate(key As String, ParamArray values As T()) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						MyBase.Item(key).Push(values)
						R = True
					Else
						MyBase.Add(key, New ObjectArray(Of T)(values))
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		''' <summary>更新数据，默认不存在则不修改</summary>
		''' <param name="key">键</param>
		''' <param name="value">值</param>
		Public Function AddOrUpdate(key As String, value As ObjectArray(Of T)) As Boolean
			Dim R = False

			If key.NotNull Then
				SyncLock _Lock
					If MyBase.ContainsKey(key) Then
						MyBase.Item(key).Push(value)
						R = True
					Else
						MyBase.Add(key, value)
						R = True
					End If
				End SyncLock
			End If

			Return R
		End Function

		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

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
		Default Public Overloads Property Item(key As String) As ObjectArray(Of T)
			Get
				If key.NotNull AndAlso MyBase.ContainsKey(key) Then
					Return MyBase.Item(key)
				Else
					Return Nothing
				End If
			End Get
			Set(value As ObjectArray(Of T))
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

		''' <summary>克隆</summary>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return New NameArrayDictionary(Of T)(Me)
		End Function

		''' <summary>遍历项目</summary>
		Public Sub ForEach(action As Action(Of String, ObjectArray(Of T)))
			If action IsNot Nothing AndAlso Count > 0 Then
				For Each KV In New NameArrayDictionary(Of T)(Me)
					action.Invoke(KV.Key, KV.Value)
				Next
			End If
		End Sub

		''' <summary>替换值中标签数据</summary>
		Public Function FormatAction(act As Func(Of ObjectArray(Of T), ObjectArray(Of T))) As NameArrayDictionary(Of T)
			Dim Ret = New NameArrayDictionary(Of T)(Me)
			If Ret.Count < 1 OrElse act Is Nothing Then Return Ret

			For Each key In Ret.Keys
				Ret(key) = act(Ret(key))
			Next

			Return Ret
		End Function

	End Class

End Namespace