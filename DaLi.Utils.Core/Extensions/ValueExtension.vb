'' ------------------------------------------------------------
''
'' 	Copyright © 2021 湖南大沥网络科技有限公司.
'' 	Dali.Utils Is licensed under Mulan PSL v2.
''
'' 	  author:	木炭(WOODCOAL)
'' 	   email:	a@hndl.vip
'' 	homepage:	http://www.hunandali.com/
''
'' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
''
'' ------------------------------------------------------------
''
'' 	值扩展操作
''
'' 	name: ValueExtension
'' 	create: 2025-01-06
'' 	memo: 值扩展操作
''
'' ------------------------------------------------------------

'Imports System.IO
'Imports System.Net
'Imports System.Reflection
'Imports System.Runtime.CompilerServices
'Imports System.Text.RegularExpressions

'Namespace Extension
'	Public Module ValueExtension

'		''' <summary>当前数据是否无内容</summary>
'		''' <param name="this">当前类型</param>
'		''' <remarks>
'		''' 文本：空值或者空字符串
'		''' 数字：0
'		''' 布尔：False
'		''' 对象、字典：无任何键
'		''' 数组、集合：长度为 0
'		''' GUID：空值
'		''' 时间：初始时间
'		''' 其他：空值
'		''' </remarks>
'		<Extension>
'		Public Function IsEmptyValue(this As Object) As Boolean
'			If this Is Nothing Then Return True

'			' 常用数据判断
'			If String.Empty.Equals(this) OrElse
'				0.Equals(this) OrElse
'				False.Equals(this) OrElse
'				Date.MinValue.Equals(this) OrElse
'				TimeSpan.Zero.Equals(this) OrElse
'				DateTimeOffset.MinValue.Equals(this) OrElse
'				Guid.Empty.Equals(this) Then Return True

'			' 处理值类型
'			Dim type = this.GetType
'			If type.IsString Then Return this.IsEmpty
'			If type.IsValueType Then
'				If type.IsEnum Then Return Convert.ToInt32(this) = 0
'				If Nullable.GetUnderlyingType(type) IsNot Nothing Then Return Not this.HasValue
'				Return this.Equals(Activator.CreateInstance(type))
'			End If

'			' 集合类型。是否 ICollection，包含 List, Dictionary, Array 等
'			If type Is GetType(ICollection) Then Return DirectCast(this, ICollection).Count = 0
'			If type Is GetType(IDictionary) Then Return DirectCast(this, IDictionary).Count = 0
'			If type Is GetType(IEnumerable) Then Return Not DirectCast(this, IEnumerable).GetEnumerator.MoveNext
'			If type.IsArray Then Return DirectCast(this, Array).Length = 0

'			' 元组
'			Dim typeName = type.Name
'			If typeName.StartsWith("Tuple") OrElse
'				typeName.StartsWith("ValueTuple") Then Return type.GetFields().All(Function(f) IsEmptyValue(f.GetValue(this)))

'			' 特殊类型
'			If type Is GetType(MemoryStream) Then Return DirectCast(this, MemoryStream).Length = 0
'			If type Is GetType(Regex) Then Return DirectCast(this, Regex).ToString = "(?:)"
'			If type Is GetType(Uri) Then Return DirectCast(this, Uri).ToString = String.Empty
'			If type Is GetType(Version) Then Return New Version(0, 0).Equals(this)
'			If type Is GetType(IPAddress) Then Return IPAddress.Any.Equals(this)

'			' 对于其他类型，可以考虑检查所有公共属性
'			Try
'				Return type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).All(Function(p) IsEmptyValue(p.GetValue(this)))
'			Catch ex As Exception
'				Return False
'			End Try
'		End Function

