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
' 	字符扩展操作
'
' 	name: Extension.StringExtension
' 	create: 2020-10-23
' 	memo: 字符扩展操作
' 	
' ------------------------------------------------------------

Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Xml

Namespace Extension

	''' <summary>字符扩展操作</summary>
	Public Module StringExtension

#Region "1. 常用字符操"

		''' <summary>文本长度阈值，超过此长度使用正则来操作文本</summary>
		Public Property LONG_THRESHOLD As Integer = 512

		''' <summary>获取字符串的实际长度，Unicode 字符长度为 2，Ascii 为 1</summary>
		<Extension>
		Public Function UnicodeLength(this As String) As Integer
			If this.IsNull Then Return 0

			Dim length = 0
			For Each c In this
				If Char.IsAscii(c) Then
					length += 1
				Else
					length += 2
				End If
			Next
			Return length
		End Function

		''' <summary>计算目的字符串在源字符串中出现的次数</summary>
		''' <param name="source">源字符串</param>
		''' <param name="target">目的字符串</param>
		''' <param name="ignoreCase">是否忽略大小写</param>
		''' <returns>目的字符串在源字符串中出现的次数</returns>
		<Extension()>
		Public Function Times(source As String, target As String, Optional ignoreCase As Boolean = True) As Integer
			If source.IsNull OrElse target.IsNull Then Return 0

			If source.Length > LONG_THRESHOLD Then
				Return Regex.Matches(source, Regex.Escape(target), If(ignoreCase, RegexOptions.IgnoreCase, RegexOptions.None)).Count
			End If

			Dim comparisonType As StringComparison = If(ignoreCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal)

			Dim count As Integer = 0
			Dim index As Integer = source.IndexOf(target, comparisonType)

			While index <> -1
				count += 1
				index = source.IndexOf(target, index + target.Length, comparisonType)
			End While

			Return count
		End Function

		''' <summary>生成重复次数相同的字符串</summary>
		''' <param name="times">重复次数</param>
		''' <returns>重复后的字符串</returns>
		<Extension()>
		Public Function Duplicate(this As String, times As Integer) As String
			If this.IsNull OrElse times < 1 Then Return ""
			If times = 1 Then Return this

			' 优化：一个字符使用 Strings.StrDup 替代循环
			If this.Length = 1 Then Return StrDup(times, this(0))

			' 多个使用 StringBuilder
			Dim sb As New StringBuilder(this.Length * times)
			For i As Integer = 0 To times - 1
				sb.Append(this)
			Next

			Return sb.ToString()
		End Function

		''' <summary>提供多个字符串，如果原始数据为空则返回后者数据，如果都不满足则返回空字符串</summary>
		''' <param name="this">原始字符串</param>
		''' <param name="moreValues">备用字符串数组</param>
		<Extension()>
		Public Function NullValue(this As String, ParamArray moreValues As String()) As String
			If this.NotNull Then Return this
			If moreValues.IsEmpty Then Return ""

			' 优化：使用 LINQ 查找第一个非空值
			Dim value = moreValues.FirstOrDefault(Function(v) v.NotNull)
			Return If(value, "")
		End Function

		''' <summary>提供多个字符串，如果原始数据为空或者空白字符串则返回后者数据，如果都不满足则返回空字符串</summary>
		''' <param name="this">原始字符串</param>
		''' <param name="moreValues">备用字符串数组</param>
		<Extension()>
		Public Function EmptyValue(this As String, ParamArray moreValues As String()) As String
			If this.NotEmpty Then Return this
			If moreValues.IsEmpty Then Return ""

			' 优化：使用 LINQ 查找第一个非空值
			Dim value = moreValues.FirstOrDefault(Function(v) v.NotEmpty)
			Return If(value, "")
		End Function

		''' <summary>将字符串反转</summary>
		''' <param name="this">要反转的字符串</param>
		''' <returns>反转后的字符串</returns>
		<Extension()>
		Public Function Reverse(this As String) As String
			If this.IsEmpty OrElse this.Length < 2 Then Return this
			Return StrReverse(this)
		End Function

		''' <summary>计算 MD5 值</summary>
		''' <param name="mode">返回模式，16位(False)或者32位(True,默认)</param>
		<Extension>
		Public Function MD5(this As String, Optional mode As Boolean = True, Optional encoding As Text.Encoding = Nothing) As String
			Return HashHelper.MD5(this, mode, encoding)
		End Function

		''' <summary>计算 SHA1 值</summary>
		<Extension>
		Public Function SHA1(this As String, Optional encoding As Text.Encoding = Nothing) As String
			Return HashHelper.SHA1(this, encoding)
		End Function

		''' <summary>长字符串缩短展示，显示头尾，中间省略</summary>
		''' <param name="this">输入字符串</param>
		''' <param name="maxLength">最大长度，包含省略字符，如果此长度小于等于省略字符长度，则返回原字符串</param>
		''' <param name="hideChats">省略字符</param>
		''' <returns>缩短后的字符串</returns>
		<Extension()>
		Public Function ShortShow(this As String, Optional maxLength As Integer = 100, Optional hideChats As String = "……") As String
			If this.IsNull OrElse maxLength <= hideChats.Length OrElse this.Length <= maxLength Then Return this

			Dim headLength As Integer = (maxLength - hideChats.Length) \ 2
			Dim tailLength As Integer = maxLength - hideChats.Length - headLength

			Return String.Concat(this.AsSpan(0, headLength), hideChats, this.AsSpan(this.Length - tailLength, tailLength))
		End Function

		''' <summary>使用自定义字符隐藏文本中间部分</summary>
		''' <param name="this">要隐藏的文本</param>
		''' <param name="headLength">头部保留长度</param>
		''' <param name="tailLength">尾部保留长度</param>
		''' <param name="maskCharacter">用于隐藏的字符</param>
		''' <returns>隐藏后的文本</returns>
		<Extension()>
		Public Function Mask(this As String, headLength As Integer, tailLength As Integer, Optional maskCharacter As Char = "*"c) As String
			If this.IsNull Then Return this

			' 文本长度不足以隐藏
			Dim textLength = this.Length
			If textLength <= headLength + tailLength Then Return this

			Dim masks As New String(maskCharacter, textLength - headLength - tailLength)

			Return String.Concat(this.AsSpan(0, headLength), masks, this.AsSpan(textLength - tailLength, tailLength))
		End Function

		''' <summary>使用自定义字符隐藏文本中间部分</summary>
		''' <param name="this">要隐藏的文本</param>
		''' <param name="maskCharacter">用于隐藏的字符</param>
		''' <returns>隐藏后的文本</returns>
		<Extension()>
		Public Function Mask(this As String, Optional maskCharacter As Char = "*"c) As String
			If this.IsNull Then Return this

			Select Case this.Length
				Case >= 11
					Return this.Mask(3, 4, maskCharacter)
				Case 10
					Return this.Mask(3, 3, maskCharacter)
				Case 9
					Return this.Mask(2, 3, maskCharacter)
				Case 8
					Return this.Mask(2, 2, maskCharacter)
				Case 7, 6
					Return this.Mask(1, 2, maskCharacter)
				Case Else
					Return this.Mask(1, 0, maskCharacter)
			End Select
		End Function

		'''' <summary>验输入字符串是否长于指定长度</summary> 
		'<Extension>
		'Public Function LengthLargen(this As String, len As Integer) As Boolean
		'	Return this?.Length > len
		'End Function

		'''' <summary>验输入字符串是否短于指定长度</summary> 
		'<Extension>
		'Public Function LengthLess(this As String, len As Integer) As Boolean
		'	Return this.IsEmpty OrElse this.Length < len
		'End Function

#End Region

