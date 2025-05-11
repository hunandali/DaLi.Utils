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
' 	FreeSQL 查询扩展
'
' 	name: Extension.QueryExtension
' 	create: 2023-02-15
' 	memo: FreeSQL 查询扩展
'
' ------------------------------------------------------------

Imports System.Data
Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports FreeSql
Imports FreeSql.Internal.Model

Namespace Extension

	Partial Public Module QueryExtension

#Region "动态查询"

		''' <summary>模糊搜索文本</summary>
		''' <param name="keywords">关键词</param>
		''' <param name="searchFields">要搜索的文本字段，默认为 title / name</param>
		<Extension()>
		Public Function WhereSearch(Of T As {IEntity, Class})(this As ISelect(Of T), keywords As String, Optional searchFields As String = Nothing) As ISelect(Of T)
			If this Is Nothing Then Return Nothing
			If keywords.IsEmpty Then Return this

			' 获取所有文本字段
			Dim fields = GetType(T).GetAllProperties.Where(Function(x) x.PropertyType.IsString).Select(Function(x) x.Name).ToArray
			If fields.IsEmpty Then Return Nothing

			' 分析搜索字段
			searchFields &= ",title,name"
			fields = searchFields.Split(","c).
				Select(Function(x)
						   x = x.Trim

						   ' ID 不做搜索
						   If x.IsEmpty OrElse x.Equals("id", StringComparison.OrdinalIgnoreCase) Then Return ""

						   ' 返回有效字段
						   Return fields.Where(Function(y) y.Equals(x, StringComparison.OrdinalIgnoreCase)).FirstOrDefault
					   End Function).
				Where(Function(x) x.NotEmpty).
				Distinct.
				ToArray

			' 无此搜索字段
			If fields.IsEmpty Then Return Nothing

			' 条件
			Dim filter As New DynamicFilterInfo With {
				.Logic = DynamicFilterLogic.Or,
				.Filters = New List(Of DynamicFilterInfo)
			}

			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType
			Dim Id = keywords.ToValue(IdType)
			If Id IsNot Nothing Then
				Dim filterID As New DynamicFilterInfo With {.Field = "ID", .[Operator] = DynamicFilterOperator.Eq, .Value = Id}
				filter.Filters.Add(filterID)
			End If

			For Each field In fields
				Dim filterKeyword As New DynamicFilterInfo With {.Field = field, .[Operator] = DynamicFilterOperator.Contains, .Value = keywords}
				filter.Filters.Add(filterKeyword)
			Next

			Return this.WhereDynamicFilter(filter)
		End Function

#End Region

#Region "Where 查询"

		''' <summary>扩展查询，工具条件查询</summary>
		<Extension>
		Public Function WhereIf(Of T)(this As ISelect(Of T), chk As Boolean, Optional trueExpress As Expression(Of Func(Of T, Boolean)) = Nothing, Optional falseExpress As Expression(Of Func(Of T, Boolean)) = Nothing) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If chk Then
				If trueExpress IsNot Nothing Then this = this.Where(trueExpress)
			Else
				If falseExpress IsNot Nothing Then this = this.Where(falseExpress)
			End If

			Return this
		End Function

		''' <summary>扩展查询，工具条件查询</summary>
		<Extension>
		Public Function WhereIf(Of T)(this As ISelect(Of T), chk As Func(Of Boolean), Optional trueExpress As Expression(Of Func(Of T, Boolean)) = Nothing, Optional falseExpress As Expression(Of Func(Of T, Boolean)) = Nothing) As ISelect(Of T)
			Return this.WhereIf(chk?.Invoke, trueExpress, falseExpress)
		End Function

		''' <summary>扩展查询，对象不为空时执行查询操作</summary>
		<Extension>
		Public Function WhereIf(Of T)(this As ISelect(Of T), obj As Object, trueExpress As Expression(Of Func(Of T, Boolean))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If obj IsNot Nothing Then this = this.Where(trueExpress)

			Return this
		End Function

		''' <summary>扩展查询，文本有内容时执行查询操作</summary>
		<Extension>
		Public Function WhereIf(Of T)(this As ISelect(Of T), obj As String, trueExpress As Expression(Of Func(Of T, Boolean))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If obj.NotEmpty Then this = this.Where(trueExpress)

			Return this
		End Function

#End Region

#Region "等于与不等于"

		''' <summary>等于查询</summary>
		<Extension()>
		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim equal = Expression.Equal(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>等于查询</summary>
		<Extension()>
		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Dim a As S? = val
			Return this.WhereEquals(a, field)
		End Function

		''' <summary>等于查询</summary>
		<Extension()>
		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim equal = Expression.Equal(field.Body, Expression.Constant(val))
				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>等于查询</summary>
		<Extension()>
		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Dim a As S? = val
			Return this.WhereEquals(a, field)
		End Function

		''' <summary>等于查询</summary>
		<Extension()>
		Public Function WhereEquals(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim equal = Expression.Equal(field.Body, Expression.Constant(val))
				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>不等于查询</summary>
		<Extension()>
		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim equal = Expression.NotEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>不等于查询</summary>
		<Extension()>
		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Dim a As S? = val
			Return this.WhereNotEquals(a, field)
		End Function

		''' <summary>不等于查询</summary>
		<Extension()>
		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim equal = Expression.NotEqual(field.Body, Expression.Constant(val))
				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>不等于查询</summary>
		<Extension()>
		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Dim a As S? = val
			Return this.WhereNotEquals(a, field)
		End Function

		''' <summary>不等于查询</summary>
		<Extension()>
		Public Function WhereNotEquals(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val.IsNull Then val = String.Empty

			Dim equal = Expression.NotEqual(field.Body, Expression.Constant(val))
			Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
			Return this.Where(where)
		End Function

#End Region

#Region "包含"

		'''' <summary>包含查询</summary>
		'<Extension()>
		'Public Function WhereContain(Of T, S)(this As ISelect(Of T), val As S(), field As Expression(Of Func(Of T, S))) As ISelect(Of T)
		'	If this Is Nothing Then Return Nothing

		'	Return this.WhereContain(val.ToList, field)
		'End Function

		''' <summary>包含查询（一项则使用等于，多项则使用包含）</summary>
		<Extension()>
		Public Function WhereContain(Of T, S)(this As ISelect(Of T), val As IEnumerable(Of S), field As Expression(Of Func(Of T, S))) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val?.Any AndAlso val(0) IsNot Nothing Then
				Dim exp As Expression
				If val.Count = 1 Then
					If GetType(S).IsNullable Then
						exp = Expression.PropertyOrField(field.Body, "Value")
						exp = Expression.Equal(exp, Expression.Constant(val.FirstOrDefault))
					Else
						exp = Expression.Equal(field.Body, Expression.Constant(val.FirstOrDefault))
					End If
				Else
					exp = Expression.Call(Expression.Constant(val.ToList), "Contains", Nothing, field.Body)
				End If

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			Else
				Return this
			End If
		End Function

		''' <summary>文本包含查询</summary>
		<Extension()>
		Public Function WhereContain(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String)), Optional ignoreCase As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val.IsNull Then
				Return this
			Else
				Dim exp As Expression

				If ignoreCase Then
					val = val.ToLowerInvariant
					exp = Expression.Call(field.Body, "ToLower", Nothing)
				Else
					exp = field.Body
				End If

				exp = Expression.Call(exp, "Contains", Nothing, Expression.Constant(val))

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

#End Region

#Region "两者之间"

#Region "S? S?"

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S?, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If valMin Is Nothing AndAlso valMax Is Nothing Then
				Return this
			Else
				Return this.WhereGreaterThan(valMin, field, includeMin).WhereLessThan(valMax, field, includeMax)
			End If
		End Function

		''' <summary>大于 / 等于？</summary>
		<Extension()>
		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim exp As BinaryExpression

				If includeVal Then
					exp = Expression.GreaterThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				Else
					exp = Expression.GreaterThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				End If

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>小于 / 等于？</summary>
		<Extension()>
		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim exp As BinaryExpression

				If includeVal Then
					exp = Expression.LessThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				Else
					exp = Expression.LessThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
				End If

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

#End Region

#Region "S S?"

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S?, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>大于 / 等于？</summary>
		<Extension()>
		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
			Dim newVal As S? = val
			Return this.WhereGreaterThan(newVal, field, includeVal)
		End Function

		''' <summary>小于 / 等于？</summary>
		<Extension()>
		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
			Dim newVal As S? = val
			Return this.WhereLessThan(newVal, field, includeVal)
		End Function

#End Region

#Region "S? S"

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S?, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If valMin Is Nothing AndAlso valMax Is Nothing Then
				Return this
			Else
				Return this.WhereGreaterThan(valMin, field, includeMin).WhereLessThan(valMax, field, includeMax)
			End If
		End Function

		''' <summary>大于 / 等于？</summary>
		<Extension()>
		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim exp As BinaryExpression

				If includeVal Then
					exp = Expression.GreaterThanOrEqual(field.Body, Expression.Constant(val))
				Else
					exp = Expression.GreaterThan(field.Body, Expression.Constant(val))
				End If

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function

		''' <summary>小于 / 等于？</summary>
		<Extension()>
		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			If val Is Nothing Then
				Return this
			Else
				Dim exp As BinaryExpression

				If includeVal Then
					exp = Expression.LessThanOrEqual(field.Body, Expression.Constant(val))
				Else
					exp = Expression.LessThan(field.Body, Expression.Constant(val))
				End If

				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
				Return this.Where(where)
			End If
		End Function
#End Region

#Region "S S"

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S?, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>两者之间查询</summary>
		<Extension()>
		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
			Dim min As S? = valMin
			Dim max As S? = valMax
			Return this.WhereBetween(min, max, field, includeMin, includeMax)
		End Function

		''' <summary>大于 / 等于？</summary>
		<Extension()>
		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
			Dim newVal As S? = val
			Return this.WhereGreaterThan(newVal, field, includeVal)
		End Function

		''' <summary>小于 / 等于？</summary>
		<Extension()>
		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
			Dim newVal As S? = val
			Return this.WhereLessThan(newVal, field, includeVal)
		End Function

#End Region

#End Region

#Region "从ID获取"

		''' <summary>ID 不等于</summary>
		<Extension()>
		Public Function WhereNotID(Of T As {IEntity, Class})(this As ISelect(Of T), id As Long) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Return this.WhereNotEquals(id, Function(x) x.ID)
		End Function

		''' <summary>ID 等于</summary>
		<Extension()>
		Public Function WhereID(Of T As {IEntity, Class})(this As ISelect(Of T), id As Long) As ISelect(Of T)
			If this Is Nothing Then Return Nothing

			Return this.WhereEquals(id, Function(x) x.ID)
		End Function

		''' <summary>ID 包含于</summary>
		<Extension()>
		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ParamArray ids As Long()) As ISelect(Of T)
			If this Is Nothing Then Return Nothing
			If ids.IsEmpty Then Return this.Where("1=0")

			Return this.WhereContain(ids, Function(x) x.ID)
		End Function

		''' <summary>ID 包含于</summary>
		<Extension()>
		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As IEnumerable(Of Long)) As ISelect(Of T)
			If this Is Nothing Then Return Nothing
			If ids.IsEmpty Then Return this.Where("1=0")

			Return this.WhereContain(ids, Function(x) x.ID)
		End Function

		''' <summary>ID 包含于</summary>
		<Extension()>
		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As String) As ISelect(Of T)
			Return this.WhereIDs(ids?.ToLongArray)
		End Function

		''' <summary>ID 包含于</summary>
		<Extension()>
		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As IEnumerable(Of Long?)) As ISelect(Of T)
			Return this.WhereIDs(ids?.Where(Function(x) x.HasValue).Select(Function(x) x.Value))
		End Function

#End Region

	End Module

End Namespace