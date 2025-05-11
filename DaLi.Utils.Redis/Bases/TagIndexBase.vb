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
' 	标签索引基类
'
' 	name: TagIndexBase
' 	create: 2024-07-26
' 	memo: 基于 Redis JSON 标签索引基类
'
' ------------------------------------------------------------

Imports FreeRedis

Namespace Base

	''' <summary>标签索引基类</summary>
	''' <typeparam name="V">ID 标识的类型</typeparam>
	Public MustInherit Class TagIndexBase(Of T As Class, V)
		Inherits IndexBase(Of T, V)

		''' <summary>构造</summary>
		Public Sub New(client As RedisClient, Optional indexName As String = "")
			MyBase.New(client, indexName, True)
		End Sub

		''' <summary>获取标签的方法</summary>
		Protected MustOverride Function GetTags(entity As T) As String()

		''' <summary>获取索引数据，仅支持文本数组或者字典对象</summary>
		Protected Overrides Function GetIndex(entity As T) As Object
			Return GetTags(entity)
		End Function

#Region "查询"

		''' <summary>获取索引列表</summary>
		Public Function Query(tag As String) As V()
			If tag.IsEmpty Then Return Nothing

			Return JSON.Get(Of V)($"$.{EntityName}.[?(@.v==""{tag}"")]._")
		End Function

		''' <summary>获取索引列表</summary>
		''' <param name="tags">需要查询的标签</param>
		''' <param name="logic">逻辑关系，True 为 And，False 为 Or</param>
		Public Function Query(tags As String(), Optional logic As Boolean = True) As V()
			If tags.IsEmpty Then Return Nothing
			If tags.Length = 1 Then Return Query(tags(0))

			Dim paths = tags.Distinct.Where(Function(x) x.NotEmpty).Select(Function(x) $"$.{EntityName}.[?(@.v==""{x}"")]._").ToArray
			Dim dic = JSON.Get(Of V)(paths)
			If dic.IsEmpty Then Return Nothing

			Dim values = dic.Values
			Dim ret = values.First
			For i = 1 To values.Count - 1
				If logic Then
					ret = ret.Intersect(values(i)).ToArray
					If ret.Length = 0 Then Exit For
				Else
					ret = ret.Union(values(i)).ToArray
				End If
			Next

			Return ret
		End Function

#End Region

	End Class

	'	''' <summary>标签索引基类</summary>
	'	''' <typeparam name="V">ID 标识的类型</typeparam>
	'	Public MustInherit Class TagIndexBase(Of T As Class, V)

	'		''' <summary>RedisJson 客户端</summary>
	'		Protected ReadOnly JSON As RedisJson

	'		''' <summary>获取标识的方法，返回数据的唯一标识</summary>
	'		Protected MustOverride Function GetID(entity As T) As V

	'		''' <summary>获取标签的方法</summary>
	'		Protected MustOverride Function GetTags(entity As T) As String()

	'		''' <summary>获取需要重建索引的实体数据</summary>
	'		''' <param name="lastID">最后操作标识</param>
	'		''' <param name="count">返回数量</param>
	'		Protected MustOverride Function GetEntities(lastID As V, count As Integer) As IEnumerable(Of T)

	'		''' <summary>类型名称</summary>
	'		Protected ReadOnly EntityName As String

	'		''' <summary>ID 标识是否数字或者布尔</summary>
	'		Protected ReadOnly HasQuoted As String

	'		''' <summary>构造</summary>
	'		Public Sub New(client As RedisClient, Optional indexName As String = "")
	'			indexName = indexName.EmptyValue(GetTypeFullName)
	'			JSON = New RedisJson(client, indexName, True)

	'			EntityName = GetType(T).FullName.Replace(".", "_")

	'			Dim Idtype = GetType(V)
	'			HasQuoted = Not Idtype.IsNullableNumber AndAlso Not Idtype.IsNullableBoolean
	'		End Sub

	'		''' <summary>分析标识类型，以便生成有效的查询条件</summary>
	'		Private Function GetQueryValue(entityID As V) As String
	'			Dim type = entityID.GetType

	'			If HasQuoted Then
	'				Return $"""{entityID}"""
	'			Else
	'				Return entityID.ToString
	'			End If
	'		End Function

	'#Region "更新索引"

	'		''' <summary>更新索引</summary>
	'		Public Function Update(entity As T) As V
	'			If entity Is Nothing Then Return Nothing
	'			Return Update(GetID(entity), GetTags(entity))
	'		End Function

	'		''' <summary>更新索引</summary>
	'		''' <param name="entityId">实体标识</param>
	'		''' <param name="tags">当前标签列表</param>
	'		Public Function Update(entityId As V, ParamArray tags As String()) As V
	'			' 初始化该对象索引
	'			JSON.Set($"$.{EntityName}", "[]", TristateEnum.TRUE)

	'			' 删除旧数据
	'			Dim entityValue = GetQueryValue(entityId)
	'			JSON.Delete($"$.{EntityName}.[?(@.v=={entityValue})]")

	'			' 更新标签
	'			If tags.NotEmpty Then
	'				tags.Select(Function(x) New Dictionary(Of String, Object) From {{"k", x}, {"v", entityId}}).
	'					ToList.
	'					ForEach(Sub(tag) JSON.ArrayAppend($"$.{EntityName}", tag))
	'			End If

	'			Return entityId
	'		End Function

	'#End Region

	'#Region "移除索引"

	'		''' <summary>移除索引</summary>
	'		Public Function Remove(entity As T) As V
	'			If entity Is Nothing Then Return Nothing
	'			Return Remove(GetID(entity))
	'		End Function

	'		''' <summary>移除索引</summary>
	'		''' <param name="entityId">实体标识</param>
	'		Public Function Remove(entityId As V) As V
	'			' 移除
	'			Dim entityValue = GetQueryValue(entityId)
	'			JSON.Delete($"$.{EntityName}.[?(@.v=={entityValue})]")

	'			Return entityId
	'		End Function

	'#End Region

	'#Region "查询"

	'		''' <summary>获取索引列表</summary>
	'		Public Function Query(tag As String) As V()
	'			If tag.IsEmpty Then Return Nothing

	'			Return JSON.Get(Of V)($"$.{EntityName}.[?(@.k==""{tag}"")].v")
	'		End Function

	'		''' <summary>获取索引列表</summary>
	'		''' <param name="tags">需要查询的标签</param>
	'		''' <param name="logic">逻辑关系，True 为 And，False 为 Or</param>
	'		Public Function Query(tags As String(), Optional logic As Boolean = True) As V()
	'			If tags.IsEmpty Then Return Nothing

	'			Dim queryString = tags.Distinct.
	'					Where(Function(x) x.NotEmpty).
	'					Select(Function(x) $"?(@.k==""{x}"")").
	'					JoinString(If(logic, " && ", " || "))

	'			Return JSON.Get(Of V)($"'$.{EntityName}.[{queryString}].v'")
	'		End Function

	'#End Region

	'#Region "索引检查及重建"

	'		''' <summary>重建全部数据索引</summary>
	'		''' <param name="force">是否强制从初始开始重建</param>
	'		Public Sub Rebuild(force As Boolean, Optional statusAction As Action(Of IndexStatus(Of V)) = Nothing)
	'			Dim status = New IndexStatus(Of V)

	'			' 获取最后一次重建 ID
	'			If Not force Then
	'				Dim statusList = JSON.Get(Of IndexStatus(Of V))($"$.{EntityName}.[0]")
	'				If statusList.IsEmpty Then
	'					' 无记录，强制重新开始
	'					force = True
	'				Else
	'					status.Update(statusList(0).LastId)
	'				End If
	'			End If

	'			' 如果没有最后标识则需要先清除索引数据
	'			If force Then JSON.Set($"$.{EntityName}", {status})

	'			' 循环标识，如果两次循环记录的值一致则强制退出循环，防止死循环
	'			Dim loopValue As V = Nothing

	'			' 记录状态
	'			statusAction?.Invoke(status)

	'			' 重建索引
	'			While True
	'				' 需要重建的数量
	'				Dim entities = GetEntities(status.LastId, 100)

	'				' 索引数据
	'				If entities.IsEmpty Then
	'					status.Finised()

	'					' 重置记录
	'					JSON.Set($"$.{EntityName}.[0]", status)
	'					statusAction?.Invoke(status)

	'					' 退出
	'					Exit While
	'				Else
	'					For Each entity In entities
	'						Dim lastId = Update(entity)
	'						If Object.Equals(loopValue, lastId) Then Exit For

	'						status.Update(lastId)
	'					Next

	'					' 记录最后标识
	'					JSON.Set($"$.{EntityName}.[0]", status)
	'					statusAction?.Invoke(status)

	'					If Object.Equals(loopValue, status.LastId) Then Exit While
	'					loopValue = status.LastId
	'				End If
	'			End While
	'		End Sub

	'		''' <summary>最后索引时间</summary>
	'		Public Function IndexLast() As Date
	'			Dim ret = JSON.Get(Of Date)($"$.{EntityName}.[?(@.action==""index"")].last")
	'			If ret Is Nothing Then Return Nothing
	'			Return ret.FirstOrDefault
	'		End Function

	'		''' <summary>最后索引标识</summary>
	'		Public Function IndexId() As V
	'			Dim ret = JSON.Get(Of V)($"$.{EntityName}.[?(@.action==""index"")].id")
	'			If ret Is Nothing Then Return Nothing
	'			Return ret.FirstOrDefault
	'		End Function

	'		''' <summary>索引是否完成</summary>
	'		Public Function IndexFinished() As Boolean
	'			Dim ret = JSON.Get($"$.{EntityName}.[?(@.action==""index"")].status")
	'			Return ret.NotEmpty AndAlso ret.FirstOrDefault = "Finish"
	'		End Function

	'		''' <summary>异步检查索引</summary>
	'		''' <param name="interval">间隔时间（单位：天）</param>
	'		Public Function IndexCheckSync(Optional interval As Integer = 30, Optional statusAction As Action(Of IndexStatus(Of V)) = Nothing) As Task
	'			Return Task.Run(Sub()
	'								If interval < 1 Then interval = 30

	'								' 检查最后索引时间，超过 指定天数 强制重新索引
	'								Dim last = IndexLast()
	'								If last.AddDays(interval) < DATE_NOW Then
	'									Task.Run(Sub() Rebuild(True, statusAction))
	'								Else
	'									' 检查是否有新数据需要索引
	'									Task.Run(Sub() Rebuild(False, statusAction))
	'								End If
	'							End Sub)
	'		End Function
	'#End Region

	'	End Class

End Namespace