#Region "2. 清除多余数据"

		''' <summary>过滤 ASCII 码中的控制字符</summary>
		''' <param name="this">要处理的字符串</param>
		''' <param name="replace">对于出现的控制符使用何种字符代替，不设置则清楚控制符</param>
		''' <returns>过滤后的字符串</returns>
		<Extension()>
		Public Function ClearControl(this As String, Optional replace As String = Nothing) As String
			If this.IsNull Then Return ""
			If replace.IsNull Then replace = ""

			If this.Length > LONG_THRESHOLD Then Return Regex.Replace(this, "[\x00-\x1F\x7F-\x9F]", replace)

			Dim sb As New StringBuilder()
			For Each c As Char In this
				If Char.IsControl(c) Then
					sb.Append(replace)
				Else
					sb.Append(c)
				End If
			Next
			Return sb.ToString()
		End Function

		''' <summary>过滤ASCII码中的控制字符</summary>
		''' <param name="isClear">是否清理掉控制符，否则用空格代替</param>
		<Extension>
		<Obsolete("请使用 ClearControl(this As String, Optional replace As String = Nothing) 代替")>
		Public Function ClearControl(this As String, isClear As Boolean) As String
			Return this.ClearControl(If(isClear, "", " "))
		End Function

		''' <summary>删除两个以上多余的空字符，仅保留一个或者不保留。（包括空格\f\n\r\t\v）</summary>
		''' <param name="this">要处理的文本</param>
		''' <param name="keepSpace">是否保留一个空格，否则将删除所有空字符，默认保留一个</param>
		''' <remarks>对于较短的文本模式文本处理模式，注意与正则替换模式有稍微区别，文本模式会包含更多空白字符</remarks>
		<Extension()>
		Public Function ClearSpace(this As String, Optional keepSpace As Boolean = True) As String
			If this.IsNull Then Return ""

			If this.Length > LONG_THRESHOLD Then
				If keepSpace Then
					Return Regex.Replace(this, "(\s){2,}", "$1").Trim()
				Else
					Return Regex.Replace(this, "(\s){2,}", "")
				End If
			Else
				Dim sb As New StringBuilder()
				Dim lastChar As Char = ControlChars.NullChar
				For Each c As Char In this
					' 空字符串
					' 不保留：不输出
					' 保留：比较最后一个字符，一致则不输出
					If Char.IsWhiteSpace(c) AndAlso (Not keepSpace OrElse c = lastChar) Then
						Continue For
					End If

					' 不保留空格的无需记录上一字符串数据
					If Not keepSpace Then
						lastChar = c
					End If

					sb.Append(c)
				Next
				Return sb.ToString().Trim()
			End If
		End Function

		''' <summary>清除整个字符串中多余的空格和Ascii控制字符</summary>
		<Extension>
		Public Function TrimFull(this As String) As String
			If this.IsEmpty Then Return ""
			Return this.ClearControl.ClearSpace
		End Function

		''' <summary>过滤文本中的控制字符，防止反序列化错误,仅保留回车换行。一般用 XML 文档</summary>
		''' <param name="this">要处理的文本</param>
		''' <param name="keepEnter">是否保留回车换行</param>
		<Extension()>
		Public Function ClearLowAscii(this As String, Optional keepEnter As Boolean = True) As String
			If this.IsNull Then Return ""

			If this.Length > LONG_THRESHOLD Then
				If keepEnter Then
					Return Regex.Replace(this, "[\x00-\x09\0B\x0C\x0E\x1F\x7F-\x9F]", "").Trim()
				Else
					Return Regex.Replace(this, "[\x00-\x1F\x7F-\x9F]", "").Trim()
				End If
			End If

			Dim sb As New StringBuilder()
			For Each c As Char In this
				If Not Char.IsControl(c) OrElse (keepEnter AndAlso (c = ControlChars.Cr OrElse c = ControlChars.Lf)) Then
					sb.Append(c)
				End If
			Next
			Return sb.ToString()
		End Function

		''' <summary>清理 HTML 字符串，移除指定标签、注释、脚本、样式等。</summary>
		''' <param name="tags">要移除的 HTML 标签数组，或以逗号分隔的标签字符串。</param>
		''' <returns>清理后的字符串。</returns>
		<Extension()>
		Public Function ClearHtml(this As String, tags As IEnumerable(Of String)) As String
			If this.IsNull Then Return ""
			If tags.IsEmpty Then Return this

			' 处理逗号分隔的标签字符串
			Dim tagList As IEnumerable(Of String)
			If tags.Any AndAlso tags(0).Contains(","c) Then
				tagList = tags(0).Split(","c, StringSplitOptions.RemoveEmptyEntries)
			Else
				tagList = tags
			End If

			' 规范化标签名称（小写并去空格）
			tagList = tagList.Select(Function(x) x.Trim().ToLowerInvariant()).Distinct()

			' 无有效标签时返回原字符串
			If Not tagList.Any Then Return this

			' 包含 all 时只保留 all
			If tagList.Contains("all", StringComparer.OrdinalIgnoreCase) Then tagList = {"all"}

			' 循环清理
			For Each tag As String In tagList
				Select Case tag
					Case "namespace"
						this = Regex.Replace(this, "<\/?[a-zA-Z0-9]+:[^>]*>", "", RegexOptions.IgnoreCase)

					Case "xml", "doctype"
						this = Regex.Replace(this, "<\/?xml[^>]*>|<\!DOCTYPE[^>]*>", "", RegexOptions.IgnoreCase)

					Case "attr", "attribute"
						this = Regex.Replace(this, "(?<=<[^/][^>]*?)(?:\s+[^\s=>]+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[^>\s]*))?)+", "", RegexOptions.IgnoreCase)

					Case "style"
						this = Regex.Replace(this, "<style((.|\n)+?)</style>|(style|id|class)=[""'][^""']*[""']", "", RegexOptions.IgnoreCase)

					Case "script"
						this = Regex.Replace(this, "<script((.|\n)+?)</script>|\bon\w+=[""'][^""']*[""']", "", RegexOptions.IgnoreCase)

					Case "注释", "comment"
						this = Regex.Replace(this, "<!--(.|\n)+?-->", "")

					Case "all"
						' 先移除其他特定标签
						this = this.ClearHtml("attr", "namespace", "xml", "script", "style", "comment")

						' 替换 HTML 实体
						this = this.Replace("&nbsp;", " ")
						this = this.Replace("&quot;", """")
						this = this.Replace("&amp;", "&")
						this = this.Replace("&lt;", "<")
						this = this.Replace("&gt;", ">")

						' HTML 解码 & 移除所有标签
						this = WebUtility.HtmlDecode(this)
						this = Regex.Replace(this, "<[^>]*?>", "")

					Case "enter"
						this = this.Replace(vbCr, "").Replace(vbLf, "")

					Case "tab"
						this = this.Replace(vbTab, "")

					Case "space"
						this = this.ClearSpace() ' 包括全角空格

					Case "trim"
						this = this.TrimFull() ' 使用内置的 Trim 方法

					Case Else
						' 移除标签块之间的内容
						If (tag.StartsWith("["c) AndAlso tag.EndsWith("]"c)) OrElse
						   (tag.StartsWith("("c) AndAlso tag.EndsWith(")"c)) OrElse
						   (tag.StartsWith("{"c) AndAlso tag.EndsWith("}"c)) OrElse
						   (tag.StartsWith("<"c) AndAlso tag.EndsWith(">"c)) Then
							Dim tagName = tag.Substring(1, tag.Length - 2)
							If tagName.NotEmpty Then this = Regex.Replace(this, $"(<{tagName}([^>])*>(.|\n)+?<\/{tagName}([^>])*>)", "", RegexOptions.IgnoreCase)
						Else
							this = Regex.Replace(this, $"(<{tag}([^>])*>|<\/{tag}([^>])*>)", "")
						End If
				End Select

				' 如果字符串已空，则退出循环
				If String.IsNullOrEmpty(this) Then Exit For
			Next

			Return this
		End Function

		''' <summary>过滤Html标签</summary>
		''' <param name="this">要操作的字符串</param>
		''' <param name="tags">要过滤的HTML标签数组，或者用逗号间隔的字符串</param>
		<Extension>
		Public Function ClearHtml(this As String, ParamArray tags() As String) As String
			If this.IsNull Then Return ""
			If tags.IsEmpty Then Return this

			Return this.ClearHtml(tags.ToList)
		End Function

#End Region

