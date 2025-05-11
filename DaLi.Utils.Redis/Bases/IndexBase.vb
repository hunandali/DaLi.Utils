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
' 	索引基类
'
' 	name: IndexBase
' 	create: 2024-07-31
' 	memo: 基于 Redis JSON 索引基类
' 			索引数据存在 类名称下的数组中，每个项目中 _ 表示项目标识；
' 			标签索引 v 表示标签；
' 			属性索引则使用属性原始对象数据
'
' ------------------------------------------------------------

Imports System.Threading
Imports DaLi.Utils.Redis.Model
Imports FreeRedis

Namespace Base

	''' <summary>索引基类</summary>
	''' <typeparam name="V">ID 标识的类型</typeparam>
	Public MustInherit Class IndexBase(Of T As Class, V)

		''' <summary>RedisJson 客户端</summary>
		Protected ReadOnly JSON As RedisJson

		''' <summary>获取标识的方法，返回数据的唯一标识</summary>
		Protected MustOverride Function GetID(entity As T) As V

		''' <summary>获取索引数据，仅支持文本数组或者字典对象</summary>
		Protected MustOverride Function GetIndex(entity As T) As Object

		''' <summary>类型名称</summary>
		Protected ReadOnly _EntityName As String

		''' <summary>类型名称</summary>
		Protected Overridable ReadOnly Property EntityName As String
			Get
				Return _EntityName
			End Get
		End Property

		''' <summary>获取需要重建索引的实体数据</summary>
		''' <param name="lastID">最后操作标识</param>
		''' <param name="count">返回数量</param>
		Protected MustOverride Function GetEntities(lastID As V, count As Integer) As IEnumerable(Of T)

		''' <summary>ID 标识是否数字或者布尔</summary>
		Protected ReadOnly HasQuoted As String

		''' <summary>构造</summary>
		Public Sub New(client As RedisClient, Optional indexName As String = "", Optional ignoreCase As Boolean = True)
			indexName = indexName.EmptyValue(GetTypeFullName)
			JSON = New RedisJson(client, indexName, ignoreCase)

			_EntityName = GetType(T).FullName.Replace(".", "_")

			Dim Idtype = GetType(V)
			HasQuoted = Not Idtype.IsNullableNumber AndAlso Not Idtype.IsNullableBoolean
		End Sub

		''' <summary>分析标识类型，以便生成有效的查询条件</summary>
		Private Function GetQueryValue(entityID As V) As String
			Dim type = entityID.GetType

			If HasQuoted Then
				Return $"""{entityID}"""
			Else
				Return entityID.ToString
			End If
		End Function

#Region "更新索引"

		''' <summary>更新索引</summary>
		Public Function Update(entity As T) As V
			If entity Is Nothing Then Return Nothing

			Dim entityId = GetID(entity)
			If entityId Is Nothing Then Return Nothing

			' 初始化该对象索引
			JSON.Set($"$.{EntityName}", "[]", TristateEnum.TRUE)

			' 删除旧数据
			Dim entityValue = GetQueryValue(entityId)
			JSON.Delete($"$.{EntityName}.[?(@._=={entityValue})]")

			' 索引记录算法
			Dim index = GetIndex(entity)
			If index IsNot Nothing Then
				Dim type = index.GetType

				' 文本数组：标签
				' 分项赋值
				If type.IsArray AndAlso type.GetElementType.IsString Then
					Dim tags = TryCast(index, String())

					tags.Select(Function(x) New Dictionary(Of String, Object) From {{"v", x}, {"_", entityId}}).
					ToList.
					ForEach(Sub(tag) JSON.ArrayAppend($"$.{EntityName}", tag))
				End If

				' 字典数据：属性
				' 直接赋值
				If type.IsComeFrom(Of IDictionary(Of String, Object)) Then
					Dim data = TryCast(index, IDictionary(Of String, Object))

					If data.ContainsKey("_") Then
						data("_") = entityId
					Else
						data.Add("_", entityId)
					End If

					' 更新标签
					JSON.ArrayAppend($"$.{EntityName}", data)
				End If
			End If

			Return entityId
		End Function

