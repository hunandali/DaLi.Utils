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
' 	状态缓存操作
'
' 	name: StatusCache
' 	create: 2024-07-02
' 	memo: 基于 Redis JSON/HASH 数据状态缓存操作
'
' ------------------------------------------------------------

Imports FreeRedis

''' <summary>基于 Redis JSON/HASH 数据状态缓存操作</summary>
Public Class StatusCache(Of T)
	Implements IDisposable

	''' <summary>Redis 客户端</summary>
	Private ReadOnly _Client As RedisClient

	''' <summary>Redis HASH 接口</summary>
	Private ReadOnly _HASH As RedisHash(Of T)

	''' <summary>Redis JSON 接口</summary>
	Private ReadOnly _JSON As RedisJson

	''' <summary>构造</summary>
	''' <param name="name">数据名</param>
	''' <param name="auto">是否自动模式，true：尝试 JSON 模式，异常则使用 HASH；false：强制使用 HASH</param>
	Public Sub New(client As RedisClient, name As String, Optional auto As Boolean = True)
		_Client = client

		If _Client IsNot Nothing Then
			IO = "HASH"

			If auto Then
				' 自动模式尝试创建 JSON
				Try
					_JSON = New RedisJson(_Client, name, False)
					IO = "JSON"
				Catch ex As Exception
				End Try
			End If

			If IO = "HASH" Then _HASH = New RedisHash(Of T)(_Client, name, False)
		Else
			IO = ""
		End If
	End Sub

	''' <summary>注销</summary>
	Public Sub Dispose() Implements IDisposable.Dispose
		_Client?.Dispose()
		GC.SuppressFinalize(Me)
	End Sub

	''' <summary>当前接口类型</summary>
	Public ReadOnly Property IO As String

#Region "通用操作"

	''' <summary>HASH 模式保存数据，使用 1 秒防抖，防止频繁写入</summary>
	Private ReadOnly _HashSave = Debounce(Sub(data As (id As String, value As T)) _HASH?.Set(data.id, data.value), 1000)

	''' <summary>获取接口全部数据</summary>
	Public Function [GET]() As IDictionary(Of String, T)
		If _JSON IsNot Nothing Then Return _JSON.GetAll(Of IDictionary(Of String, T))
		If _HASH IsNot Nothing Then Return _HASH.GetAll

		Return Nothing
	End Function

	''' <summary>获取所有数据</summary>
	Public Function [GET](id As String) As T
		If _JSON IsNot Nothing Then
			Dim data = _JSON.Get(Of T)(id)
			Return If(data.IsEmpty, Nothing, data(0))
		End If
		If _HASH IsNot Nothing Then Return _HASH.Item(id)

		Return Nothing
	End Function

	''' <summary>获取指定属性数据</summary>
	''' <param name="field">获取的字段</param>
	Public Function [GET](Of S)(id As String, field As String) As S
		If _JSON IsNot Nothing Then
			Dim data = _JSON.Get(Of S)($"{id}.{field}")
			Return If(data.IsEmpty, Nothing, data(0))
		End If
		If _HASH Is Nothing Then Return Nothing

		Dim value = _HASH.Item(id)
		If value Is Nothing Then Return Nothing

		Dim pro = value.GetType.GetSingleProperty(field)
		Return TypeExtension.ChangeType(Of S)(pro?.GetValue(value))
	End Function

	''' <summary>保存所有数据</summary>
	''' <param name="value">设置的值</param>
	Public Function [SET](id As String, value As T) As Boolean
		If _JSON IsNot Nothing Then Return _JSON.Set($"$.{id}", value)
		If _HASH IsNot Nothing Then Return _HASH.Set(id, value)
		Return False
	End Function

	''' <summary>向属性设置空值，如清除某个属性的值</summary>
	''' <param name="field">设置的字段</param>
	Public Function [SET](Of S)(id As String, field As String, value As S) As Boolean
		If _JSON IsNot Nothing Then
			' 检查是否存在 id 键，不存在则创建
			Dim key = $"$.{id}"
			If _JSON.Type(key).IsEmpty Then _JSON.Set(key, "{}")

			If field.NotEmpty Then key = $"$.{id}.{field}"
			Return _JSON.Set(key, value)
		End If

		If _HASH Is Nothing Then Return False

		Dim item = _HASH.Item(id)
		If item Is Nothing Then Return False

		Dim pro = item.GetType.GetSingleProperty(field)
		pro.SetValue(item, value)

		Return _HashSave((id, item))
	End Function

	''' <summary>删除数据</summary>
	Public Function DEL(ParamArray ids As String()) As Boolean
		If _JSON IsNot Nothing Then
			For Each id In ids
				_JSON.Delete($"$.{id}")
			Next
			Return True
		End If

		If _HASH IsNot Nothing Then Return _HASH.Del(ids) > 0

		Return False
	End Function

	''' <summary>清空所有数据</summary>
	Public Function CLEAR() As Boolean
		If _JSON IsNot Nothing Then Return _JSON.DeleteAll > 0
		If _HASH IsNot Nothing Then
			_HASH.Clear()
			Return True
		End If

		Return False
	End Function

	''' <summary>向数组属性追加内容，超过最大长度则删除第一个元素</summary>
	''' <param name="field">设置的字段，为空则表示设置上级为数组，而不设置子级</param>
	''' <param name="maxLength">最大长度</param>
	Public Function APPEND(Of S)(id As String, field As String, value As S, Optional maxLength As Integer = 100) As Boolean
		If _JSON IsNot Nothing Then
			' 检查是否存在 id 键，不存在则创建
			Dim key = $"$.{id}"
			If field.NotEmpty Then
				If _JSON.Type(key).IsEmpty Then _JSON.Set(key, "{}")

				key = $"$.{id}.{field}"
			End If

			' 分析数组
			Dim len = _JSON.ArrayLength(key).FirstOrDefault
			If len < 1 Then
				_JSON.Set(key, "[]")
			ElseIf len >= maxLength Then
				_JSON.ArrayTrim(key, len - maxLength + 1, -1)
			End If

			_JSON.ArrayAppend(key, value)
		End If

		If _HASH Is Nothing Then Return False

		Dim item = _HASH.Item(id)
		If item Is Nothing Then Return False

		Dim pro = item.GetType.GetSingleProperty(field)
		Dim values = ChangeType(Of IEnumerable(Of S))(pro?.GetValue(item))
		If values.IsEmpty Then values = New List(Of S)

		If values.Count >= maxLength Then values = values.TakeLast(maxLength - 1).ToList
		values = values.Append(value)

		pro.SetValue(item, values)
		Return _HashSave((id, item))
	End Function

#End Region

End Class