#Region "3. 替换字符串"

		''' <summary>正则表达式替换</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="oldValue"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换"）</param>
		''' <param name="newValue">替换的内容</param>
		<Extension>
		Public Function ReplaceRegex(this As String, oldValue As String, Optional newValue As String = "") As String
			If this.IsNull OrElse oldValue.IsNull Then Return this
			If newValue.IsNull Then newValue = ""

			' 更新并判断正则条件
			Dim Pattern = PatternUpdate(oldValue)
			If Pattern.IsPattern Then
				Return Regex.Replace(this, Pattern.Pattern, newValue, RegexOptions.IgnoreCase)
			Else
				Return this.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase)
			End If

			Return this
		End Function

		''' <summary>替换一组数据</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="oldValues"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="newValue">替换的内容</param>
		<Extension>
		Public Function ReplaceMutli(this As String, oldValues As String(), Optional newValue As String = "") As String
			If this.IsNull OrElse oldValues.IsEmpty Then Return this

			For Each oldValue In oldValues
				this = this.ReplaceRegex(oldValue, newValue)
				If this.IsNull Then Exit For
			Next

			Return this
		End Function

		''' <summary>内链替换</summary>
		''' <param name="this">源内容</param>
		''' <param name="formatLink">格式，Markdown文档则使用默认格式：关键词用[key]表示；链接用[link]表示；为空时，自动分析，如果Link为网址则生成连接，否则直接输出Link</param>
		''' <param name="count">替换数量</param>
		''' <param name="isMarkdown">是否Markdown文档</param>
		<Extension>
		Public Function ReplaceLink(this As String, links As NameValueDictionary, Optional formatLink As String = "", Optional count As Integer = 10, Optional isMarkdown As Boolean = False) As String
			' 数据无效或者链接不包含{link}，直接返回
			If links.IsEmpty OrElse this.IsEmpty Then Return this
			If formatLink.NotEmpty AndAlso Not formatLink.Contains("{link}", StringComparison.OrdinalIgnoreCase) Then Return this

			' 默认链接格式
			Dim LinkFormat As String
			If formatLink.IsEmpty Then
				LinkFormat = If(isMarkdown, "[{key}]({link})", "<a href=""{link}"" target=""_blank"">{key}</a>")
			Else
				LinkFormat = formatLink
			End If

			' 默认替换数量
			If count < 1 Then count = Integer.MaxValue

			' 返回结果
			Dim Ret = this

			'所有标签内与链接中不能添加链接
			'<[^>]*>
			'<a [^>]*>(.*?)</a>
			Dim Tags As New StringDictionary(Of String)
			Dim Key = ":" & RandomHelper.Mix(6) & ":"
			Dim Idx = 0

			If Not isMarkdown Then
				Dim TagExp As String = "<a [*]</a>"
				Dim TagList As String() = this.Cut(TagExp, 0, True)
				If TagList.NotEmpty Then
					For Each Tag As String In TagList
						Idx += 1
						Dim Hash As String = Key & Idx
						If Not Tags.ContainsKey(Hash) Then
							Tags.Add(Hash, Tag)
							Ret = Ret.Replace(Tag, Hash)
						End If
					Next
				End If

				TagExp = "<[^>]*>"
				TagList = this.Cut(TagExp, 0, True)
				If TagList.NotEmpty Then
					For Each Tag As String In TagList
						Idx += 1
						Dim Hash As String = Key & Idx
						If Not Tags.ContainsKey(Hash) Then
							Tags.Add(Hash, Tag)
							Ret = Ret.Replace(Tag, Hash)
						End If
					Next
				End If
			End If

			'过滤掉所有链接
			For Each name As String In links.Keys
				Dim link = links(name)
				If formatLink.NotEmpty OrElse link.IsUrl Then link = LinkFormat.Replace("{key}", name).Replace("{link}", link)

				Idx += 1
				Dim Hash As String = Key & Idx
				Tags.Add(Hash, link)

				' 次数不限，直接替换
				If count = Integer.MaxValue Then
					Ret = Ret.Replace(name, Hash)
				Else
					' 次数有限，只替换其中一个
					' 总次数不能超过

					Dim b = Ret.IndexOf(name, StringComparison.OrdinalIgnoreCase)
					If b > -1 Then
						Dim e = b + name.Length
						If e <= Ret.Length Then
							Ret = String.Concat(Ret.AsSpan(0, b), Hash, Ret.AsSpan(e))

							count -= 1
							If count < 1 Then Exit For
						End If
					End If
				End If
			Next

			' 还原链接
			For Each Hash In Tags.Keys
				Ret = Ret.Replace(Hash, Tags(Hash))
			Next

			Return Ret
		End Function

		'''' <summary>使用简单标签替换，即{}包含的文本</summary>
		'''' <param name="this">要获取的源字符串</param>
		'''' <param name="key">标签名称，如果包含小数点，则替换对应标签的类型</param>
		'''' <param name="value">替换值</param>
		'<Extension>
		'Public Function FormatTemplate(this As String, key As String, value As String) As String
		'	Return SimpleTemplate.Format(this, key, value)
		'End Function

		'''' <summary>使用简单标签替换，即{}包含的文本</summary>
		'''' <param name="this">要获取的源字符串</param>
		'''' <param name="values">替换标签名值字典数据</param>
		'''' <param name="clearTag">是否清除所有未适配的标签，为防止错误清除，可以将 {{ 代替 {，}} 代替 }</param>
		'<Extension>
		'Public Function FormatTemplate(this As String, values As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As String
		'	If this.IsEmpty Then Return this
		'	If Not Regex.IsMatch(this, "\{[^\}]*\}") Then Return this

		'	' 如果需要替换全部标签，则先将转义的{}替换成其他标签，防止被错误替换
		'	Dim tick = Date.Now.Subtract(New Date(2020, 1, 1)).TotalHours
		'	Dim s1 = $"$[[{tick}"
		'	Dim s2 = $"{tick}]]$"

		'	this = this.Replace("{{", s1).Replace("}}", s2)
		'	this = SimpleTemplate.Format(this, values)

		'	If this.NotEmpty Then
		'		If clearTag Then this = Regex.Replace(this, "\{[^\}]*\}", "")
		'		this = this.Replace(s1, "{").Replace(s2, "}")
		'	End If

		'	Return this
		'End Function

		'''' <summary>使用自定义前后缀标签替换</summary>
		'''' <param name="this">要获取的源字符串</param>
		'''' <param name="values">替换标签名值字典数据</param>
		'''' <param name="prefix">前缀</param>
		'''' <param name="suffix">后缀</param>
		'''' <param name="skipAttribute">忽略属性操作</param>
		'<Extension>
		'Public Function FormatTemplateEx(this As String, values As IDictionary(Of String, Object), Optional prefix As String = "${", Optional suffix As String = "}", Optional skipAttribute As Boolean = False) As String
		'	If this.IsEmpty OrElse prefix.IsEmpty OrElse suffix.IsEmpty Then Return this

		'	Dim pre = Regex.Escape(prefix)
		'	Dim suf = Regex.Escape(suffix)

		'	Dim matches = Regex.Matches(this, $"{pre}((.|\n)*?){suf}")
		'	If matches.IsEmpty Then Return this

		'	' 处理重复数据
		'	Dim ms = matches.Select(Function(x) x.Groups(1).Value).Distinct.ToList
		'	If ms.IsEmpty Then Return this

		'	' 分类标签与属性
		'	' 1. 分析名称与属性，属性按标题排序
		'	' 2. 获取标签 Hash
		'	' 3. Hash 分组，将同组的标签合并标记
		'	Dim tags = ms.Select(Function(x)
		'							 Dim hash = ""
		'							 Dim s = x.TrimFull & " "
		'							 Dim name = s.Split(" ")(0)

		'							 Dim attrs As List(Of (Key As String, Value As String)) = Nothing
		'							 If skipAttribute Then
		'								 hash = name.ToLowerInvariant
		'							 Else
		'								 attrs = TemplateHelper.
		'										GetAttributes(s.Substring(name.Length))?.
		'										OrderBy(Function(a) a.Key).
		'										ToList

		'								 hash = attrs?.Select(Function(a) $"{a.Key}={a.Value}").JoinString(vbCrLf)
		'								 hash = $"{name}|{hash}".MD5
		'							 End If

		'							 Return New With {.source = x, name, attrs, hash}
		'						 End Function).
		'						 GroupBy(Function(x) x.hash).
		'						 ToList.
		'						 Select(Function(x)
		'									Dim sources = x.Select(Function(t) $"{prefix}{t.source}{suffix}").ToList
		'									Dim item = x(0)
		'									Return New With {item.name, item.attrs, sources}
		'								End Function).
		'						ToList

		'	' 复制并替换
		'	Dim data = values.ToJson.ToJsonNameValues
		'	tags.ForEach(Sub(tag)
		'					 ' 获取值
		'					 Dim value = data(tag.name)
		'					 If value IsNot Nothing AndAlso Not skipAttribute Then value = TemplateAction.Default.Execute(value, tag.attrs)

		'					 ' 替换
		'					 tag.sources.ForEach(Sub(source) this = this.Replace(source, value, StringComparison.OrdinalIgnoreCase))
		'				 End Sub)

		'	Return this
		'End Function

		'''' <summary>使用简单标签替换，即{}包含的文本</summary>
		'''' <param name="this">要获取的源字符串</param>
		'''' <param name="value">替换的对象</param>
		'<Extension>
		'Public Function Format(Of T As Class)(this As String, value As T, <CallerArgumentExpression("value")> Optional name As String = Nothing) As String
		'	If this.IsEmpty OrElse value Is Nothing OrElse name.IsEmpty Then Return this

		'	Return SimpleTemplate.Format(this, name, value.ToDictionary(False))
		'End Function

#End Region

#Region "4. 是否包含"

		''' <summary>数据是否存在指定内容，仅作简单比较，复杂比较请使用 Include 函数</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="values">
		''' 要检查的字符串数组，不区分大小写；
		''' 星号表示变化部分，不存在则需要整个字符完全匹配；
		''' * 表示 表示 不为空的数据；
		''' *xxxx 表示 以 xxxx 结尾数据；
		''' xxxx* 表示 以 xxxx 开头的数据；
		''' xxxx*yyyy 表示 以 xxxx 开头，yyyy 结尾的数据；
		''' *xxxx* 表示 数据中存在 xxxx；
		''' *xxxx*yyyy* 表示 数据中存在 xxxx开头，yyyy结尾的数据；
		''' 使用圆括号起始则使用正则表达式
		''' </param>
		<Extension>
		Public Function [Like](this As String, ParamArray values As String()) As Boolean
			If this.NotEmpty AndAlso values.NotEmpty Then
				Dim Flag = False

				For Each value In values.Where(Function(x) x.NotEmpty).Select(Function(x) x.ToLowerInvariant).Distinct.ToList
					If value = "*" Then
						Flag = True

					ElseIf value.StartsWith("*"c) AndAlso value.EndsWith("*"c) Then
						value = value.Substring(1, value.Length - 2)
						Dim pattern = value.Trim("*"c)
						If pattern.NotEmpty AndAlso pattern.Contains("*"c) Then
							' 包含区域内容其实
							pattern = Regex.Escape(pattern).Replace("\*", "((.|\n)*?)")
							Flag = Regex.IsMatch(this, pattern, RegexOptions.IgnoreCase)

						Else
							Flag = this.Contains(value, StringComparison.OrdinalIgnoreCase)
						End If

					ElseIf value.StartsWith("*"c) AndAlso Not value.EndsWith("*"c) Then
						Flag = this.EndsWith(value.Substring(1), StringComparison.OrdinalIgnoreCase)

					ElseIf Not value.StartsWith("*"c) AndAlso value.EndsWith("*"c) Then
						Flag = this.StartsWith(value.Substring(0, value.Length - 1), StringComparison.OrdinalIgnoreCase)

					ElseIf value.Contains("*"c) Then
						Dim Vs = value.Split("*"c)
						If Vs.Length = 2 Then Flag = this.StartsWith(Vs(0), StringComparison.OrdinalIgnoreCase) AndAlso this.EndsWith(Vs(1), StringComparison.OrdinalIgnoreCase)

					ElseIf value.StartsWith("("c) AndAlso value.EndsWith(")"c) Then
						value = value.Substring(1, value.Length - 2)
						If value.NotEmpty Then Flag = Regex.IsMatch(this, value, RegexOptions.IgnoreCase)

					End If

					' 全部内容再匹配一次
					If Not Flag Then Flag = this.Equals(value, StringComparison.OrdinalIgnoreCase)

					If Flag Then Exit For
				Next

				Return Flag
			Else
				Return True
			End If
		End Function

		''' <summary>用正则表达式检查字符串中包含指定内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="value">要检查的正则表达式</param>
		<Extension>
		<Obsolete("请使用 like 代替")>
		Public Function Include(this As String, value As String) As Boolean
			If this.NotNull Then
				Dim Pattern = PatternUpdate(value)
				If Pattern.IsPattern Then
					Return Regex.IsMatch(this, Pattern.Pattern, RegexOptions.IgnoreCase)
				Else
					Return this.Contains(value, StringComparison.OrdinalIgnoreCase)
				End If
			End If

			Return False
		End Function

		''' <summary>用指定字符串组检查字符串中包含指定内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="values">是否包含的字符串组</param>
		<Extension>
		<Obsolete("请使用 like 代替")>
		Public Function Include(this As String, ParamArray values As String()) As Boolean
			If this.NotNull AndAlso values?.Length > 0 Then
				For Each value In values.Select(Function(x) x.ToLowerInvariant).Distinct
					If this.Include(value) Then Return True
				Next
			End If

			Return False
		End Function

#End Region

