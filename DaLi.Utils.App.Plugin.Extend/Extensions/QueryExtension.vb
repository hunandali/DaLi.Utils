﻿'' ------------------------------------------------------------
''
'' 	Copyright © 2021 湖南大沥网络科技有限公司.
'' 	Dali.Utils Is licensed under Mulan PSL v2.
''
'' 	  author:	木炭(WOODCOAL)
'' 	   email:	i@woodcoal.cn
'' 	homepage:	http://www.hunandali.com/
''
'' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
''
'' ------------------------------------------------------------
''
'' 	FreeSQL 查询扩展
''
'' 	name: Extension.QueryExtension
'' 	create: 2023-02-15
'' 	memo: FreeSQL 查询扩展
''
'' ------------------------------------------------------------

'Imports System.Data
'Imports System.Linq.Expressions
'Imports System.Reflection
'Imports System.Runtime.CompilerServices
'Imports FreeSql
'Imports FreeSql.Internal.Model

'Namespace Extension

'	''' <summary>FreeSQL 查询扩展</summary>
'	Partial Public Module QueryExtension

'#Region "动态查询"

'		''' <summary>根据类型创建查询对象 ISelect(Of )</summary>
'		<Extension>
'		Public Function [Select](this As IFreeSql, dataType As Type) As ISelect(Of IEntity)
'			If this Is Nothing OrElse dataType Is Nothing OrElse Not dataType.IsComeFrom(Of IEntity) Then Return Nothing
'			Return this.Select(Of IEntity).AsType(dataType)
'		End Function

'		'		''' <summary>动态查询，用于 标签中 SQL 语句查询</summary>
'		'		<Extension>
'		'		Public Function WhereDynamic(Of T As {IEntity, Class})(this As IFreeSql, attrs As NameValueDictionary) As ISelect(Of T)
'		'			If this Is Nothing OrElse attrs.IsEmpty Then Return this

'		'			'查询对象
'		'			Dim dataQuery = this.Select(Of T)

'		'			' 类型
'		'			Dim dataType = GetType(T)

'		'#Region "父级处理"

'		'			'---------------------------------------------------------
'		'			' 处理父级对象，存在 ParentId 字段，且类型为 Long
'		'			'---------------------------------------------------------
'		'			Dim parentIdType = dataType.GetSingleProperty("ParentId")?.PropertyType
'		'			If parentIdType IsNot Nothing AndAlso parentIdType.IsNullableNumber AndAlso attrs("parent").NotEmpty Then
'		'				' 检查是否存在层级，存在层级需要递归查询，否则，直接使用 parentId 来处理
'		'				Dim parentValue = attrs("parent")

'		'				' 如果上级为 Long 且有效则继续分析
'		'				Dim parentIds = parentValue.ToLongList
'		'				If parentIds.NotEmpty Then
'		'					' 当前模式，信息模式还是栏目模式
'		'					' 信息模式，上级为信息栏目标识
'		'					' 栏目模式，上级为父级栏目，此时如果 level 存在 self 则需要查询本身
'		'					' 如果存在 Parent 字段，且字段的类型与当前类型一致则认为是栏目模式，否则无法分析
'		'					' self 表示需要分析当前父级栏目（对信息无效）
'		'					' all 表示需要获取所有下级，需要存在 Parent 字段
'		'					' selfall 表示需要当前栏目以及此栏目下所有下级，需要存在 Parent 字段
'		'					' 所以 Parent 字段与 Level 需要同时存在才能继续操作

'		'					Dim parentIsNull = parentIdType.IsNullable
'		'					Dim parentType = dataType.GetSingleProperty("parent")?.PropertyType
'		'					Dim IsNode = parentType IsNot Nothing AndAlso parentType = dataType

'		'					Dim parentLevel = attrs("parentLevel").EmptyValue.ToLowerInvariant
'		'					If parentIds.Count = 1 AndAlso parentLevel.NotEmpty AndAlso parentType IsNot Nothing Then
'		'						' 只有一个 ParentId，且存在 Level 则需要继续分析
'		'						Dim parentId = parentIds(0)

'		'						Select Case parentLevel
'		'							Case "self"
'		'								' self 表示需要分析当前父级栏目（对信息无效）
'		'								If IsNode Then
'		'									' 仅查询当前 ID 项目及当前子项目
'		'									' this.Where(Function(x) x.ID.Equals(ParentId) OrElse x.ParentId.Equals(ParentId))
'		'									Dim expPar = Expression.Parameter(dataType, "x")
'		'									Dim expID = Expression.Property(expPar, "ID")
'		'									Dim expParent = Expression.Property(expPar, "ParentId")
'		'									If parentIsNull Then expParent = Expression.PropertyOrField(expParent, "Value")

'		'									Dim expValue = Expression.Constant(parentId)
'		'									Dim expLeft = Expression.Equal(expID, expValue)
'		'									Dim expRight = Expression.Equal(expParent, expValue)

'		'									' 查询
'		'									dataQuery.Where(Expression.Lambda(Of Func(Of T, Boolean))(Expression.OrElse(expLeft, expRight), expPar))
'		'								End If

'		'								' 置空列表，防止再此查询
'		'								parentIds = Nothing

'		'							Case "all", "selfall"
'		'								Dim parentList = If(this.SelectChildIds(parentType, parentId), New List(Of Guid))

'		'								' 包含本身
'		'								If Not IsNode AndAlso parentLevel = "selfall" Then parentList.Add(parentId)

'		'								parentIds = parentList.ToArray
'		'						End Select
'		'					End If

'		'					' 存在查询列表，设置到参数 parentId以便后续查询
'		'					If parentIds.NotEmpty Then attrs("parentId") = parentIds.JoinString

'		'					'If parentIds.NotEmpty Then
'		'					'	Dim expPar = Expression.Parameter(dataType, "x")
'		'					'	Dim expParent = Expression.Property(expPar, "ParentId")
'		'					'	Dim expValue As ConstantExpression

'		'					'	If parentIsNull Then
'		'					'		Dim ParentList As New List(Of Guid?)
'		'					'		For I = 0 To parentIds.Length - 1
'		'					'			If Not ParentList.Contains(parentIds(I)) Then ParentList.Add(parentIds(I))
'		'					'		Next

'		'					'		expValue = Expression.Constant(ParentList)
'		'					'	Else
'		'					'		expValue = Expression.Constant(parentIds)
'		'					'	End If

'		'					'	dataQuery.Where(Expression.Lambda(Of Func(Of T, Boolean))(Expression.Call(expValue, "Contains", Nothing, expParent)))
'		'					'End If
'		'				End If
'		'			End If

'		'#End Region

'		'			'------------------
'		'			' 属性查询
'		'			'------------------
'		'			Dim pros = dataType.GetAllProperties?.Where(Function(x) attrs.ContainsKey(x.Name) AndAlso x.GetCustomAttribute(Of NotMappedAttribute) Is Nothing).ToList

'		'			If pros?.Count > 0 Then
'		'				For Each pro In pros
'		'					' 分析属性值，进行不同比较
'		'					dataQuery = dataQuery.WhereDynamic(pro.Name, pro.PropertyType, attrs(pro.Name))
'		'					If dataQuery Is Nothing Then Return Nothing
'		'				Next
'		'			End If

'		'			'------------------
'		'			' Where 条件参数
'		'			'------------------
'		'			Dim Where = attrs("Where")
'		'			If Where.NotEmpty Then
'		'				Where = Where.Replace("'", """")
'		'				dataQuery = dataQuery.Where(Where)
'		'			End If

'		'			'------------------
'		'			' 排序
'		'			'------------------
'		'			Dim Order = attrs("Order")
'		'			If Order.NotEmpty Then dataQuery = dataQuery.OrderBy(Order)

'		'			Return dataQuery
'		'		End Function

'		'		''' <summary>动态查询，用于 标签中 SQL 语句查询</summary>
'		'<Extension>
'		'Public Function WhereDynamic(Of T As {IEntity, Class})(this As ISelect(Of T), proName As String, proType As Type, proValue As String) As ISelect(Of T)
'		'	' 参数是否有效
'		'	If this Is Nothing OrElse proName.IsEmpty OrElse proType Is Nothing Then Return this

