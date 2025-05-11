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
Imports System.Runtime.CompilerServices
Imports FreeSql

Namespace Extension
	Partial Public Module QueryExtension

		''' <summary>将查询结果返回为数据列表</summary>
		''' <param name="keyword">高亮关键词，存在关键词则用【】包含</param>
		''' <param name="listUpdate">自定义结果输出操作</param>
		''' <param name="nameFields">用于文本结果输出的字段，多个逗号间隔</param>
		''' <param name="disabledField">结果是否禁用状态的字段</param>
		''' <param name="extField">扩展内容字段</param>
		''' <param name="maxTextLength">文本字段最多输出字符数，默认 100</param>
		<Extension>
		Public Function ToDataList(Of T As {IEntity, Class})(this As IEnumerable(Of T),
															 Optional keyword As String = "",
															 Optional listUpdate As Func(Of T, DataList, DataList) = Nothing,
															 Optional nameFields As String = Nothing,
															 Optional disabledField As String = Nothing,
															 Optional extField As String = Nothing,
															 Optional maxTextLength As Integer = 100) As List(Of DataList)
			If this.IsEmpty Then Return Nothing

			' 分析名称键
			Dim pros = GetType(T).GetAllProperties.ToDictionary(Function(x) x.Name, Function(x) x.PropertyType.IsString)
			Dim allFields = pros.Keys
			Dim strFields = pros.Where(Function(x) x.Value).Select(Function(x) x.Key).ToArray

			' 上级
			Dim parentField = If(allFields.Contains("ParentId", StringComparer.OrdinalIgnoreCase), "ParentId", "")

			' 名称键
			Dim textFields = {nameFields, "title", "name", "id"}.Where(Function(x) strFields.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			' 禁用键
			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) allFields.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 描述键
			extField = If(allFields.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.StartsWith("en", StringComparison.OrdinalIgnoreCase)

			' 最多输出文本长度
			maxTextLength = maxTextLength.Range(1, 1000)

			' 要输出的字段
			Dim fields = {"ID", parentField, disabledField, extField}.Union(textFields).Where(Function(x) x.NotEmpty).ToArray

			' 查询结果
			Return this.Select(Function(item)
								   Dim dic = item.ToDictionary(False, fields)
								   Dim data As New DataList(dic, Nothing, "ID", parentField, disabledField, extField)

								   ' 无效值，不再处理
								   If data.Value Is Nothing Then Return Nothing

								   ' disabled 需要反转
								   If isRev Then data.Disabled = Not data.Disabled

								   ' 从所有名称字段中分析值
								   Dim text = ""
								   If keyword.IsEmpty Then
									   For Each field In textFields
										   text = dic.Where(Function(x) x.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString
										   If text.NotEmpty Then
											   data.Text = text.ShortShow(maxTextLength)
											   Exit For
										   End If
									   Next
								   Else
									   For Each field In textFields
										   text = dic.Where(Function(x) x.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString

										   ' 包含关键词
										   If text.NotEmpty AndAlso text.Contains(keyword, StringComparison.OrdinalIgnoreCase) Then
											   text = text.ShortShow(maxTextLength)
											   Exit For
										   End If
									   Next
								   End If

								   ' 如果获取不到值，则直接返回最后文本，标识
								   data.Text = data.Text.EmptyValue(text, data.Value)

								   ' 二次处理值
								   Return If(listUpdate?.Invoke(item, data), data)
							   End Function).
						Where(Function(x) x.Value IsNot Nothing).
						ToList
		End Function

		''' <summary>将查询结果返回为树形数据列表</summary>
		''' <param name="parentId">默认顶级项目标识</param>
		''' <param name="nameFields">用于文本结果输出的字段，多个逗号间隔</param>
		''' <param name="disabledField">结果是否禁用状态的字段</param>
		''' <param name="extField">扩展内容字段</param>
		''' <param name="treeUpdate">自定义结果输出操作</param>
		''' <param name="maxTextLength">文本字段最多输出字符数，默认 100</param>
		<Extension>
		Public Function ToDataTree(Of T As {IEntityParent, Class})(this As IEnumerable(Of T),
																   Optional parentId As Long? = Nothing,
																   Optional treeUpdate As Func(Of T, DataTree, DataTree) = Nothing,
																   Optional nameFields As String = Nothing,
																   Optional disabledField As String = Nothing,
																   Optional extField As String = Nothing,
																   Optional maxTextLength As Integer = 100) As List(Of DataTree)
			If this.IsEmpty Then Return Nothing

			' 获取当前上级的数据
			Dim datas As List(Of T)
			If parentId.IsEmpty Then
				datas = this.Where(Function(x) x.ParentId Is Nothing OrElse x.ParentId = 0).ToList
			Else
				datas = this.Where(Function(x) x.ParentId.HasValue AndAlso x.ParentId.Value = parentId.Value).ToList
			End If

			If datas.IsEmpty Then Return Nothing

			' 分析名称键
			Dim pros = GetType(T).GetAllProperties.ToDictionary(Function(x) x.Name, Function(x) x.PropertyType.IsString)
			Dim allFields = pros.Keys
			Dim strFields = pros.Where(Function(x) x.Value).Select(Function(x) x.Key).ToArray

			' 上级，只能默认值：ParentId
			Dim parentField = "ParentId"

			' 名称键
			Dim textFields = {nameFields, "title", "name", "id"}.Where(Function(x) strFields.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray

			' 禁用键
			disabledField = {disabledField, "disabled", "enabled"}.Where(Function(x) allFields.Contains(x, StringComparer.OrdinalIgnoreCase)).FirstOrDefault

			' 描述键
			extField = If(allFields.Contains(extField, StringComparer.OrdinalIgnoreCase), extField, "")

			' 是否禁用反向，有可能字段是允许，所以要反向
			Dim isRev = disabledField.NotEmpty AndAlso disabledField.StartsWith("en", StringComparison.OrdinalIgnoreCase)

			' 最多输出文本长度
			maxTextLength = maxTextLength.Range(1, 1000)

			' 要输出的字段
			Dim fields = {"ID", parentField, disabledField, extField}.Union(textFields).Where(Function(x) x.NotEmpty).ToArray

			' 查询结果
			Return datas.Select(Function(item)
									Dim dic = item.ToDictionary(False, fields)
									Dim data As New DataList(dic, Nothing, "ID", parentField, disabledField, extField)

									' 无效值，不再处理
									If data.Value Is Nothing Then Return Nothing

									' disabled 需要反转
									If isRev Then data.Disabled = Not data.Disabled

									' 从所有名称字段中分析值
									Dim text = ""
									For Each field In textFields
										text = dic.Where(Function(x) x.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).FirstOrDefault?.ToString
										If text.NotEmpty Then
											data.Text = text.ShortShow(maxTextLength)
											Exit For
										End If
									Next

									' 如果获取不到值，则直接返回最后文本，标识
									If data.Text.IsEmpty Then data.Text = data.Value

									' 创建树形数据
									Dim tree As New DataTree(data)

									' 查询下级数据，标识为空则不再查询
									tree.Children = this.ToDataTree(tree.Value, treeUpdate, nameFields, disabledField, extField, maxTextLength)

									' 二次处理值
									tree = If(treeUpdate?.Invoke(item, tree), tree)

									Return tree
								End Function).
						Where(Function(x) x.Value IsNot Nothing).
						ToList
		End Function
	End Module

End Namespace