#Region "5. 截取字符串"

		''' <summary>前缀</summary>
		Private Const BEGIN_STRING = "|_B.E.G.I.N_|"

		''' <summary>后缀</summary>
		Private Const END_STRING = "|_E.N.D_|"

		''' <summary>保留左侧字符长度</summary>
		<Extension>
		Public Function Left(this As String, stringLength As Integer, Optional lastAppend As String = "") As String
			If this.IsNull OrElse stringLength < 1 Then Return ""
			If this.Length < stringLength Then Return this

			If lastAppend.IsEmpty Then Return this.Substring(0, stringLength)

			stringLength -= lastAppend.Length
			If stringLength < 1 Then Return lastAppend

			Return String.Concat(this.AsSpan(0, stringLength), lastAppend)
		End Function

		''' <summary>保留右侧字符长度</summary>
		<Extension>
		Public Function Right(this As String, stringLength As Integer, Optional firstInsert As String = "") As String
			If this.IsNull OrElse stringLength < 1 Then Return ""
			If this.Length < stringLength Then Return this

			If firstInsert.IsEmpty Then Return this.Substring(this.Length - stringLength, stringLength)

			stringLength -= firstInsert.Length
			If stringLength < 1 Then Return firstInsert

			Return String.Concat(firstInsert, this.AsSpan(this.Length - stringLength, stringLength))
		End Function

		''' <summary>获取指定长度字符，一个汉字占用两个字符</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="unicodeLength">截取的长度，注意汉字为双字节</param>
		''' <param name="lastAppend">末尾添加字符串</param>
		<Extension>
		Public Function Cut(this As String, unicodeLength As Integer， Optional lastAppend As String = "…") As String
			If this.IsNull Then Return ""

			If lastAppend.IsEmpty Then lastAppend = "…"
			unicodeLength -= lastAppend.Length
			If unicodeLength < 1 Then Return ""

			Dim I As Integer = 0
			Dim J As Integer = 0

			'为汉字或全脚符号长度加2否则加1
			For Each C In this
				If Char.IsAscii(C) Then
					I += 1
				Else
					I += 2
				End If

				If I >= unicodeLength Then
					this = String.Concat(this.AsSpan(0, J), lastAppend)
					Exit For
				End If

				J += 1
			Next

			Return this
		End Function

		''' <summary>通过指定的正则表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="partNumber"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		<Extension>
		Public Function Cut(this As String, pattern As String, Optional partNumber As Integer = 0, Optional moreValue As Boolean = True) As Object
			If this.IsNull Then Return Nothing

			Dim Query = PatternUpdate(pattern)
			If Not Query.IsPattern Then Return Nothing

			If partNumber < 1 Then partNumber = 0

			Dim Res As New List(Of String)
			Try
				Dim Ms = Regex.Matches(this, Query.Pattern, RegexOptions.IgnoreCase)
				If Ms?.Count > 0 Then
					For I = 0 To Ms.Count - 1
						Dim Ret As String = Nothing

						If Ms(I)?.Groups?.Count > partNumber Then
							Ret = Ms(I).Groups(partNumber).Value
						ElseIf Ms(I)?.Length > 0 Then
							Ret = Ms(I).Value
						End If

						If Ret.StartsWith(BEGIN_STRING) Then Ret = Ret.Substring(BEGIN_STRING.Length)
						If Ret.EndsWith(END_STRING) Then Ret = Ret.Substring(0, Ret.Length - END_STRING.Length)

						If Ret IsNot Nothing Then Res.Add(Ret)
						If Not moreValue Then Exit For
					Next
				End If
			Catch ex As Exception
			End Try

			If Res.Count > 0 Then Return If(moreValue, Res.ToArray, Res(0))
			Return Nothing
		End Function

		''' <summary>通过指定的表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="begin">开始部分</param>
		''' <param name="last">结束部分，全部内容为 0</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		''' <param name="isIncludeSelf">是否返回包含本身，默认为False</param>
		<Extension>
		Public Function Cut(this As String, begin As String, last As String, Optional moreValue As Boolean = True, Optional isIncludeSelf As Boolean = False) As Object
			If this.IsNull Then Return Nothing
			If begin.IsNull AndAlso last.IsNull Then Return this

			Dim strBegin As String = BEGIN_STRING
			Dim strEnd As String = END_STRING

			If begin.IsNull Then this = strBegin & this Else strBegin = begin
			If last.IsNull Then this &= strEnd Else strEnd = last

			Dim strPattern As String = strBegin & "[*]" & strEnd

			If isIncludeSelf Then
				Return Cut(this, strPattern, 0, moreValue)
			Else
				Return Cut(this, strPattern, 1, moreValue)
			End If
		End Function

		''' <summary>通过指定的表达试来获取内容</summary>
		''' <param name="this">要获取的源字符串</param>
		''' <param name="pattern"> 要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换，一个字符只能包含一个"[*]"）</param>
		''' <param name="moreValue">是否返回多项内容，默认为True，多项内容为数组格式</param>
		<Extension>
		Public Function Cut(this As String, pattern As String, moreValue As Boolean) As Object
			Return Cut(this, pattern, 1, moreValue)
		End Function

		''' <summary>以指定分割符作为标志，剪切文本到到最大长度</summary>
		''' <param name="separator">分隔字符</param>
		''' <param name="maxLength">返回最大长度</param>
		<Extension>
		Public Function Cut(this As String, separator As String, maxLength As Integer) As String
			If this.IsEmpty OrElse maxLength < 1 OrElse this.Length < maxLength Then Return this
			If separator.IsEmpty Then Return this.Substring(0, maxLength)

			Dim path = this.IndexOf(separator, maxLength)
			If path > maxLength Then
				this = this.Substring(0, path)
			Else
				this = this.Substring(0, maxLength)
			End If

			maxLength = this.LastIndexOf(separator)
			If maxLength > -1 Then
				Return this.Substring(0, maxLength)
			Else
				Return this
			End If
		End Function

#End Region

#Region "6. 判断类型"

		'''' <summary>不为空</summary>
		'<Extension>
		'Public Function NotNull(this As String) As Boolean
		'	Return Not this.IsNull
		'End Function

		'''' <summary>包含文本内容</summary>
		'<Extension>
		'Public Function NotEmpty(this As String) As Boolean
		'	Return Not this.IsEmpty
		'End Function

		'''' <summary>是否为空</summary>
		'<Extension>
		'Public Function IsNull(this As String) As Boolean
		'	Return String.IsNullOrEmpty(this)
		'End Function

		'''' <summary>是否为空或者空格</summary>
		'<Extension>
		'Public Function IsEmpty(this As String) As Boolean
		'	Return String.IsNullOrWhiteSpace(this)
		'End Function

		'''' <summary>两字符串是否相同</summary>
		'''' <param name="CheckCase">是否比较大小写</param>
		'<Extension>
		'Public Function IsSame(this As String, target As String, Optional checkCase As Boolean = False) As Boolean
		'	If this.IsNull Then Return target.IsNull
		'	If target.IsNull Then Return False
		'	If checkCase Then Return this = target

		'	Return this.Equals(target, StringComparison.OrdinalIgnoreCase)
		'End Function

#Region "	6.1 格式验证"

		''' <summary>验证 Email</summary>
		<Extension>
		Public Function IsEmail(this As String) As Boolean
			Return this.IsMatch("^[\w\-\.]+@([\w\-]+\.)+[\w-]{2,}$")
		End Function

		''' <summary>验证 GUID</summary>
		<Extension>
		Public Function IsGUID(this As String) As Boolean
			Return this.IsMatch("^(\{?)[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}(\}?)$")
		End Function

		''' <summary>验证 IPv4</summary>
		<Extension>
		Public Function IsIPv4(this As String) As Boolean
			Return this.IsMatch("^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$")
		End Function

		''' <summary>验证 IPv6</summary>
		<Extension>
		Public Function IsIPv6(this As String) As Boolean
			Return this.IsMatch("^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$")
			' Return this.NotEmpty AndAlso Regex.IsMatch(this, "^(([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4}|:))|(([0-9a-fA-F]{1,4}:){6}(((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3})|:[0-9a-fA-F]{1,4}|:)|(([0-9a-fA-F]{1,4}:){5}(:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|((:[0-9a-fA-F]{1,4}){1,3})|:)|(([0-9a-fA-F]{1,4}:){4}(((:[0-9a-fA-F]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,3})|:)|(([0-9a-fA-F]{1,4}:){3}(((:[0-9a-fA-F]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,4})|:)|(([0-9a-fA-F]{1,4}:){2}(((:[0-9a-fA-F]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,5})|:)|(([0-9a-fA-F]{1,4}:){1}(((:[0-9a-fA-F]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,6})|:)|(:(((:[0-9a-fA-F]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d))){3}))|((:[0-9a-fA-F]{1,4}){1,7})|:)$")
		End Function

		''' <summary>验证 IP</summary>
		<Extension>
		Public Function IsIP(this As String) As Boolean
			Return this.IsIPv4 OrElse this.IsIPv6
		End Function

#End Region

#Region "	6.2 电话号码验证"

		''' <summary>固定电话号码验证</summary>
		<Extension>
		Public Function IsPhone(this As String) As Boolean
			Return this.IsMatch("^(((0\d{2,3}[ \-]?)?[1-9]\d{6,7}([ \-]\d{1,4})?)|(1[3-9]\d{9}))$")
		End Function

		''' <summary>手机号码验证</summary>
		<Extension>
		Public Function IsMobilePhone(this As String) As Boolean
			Return this.IsMatch("^1[3-9]\d{9}$")
		End Function

#End Region