'		'	' 调整参数值
'		'	If proValue.IsEmpty Then Return Nothing

'		'	' 检查是否带叹号，叹号表示非，如果过滤叹号内容为空，则此查询也无效
'		'	Dim IsReverse = False

'		'	If proValue.StartsWith("!") OrElse proValue.StartsWith("！") Then
'		'		proValue = proValue.Substring(1).Trim
'		'		IsReverse = True
'		'	End If
'		'	If proValue.IsEmpty Then Return Nothing

'		'	' 判断是否 T? 类型，如果是则去实际类型
'		'	Dim IsNullable = proType.IsNullable
'		'	If IsNullable Then proType = proType.GetGenericArguments?(0)

'		'	' GUID 使用字符方式处理
'		'	Dim type = proType.GetTypeCode
'		'	If proType.IsGuid Then type = TypeCode.String

'		'	' 创建表达式 Function(x)x.name = value
'		'	' 参数
'		'	Dim expPar = Expression.Parameter(GetType(T), "x")

'		'	' 属性
'		'	Dim expPro = Expression.Property(expPar, proName)
'		'	If IsNullable Then expPro = Expression.PropertyOrField(expPro, "Value")

'		'	' 查询表达式
'		'	Dim expBin As Expression = Nothing

'		'	' 分析属性值，进行不同比较

'		'	' 以 ! 开头标识不等于
'		'	' 以 ~ 标识区间
'		'	' 以 * 标识文本like
'		'	' 以 > 标识大于
'		'	' 以 < 标识小于
'		'	Select Case type
'		'		Case TypeCode.Object, TypeCode.DBNull, TypeCode.Empty
'		'			' 无效，不操作直接返回
'		'			Return this

'		'		Case TypeCode.Boolean
'		'			' 是非
'		'			Dim expVal = Expression.Constant(proValue.ToBoolean)

'		'			If IsReverse Then
'		'				expBin = Expression.NotEqual(expPro, expVal)
'		'			Else
'		'				expBin = Expression.Equal(expPro, expVal)
'		'			End If

'		'		Case TypeCode.String
'		'			' 字符 / GUID

'		'			If proType.IsGuid Then
'		'				' GUID
'		'				' 等于 不等于 数组包含

'		'				' 如果不存在 Guid 则表示无效，返回无效查询
'		'				Dim Ids = proValue.ToGuidList
'		'				If Ids.IsEmpty Then
'		'					Return this
'		'				Else
'		'					'存在数据，1个GUID表示等于，多个表示包含
'		'					If Ids.Length = 1 Then
'		'						Dim expVal = Expression.Constant(Ids(0))

'		'						'If IsNullable Then
'		'						'	Dim Id As Guid? = Ids(0)
'		'						'	expVal = Expression.Constant(Id)
'		'						'Else
'		'						'	expVal = Expression.Constant(Ids(0))
'		'						'End If

'		'						If IsReverse Then
'		'							expBin = Expression.NotEqual(expPro, expVal)
'		'						Else
'		'							expBin = Expression.Equal(expPro, expVal)
'		'						End If
'		'					Else
'		'						If IsNullable Then
'		'							Dim IdList As New List(Of Guid?)
'		'							For I = 0 To Ids.Length - 1
'		'								If Not IdList.Contains(Ids(I)) Then IdList.Add(Ids(I))
'		'							Next
'		'							expBin = Expression.Call(Expression.Constant(IdList), "Contains", Nothing, expPro.Expression)
'		'						Else
'		'							expBin = Expression.Call(Expression.Constant(Ids.ToList), "Contains", Nothing, expPro)
'		'						End If

'		'						If IsReverse Then expBin = Expression.Not(expBin)
'		'					End If
'		'				End If

'		'			Else
'		'				' 字符
'		'				' 简单 Like
'		'				If proValue <> "*" Then
'		'					If proValue.StartsWith("*"c) AndAlso proValue.EndsWith("*"c) Then
'		'						' 包含内容
'		'						proValue = proValue.Substring(1, proValue.Length - 2)
'		'						expBin = Expression.Call(expPro, "Contains", Nothing, Expression.Constant(proValue))
'		'						If IsReverse Then expBin = Expression.Not(expBin)

'		'					ElseIf proValue.StartsWith("*"c) AndAlso Not proValue.EndsWith("*"c) Then
'		'						' EndWith
'		'						proValue = proValue.Substring(1)
'		'						expBin = Expression.Call(expPro, "EndsWith", Nothing, Expression.Constant(proValue))
'		'						If IsReverse Then expBin = Expression.Not(expBin)

'		'					ElseIf Not proValue.StartsWith("*"c) AndAlso proValue.EndsWith("*"c) Then
'		'						' StartsWith
'		'						proValue = proValue.Substring(0, proValue.Length - 1)
'		'						expBin = Expression.Call(expPro, "StartsWith", Nothing, Expression.Constant(proValue))
'		'						If IsReverse Then expBin = Expression.Not(expBin)

'		'					ElseIf proValue.Contains("*"c) Then
'		'						' 包含起始
'		'						Dim Vs = proValue.Split("*"c)
'		'						Dim V1 = Vs(0)
'		'						Dim V2 = Vs(Vs.Length - 1)

'		'						Dim expV1 = Expression.Call(expPro, "StartsWith", Nothing, Expression.Constant(V1))
'		'						Dim expV2 = Expression.Call(expPro, "EndsWith", Nothing, Expression.Constant(V2))

'		'						If IsReverse Then
'		'							expBin = Expression.OrElse(Expression.Not(expV1), Expression.Not(expV2))
'		'						Else
'		'							expBin = Expression.AndAlso(expV1, expV2)
'		'						End If

'		'					Else
'		'						' 整个相等 / 不相等
'		'						Dim expVal = Expression.Constant(proValue)

'		'						If IsReverse Then
'		'							expBin = Expression.NotEqual(expPro, expVal)
'		'						Else
'		'							expBin = Expression.Equal(expPro, expVal)
'		'						End If
'		'					End If
'		'				End If
'		'			End If

'		'		Case Else
'		'			' 数字，日期，枚举
'		'			If proType.IsEnum Then
'		'				' 枚举
'		'				' 只能比较等于或者不等于

'		'				' 获取枚举数据
'		'				Dim em = proType.EnumValue(proValue)
'		'				If em Is Nothing Then
'		'					Return this
'		'				Else
'		'					Dim expVal = Expression.Constant(em)

