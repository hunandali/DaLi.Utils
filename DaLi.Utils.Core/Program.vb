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
' 	核心操作公共参数
'
' 	name: Program
' 	create: 2020-11-16
' 	memo: 核心操作公共参数
'
' ------------------------------------------------------------

Imports System.Net
Imports System.Text

''' <summary>核心操作公共参数</summary>
Public Module Main

#Region "系统常量"

	''' <summary>中文左引号</summary>
	Public ReadOnly vbLQ As Char = Convert.ToChar(8220)

	''' <summary>中文右引号</summary>
	Public ReadOnly vbRQ As Char = Convert.ToChar(8221)

#End Region

#Region "全局环境变量"

	''' <summary>代理服务器，用于 Net.HttpClient 或者  Net.WebClient</summary>
	Public Property SYS_PROXY As WebProxy

	''' <summary>系统启动时间</summary>
	Public ReadOnly SYS_START As DateTimeOffset = DATE_FULL_NOW

	''' <summary>系统初始路径</summary>
	Public ReadOnly SYS_ROOT As String = SystemHelper.RootFolder

#End Region

#Region "时间常量"

	''' <summary>1970-1-1 的时间戳数值</summary>
	Public Const TICKS_19700101 = 621355968000000000

	''' <summary>1970年1月1日</summary>
	Public ReadOnly DATE_1970 As New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

	''' <summary>2000年1月1日</summary>
	Public ReadOnly DATE_2000 As New Date(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)

	''' <summary>2038年1月1日，对于 Unix 时间来说，使用秒作为单位将存在 2038 年问题</summary>
	Public ReadOnly DATE_2038 As New Date(2038, 1, 1, 0, 0, 0, DateTimeKind.Utc)

	''' <summary>2050年1月1日</summary>
	Public ReadOnly DATE_2050 As New Date(2050, 1, 1, 0, 0, 0, DateTimeKind.Utc)

	''' <summary>国家法定休息日列表</summary>
	Public Property DATE_HOLIDAY As Date()

	''' <summary>国家法定调休日期，原本是周末休息日，但是因为放假需要调休的日期</summary>
	Public Property DATE_ADJUST As Date()

	''' <summary>当前系统时间与实际时间的时差（单位：秒）</summary>
	Public Property TIME_DELAY As Integer = 0

	''' <summary>当前时间</summary>
	Public ReadOnly Property DATE_NOW As Date
		Get
			Return If(TIME_DELAY = 0, Date.Now, Date.Now.AddSeconds(TIME_DELAY))
		End Get
	End Property

	''' <summary>当前时间</summary>
	Public ReadOnly Property DATE_NOW_STR(Optional format As String = "yyyy-MM-dd HH:mm:ss") As String
		Get
			Return DATE_NOW.ToString(format)
		End Get
	End Property

	''' <summary>当前时间</summary>
	Public ReadOnly Property DATE_FULL_NOW As DateTimeOffset
		Get
			Return If(TIME_DELAY = 0, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(TIME_DELAY))
		End Get
	End Property

#End Region

#Region "注册字符编码，否则无法使用 Text.Encoding"

	''' <summary>编码是否注册</summary>
	Private _EncodingRegister As Boolean = False

	''' <summary>注册编码，防止中文 GB2312 无法使用</summary>
	Public Sub EncodingRegister()
		If _EncodingRegister Then Return
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
		_EncodingRegister = True
	End Sub

	''' <summary>GB2312(GBK)</summary>
	Private _GB2312 As Encoding = Nothing

	''' <summary>GB2312(GBK)</summary>
	Public ReadOnly Property GB2312 As Encoding
		Get
			If _GB2312 Is Nothing Then
				EncodingRegister()
				_GB2312 = Encoding.GetEncoding(936)
			End If

			Return _GB2312
		End Get
	End Property

	''' <summary>UTF8</summary>
	Public ReadOnly Property UTF8 As Encoding
		Get
			Return Encoding.UTF8
		End Get
	End Property

#End Region

End Module