#Region "	6.3 证件验证"

		''' <summary>验证身份证</summary>
		<Extension>
		Public Function IsCardID(this As String) As Boolean
			Return CardIDHelper.Validate(this)
		End Function

		''' <summary>验证营业执照</summary>
		<Extension>
		Public Function IsBusinessID(this As String) As Boolean
			Return this.IsMatch("^(?![IOZSV])[A-Z\d]{2}\d{6}(?![IOZSV])[A-Z\d]{10}$|^\d{15}$")
		End Function

		''' <summary>验证护照</summary>
		<Extension>
		Public Function IsPassport(this As String) As Boolean
			Return this.IsMatch("^(1[45]\d{7}|G\d{8}|P\d{7}|S\d{7,8})$")
		End Function

		''' <summary>验证港澳居民通行证</summary>
		<Extension>
		Public Function IsHKMO(this As String) As Boolean
			Return this.IsMatch("^[HMhm]\d{8,10}$")
		End Function

		''' <summary>验证台湾居民来往大陆通行证</summary>
		<Extension>
		Public Function IsTaiWan(this As String) As Boolean
			Return this.IsMatch("^\d{8}$")
		End Function

		''' <summary>验输入字符串是否为车牌</summary> 
		<Extension>
		Public Function IsCar(this As String) As Boolean
			Return this.ToCar.NotNull
		End Function

#End Region

#Region "	6.4 数字验证"

		''' <summary>无符号整数验证(正整数)</summary> 
		<Extension>
		Public Function IsUInt(this As String) As Boolean
			Return this.IsMatch("^\d+$")
		End Function

		''' <summary>整数验证</summary> 
		<Extension>
		Public Function IsInteger(this As String) As Boolean
			Return this.IsMatch("^[+-]?\d+$")
		End Function

		''' <summary>数字（整数或浮点数）验证</summary> 
		<Extension>
		Public Function IsNumber(this As String) As Boolean
			Return this.IsMatch("^[+-]?(?:\d+|\d*\.\d+)$")
		End Function

		''' <summary>判断字符串是否为A-Z、0-9及下划线以内的字符</summary> 
		<Extension>
		Public Function IsLetterNumber(this As String) As Boolean
			Return this.IsMatch("^[\w\-]+$")
		End Function

#End Region

#Region "	6.5 用户账户验证"

		''' <summary>
		''' 验证用户名格式<para />
		''' 规则：字母开头，允许数字/下划线/减号，长度 5 字符以上
		''' </summary> 
		''' <param name="maxLength">最多字符数，必须大于6</param>
		''' <param name="enDot">是否允许包含小数点</param>
		<Extension>
		Public Function IsUserName(this As String, Optional maxLength As Integer = 24, Optional enDot As Boolean = False) As Boolean
			If this.IsEmpty Then Return False

			If maxLength < 6 Then maxLength = 6
			maxLength -= 1

			Dim Pattern As String
			If enDot Then
				Pattern = $"^[a-zA-Z]{1}[\w\-\.]{{1,{maxLength}}}$"
			Else
				Pattern = $"^[a-zA-Z]{1}[\w\-]{{1,{maxLength}}}$"
			End If

			Return Regex.IsMatch(this, Pattern)
		End Function

		''' <summary>验证密码强度：6-20位，必须包含字母</summary> 
		<Extension>
		Public Function IsPassword(this As String) As Boolean
			Return this.NotEmpty AndAlso this.Length > 5 AndAlso this.Length < 21 AndAlso Regex.IsMatch(this, "[a-zA-Z]")
		End Function

		''' <summary>验证密码强度：6-20位，必须包含数字或大写字母</summary> 
		<Extension>
		Public Function IsPasswordNumberLetter(this As String) As Boolean
			Return this.IsPassword AndAlso Regex.IsMatch(this, "[0-9A-Z]")
		End Function

		''' <summary>验证密码强度：6-20位；必须包含数字；必须包含小写或大写字母；必须包含特殊符号）</summary> 
		<Extension>
		Public Function IsPasswordComplex(this As String) As Boolean
			Return this.IsPasswordNumberLetter AndAlso Regex.IsMatch(this, "[\x21-\x7e]")
		End Function

#End Region

#Region "	6.6 特殊格式验证"

		''' <summary>判断MD5Hash</summary>
		''' <param name="fullLength">True：32位； False：16位</param>
		<Extension>
		Public Function IsMD5Hash(this As String, Optional fullLength As Boolean = True) As Boolean
			If fullLength Then
				Return this.IsMatch("^[a-fA-F0-9]{32}$", RegexOptions.None)
			Else
				Return this.IsMatch("^[a-fA-F0-9]{16}$", RegexOptions.None)
			End If
		End Function

		''' <summary>验输入字符串是否全为中文/日文/韩文字符</summary> 
		<Extension>
		Public Function IsChinese(this As String) As Boolean
			Return this.IsMatch("^[\u4E00-\u9FFF]+$", RegexOptions.None)
		End Function

		''' <summary>验输入字符串是否全为 ASCII 字符</summary> 
		<Extension>
		Public Function IsAscii(this As String) As Boolean
			Return this.IsMatch("^[\x00-\x7F]+$", RegexOptions.None)
		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary> 
		<Extension>
		Public Function IsDateTime(this As String) As Boolean
			Return this.NotEmpty AndAlso Date.TryParse(this, Nothing)
		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary>
		<Extension>
		Public Function IsDate(this As String) As Boolean
			Return this.IsMatch("^\d{4}([-/.])(0?[1-9]|1[0-2])\1(0?[1-9]|[12][0-9]|3[01])$", RegexOptions.None)
		End Function

		''' <summary>验输入字符串是否可以转换成时间（包含字符表达式）</summary>	
		''' <remarks>
		''' 支持部分字符表达式：<para />
		''' now: 当前时间<para />
		''' today: 当前日期<para />
		''' tomorrow: 明天日期<para />
		''' yesterday: 昨天日期<para />
		''' year: 今年第一天<para />
		''' month: 本月第一天<para />
		''' week: 本周第一天<para />
		''' year_end: 今年最后一天<para />
		''' month_end: 本月最后一天<para />
		''' week_end: 本周最后一天<para />
		''' </remarks>
		<Extension>
		Public Function IsDateWithExp(this As String) As Boolean
			Return this.IsMatch("^(\d{4}([-/.])(0?[1-9]|1[0-2])\1(0?[1-9]|[12][0-9]|3[01])|(now|today|tomorrow|yesterday|year|month|week|year_end|month_end|week_end)$")
		End Function

		''' <summary>验输入字符串是否可以转换成时间</summary> 
		<Extension>
		Public Function IsTime(this As String) As Boolean
			Return this.IsMatch("^(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?$", RegexOptions.None)
		End Function

		''' <summary>路径是否合法，不能否含有“/\&gt;>:.?*|$]”特殊字符</summary> 
		<Extension>
		Public Function IsPath(this As String) As Boolean
			Return this.NotEmpty AndAlso this.IndexOfAny(IO.Path.GetInvalidPathChars)
		End Function

		''' <summary>文件名是否合法，不能否含有“/\&gt;>:.?*|$]”特殊字符</summary> 
		<Extension>
		Public Function IsFileName(this As String) As Boolean
			Return this.NotEmpty AndAlso this.IndexOfAny(IO.Path.GetInvalidFileNameChars)
		End Function

		''' <summary>验证 URL 格式</summary>
		<Extension>
		Public Function IsUrl(this As String) As Boolean
			Dim uri As Uri = Nothing
			Return this.NotEmpty AndAlso
				Uri.TryCreate(this, UriKind.Absolute, uri) AndAlso
				{"http", "https", "ftp"}.Contains(uri.Scheme) AndAlso
				uri.Host.NotEmpty
		End Function

		''' <summary>验证 JSON 格式</summary>
		''' <param name="strict">是否严格模式</param>
		<Extension>
		Public Function IsJson(this As String, Optional strict As Boolean = True) As Boolean
			If this.IsEmpty Then Return False

			Try
				Using doc = JsonDocument.Parse(this)
					Return Not strict OrElse doc.RootElement.ValueKind = JsonValueKind.Object OrElse doc.RootElement.ValueKind = JsonValueKind.Array
				End Using
			Catch
				Return False
			End Try
		End Function

		''' <summary>验输入字符串是否有效的XML内容</summary> 
		<Extension>
		Public Function IsXml(this As String) As Boolean
			If this.IsEmpty Then Return False

			Try
				' 添加 XmlReaderSettings 配置：忽略注释提升性能；禁用 DTD 处理避免外部实体攻击
				Dim settings = New XmlReaderSettings With {.IgnoreComments = True, .DtdProcessing = DtdProcessing.Ignore}

				' 使用 XmlReader 代替 XmlDocument 更高效
				Using reader = XmlReader.Create(New StringReader(this), settings)
					While reader.Read()
						' 遍历所有节点进行完整解析
					End While

					Return True
				End Using
			Catch ex As Exception
				Return False
			End Try
		End Function

#End Region

#End Region