'		'					If IsReverse Then
'		'						expBin = Expression.NotEqual(expPro, expVal)
'		'					Else
'		'						expBin = Expression.Equal(expPro, expVal)
'		'					End If
'		'				End If
'		'			Else
'		'				' 数字 日期
'		'				' 支持 等于，不等于(!)
'		'				' 支持 大于，大于等于，小于，小于等于(全角符号 ＞，≥，＜，≤)
'		'				' 支持 区间(~)
'		'				Dim value = proValue

'		'				If value.Contains("~"c) OrElse value.Contains("～"c) Then
'		'					' 区间
'		'					Dim Vs = value.SplitEx({"~", "～"}, SplitEnum.CLEAR_TRIM Or SplitEnum.REMOVE_EMPTY_ENTRIES Or SplitEnum.RETRUN_DBC)

'		'					' 参数无效直接返回
'		'					If Vs.IsEmpty OrElse Vs.Length <> 2 OrElse Vs(0).IsEmpty OrElse Vs(1).IsEmpty Then Return this

'		'					' 第一个值已经初始处理过
'		'					Dim IncV1 = Not IsReverse
'		'					Dim V1 = Vs(0).ToValue(type)

'		'					' 第二个值处理
'		'					Dim IncV2 = True
'		'					If Vs(1).StartsWith("!"c) OrElse Vs(1).StartsWith("！"c) Then
'		'						IncV2 = False
'		'						Vs(1) = Vs(1).Substring(1)
'		'						If Vs(1).IsEmpty Then Return this
'		'					End If
'		'					Dim V2 = Vs(1).ToValue(type)

'		'					' 参数无效
'		'					If V1 Is Nothing OrElse V2 Is Nothing Then Return this

'		'					Dim expVA = Expression.Constant(V1)
'		'					Dim expVB = Expression.Constant(V2)

'		'					Dim expV1 = If(IncV1, Expression.GreaterThanOrEqual(expPro, expVA), Expression.GreaterThan(expPro, expVA))
'		'					Dim expV2 = If(IncV2, Expression.LessThanOrEqual(expPro, expVB), Expression.LessThan(expPro, expVB))

'		'					expBin = Expression.AndAlso(expV1, expV2)

'		'				ElseIf value.StartsWith(">=") OrElse value.StartsWith("≥") Then
'		'					' 大于等于
'		'					If value.StartsWith(">=") Then
'		'						value = value.Substring(2)
'		'					Else
'		'						value = value.Substring(1)
'		'					End If

'		'					If IsReverse Then
'		'						expBin = Expression.LessThan(expPro, Expression.Constant(value.ToValue(type)))
'		'					Else
'		'						expBin = Expression.GreaterThanOrEqual(expPro, Expression.Constant(value.ToValue(type)))
'		'					End If

'		'				ElseIf value.StartsWith(">"c) OrElse value.StartsWith("＞"c) Then
'		'					' 大于
'		'					value = value.Substring(1)
'		'					If IsReverse Then
'		'						expBin = Expression.LessThanOrEqual(expPro, Expression.Constant(value.ToValue(type)))
'		'					Else
'		'						expBin = Expression.GreaterThan(expPro, Expression.Constant(value.ToValue(type)))
'		'					End If

'		'				ElseIf value.StartsWith("<=") OrElse value.StartsWith("≤") Then
'		'					' 小于等于
'		'					If value.StartsWith("<=") Then
'		'						value = value.Substring(2)
'		'					Else
'		'						value = value.Substring(1)
'		'					End If

'		'					If IsReverse Then
'		'						expBin = Expression.GreaterThan(expPro, Expression.Constant(value.ToValue(type)))
'		'					Else
'		'						expBin = Expression.LessThanOrEqual(expPro, Expression.Constant(value.ToValue(type)))
'		'					End If

'		'				ElseIf value.StartsWith("<"c) OrElse value.StartsWith("＜"c) Then
'		'					' 小于
'		'					value = value.Substring(1)

'		'					If IsReverse Then
'		'						expBin = Expression.GreaterThanOrEqual(expPro, Expression.Constant(value.ToValue(type)))
'		'					Else
'		'						expBin = Expression.LessThan(expPro, Expression.Constant(value.ToValue(type)))
'		'					End If

'		'				ElseIf value.Contains(","c) Then
'		'					' 包含
'		'					Dim Ids = value.SplitDistinct
'		'					If Ids.NotEmpty Then
'		'						Dim IdList As IList = Nothing

'		'						Select Case type
'		'							Case TypeCode.Char
'		'								IdList = If(IsNullable, New List(Of Char?), New List(Of Char))

'		'							Case TypeCode.SByte
'		'								IdList = If(IsNullable, New List(Of SByte?), New List(Of SByte))

'		'							Case TypeCode.Byte
'		'								IdList = If(IsNullable, New List(Of Byte?), New List(Of Byte))

'		'							Case TypeCode.Int16
'		'								IdList = If(IsNullable, New List(Of Short?), New List(Of Short))

'		'							Case TypeCode.UInt16
'		'								IdList = If(IsNullable, New List(Of UShort?), New List(Of UShort))

'		'							Case TypeCode.Int32
'		'								IdList = If(IsNullable, New List(Of Integer?), New List(Of Integer))

'		'							Case TypeCode.UInt32
'		'								IdList = If(IsNullable, New List(Of UInteger?), New List(Of UInteger))

'		'							Case TypeCode.Int64
'		'								IdList = If(IsNullable, New List(Of Long?), New List(Of Long))

'		'							Case TypeCode.UInt64
'		'								IdList = If(IsNullable, New List(Of ULong?), New List(Of ULong))

'		'							Case TypeCode.Single
'		'								IdList = If(IsNullable, New List(Of Single?), New List(Of Single))

'		'							Case TypeCode.Double
'		'								IdList = If(IsNullable, New List(Of Double?), New List(Of Double))

'		'							Case TypeCode.Decimal
'		'								IdList = If(IsNullable, New List(Of Decimal?), New List(Of Decimal))

'		'							Case TypeCode.DateTime
'		'								IdList = If(IsNullable, New List(Of Date?), New List(Of Date))

'		'						End Select

'		'						For I = 0 To Ids.Length - 1
'		'							Dim Id = Ids(I).ToValue(type)
'		'							If Id IsNot Nothing AndAlso Not IdList.Contains(Id) Then IdList.Add(Id)
'		'						Next

'		'						If IdList.Count > 0 Then
'		'							If IsNullable Then expPro = expPro.Expression

'		'							expBin = Expression.Call(Expression.Constant(IdList), "Contains", Nothing, expPro)
'		'							If IsReverse Then expBin = Expression.Not(expBin)
'		'						End If
'		'					End If
'		'				Else
'		'					Dim expVal = Expression.Constant(value.ToValue(type))

'		'					If IsReverse Then
'		'						expBin = Expression.NotEqual(expPro, expVal)
'		'					Else
'		'						expBin = Expression.Equal(expPro, expVal)
'		'					End If
'		'				End If
'		'			End If
'		'	End Select

'		'	If expBin Is Nothing Then
'		'		Return this
'		'	Else
'		'		Return this.Where(Expression.Lambda(Of Func(Of T, Boolean))(expBin, expPar))
'		'	End If
'		'End Function