#End Region

#Region "移除索引"

		''' <summary>移除索引</summary>
		Public Function Remove(entity As T) As V
			If entity Is Nothing Then Return Nothing
			Return Remove(GetID(entity))
		End Function

		''' <summary>移除索引</summary>
		''' <param name="entityId">实体标识</param>
		Public Function Remove(entityId As V) As V
			' 移除
			Dim entityValue = GetQueryValue(entityId)
			JSON.Delete($"$.{EntityName}.[?(@._=={entityValue})]")

			Return entityId
		End Function

#End Region

#Region "索引检查及重建"

		''' <summary>是否正在更新索引</summary>
		Protected IsRebuild As Boolean = False

		''' <summary>重建全部数据索引</summary>
		''' <param name="force">是否强制从初始开始重建</param>
		Public Sub Rebuild(force As Boolean, Optional statusAction As Action(Of IndexStatus(Of V)) = Nothing, Optional stoppingToken As CancellationToken = Nothing)
			If IsRebuild Then Return
			IsRebuild = True

			Dim status = New IndexStatus(Of V)

			' 获取最后一次重建 ID
			If Not force Then
				Dim statusList = JSON.Get(Of IndexStatus(Of V))($"$.{EntityName}.[0]")
				If statusList.IsEmpty Then
					' 无记录，强制重新开始
					force = True
				Else
					status.LastId = statusList(0).LastId
					status.LastTime = statusList(0).LastTime
				End If
			End If

			' 如果没有最后标识则需要先清除索引数据
			If force Then JSON.Set($"$.{EntityName}", {status})

			' 循环标识，如果两次循环记录的值一致则强制退出循环，防止死循环
			Dim loopValue As V = Nothing

			' 记录状态
			statusAction?.Invoke(status)

			' 强制终止
			If stoppingToken.IsCancellationRequested Then Return

			' 重建索引
			While True
				' 需要重建的数量
				Dim entities = GetEntities(status.LastId, 100)

				' 索引数据
				If entities.IsEmpty Then
					status.Finished()

					' 重置记录
					JSON.Set($"$.{EntityName}.[0]", status)
					statusAction?.Invoke(status)

					' 退出
					Exit While
				Else
					For Each entity In entities
						Dim lastId = Update(entity)
						If Object.Equals(loopValue, lastId) Then Exit For

						status.Update(lastId)
					Next

					' 记录最后标识
					JSON.Set($"$.{EntityName}.[0]", status)
					statusAction?.Invoke(status)

					If Object.Equals(loopValue, status.LastId) Then Exit While
					loopValue = status.LastId
				End If

				' 强制终止
				If stoppingToken.IsCancellationRequested Then Exit While
			End While

			IsRebuild = False
		End Sub

		''' <summary>索引状态</summary>
		Public Function IndexStatus() As IndexStatus(Of V)
			Dim statusList = JSON.Get(Of IndexStatus(Of V))($"$.{EntityName}.[0]")
			If statusList.IsEmpty Then Return Nothing
			Return statusList(0)
		End Function

		''' <summary>异步检查索引</summary>
		''' <param name="interval">间隔时间（单位：天）</param>
		Public Function IndexCheckSync(Optional interval As Integer = 30, Optional statusAction As Action(Of IndexStatus(Of V)) = Nothing, Optional stoppingToken As CancellationToken = Nothing) As Task
			Return Task.Run(Sub()
								If interval < 1 Then interval = 30

								Dim last As Date
								Dim staus = IndexStatus()
								If staus IsNot Nothing Then last = staus.LastTime

								' 检查最后索引时间，超过 指定天数 强制重新索引
								If last.AddDays(interval) < DATE_NOW Then
									Task.Run(Sub() Rebuild(True, statusAction, stoppingToken), stoppingToken)
								Else
									' 检查是否有新数据需要索引
									Task.Run(Sub() Rebuild(False, statusAction, stoppingToken), stoppingToken)
								End If
							End Sub, stoppingToken)
		End Function
#End Region

	End Class

End Namespace