#Region "7. 转换格式"

		''' <summary>转换成全角</summary>
		<Extension>
		Public Function ToSBC(this As String) As String
			If this.IsNull Then Return ""

			Dim arr = this.ToCharArray()
			For I = 0 To arr.Length - 1
				Dim C As Integer = AscW(arr(I))
				If C = 32 Then
					arr(I) = ChrW(12288)
				ElseIf C < 127 Then
					arr(I) = ChrW(C + 65248)
				End If
			Next
			Return New String(arr)
		End Function

		''' <summary>转换成半角</summary>
		<Extension>
		Public Function ToDBC(this As String) As String
			If this.IsNull Then Return ""

			Dim arr As Char() = this.ToCharArray()
			For I = 0 To arr.Length - 1
				Dim C As Integer = AscW(arr(I))
				If C = 12288 Then
					arr(I) = ChrW(32)
				ElseIf C > 65280 And C < 65375 Then
					arr(I) = ChrW(C - 65248)
				End If
			Next
			Return New String(arr)
		End Function

		''' <summary>转换为 Boolean</summary>
		<Extension>
		Public Function ToBoolean(this As String) As Boolean
			If this.IsEmpty Then Return False

			Select Case this.ToDBC.Trim.ToLowerInvariant
				Case "1", "true", "yes", "ok", "success", "on", "right", "good", "big", "more", "long", "wide", "tall", "high", "fat", "是", "有", "真", "对", "好", "正确", "女", "大", "多", "高", "长", "宽", "胖"
					Return True
				Case Else
					Return False
			End Select
		End Function

		''' <summary>转换为 TriState 三态</summary>
		<Extension>
		Public Function ToTriState(this As String) As TristateEnum
			If this.IsEmpty Then Return TristateEnum.DEFAULT

			this = this.ToDBC.Trim
			Select Case this.ToLowerInvariant
				Case "false", "no", "err", "error", "off", "left", "bad", "非", "无", "假", "错", "坏", "错误", "男"
					Return TristateEnum.FALSE
				Case "true", "yes", "ok", "success", "on", "right", "good", "是", "有", "真", "对", "好", "正确", "女"
					Return TristateEnum.TRUE
				Case "usedefault", "default", "other", "unknow", "unknown", "默认", "未知", "其他", "待定"
					Return TristateEnum.DEFAULT
				Case Else
					Select Case this.ToInteger(False)
						Case Is < 0
							Return TristateEnum.FALSE
						Case Is > 0
							Return TristateEnum.TRUE
						Case Else
							Return TristateEnum.DEFAULT
					End Select
			End Select
		End Function

		''' <summary>转换为 DateTime</summary>
		''' <param name="this">6位：yyMMdd / 8位：yyyyMMdd / 10,13位：Js Timer / 12位：yyMMddHHmmss / 14位：yyyyMMddHHmmss / 其他数字转换成Timespan / 字符则系统自动转换</param>
		<Extension>
		Public Function ToDateTime(this As String, Optional defaultDate As Date = Nothing) As Date
			Return this.ToDate(defaultDate)
		End Function

		''' <summary>调整日期字符串组，将重复或者不规范的日期过滤</summary>
		''' <param name="this">日期数据，格式：YYYY-MM-dd 多个用逗号间隔</param>
		''' <remarks>2016-09-25</remarks>
		<Extension>
		Public Function ToDateList(this As String, Optional defaultDate As Date = Nothing) As List(Of Date)
			Return this.ToDates(defaultDate)
		End Function

		''' <summary>转换为数字，仅包含0-9，负号和小数点</summary>
		''' <param name="coverALL">是否转换所有字符，True：整个字符非数字全过滤并合并成一个数字，False：过滤后仅保留第一段数字</param>
		''' <param name="toDBC">是否将全角数字转换为半角数字</param>
		''' <param name="incluedPointer">是否包含小数点后的数，默认包含</param>
		<Extension>
		Public Function ToNumber(this As String, Optional coverALL As Boolean = False, Optional toDBC As Boolean = False, Optional incluedPointer As Boolean = True) As Decimal
			If this.NotEmpty Then
				' 全角数字转换为半角数字
				If toDBC Then this = this.ToDBC

				If coverALL Then
					this = Regex.Replace(this, "[^\d\.\-\+]", "").Trim

				Else
					this = Regex.Replace(this, "[^0-9\.\-]", " ").Trim

					' 取第一段数据
					If this.NotNull AndAlso this.Contains(" "c) Then this = this.Split(" "c)(0)
				End If

				' 正负号仅用于第一个字符，如果中间存在多个则取第一个
				Dim chars = {"+"c, "-"c}
				Dim path = this.IndexOfAny(chars, 1)
				If path > 0 Then this = this.Substring(0, path)

				' 去掉正号，因为数据转换后自动会变成正数
				If this.StartsWith("+"c) Then this = this.Substring(1)

				' 只能保留一个小数点
				If this.Contains("."c) Then
					Dim dot = $"0{this}0".Split("."c)
					this = If(incluedPointer, $"{dot(0)}.{dot(1)}", dot(0))
				End If
			End If

			If this.IsEmpty Then Return 0

			Return Convert.ToDecimal(this)
		End Function

		''' <summary>更新原始字符串</summary>
		Private Function UpdateSource(this As String, update As Boolean) As String
			Return If(update, this.ToNumber, this)
		End Function

		''' <summary>转换为 Double</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToDouble(this As String, Optional updateInput As Boolean = False) As Double
			If this.IsEmpty Then Return 0

			Dim value As Double = 0
			If Double.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 Single</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToSingle(this As String, Optional updateInput As Boolean = False) As Single
			If this.IsEmpty Then Return 0

			Dim value As Single = 0
			If Single.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 Int64</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToLong(this As String, Optional updateInput As Boolean = False) As Long
			If this.IsEmpty Then Return 0

			Dim value As Long = 0
			If Long.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 Int32</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToInteger(this As String, Optional updateInput As Boolean = False) As Integer
			If this.IsEmpty Then Return 0

			Dim value As Integer = 0
			If Integer.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 Int16</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToShort(this As String, Optional updateInput As Boolean = False) As Short
			If this.IsEmpty Then Return 0

			Dim value As Short = 0
			If Short.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 Char，将字符串中的数字转换成十进制数进行转换</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToChar(this As String, Optional updateInput As Boolean = False) As Char
			If this.IsEmpty Then Return Char.MinValue

			Dim value As Char = Char.MinValue
			If Char.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return Char.MinValue
		End Function

		''' <summary>转换为 Byte</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToByte(this As String, Optional updateInput As Boolean = False) As Byte
			If this.IsEmpty Then Return 0

			Dim value As Byte = 0
			If Byte.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 UInt64</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToULong(this As String, Optional updateInput As Boolean = False) As ULong
			If this.IsEmpty Then Return 0

			Dim value As ULong = 0
			If ULong.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 UInt32</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToUInteger(this As String, Optional updateInput As Boolean = False) As UInteger
			If this.IsEmpty Then Return 0

			Dim value As UInteger = 0
			If UInteger.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 UInt16</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToUShort(this As String, Optional updateInput As Boolean = False) As UShort
			If this.IsEmpty Then Return 0

			Dim value As UShort = 0
			If UShort.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换为 SByte</summary>
		''' <param name="updateInput">是否转换过滤字符串中无效的数字负号</param>
		<Extension>
		Public Function ToSByte(this As String, Optional updateInput As Boolean = False) As SByte
			If this.IsEmpty Then Return 0

			Dim value As SByte = 0
			If SByte.TryParse(UpdateSource(this, updateInput), value) Then Return value
			Return 0
		End Function

		''' <summary>转换 GUID</summary>
		<Extension>
		Public Function ToGuid(this As String, Optional updateInput As Boolean = False) As Guid
			If this.IsEmpty Then Return Guid.Empty

			this = If(updateInput, this.ToDBC.Replace("[^0-9a-fA-F\{\}\-]", ""), this)
			this = this.Trim

			Dim Ret As Guid
			If Guid.TryParse(this, Ret) Then
				Return Ret
			Else
				Return Guid.Empty
			End If
		End Function

#Region "	车牌"

		''' <summary>验证中国车牌号合法性（支持所有现行车牌类型）最后更新：2023年10月</summary>
		Private ReadOnly _REGEX_CAR As New Regex("^(
        # 普通民用车牌（蓝牌）
        ([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-HJ-NP-Z](?:[0-9]{5}[DF]|[DF][A-HJ-NP-Z0-9][0-9]{4}))|
        # 新能源车牌（绿牌）
        ([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-HJ-NP-Z][A-HJ-NP-Z0-9]{4,5}[DF])|
        # 港澳入出境车牌
        (港澳[1-9][0-9]{4}[0-9A-Z])|
        # 使馆/领馆车牌
        (使[0-9]{6}|领[0-9]{5})|
        # 警用车辆
        ([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼][A-HJ-NP-Z]?[0-9]{4}警)|
        # 武警车辆
        (WJ[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼][0-9]{4}[TD])|
        # 应急管理车辆
        ([0-9]{3}应急[0-9]{3})|
        # 临时车牌
        (?:[0-9]{3}临[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼]|临[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼][0-9]{5})|
        # 教练车牌
        ([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼]通[0-9]{4}学)|
        # 民航车牌
        (民航[A-HJ-NP-Z][0-9]{4})|
        # 测试/样品车辆
        ([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼][A-HJ-NP-Z]试[0-9]{3})
    )$", RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.IgnorePatternWhitespace)

		''' <summary>转换成有效的车牌</summary>
		<Extension>
		Public Function ToCar(this As String) As String
			If this.IsEmpty Then Return ""

			this = this.ToDBC.
				Replace(" ", "").Replace("O", "0").Replace("I", 1).
				Replace(".", "").Replace("·", "").Replace("﹐", "").
				Replace("-", "").Replace("－", "").Replace("_", "").
				TrimFull.ToUpperInvariant

			If Not _REGEX_CAR.IsMatch(this) Then Return ""

			' 添加分隔符逻辑
			Dim InsertSeparator As Func(Of String, Integer, Integer, String) = Function(input, index, length) If(input.Length > index + length, $"{input.Substring(0, index)}·{input.Substring(index, length)}", input)

			' 武警车牌
			If this.StartsWith("WJ") Then Return InsertSeparator(this, 2, 6)

			' 领馆车牌
			If this.StartsWith("领"c) Then Return InsertSeparator(this, 1, 5)

			' 使馆车牌
			If this.StartsWith("使"c) Then Return InsertSeparator(this, 1, 6)

			' 应急车辆
			If this.Contains("应急") Then Return InsertSeparator(this, 3, 6)

			' 临时车牌
			If this.Contains("临"c) Then Return InsertSeparator(this, this.IndexOf("临"c) + 1, 5)

			' 测试车辆
			If this.Contains("试"c) Then Return InsertSeparator(this, this.IndexOf("试"c) + 1, 3)

			' 默认格式
			Return InsertSeparator(this, 2, this.Length - 2)
		End Function