'		''' <summary>创建查询对象</summary>
'		<Extension>
'		Public Function Query(this As IFreeSql, moduleType As Type, filter As DynamicFilterInfo, Optional count As Integer = 0, Optional sort As String = Nothing) As IEnumerable(Of IEntity)
'			If this Is Nothing OrElse moduleType Is Nothing OrElse filter Is Nothing Then Return Nothing

'			' DB.Select(of T)
'			Dim dbMethod = this.GetType.
'				GetMember("Select").Cast(Of MethodInfo).
'				Where(Function(x) x.IsGenericMethodDefinition).
'				FirstOrDefault
'			dbMethod = dbMethod?.MakeGenericMethod(moduleType)

'			Dim selectObj = dbMethod?.Invoke(this, Nothing)
'			If selectObj Is Nothing Then Return Nothing

'			Dim queryMethod = GetType(QueryExtension).GetMethod("DynamicQuery")
'			queryMethod = queryMethod?.MakeGenericMethod(moduleType)

'			Return queryMethod?.Invoke(Nothing, {selectObj, filter, count, sort})
'		End Function

'		'''' <summary>动态查询</summary>
'		<Extension()>
'		Public Function DynamicQuery(Of T As {IEntity, Class})(this As ISelect(Of T), filter As DynamicFilterInfo, Optional count As Integer = 0, Optional sort As String = Nothing) As List(Of T)
'			If this Is Nothing OrElse filter Is Nothing Then Return Nothing

'			Dim ret = this.WhereDynamicFilter(filter)

'			If sort.NotEmpty Then ret.OrderBy(sort)

'			If count > 0 Then ret = ret.Take(count)
'			Return ret.ToList
'		End Function

'#End Region

'#Region "Where 查询"

'		''' <summary>扩展查询，工具条件查询</summary>
'		<Extension>
'		Public Function WhereIf(Of T)(this As ISelect(Of T), chk As Boolean, Optional trueExpress As Expression(Of Func(Of T, Boolean)) = Nothing, Optional falseExpress As Expression(Of Func(Of T, Boolean)) = Nothing) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If chk Then
'				If trueExpress IsNot Nothing Then this = this.Where(trueExpress)
'			Else
'				If falseExpress IsNot Nothing Then this = this.Where(falseExpress)
'			End If

'			Return this
'		End Function

'		''' <summary>扩展查询，工具条件查询</summary>
'		<Extension>
'		Public Function WhereIf(Of T)(this As ISelect(Of T), chk As Func(Of Boolean), Optional trueExpress As Expression(Of Func(Of T, Boolean)) = Nothing, Optional falseExpress As Expression(Of Func(Of T, Boolean)) = Nothing) As ISelect(Of T)
'			Return this.WhereIf(chk?.Invoke, trueExpress, falseExpress)
'		End Function

'		''' <summary>扩展查询，对象不为空时执行查询操作</summary>
'		<Extension>
'		Public Function WhereIf(Of T)(this As ISelect(Of T), obj As Object, trueExpress As Expression(Of Func(Of T, Boolean))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If obj IsNot Nothing Then this = this.Where(trueExpress)

'			Return this
'		End Function

'		''' <summary>扩展查询，文本有内容时执行查询操作</summary>
'		<Extension>
'		Public Function WhereIf(Of T)(this As ISelect(Of T), obj As String, trueExpress As Expression(Of Func(Of T, Boolean))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If obj.NotEmpty Then this = this.Where(trueExpress)

'			Return this
'		End Function

'#End Region

'#Region "等于与不等于"

'		''' <summary>等于查询</summary>
'		<Extension()>
'		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim equal = Expression.Equal(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>等于查询</summary>
'		<Extension()>
'		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim a As S? = val
'			Return this.WhereEquals(a, field)
'		End Function

'		''' <summary>等于查询</summary>
'		<Extension()>
'		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim equal = Expression.Equal(field.Body, Expression.Constant(val))
'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>等于查询</summary>
'		<Extension()>
'		Public Function WhereEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim a As S? = val
'			Return this.WhereEquals(a, field)
'		End Function

'		''' <summary>等于查询</summary>
'		<Extension()>
'		Public Function WhereEquals(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim equal = Expression.Equal(field.Body, Expression.Constant(val))
'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>不等于查询</summary>
'		<Extension()>
'		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim equal = Expression.NotEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>不等于查询</summary>
'		<Extension()>
'		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim a As S? = val
'			Return this.WhereNotEquals(a, field)
'		End Function

'		''' <summary>不等于查询</summary>
'		<Extension()>
'		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim equal = Expression.NotEqual(field.Body, Expression.Constant(val))
'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>不等于查询</summary>
'		<Extension()>
'		Public Function WhereNotEquals(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim a As S? = val
'			Return this.WhereNotEquals(a, field)
'		End Function

'		''' <summary>不等于查询</summary>
'		<Extension()>
'		Public Function WhereNotEquals(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If String.IsNullOrEmpty(val) Then val = String.Empty

'			Dim equal = Expression.NotEqual(field.Body, Expression.Constant(val))
'			Dim where = Expression.Lambda(Of Func(Of T, Boolean))(equal, field.Parameters(0))
'			Return this.Where(where)
'		End Function

'#End Region

'#Region "包含"

'		'''' <summary>包含查询</summary>
'		'<Extension()>
'		'Public Function WhereContain(Of T, S)(this As ISelect(Of T), val As S(), field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'		'	If this Is Nothing Then Return Nothing

'		'	Return this.WhereContain(val.ToList, field)
'		'End Function

'		''' <summary>包含查询（一项则使用等于，多项则使用包含）</summary>
'		<Extension()>
'		Public Function WhereContain(Of T, S)(this As ISelect(Of T), val As IEnumerable(Of S), field As Expression(Of Func(Of T, S))) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val?.Any AndAlso val(0) IsNot Nothing Then
'				Dim exp As Expression
'				If val.Count = 1 Then
'					If GetType(S).IsNullable Then
'						exp = Expression.PropertyOrField(field.Body, "Value")
'						exp = Expression.Equal(exp, Expression.Constant(val.FirstOrDefault))
'					Else
'						exp = Expression.Equal(field.Body, Expression.Constant(val.FirstOrDefault))
'					End If
'				Else
'					exp = Expression.Call(Expression.Constant(val.ToList), "Contains", Nothing, field.Body)
'				End If

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			Else
'				Return this
'			End If
'		End Function

'		''' <summary>文本包含查询</summary>
'		<Extension()>
'		Public Function WhereContain(Of T)(this As ISelect(Of T), val As String, field As Expression(Of Func(Of T, String)), Optional ignoreCase As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If String.IsNullOrEmpty(val) Then
'				Return this
'			Else
'				Dim exp As Expression

'				If ignoreCase Then
'					val = val.ToLowerInvariant
'					exp = Expression.Call(field.Body, "ToLower", Nothing)
'				Else
'					exp = field.Body
'				End If

'				exp = Expression.Call(exp, "Contains", Nothing, Expression.Constant(val))

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'#End Region

'#Region "两者之间"

'#Region "S? S?"

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S?, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If valMin Is Nothing AndAlso valMax Is Nothing Then
'				Return this
'			Else
'				Return this.WhereGreaterThan(valMin, field, includeMin).WhereLessThan(valMax, field, includeMax)
'			End If
'		End Function

'		''' <summary>大于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim exp As BinaryExpression

'				If includeVal Then
'					exp = Expression.GreaterThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				Else
'					exp = Expression.GreaterThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				End If

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>小于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim exp As BinaryExpression

