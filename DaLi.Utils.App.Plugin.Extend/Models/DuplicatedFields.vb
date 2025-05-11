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
' 	重复字段的表达式
'
' 	name: Model.DuplicatedFields
' 	create: 2023-02-20
' 	memo: 重复字段的表达式
' 	
' ------------------------------------------------------------

Imports System.Data
Imports System.Linq.Expressions
Imports FreeSql
Imports FreeSql.Internal.Model

Namespace Model

	''' <summary>重复字段的表达式</summary>
	Public Class DuplicatedFields

		''' <summary>数据库对象</summary>
		Private ReadOnly _Db As IFreeSql

		''' <summary>要操作的实体</summary>
		Private ReadOnly _Entity As IEntity

		''' <summary>要操作的实体</summary>
		Private ReadOnly _EntityType As Type

		''' <summary>要校验的字段</summary>
		Private ReadOnly _Instance As List(Of String())

		Public Sub New(entity As IEntity, db As IFreeSql)
			ArgumentNullException.ThrowIfNull(entity)
			ArgumentNullException.ThrowIfNull(db)

			_Db = db
			_Entity = entity
			_EntityType = entity.GetType
			_Instance = New List(Of String())
		End Sub

		''' <summary>创建表达式</summary>
		''' <param name="fields">字段名称</param>
		Public Function Insert(ParamArray fields As String()) As DuplicatedFields
			If fields.NotEmpty AndAlso Not _Instance.Contains(fields) Then _Instance.Add(fields)
			Return Me
		End Function

		''' <summary>获取组合表达式</summary>
		Public Function MakeQuery() As Dictionary(Of String(), ISelect(Of IEntity))
			If _Instance.IsEmpty Then Return Nothing

			' 创建表达式
			Return _Instance.Select(Function(fields)
										' 检查字段名称，方式重复
										'Dim names As New List(Of String)

										'' 已经添加全局过滤器，无需 HiddenFilter
										'Dim query = _Db.Select(_EntityType).WhereNotEquals(_Entity.ID, Function(x) x.ID)

										Dim filter As New DynamicFilterInfo With {.Logic = DynamicFilterLogic.And, .Filters = New List(Of DynamicFilterInfo)}
										filter.Filters.Add(New DynamicFilterInfo With {
														   .Field = NameOf(IEntity.ID),
														   .[Operator] = DynamicFilterOperator.NotEqual,
														   .Value = _Entity.ID})

										For Each field In fields
											Dim value = _EntityType.GetSingleProperty(field)?.GetValue(_Entity)
											If value Is Nothing Then Continue For

											' 当检查表达式项目只有一条，且当前对象的值：文本为空则不进行检测
											If fields.Length = 1 AndAlso value.GetType.IsString AndAlso value.ToString.IsEmpty Then Continue For

											' 查询
											filter.Filters.Add(New DynamicFilterInfo With {
														   .Field = field,
														   .[Operator] = DynamicFilterOperator.Equal,
														   .Value = value})

											'' 参数 x
											'Dim expParam = Expression.Parameter(_EntityType, "x")

											'' x.Name
											'Dim expMember = Expression.Property(expParam, field)

											'' value
											'Dim expConstant = Expression.Constant(value)

											'' x.Name = value
											'Dim expEqual = Expression.Equal(expMember, expConstant)

											'' function(x) x.Name == value
											'Dim expDelegate As Type = GetType(Func(Of ,)).MakeGenericType(_EntityType, GetType(Boolean))
											'Dim exp = Expression.Lambda(expDelegate, expEqual, expParam)

											'' 查询
											'query = query.Where(exp)
										Next

										Dim query = _Db.Select(_EntityType, filter)

										Return (fields, query)
									End Function).
									Distinct(Function(x) x.fields).
									ToDictionary(Function(x) x.fields, Function(x) x.query)
		End Function

		''' <summary>执行查询，返回重复字段</summary>
		Public Function Execute() As List(Of String())
			Dim datas = MakeQuery()
			Return datas?.Where(Function(x) x.Value.Any).Select(Function(x) x.Key).ToList
		End Function
	End Class

	''' <summary>重复字段的表达式</summary>
	Public Class DuplicatedFields(Of T As {IEntity, Class})

		Private ReadOnly _Instance As New List(Of DuplicatedExpression(Of T))

		'''' <summary>创建表达式</summary>
		'''' <param name="field">字段名称</param>
		'''' <param name="entityType">实际类型，当泛类 T 不能表达实际类型时，可以使用 entityType 来指定</param>
		'Public Function Insert(field As String, Optional entityType As Type = Nothing) As DuplicatedFields(Of T)
		'	If field.IsEmpty Then Return Me
		'	entityType = If(entityType, GetType(T))

		'	' 检查属性是否存在
		'	Dim pro = entityType.GetSingleProperty(field)
		'	If pro Is Nothing Then Return Me

		'	' 创建表达式 function(x) x.field
		'	Dim expPar = Expression.Parameter(entityType, "x")
		'	Dim expPro = Expression.Property(expPar, field)
		'	Dim exp = Expression.Lambda(expPro, expPar)

		'	Return Insert(exp)
		'End Function

		'''' <summary>创建表达式</summary>
		'''' <param name="fields">字段名称</param>
		'''' <param name="entityType">实际类型，当泛类 T 不能表达实际类型时，可以使用 entityType 来指定</param>
		'Public Function Insert(fields As String(), Optional entityType As Type = Nothing) As DuplicatedFields(Of T)
		'	If fields.IsEmpty Then Return Me
		'	entityType = If(entityType, GetType(T))

		'	Dim exps As New DuplicatedExpression(Of T)
		'	Dim pros = entityType.GetAllProperties.Select(Function(x) x.Name).ToList

		'	fields.Where(Function(x) x.NotEmpty AndAlso pros.Contains(x, StringComparer.OrdinalIgnoreCase)).
		'		Distinct.ToList.
		'		ForEach(Sub(field)
		'					Dim expPar = Expression.Parameter(entityType, "x")
		'					Dim expPro = Expression.Property(expPar, field)
		'					Dim exp = Expression.Lambda(expPro, expPar)

		'					exps.Insert(exp)
		'				End Sub)

		'	Return Insert(exps)
		'End Function

		''' <summary>创建表达式</summary>
		Public Function Insert(exps As DuplicatedExpression(Of T)) As DuplicatedFields(Of T)
			If exps IsNot Nothing AndAlso exps.HasExpression AndAlso Not _Instance.Contains(exps) Then _Instance.Add(exps)
			Return Me
		End Function

		''' <summary>创建表达式</summary>
		Public Function Insert(exp As Expression(Of Func(Of T, Object))) As DuplicatedFields(Of T)
			Dim exps As New DuplicatedExpression(Of T)
			exps.Insert(exp)

			Return Insert(exps)
		End Function

		''' <summary>创建表达式</summary>
		Public Function Insert(ParamArray expArr As Expression(Of Func(Of T, Object))()) As DuplicatedFields(Of T)
			Dim exps As New DuplicatedExpression(Of T)
			For Each exp In expArr
				exps.Insert(exp)
			Next

			Return Insert(exps)
		End Function

		''' <summary>获取组合表达式</summary>
		Public Function MakeQuerys(db As IFreeSql, entity As T) As Dictionary(Of String, ISelect(Of T))
			Dim Ret As New Dictionary(Of String, ISelect(Of T))

			For Each exp In _Instance
				Dim res = exp.MakeQuery(db, entity)
				If res.Name.NotEmpty AndAlso Not Ret.ContainsKey(res.Name) Then
					Ret.Add(res.Name, res.Query)
				End If
			Next

			Return Ret
		End Function
	End Class

End Namespace