#End Region

		''' <summary>获取有效的电话字符</summary>
		<Extension>
		Public Function ToPhone(this As String) As String
			If this.IsEmpty Then Return ""
			Return Regex.Replace(this, "[^\d\.\-\+\(\) ]", "")
		End Function

		''' <summary>获取有效的手机号码</summary>
		<Extension>
		Public Function ToMobilePhone(this As String) As String
			If this.IsMobilePhone Then
				Return this
			Else
				Return ""
			End If
		End Function

		''' <summary>转换成文件路径，去除无效的字符</summary>
		<Extension>
		Public Function ToPath(this As String) As String
			If this.IsEmpty Then Return ""
			Return String.Concat(this.Split(Path.GetInvalidPathChars))
		End Function

		''' <summary>转换成文件名，去除无效的字符</summary>
		<Extension>
		Public Function ToFileName(this As String) As String
			If this.IsEmpty Then Return ""
			Return String.Concat(this.Split(Path.GetInvalidFileNameChars))
		End Function

		''' <summary>转换成URL路径</summary>
		<Extension>
		Public Function ToUrl(this As String) As String
			Dim uri As Uri = Nothing
			If Uri.TryCreate(this, UriKind.Absolute, uri) Then Return uri.AbsoluteUri
			Return ""
		End Function

		'''' <summary>大写转换成小写驼峰</summary>
		'<Extension>
		'Public Function ToCamelCase(this As String, Optional isLine As Boolean = False) As String
		'	If this.NotEmpty Then
		'		this = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this)
		'		this = String.Concat(this.Substring(0, 1).ToLowerInvariant, this.AsSpan(1))

		'		If isLine Then
		'			With New Text.StringBuilder
		'				For Each c In this
		'					' 大写则添加横线
		'					If c >= "A"c AndAlso c <= "Z"c Then .Append("-"c)
		'					.Append(c)
		'				Next

		'				this = .ToString
		'			End With
		'		End If
		'	End If

		'	Return this
		'End Function

		''' <summary>转换成有效的XML字符串</summary>
		<Extension>
		Public Function ToXML(this As String) As String
			Dim Doc As New XmlDocument
			Doc.LoadXml("<xml />")

			Try
				Doc.DocumentElement.InnerXml = this
			Catch ex As Exception
				Doc.DocumentElement.InnerText = this
			End Try

			Return Doc.DocumentElement.InnerXml
		End Function

#Region "拼音"

		Private ReadOnly _PYV As Integer() = {-20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, -20051, -20036, -20032, -20026, -20002, -19990, -19986, -19982, -19976, -19805, -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728, -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, -19243, -19242, -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, -19003, -18996, -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, -18211, -18201, -18184, -18183, -18181, -18012, -17997, -17988, -17970, -17964, -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730, -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, -16970, -16942, -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, -16465, -16459, -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412, -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, -15915, -15903, -15889, -15878, -15707, -15701, -15681, -15667, -15661, -15659, -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416, -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, -15140, -15139, -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, -14929, -14928, -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, -14353, -14345, -14170, -14159, -14151, -14149, -14145, -14140, -14137, -14135, -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090, -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, -13406, -13404, -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, -13340, -13329, -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594, -12585, -12556, -12359, -12346, -12320, -12300, -12120, -12099, -12089, -12074, -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604, -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, -11018, -11014, -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533, -10519, -10331, -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281, -10274, -10270, -10262, -10260, -10256, -10254}

		Private ReadOnly _PYS As String() = {"A", "Ai", "An", "Ang", "Ao", "Ba", "Bai", "Ban", "Bang", "Bao", "Bei", "Ben", "Beng", "Bi", "Bian", "Biao", "Bie", "Bin", "Bing", "Bo", "Bu", "Ca", "Cai", "Can", "Cang", "Cao", "Ce", "Ceng", "Cha", "Chai", "Chan", "Chang", "Chao", "Che", "Chen", "Cheng", "Chi", "Chong", "Chou", "Chu", "Chuai", "Chuan", "Chuang", "Chui", "Chun", "Chuo", "Ci", "Cong", "Cou", "Cu", "Cuan", "Cui", "Cun", "Cuo", "Da", "Dai", "Dan", "Dang", "Dao", "De", "Deng", "Di", "Dian", "Diao", "Die", "Ding", "Diu", "Dong", "Dou", "Du", "Duan", "Dui", "Dun", "Duo", "E", "En", "Er", "Fa", "Fan", "Fang", "Fei", "Fen", "Feng", "Fo", "Fou", "Fu", "Ga", "Gai", "Gan", "Gang", "Gao", "Ge", "Gei", "Gen", "Geng", "Gong", "Gou", "Gu", "Gua", "Guai", "Guan", "Guang", "Gui", "Gun", "Guo", "Ha", "Hai", "Han", "Hang", "Hao", "He", "Hei", "Hen", "Heng", "Hong", "Hou", "Hu", "Hua", "Huai", "Huan", "Huang", "Hui", "Hun", "Huo", "Ji", "Jia", "Jian", "Jiang", "Jiao", "Jie", "Jin", "Jing", "Jiong", "Jiu", "Ju", "Juan", "Jue", "Jun", "Ka", "Kai", "Kan", "Kang", "Kao", "Ke", "Ken", "Keng", "Kong", "Kou", "Ku", "Kua", "Kuai", "Kuan", "Kuang", "Kui", "Kun", "Kuo", "La", "Lai", "Lan", "Lang", "Lao", "Le", "Lei", "Leng", "Li", "Lia", "Lian", "Liang", "Liao", "Lie", "Lin", "Ling", "Liu", "Long", "Lou", "Lu", "Lv", "Luan", "Lue", "Lun", "Luo", "Ma", "Mai", "Man", "Mang", "Mao", "Me", "Mei", "Men", "Meng", "Mi", "Mian", "Miao", "Mie", "Min", "Ming", "Miu", "Mo", "Mou", "Mu", "Na", "Nai", "Nan", "Nang", "Nao", "Ne", "Nei", "Nen", "Neng", "Ni", "Nian", "Niang", "Niao", "Nie", "Nin", "Ning", "Niu", "Nong", "Nu", "Nv", "Nuan", "Nue", "Nuo", "O", "Ou", "Pa", "Pai", "Pan", "Pang", "Pao", "Pei", "Pen", "Peng", "Pi", "Pian", "Piao", "Pie", "Pin", "Ping", "Po", "Pu", "Qi", "Qia", "Qian", "Qiang", "Qiao", "Qie", "Qin", "Qing", "Qiong", "Qiu", "Qu", "Quan", "Que", "Qun", "Ran", "Rang", "Rao", "Re", "Ren", "Reng", "Ri", "Rong", "Rou", "Ru", "Ruan", "Rui", "Run", "Ruo", "Sa", "Sai", "San", "Sang", "Sao", "Se", "Sen", "Seng", "Sha", "Shai", "Shan", "Shang", "Shao", "She", "Shen", "Sheng", "Shi", "Shou", "Shu", "Shua", "Shuai", "Shuan", "Shuang", "Shui", "Shun", "Shuo", "Si", "Song", "Sou", "Su", "Suan", "Sui", "Sun", "Suo", "Ta", "Tai", "Tan", "Tang", "Tao", "Te", "Teng", "Ti", "Tian", "Tiao", "Tie", "Ting", "Tong", "Tou", "Tu", "Tuan", "Tui", "Tun", "Tuo", "Wa", "Wai", "Wan", "Wang", "Wei", "Wen", "Weng", "Wo", "Wu", "Xi", "Xia", "Xian", "Xiang", "Xiao", "Xie", "Xin", "Xing", "Xiong", "Xiu", "Xu", "Xuan", "Xue", "Xun", "Ya", "Yan", "Yang", "Yao", "Ye", "Yi", "Yin", "Ying", "Yo", "Yong", "You", "Yu", "Yuan", "Yue", "Yun", "Za", "Zai", "Zan", "Zang", "Zao", "Ze", "Zei", "Zen", "Zeng", "Zha", "Zhai", "Zhan", "Zhang", "Zhao", "Zhe", "Zhen", "Zheng", "Zhi", "Zhong", "Zhou", "Zhu", "Zhua", "Zhuai", "Zhuan", "Zhuang", "Zhui", "Zhun", "Zhuo", "Zi", "Zong", "Zou", "Zu", "Zuan", "Zui", "Zun", "Zuo"}

		''' <summary>汉字转拼音</summary>
		''' <param name="this">汉字字符串</param>
		''' <param name="removeAscii">是否删除所有Ascii字符</param>
		''' <param name="firstLetterOnly">只返回拼音首字母</param>
		''' <param name="separator">多个拼音之间的连接符</param>
		<Extension>
		Public Function ToPinYin(this As String, Optional removeAscii As Boolean = True, Optional firstLetterOnly As Boolean = False, Optional separator As String = "") As String
			If this.IsEmpty Then Return ""

			With New Text.StringBuilder
				For Each C In this
					Dim Bs As Byte() = GB2312.GetBytes(C)
					Dim Code As Integer

					If Bs?.Length > 1 Then
						Code = (Bs(0) * 256) + Bs(1) - 65536
					Else
						Code = Bs(0)
					End If

					' 英文
					If Code > 0 Then
						.Append(If(removeAscii, " ", C))
					Else
						' 中文
						For J = _PYV.Length - 1 To 0 Step -1
							If _PYV(J) <= Code Then
								.Append(If(firstLetterOnly, _PYS(J).Substring(0, 1), _PYS(J)))
								If J > 0 Then .Append(separator)
								Exit For
							End If
						Next
					End If
				Next

				Return .ToString
			End With
		End Function

#End Region

#End Region