'				If includeVal Then
'					exp = Expression.LessThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				Else
'					exp = Expression.LessThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(val))
'				End If

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'#End Region

'#Region "S S?"

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S?, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S, field As Expression(Of Func(Of T, S?)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>大于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			Dim newVal As S? = val
'			Return this.WhereGreaterThan(newVal, field, includeVal)
'		End Function

'		''' <summary>小于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S?)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			Dim newVal As S? = val
'			Return this.WhereLessThan(newVal, field, includeVal)
'		End Function

'#End Region

'#Region "S? S"

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S?, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If valMin Is Nothing AndAlso valMax Is Nothing Then
'				Return this
'			Else
'				Return this.WhereGreaterThan(valMin, field, includeMin).WhereLessThan(valMax, field, includeMax)
'			End If
'		End Function

'		''' <summary>大于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim exp As BinaryExpression

'				If includeVal Then
'					exp = Expression.GreaterThanOrEqual(field.Body, Expression.Constant(val))
'				Else
'					exp = Expression.GreaterThan(field.Body, Expression.Constant(val))
'				End If

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function

'		''' <summary>小于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S?, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			If val Is Nothing Then
'				Return this
'			Else
'				Dim exp As BinaryExpression

'				If includeVal Then
'					exp = Expression.LessThanOrEqual(field.Body, Expression.Constant(val))
'				Else
'					exp = Expression.LessThan(field.Body, Expression.Constant(val))
'				End If

'				Dim where = Expression.Lambda(Of Func(Of T, Boolean))(exp, field.Parameters(0))
'				Return this.Where(where)
'			End If
'		End Function
'#End Region

'#Region "S S"

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S?, valMax As S, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S?, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>两者之间查询</summary>
'		<Extension()>
'		Public Function WhereBetween(Of T, S As Structure)(this As ISelect(Of T), valMin As S, valMax As S, field As Expression(Of Func(Of T, S)), Optional includeMin As Boolean = True, Optional includeMax As Boolean = True) As ISelect(Of T)
'			Dim min As S? = valMin
'			Dim max As S? = valMax
'			Return this.WhereBetween(min, max, field, includeMin, includeMax)
'		End Function

'		''' <summary>大于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereGreaterThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			Dim newVal As S? = val
'			Return this.WhereGreaterThan(newVal, field, includeVal)
'		End Function

'		''' <summary>小于 / 等于？</summary>
'		<Extension()>
'		Public Function WhereLessThan(Of T, S As Structure)(this As ISelect(Of T), val As S, field As Expression(Of Func(Of T, S)), Optional includeVal As Boolean = True) As ISelect(Of T)
'			Dim newVal As S? = val
'			Return this.WhereLessThan(newVal, field, includeVal)
'		End Function

'#End Region

'#End Region

'#Region "从ID获取"

'		''' <summary>ID 不等于</summary>
'		<Extension()>
'		Public Function WhereNotID(Of T As {IEntity, Class})(this As ISelect(Of T), id As Long) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Return this.WhereNotEquals(id, Function(x) x.ID)
'		End Function

'		''' <summary>ID 等于</summary>
'		<Extension()>
'		Public Function WhereID(Of T As {IEntity, Class})(this As ISelect(Of T), id As Long) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing

'			Return this.WhereEquals(id, Function(x) x.ID)
'		End Function

'		''' <summary>ID 包含于</summary>
'		<Extension()>
'		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ParamArray ids As Long()) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing
'			If ids.IsEmpty Then Return this.Where("1=0")

'			Return this.WhereContain(ids, Function(x) x.ID)
'		End Function

'		''' <summary>ID 包含于</summary>
'		<Extension()>
'		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As IEnumerable(Of Long)) As ISelect(Of T)
'			If this Is Nothing Then Return Nothing
'			If ids.IsEmpty Then Return this.Where("1=0")

'			Return this.WhereContain(ids, Function(x) x.ID)
'		End Function

'		''' <summary>ID 包含于</summary>
'		<Extension()>
'		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As String) As ISelect(Of T)
'			Return this.WhereIDs(ids?.ToLongList)
'		End Function

'		''' <summary>ID 包含于</summary>
'		<Extension()>
'		Public Function WhereIDs(Of T As {IEntity, Class})(this As ISelect(Of T), ids As IEnumerable(Of Long?)) As ISelect(Of T)
'			Return this.WhereIDs(ids?.Where(Function(x) x.HasValue).Select(Function(x) x.Value))
'		End Function

'#End Region

'#Region "父级操作 基于 IEntityParent"

'		''' <summary>递归获取指定节点的所有父级节点</summary>
'		<Extension>
'		Public Function SelectParents(Of T As {IEntityParent, Class})(this As IFreeSql, parentId As Long?) As List(Of T)
'			If parentId.IsEmpty OrElse this Is Nothing Then Return Nothing

'			Dim parent = this.Select(Of T).WhereID(parentId.Value).ToOne
'			If parent Is Nothing Then Return Nothing

'			Dim datas As New List(Of T) From {
'				parent
'			}

'			' 递归
'			Dim parentList = this.SelectParents(Of T)(parent.ParentId)
'			If parentList.NotEmpty Then datas.AddRange(parentList)

'			Return datas
'		End Function

'		''' <summary>递归获取指定节点的所有父级节点</summary>
'		<Extension>
'		Public Function SelectParents(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T) As List(Of T)
'			If entity Is Nothing OrElse entity.ParentId.IsEmpty OrElse this Is Nothing Then Return Nothing
'			Return this.SelectParents(Of T)(entity.ParentId)
'		End Function

'		''' <summary>递归获取指定节点的所有父级节点</summary>
'		<Extension>
'		Public Function SelectParentIds(Of T As {IEntityParent, Class})(this As IFreeSql, parentId As Long?) As List(Of Long)
'			If parentId.IsEmpty OrElse this Is Nothing Then Return Nothing

'			Dim parent = this.Select(Of T).WhereID(parentId.Value).ToOne
'			If parent Is Nothing Then Return Nothing

'			Dim datas As New List(Of Long) From {
'				parent.ID
'			}

'			' 递归
'			Dim parentList = this.SelectParentIds(Of T)(parent.ParentId)
'			If parentList.NotEmpty Then datas.AddRange(parentList)

'			Return datas
'		End Function

'		''' <summary>递归获取指定节点的所有父级节点编号</summary>
'		<Extension>
'		Public Function SelectParentIds(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T) As List(Of Long)
'			If entity Is Nothing OrElse entity.ParentId.IsEmpty OrElse this Is Nothing Then Return Nothing
'			Return this.SelectParentIds(Of T)(entity.ParentId)
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点</summary>
'		<Extension>
'		Public Function SelectChilds(Of T As {IEntityParent, Class})(this As IFreeSql, Optional id As Long? = Nothing, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			If id.NotEmpty Then query.WhereEquals(id, Function(x) x.ParentId)
'			If Not query.Any Then Return Nothing

'			Dim ret = query.ToList

'			ret.ForEach(Sub(x)
'							Dim cs = this.SelectChilds(Of T)(x.ID)
'							If cs.NotEmpty Then ret.AddRange(cs)
'						End Sub)