'		''' <summary>当前数据是否无内容</summary>
'		''' <param name="this">当前类型</param>
'		''' <param name="validate">二次验证函数，返回 True 则认为无内容</param>
'		''' <remarks>
'		''' 文本：空值或者空字符串
'		''' 数字：0
'		''' 布尔：False
'		''' 对象、字典：无任何键
'		''' 数组、集合：长度为 0
'		''' GUID：空值
'		''' 时间：初始时间
'		''' 其他：空值
'		''' </remarks>
'		<Extension>
'		Public Function IsEmptyValue(Of T)(this As T, Optional validate As Func(Of T, Boolean) = Nothing) As Boolean
'			If validate IsNot Nothing Then
'				Return validate.Invoke(this)
'			Else
'				Dim data As Object = this
'				Return IsEmptyValue(data)
'			End If
'		End Function

'		''' <summary>获取类型的默认值</summary>
'		''' <param name="defaultValue">产生不了默认值时，默认替代的值。如果为空，则返回 defaultValue。注意的时对于值类型或者字符串，默存在默认值，所以此参数无效</param>
'		<Extension>
'		Public Function GetDefaultValue(this As Type, Optional defaultValue As Object = Nothing) As Object
'			If this Is Nothing Then Return defaultValue

'			' 处理值类型
'			If this.IsValueType Then
'				' 处理可空类型
'				Dim underlyingType As Type = Nullable.GetUnderlyingType(this)
'				If underlyingType IsNot Nothing Then Return defaultValue

'				' 处理枚举类型
'				If this.IsEnum Then Return [Enum].ToObject(this, 0)

'				' 处理其他值类型
'				Return Activator.CreateInstance(this)
'			End If

'			' 处理字符串类型
'			If this Is GetType(String) Then Return String.Empty

'			' 处理数组类型
'			If this.IsArray Then Return Array.CreateInstance(this.GetElementType(), 0)

'			' 处理其他引用类型
'			Dim ret = Nothing

'			' 尝试创建一个没有参数的实例
'			Try
'				ret = Activator.CreateInstance(this)
'			Catch ex As Exception
'			End Try

'			Return If(ret, defaultValue)
'		End Function

'		''' <summary>获取类型的默认值</summary>
'		''' <param name="defaultValue">产生不了默认值时，默认替代的值。如果为空，则返回 defaultValue。注意的时对于值类型或者字符串，默存在默认值，所以此参数无效</param>
'		Public Function GetDefaultValue(Of T)(Optional defaultValue As T = Nothing) As T
'			Return GetDefaultValue(GetType(T), defaultValue)
'		End Function

'		''' <summary>获取常用系统数据类型的默认值</summary>
'		''' <param name="this">类型</param>
'		<Extension>
'		Public Function GetDefaultValue(this As TypeCode) As Object
'			Select Case this
'				Case TypeCode.Boolean
'					Return False

'				Case TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
'					Return 0

'				Case TypeCode.DateTime
'					Return Date.MinValue

'				Case TypeCode.String
'					Return String.Empty
'			End Select

'			Return Nothing
'		End Function

'		''' <summary>获取常用系统数据类型的默认值</summary>
'		<Extension>
'		Public Function GetDefaultValue(this As FieldTypeEnum) As Object
'			Select Case this
'				Case FieldTypeEnum.BOOLEAN
'					Return False

'				Case FieldTypeEnum.TRISTATE
'					Return TristateEnum.DEFAULT

'				Case FieldTypeEnum.BYTES, FieldTypeEnum.DOUBLE, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.NUMBER, FieldTypeEnum.SINGLE
'					Return 0

'				Case FieldTypeEnum.DATETIME
'					Return Date.MinValue

'				Case FieldTypeEnum.TIME
'					Return "00:00:00"   ' Date.MinValue.ToString("HH:mm:ss")

'				Case FieldTypeEnum.DATE
'					Return "0000-01-01" 'Date.MinValue.ToString("yyyy-MM-dd")

'				Case FieldTypeEnum.GUID
'					Return Guid.Empty

'				Case FieldTypeEnum.DICTIONARY
'					Return New KeyValueDictionary

'				Case Else
'					Return ""
'			End Select
'		End Function

'	End Module
'End Namespace