#Region "8. 获取格式化内容"

		''' <summary>获取 ASCII</summary>
		''' <param name="this">要操作的字符串</param>
		<Extension>
		Public Function GetAscii(this As String) As String
			If this.IsNull Then Return ""
			Return Regex.Replace(this, "[^\x21-\x7E]", "").Trim
		End Function

		''' <summary>获取任意字母，数字，下划线，汉字的字符</summary>
		''' <param name="this">要操作的字符串</param>
		<Extension>
		Public Function GetChars(this As String) As String
			If this.IsNull Then Return ""
			Return Regex.Replace(this, "[\W]", "").Trim
		End Function

		''' <summary>获取汉字的字符</summary>
		''' <param name="this">要操作的字符串</param>
		<Extension>
		Public Function GetChinese(this As String) As String
			If this.IsEmpty Then Return ""
			Return Regex.Replace(this, "[^\u4e00-\u9fa5]", "").Trim
		End Function

		''' <summary>获取所有大写字母</summary>
		''' <param name="this">要操作的字符串</param>
		<Extension>
		Public Function GetUpper(this As String) As String
			If this.IsEmpty Then Return ""
			Return Regex.Replace(this, "[^\x41-\x5A]", "").Trim
		End Function

		''' <summary>转换时间格式</summary>
		<Extension>
		Public Function GetDateTime(this As String, Optional strFormat As String = "") As String
			Return this.ToDateTime(DATE_NOW).ToString(strFormat.EmptyValue("yyyy-MM-dd HH:mm:ss"))
		End Function

		''' <summary>转换时间格式</summary>
		''' <param name="prefix">前缀</param>
		''' <param name="suffix">后缀</param>
		''' <remarks>仅支持日期格式字符串，参考：https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-date-and-time-format-strings</remarks>
		<Extension>
		Public Function GetDateTime(this As String, prefix As String, suffix As String, Optional dateAction As Date? = Nothing) As String
			If this.IsEmpty OrElse prefix.IsEmpty OrElse suffix.IsEmpty Then Return this

			If dateAction Is Nothing Then dateAction = DATE_NOW

			Dim pre = Regex.Escape(prefix)
			Dim suf = Regex.Escape(suffix)

			Dim matches = Regex.Matches(this, $"{pre}([a-z\:\-_年月日]*){suf}", RegexOptions.IgnoreCase)
			If matches.IsEmpty Then Return this

			' 处理重复数据
			Dim ms = matches.Select(Function(x) x.Groups(1).Value).Distinct.ToList
			If ms.IsEmpty Then Return this

			' 替换操作
			ms.ForEach(Sub(m)
						   Try
							   Dim v = dateAction.Value.ToString(m)
							   If v.NotEmpty Then this = this.Replace($"{prefix}{m}{suffix}", v)
						   Catch ex As Exception
						   End Try
					   End Sub)

			Return this
		End Function

		''' <summary>格式化日期类标题</summary>
		''' <param name="this">要格式化的内容</param>
		''' <param name="dateTimeName">标签前缀，使用时使用点间隔</param>
		''' <param name="dateAction">操作时间</param>
		''' <param name="dayBegin">提前计算天数</param>
		''' <param name="dayEnd">结束计算天数</param>
		<Extension>
		Public Function GetDateTime(this As String, dateAction As Date?, Optional dateTimeName As String = "", Optional dayBegin As Integer = -1, Optional dayEnd As Integer = 1) As String
			If this.IsEmpty OrElse dateAction Is Nothing Then Return this

			' 检查前缀是否存在
			Dim hasReplace = True
			If dateTimeName.NotEmpty Then
				dateTimeName = "[" & dateTimeName.ToLowerInvariant & "."

				' 存在前缀，不区分大小写，替换成标准前缀
				If this.Contains(dateTimeName, StringComparison.OrdinalIgnoreCase) Then
					this = this.Replace(dateTimeName, dateTimeName, StringComparison.OrdinalIgnoreCase)
					dateTimeName = dateTimeName.Substring(1)
				Else
					' 默认需要替换，但是如果存在默认前缀时，但是内容中无前缀，则无需替换操作
					hasReplace = False
				End If
			ElseIf Not this.Contains("["c) OrElse Not this.Contains("]"c) Then
				' 不存在替换标签
				hasReplace = False
			End If

			' 需要替换操作
			If hasReplace Then
				Dim dateNow As Date = dateAction
				If dateNow < New Date(1900, 1, 1) Then dateNow = Date.Now

				Dim dS As Integer = Math.Min(dayBegin, dayEnd)
				Dim dE As Integer = Math.Max(dayBegin, dayEnd)

				For I As Integer = dS To dE
					Dim N = ""
					If I > 0 Then N = "+" & I
					If I < 0 Then N = I

					Dim d As Date = dateNow.AddDays(I)

					this = this.Replace("[" & dateTimeName & "YYYY" & N & "]", d.Year, StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "MM" & N & "]", d.ToString("MM"))
					this = this.Replace("[" & dateTimeName & "DD" & N & "]", d.ToString("dd"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "YY" & N & "]", d.ToString("yy"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "M" & N & "]", d.Month)
					this = this.Replace("[" & dateTimeName & "D" & N & "]", d.Day, StringComparison.OrdinalIgnoreCase)

					this = this.Replace("[" & dateTimeName & "hh" & N & "]", d.ToString("HH"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "mm" & N & "]", d.ToString("mm"))
					this = this.Replace("[" & dateTimeName & "ss" & N & "]", d.ToString("ss"), StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "h" & N & "]", d.Hour, StringComparison.OrdinalIgnoreCase)
					this = this.Replace("[" & dateTimeName & "m" & N & "]", d.Minute)
					this = this.Replace("[" & dateTimeName & "s" & N & "]", d.Second, StringComparison.OrdinalIgnoreCase)

					this = this.Replace("[" & dateTimeName & "DATE" & N & "]", d.ToLongDateString)
					this = this.Replace("[" & dateTimeName & "TIME" & N & "]", d.ToLongTimeString)
					this = this.Replace("[" & dateTimeName & "date" & N & "]", d.ToShortDateString)
					this = this.Replace("[" & dateTimeName & "time" & N & "]", d.ToShortTimeString)

					Select Case d.DayOfWeek
						Case DayOfWeek.Monday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 1)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "一")
						Case DayOfWeek.Tuesday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 2)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "二")
						Case DayOfWeek.Wednesday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 3)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "三")
						Case DayOfWeek.Thursday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 4)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "四")
						Case DayOfWeek.Friday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 5)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "五")
						Case DayOfWeek.Saturday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 6)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "六")
						Case DayOfWeek.Sunday
							this = this.Replace("[" & dateTimeName & "w" & N & "]", 7)
							this = this.Replace("[" & dateTimeName & "W" & N & "]", "日")
					End Select
				Next

				this = this.Replace("[rnd]", dateNow.Ticks)
				this = this.Replace("[RND]", Guid.NewGuid.ToString)
			End If

			Return this
		End Function

		''' <summary>调整日期字符串组，将重复或者不规范的日期过滤</summary>
		''' <param name="this">日期数据，格式：YYYY-MM-dd 多个用逗号间隔</param>
		''' <param name="onlyDate">只返回日期部分</param>
		<Extension>
		Public Function GetDateList(this As String, Optional defaultDate As Date = Nothing, Optional onlyDate As Boolean = True) As String
			Dim Ds = this.ToDateList(defaultDate)
			Dim Df = If(onlyDate, "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss")
			If Ds?.Count > 0 Then
				Return String.Join(",", Ds.Select(Function(x) x.ToString(Df)))
			Else
				Return ""
			End If
		End Function

		'''' <summary>分析并调整关键词类数组组合成的字符串，防止其超过最大长度</summary>
		'<Extension>
		'<Obsolete("等同于 Cut(this string input, string separator, int maxLength)")>
		'Public Function GetArrayString(this As String, Optional maxLength As Integer = Integer.MaxValue, Optional separator As String = Nothing) As String
		'	If this.IsEmpty Or maxLength < 1 Then Return ""

		'	Dim joinString = separator.NullValue(",")

		'	Dim list = this.SplitDistinct(separator)
		'	If list.IsEmpty Then Return ""

		'	Array.Sort(list)

		'	Dim ret = list.JoinString(joinString)
		'	If ret.Length > maxLength Then
		'		Dim Last = ret.LastIndexOf(joinString, maxLength)
		'		If Last > 0 Then
		'			ret = ret.Substring(0, Last)
		'		Else
		'			ret = ret.Substring(0, maxLength)
		'		End If
		'	End If

		'	Return Ret
		'End Function

		'''' <summary>分析并调整关键词类数组组合成的字符串，防止其超过最大长度</summary>
		'''' <remarks>一个标准 GUID 为36个字节</remarks>
		'<Extension>
		'Public Function GetGuidString(this As String, Optional maxLength As Integer = Integer.MaxValue, Optional separator As String = "") As String
		'	If this.IsEmpty OrElse maxLength < 36 Then Return ""

		'	Dim Arr = this.ToGuidList(separator)
		'	If Arr.NotEmpty Then
		'		separator = separator.NullValue(",")

		'		Dim len = 36 + separator.Length
		'		Dim max As Integer = Math.Floor(maxLength / len)

		'		Return Arr.Take(max).JoinString(separator)
		'	Else
		'		Return ""
		'	End If
		'End Function

#End Region

#Region "0. 公共调用内置操作"

		''' <summary>更新当前正则参数，并返回参数类型</summary>
		''' <param name="this">要匹配的正则表达式或者指定字符串（正则表达式用括号包含，字符串则变化部分用"[*]"替换"）</param>
		''' <returns>
		''' 1. 如果括号起始表示为正则表达参数
		''' 2. 包含 [*] 字符串表示需要替换参数的正则参数
		''' 3. 其他类型为通用字符串
		''' </returns>
		Private Function PatternUpdate(this As String) As (IsPattern As Boolean, Pattern As String)
			If this.NotEmpty AndAlso this.Length > 2 Then
				If this.StartsWith("("c) AndAlso this.EndsWith(")"c) AndAlso this.Length > 2 Then
					this = this.Substring(1, this.Length - 2)
					Return (True, this)
				ElseIf this.Contains("[*]") Then
					this = Regex.Escape(this)
					this = this.Replace("\[\*]", "((.|\n)*?)")
					Return (True, this)
				End If
			Else
				If this.IsNull Then this = ""
			End If

			Return (False, this)
		End Function

#End Region

	End Module
End Namespace