'			Return ret
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点</summary>
'		<Extension>
'		Public Function SelectChilds(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectChilds(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点编号</summary>
'		<Extension>
'		Public Function SelectChildIds(Of T As {IEntityParent, Class})(this As IFreeSql, Optional id As Long? = Nothing, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If this Is Nothing Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			If id.NotEmpty Then query.WhereEquals(id, Function(x) x.ParentId)
'			If Not query.Any Then Return Nothing

'			Dim ret = query.ToList(Function(x) x.ID)

'			ret.ForEach(Sub(x)
'							Dim cs = this.SelectChilds(Of T)(x)
'							If cs.NotEmpty Then ret.AddRange(cs)
'						End Sub)

'			Return ret
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点编号</summary>
'		<Extension>
'		Public Function SelectChildIds(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectChildIds(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点编号</summary>
'		<Extension>
'		Public Function SelectChildIds(this As IFreeSql, parentType As System.Type, parentId As Long) As List(Of Long)
'			If this Is Nothing OrElse parentType Is Nothing OrElse parentId.IsEmpty Then Return Nothing

'			' 所有 SelectChildIds
'			' 1. 存在泛类操作
'			' 2. 第二个参数为 Guid 类型
'			' SelectChildIds(this As IFreeSql, parentType As Type, parentId As Guid) As List(Of Guid)
'			' √ SelectChildIds(Of T As Entity_Parent_Base)(this As IFreeSql, id As Guid) As List(Of Guid)
'			' SelectChildIds(Of T As Entity_Parent_Base)(this As IFreeSql, entity As T) As List(Of Guid)
'			Dim result = GetType(QueryExtension) _
'					.GetMember("SelectChildIds") _
'					.Cast(Of MethodInfo) _
'					.Where(Function(x) x.IsGenericMethodDefinition AndAlso x.GetParameters(1).ParameterType.IsLong) _
'					.FirstOrDefault _
'					.MakeGenericMethod(parentType) _
'					.Invoke(Nothing, {this, parentId, Nothing})

'			Return TryCast(result, List(Of Long))
'		End Function

'		''' <summary>递归获取指定节点的所有子集节点编号</summary>
'		<Extension>
'		Public Function HasChilds(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T) As Boolean
'			If entity Is Nothing OrElse entity.ID = 0 Then Return False

'			Return this.Select(Of T).WhereEquals(entity.ID, Function(x) x.ParentId).Any
'		End Function

'		''' <summary>递归获取树级列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
'		''' <param name="parentId">上级</param>
'		''' <param name="nameField">标题字段</param>
'		''' <param name="disabledField">是否禁用字段</param>
'		''' <param name="extField">扩展字段</param>
'		''' <param name="queryVM">查询条件</param>
'		''' <param name="treeUpdate">二次处理结果</param>
'		<Extension>
'		Public Function SelectDataTree(Of T As {IEntityParent, Class})(this As IFreeSql,
'																		   parentId As String,
'																		   Optional nameField As String = Nothing,
'																		   Optional disabledField As String = Nothing,
'																		   Optional extField As String = Nothing,
'																		   Optional queryVM As QueryBase(Of T) = Nothing,
'																		   Optional treeUpdate As Func(Of T, DataTree, DataTree) = Nothing
'																		) As List(Of DataTree)
'			' 分析名称键
'			Dim pros = GetType(T).
'				GetAllProperties.
'				Select(Function(x) x.Name).
'				ToList

'			' 名称键
'			nameField = {nameField, "title", "name", "id"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 禁用键
'			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 是否禁用反向，有可能字段是允许，所以要反向
'			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

'			' 扩展内容键
'			extField = If(pros.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

'			' 主键类型
'			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType

'			' 查询所有数据
'			Dim queryAction As Func(Of String, List(Of DataTree)) = Function(id)
'																		Dim Ret As New List(Of DataTree)

'																		' 查询
'																		Dim query = this.Select(Of T)
'																		queryVM?.QueryExecute(query)

'																		' 上级非空则查询上级，否则查询空上级
'																		If id.NotEmpty Then
'																			Dim tureId = id.ToValue(IdType)
'																			query.Where(Function(x) x.ParentId.Equals(tureId))
'																		Else
'																			If IdType.IsString Then
'																				query.Where("ParentId Is Null Or ParentId = ''")

'																			ElseIf IdType.IsNumber Then
'																				query.Where("ParentId Is Null Or ParentId = 0")

'																			Else
'																				query.Where(Function(x) x.ParentId Is Nothing)
'																			End If
'																		End If

'																		query.ToList.
'																			ForEach(Sub(item)
'																						Dim fields = {"ID", "ParentId", nameField, disabledField, extField}.Where(Function(x) x.NotEmpty).ToArray

'																						' 数据结果对象
'																						Dim dic = item.ToDictionary(False, fields)
'																						Dim data As New DataList(dic, nameField, "ID", "ParentId", disabledField, extField)
'																						If data.Value IsNot Nothing Then
'																							' disabled 需要反转
'																							If isRev Then data.Disabled = Not data.Disabled

'																							Dim tree As New DataTree(data)

'																							' 查询下级数据，标识为空则不再查询
'																							tree.Children = queryAction(tree.Value)

'																							' 二次处理值
'																							tree = If(treeUpdate?.Invoke(item, tree), tree)

'																							Ret.Add(tree)
'																						End If
'																					End Sub)

'																		Return Ret
'																	End Function

'			' 执行查询
'			Return queryAction(parentId)
'		End Function

'#End Region

'#Region "树形操作 基于 IEntityTree"

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTree(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, Optional id As Long? = Nothing, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If this Is Nothing Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			If id.IsEmpty Then
'				' 查询全部数据，直接返回
'				Return query.ToTreeList
'			Else
'				' 需要查询子集
'				Return query.WhereEquals(id, Function(x) x.ParentId).AsTreeCte.ToTreeList
'			End If
'		End Function

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTree(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectTree(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>基于数据库的 SQL 查询语句获取指定节点下所有子项目标识</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeIDs(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, Optional id As Long? = Nothing, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If this Is Nothing Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			If id.IsEmpty Then
'				' 查询全部数据，直接返回
'				Return query.ToList(Function(x) x.ID)
'			Else
'				' 需要查询子集
'				Return query.WhereEquals(id, Function(x) x.ParentId).AsTreeCte.ToList(Function(x) x.ID)
'			End If
'		End Function

'		''' <summary>基于数据库的 SQL 查询语句获取指定节点下所有子项目标识</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeIDs(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectTreeIDs(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeParents(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, id As Long, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If this Is Nothing OrElse id.IsEmpty Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			Return query.WhereEquals(id, Function(x) x.ParentId).AsTreeCte(, True).ToList
'		End Function

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeParents(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of T)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectTreeParents(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeParentIDs(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, id As Long, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If this Is Nothing OrElse id.IsEmpty Then Return Nothing

'			Dim query = this.Select(Of T)
'			queryAction?.Invoke(query)

'			Return query.WhereEquals(id, Function(x) x.ParentId).AsTreeCte(, True).ToList(Function(x) x.ID)
'		End Function

'		''' <summary>基于数据库的 SQL　查询语句获取</summary>
'		''' <remarks>https://freesql.net/guide/select-as-tree.html</remarks>
'		''' <returns>以树形机构返回所有数据</returns>
'		<Extension>
'		Public Function SelectTreeParentIDs(Of T As {IEntityTree(Of T), Class})(this As IFreeSql, entity As T, Optional queryAction As Action(Of ISelect(Of T)) = Nothing) As List(Of Long)
'			If entity Is Nothing Then Return Nothing
'			Return this.SelectTreeParentIDs(Of T)(entity.ID, queryAction)
'		End Function

'		''' <summary>获取树级列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
'		''' <param name="nameField">标题字段</param>
'		''' <param name="disabledField">是否禁用字段</param>
'		''' <param name="extField">扩展字段</param>
'		''' <param name="queryVM">查询条件</param>
'		''' <param name="treeUpdate">二次处理结果</param>
'		<Extension>
'		Public Function SelectTreeList(Of T As {IEntityTree(Of T), Class})(this As IFreeSql,
'																		   Optional id As Long? = Nothing,
'																		   Optional nameField As String = Nothing,
'																		   Optional disabledField As String = Nothing,
'																		   Optional extField As String = Nothing,
'																		   Optional queryVM As QueryBase(Of T) = Nothing,
'																		   Optional treeUpdate As Func(Of T, DataTree, DataTree) = Nothing
'																		) As List(Of DataTree)
'			' 查询所有数据
'			Dim datas = this.SelectTree(Of T)(id, Sub(x) queryVM?.QueryExecute(x))
'			If datas.IsEmpty Then Return Nothing

'			' 分析名称键
'			Dim pros = GetType(T).
'				GetAllProperties.
'				Select(Function(x) x.Name).
'				ToList

'			' 名称键
'			nameField = {nameField, "title", "name", "id"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 禁用键
'			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 是否禁用反向，有可能字段是允许，所以要反向
'			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

'			' 扩展内容键
'			extField = If(pros.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

'			' 查询结果
'			Dim queryAction As Func(Of Long?, List(Of DataTree)) = Function(parentId)
'																	   Dim list As IEnumerable(Of T)
'																	   If parentId.IsEmpty Then
'																		   list = datas.Where(Function(x) x.ParentId Is Nothing OrElse x.ParentId = 0)
'																	   Else
'																		   list = datas.Where(Function(x) x.ParentId = parentId)
'																	   End If

'																	   Dim Ret As New List(Of DataTree)

'																	   list.ToList.
'																			ForEach(Sub(item)
'																						Dim fields = {"ID", "ParentId", nameField, disabledField, extField}.Where(Function(x) x.NotEmpty).ToArray

'																						' 数据结果对象
'																						Dim dic = item.ToDictionary(False, fields)
'																						Dim data As New DataList(dic, nameField, "ID", "ParentId", disabledField, extField)
'																						If data.Value IsNot Nothing Then
'																							' disabled 需要反转
'																							If isRev Then data.Disabled = Not data.Disabled

'																							Dim tree As New DataTree(data)

'																							' 查询下级数据，标识为空则不再查询
'																							tree.Children = queryAction(tree.Value)

'																							' 二次处理值
'																							tree = If(treeUpdate?.Invoke(item, tree), tree)

'																							Ret.Add(tree)
'																						End If
'																					End Sub)

'																	   Return Ret
'																   End Function

'			' 执行查询
'			Return queryAction(id)
'		End Function
'#End Region

'#Region "对象库查询"

'		''' <summary>查询单个项目，并将结果转换成字典类型</summary>
'		<Extension>
'		Public Function QueryDictionary(Of T As {IEntity, Class})(this As IFreeSql, queryVM As QueryBase(Of T), Optional entityConvert As Action(Of T) = Nothing) As IDictionary(Of String, Object)
'			If queryVM Is Nothing Then Return Nothing

'			' 默认查询对象
'			Dim query = this.Select(Of T)

'			' 查询处理
'			queryVM.QueryExecute(query)

'			' 查询结果
'			Dim Result = query.ToOne
'			If Result Is Nothing Then Return Nothing

'			' 转换
'			entityConvert?.Invoke(Result)

'			' 结果处理
'			Return queryVM.ConvertDictionary(Result, False, this)
'		End Function

'		''' <summary>查询列表数量，不转换结果</summary>
'		<Extension>
'		Public Function QueryList(Of T As {IEntity, Class})(this As IFreeSql, queryVM As QueryBase(Of T)) As (Data As List(Of T), Count As Integer, Pages As Integer)
'			If queryVM Is Nothing Then Return Nothing

'			' 默认查询对象
'			Dim query = this.Select(Of T)

'			' 更新查询条件，参数
'			queryVM.QueryExecute(query)

'			' 强制返回最大数量
'			Dim PageMax = queryVM.Max.Range(0, queryVM.MaxCount)

'			' 当前页
'			Dim PageNow = queryVM.Page.Range(1, queryVM.MaxPages)

'			' 每页记录数
'			Dim PageLimt = queryVM.Limit.Range(5, queryVM.MaxLimit)

'			' 总记录数
'			Dim Count = query.Count
'			If Count < 1 Then Return Nothing

'			' 分析结果
'			Dim Pages = 0

'			' 存在强制返回最大数量
'			If PageMax > 0 Then
'				' 如果 pageMax < Count 则需要限制大小
'				If PageMax < Count Then
'					' 返回的最大记录数
'					Count = PageMax

'					query.Take(PageMax)
'				End If
'			Else
'				' 使用分页
'				Pages = Math.Ceiling(Count / PageLimt)

'				' 最大页数
'				If Pages > queryVM.MaxPages Then Pages = queryVM.MaxPages

'				' 最大当前页数
'				If Pages < PageNow Then PageNow = Pages

'				query.Page(PageNow, PageLimt)
'			End If

'			' 执行操作
'			Return (query.ToList, Count, Pages)
'		End Function

'		''' <summary>查询列表数量，并将结果转换成字典类型</summary>
'		<Extension>
'		Public Function QueryDictionaries(Of T As {IEntity, Class})(this As IFreeSql, queryVM As QueryBase(Of T), Optional returnSimple As Boolean = True, Optional entityConvert As Action(Of List(Of T)) = Nothing, Optional updateDictionary As Boolean = False) As (Data As List(Of IDictionary(Of String, Object)), Count As Integer, Pages As Integer)
'			If queryVM Is Nothing Then Return Nothing

'			' 执行操作
'			Dim Result = this.QueryList(queryVM)
'			If Result.Data.IsEmpty Then Return (Nothing, Result.Count, Result.Pages)

'			' 获取字典值
'			If updateDictionary Then
'				Dim dics = SYS.GetService(Of IAppDictionaryProvider)
'				dics?.EntitiesUpdate(Result.Data)
'			End If

'			entityConvert?.Invoke(Result.Data)

'			' 存在 查询条件，处理结果数据
'			Dim Data As New List(Of IDictionary(Of String, Object))

'			For I = 0 To Result.Data.Count - 1
'				Data.Add(queryVM.ConvertDictionary(Result.Data(I), returnSimple, this))
'			Next

'			Return (Data, Result.Count, Result.Pages)
'		End Function

'		''' <summary>获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
'		''' <param name="nameField">标题字段</param>
'		''' <param name="disabledField">是否禁用字段</param>
'		''' <param name="extField">扩展字段</param>
'		''' <param name="queryVM">查询条件</param>
'		''' <param name="listUpdate">二次处理结果</param>
'		<Extension>
'		Public Function QueryDataList(Of T As {IEntity, Class})(this As IFreeSql,
'									 Optional nameField As String = Nothing,
'									 Optional disabledField As String = Nothing,
'									 Optional extField As String = Nothing,
'									 Optional queryVM As QueryBase(Of T) = Nothing,
'									 Optional listUpdate As Func(Of T, DataList, DataList) = Nothing
'									) As List(Of DataList)
'			' 分析名称键
'			Dim pros = GetType(T).
'				GetAllProperties.
'				Select(Function(x) x.Name).
'				ToList

'			' 上级
'			Dim parentField = If(pros.Contains("ParentId", StringComparer.OrdinalIgnoreCase), "ParentId", "")

'			' 名称键
'			nameField = {nameField, "title", "name", "id"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 禁用键
'			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 描述键
'			extField = If(pros.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

'			' 是否禁用反向，有可能字段是允许，所以要反向
'			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

'			' 主键类型
'			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType

'			' 默认查询对象
'			Dim query = this.Select(Of T)

'			' 最大返回记录数
'			Dim pageMax = 10000

'			' 更新查询条件，参数
'			If queryVM IsNot Nothing Then
'				pageMax = queryVM.Max.Range(10, queryVM.MaxCount)
'				queryVM.QueryExecute(query)
'			End If

'			' 查询结果
'			Return query.Take(pageMax).ToList?.Select(Function(item)
'														  ' 要查询的字段，为空则过滤
'														  Dim fields = {"ID", nameField, parentField, disabledField, extField}.Where(Function(x) x.NotEmpty).ToArray

'														  Dim dic = item.ToDictionary(False, fields)
'														  Dim data As New DataList(dic, nameField, "ID", "ParentId", disabledField, extField)

'														  ' disabled 需要反转
'														  If isRev Then data.Disabled = Not data.Disabled

'														  ' 二次处理值
'														  data = If(listUpdate?.Invoke(item, data), data)

'														  Return data
'													  End Function).ToList
'		End Function

'		''' <summary>获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
'		''' <param name="keywords">查询的值</param>
'		''' <param name="nameField">查询字段，多个字段用逗号间隔</param>
'		''' <param name="disabledField">是否禁用字段</param>
'		''' <param name="nameUpdate">名称更新函数</param>
'		<Extension>
'		Public Function QuerySearch(Of T As {IEntity, Class})(this As IFreeSql,
'									 keywords As String(),
'									 Optional nameField As String = Nothing,
'									 Optional disabledField As String = "",
'									 Optional valueField As String = "",
'									 Optional count As Integer = 25,
'									 Optional queryAction As Action(Of ISelect(Of T)) = Nothing,
'									 Optional nameUpdate As Func(Of T, String, String) = Nothing
'			) As List(Of DataList)
'			If keywords.IsEmpty Then Return Nothing

'			Dim ret As New List(Of DataList)

'			For Each keyword In keywords
'				Dim items = this.QuerySearch(keyword, nameField, disabledField, valueField, count, queryAction, nameUpdate)
'				If items IsNot Nothing Then ret.AddRange(items.FirstOrDefault)
'			Next

'			Return ret.Where(Function(x) x IsNot Nothing).Distinct(Function(x) x.Value).ToList
'		End Function

'		''' <summary>模糊查询，获取列表数据，如果未设置名称字段则自动分析 name / title ，禁用使用 disabled / enabled</summary>
'		''' <param name="keyword">查询的值</param>
'		''' <param name="nameField">查询字段，多个字段用逗号间隔</param>
'		''' <param name="disabledField">是否禁用字段</param>
'		''' <param name="nameUpdate">名称更新函数</param>
'		<Extension>
'		Public Function QuerySearch(Of T As {IEntity, Class})(this As IFreeSql,
'									 keyword As String,
'									 Optional nameField As String = Nothing,
'									 Optional disabledField As String = "",
'									 Optional valueField As String = "",
'									 Optional count As Integer = 25,
'									 Optional queryAction As Action(Of ISelect(Of T)) = Nothing,
'									 Optional nameUpdate As Func(Of T, String, String) = Nothing
'			) As List(Of DataList)
'			' 分析名称键
'			Dim pros = GetType(T).
'				GetAllProperties.
'				Select(Function(x) x.Name).
'				ToList

'			' 名称键
'			nameField &= ",title,name"
'			Dim nameFields = nameField.ToLowerInvariant.SplitDistinct.Where(Function(x) x <> "id" AndAlso pros.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

'			' 禁用键
'			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) pros.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

'			' 值键
'			valueField = valueField.EmptyValue("ID")
'			If Not pros.Contains(valueField, StringComparer.OrdinalIgnoreCase) Then valueField = "ID"

'			' 是否禁用反向，有可能字段是允许，所以要反向
'			Dim isRev = disabledField.NotEmpty AndAlso disabledField.IsSame("enabled")

'			' 返回数量
'			count = count.Range(1, 100)

'			' 条件
'			Dim filter As New DynamicFilterInfo With {
'				.Logic = DynamicFilterLogic.Or,
'				.Filters = New List(Of DynamicFilterInfo)
'			}

'			Dim IdType = GetType(T).GetSingleProperty("ID").PropertyType
'			Dim Id = keyword.ToValue(IdType)
'			If Id IsNot Nothing Then
'				Dim filterID As New DynamicFilterInfo With {.Field = "ID", .[Operator] = DynamicFilterOperator.Eq, .Value = Id}
'				filter.Filters.Add(filterID)
'			End If

'			For Each nameField In nameFields
'				Dim filterName As New DynamicFilterInfo With {.Field = nameField, .[Operator] = DynamicFilterOperator.Contains, .Value = keyword
'				}
'				filter.Filters.Add(filterName)
'			Next

'			' 默认查询对象
'			Dim query = this.Select(Of T)

'			' 其他查询
'			query.WhereDynamicFilter(filter)
'			queryAction?.Invoke(query)

'			' 数量限制
'			query.Take(count)

'			' 所有结果字段 
'			Dim retuenFields = nameFields.Union({disabledField, valueField}).Distinct.ToArray

'			' 是否 id
'			Dim isId = keyword.ToLong.ToString = keyword

'			' 查询结果
'			Return query.ToList.
'				Select(Function(item)
'						   Dim dic = item.ToDictionary(False, retuenFields)
'						   Dim data As New DataList(dic, Nothing, valueField, Nothing, disabledField)

'						   ' disabled 需要反转
'						   If isRev Then data.Disabled = Not data.Disabled

'						   ' 从所有名称字段中分析值
'						   Dim text = ""
'						   For Each nameField In nameFields
'							   text = dic.Where(Function(x) x.Key.Equals(nameField, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString
'							   If text.NotEmpty AndAlso (isId OrElse text.Contains(keyword, StringComparison.OrdinalIgnoreCase)) Then
'								   data.Text = text
'								   Exit For
'							   End If
'						   Next

'						   ' 如果获取不到值，则直接返回 标识
'						   If data.Text.IsEmpty Then data.Text = data.Value

'						   ' 如果存在自定义名称函数，更新名称
'						   If nameUpdate IsNot Nothing Then
'							   text = nameUpdate.Invoke(item, data.Text)
'							   If text.NotEmpty Then data.Text = text
'						   End If

'						   Return data
'					   End Function).
'				Distinct(Function(x) x.Text).ToList
'		End Function
'#End Region

'	End Module

'End Namespace