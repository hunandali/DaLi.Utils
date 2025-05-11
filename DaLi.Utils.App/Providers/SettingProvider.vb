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
' 	参数管理
'
' 	name: Provider.SettingProvider
' 	create: 2023-02-14
' 	memo: 参数管理
'
' ------------------------------------------------------------

Imports System.Collections.Immutable

Namespace Provider
	''' <summary>参数管理</summary>
	Public Class SettingProvider
		Implements ISettingProvider

		''' <summary>参数值集合</summary>
		Private _Instance As ImmutableDictionary(Of Type, ISetting)

		''' <summary>需要隐藏的设置名称</summary>
		Private _Hidden As String()

		''' <summary>锁定对象</summary>
		Private ReadOnly _Lock As New Object

		''' <summary>参数值集合</summary>
		Public Property Instance As ImmutableDictionary(Of Type, ISetting)
			Get
				If _Instance Is Nothing Then _Instance = ImmutableDictionary.Create(Of Type, ISetting)
				Return _Instance
			End Get
			Protected Set(value As ImmutableDictionary(Of Type, ISetting))
				_Instance = value
			End Set
		End Property

		''' <summary>隐藏后台参数设置，可以使用通配符 *，匹配 ModuleName</summary>
		Public Sub Hidden(ParamArray moduleName() As String) Implements ISettingProvider.Hidden
			_Hidden = moduleName
		End Sub

		''' <summary>所有缓存的模块名称，模块名称为类型的简化名称</summary>
		Public ReadOnly Property Modules As NameValueDictionary Implements ISettingProvider.Modules
			Get
				Dim keys = Instance.Keys
				If _Hidden.NotEmpty Then keys = keys.Where(Function(x) Not x.FullName.Like(_Hidden))
				Return keys.ToNameValueDictionary(Function(x) x.FullName, Function(x) CommonHelper.UpdateName(x.FullName, "setting"))
			End Get
		End Property

		''' <summary>获取配置</summary>
		Public Function GetSetting(Of T As ISetting)() As T Implements ISettingProvider.GetSetting
			Dim type = GetType(T)
			Dim setting As ISetting = Nothing
			If Instance.TryGetValue(type, setting) Then
				Return setting
			Else
				' 如果分析不到则从基类获取
				Return Instance.Where(Function(x) x.Key.IsComeFrom(type)).Select(Function(x) x.Value).FirstOrDefault
			End If
		End Function

		''' <summary>加载并更新本地参数</summary>
		Public Sub Load(setting As ISetting)
			' 对象不存在
			If setting Is Nothing Then Return

			' 加载参数
			setting.Load(Me)

			' 不注入，直接退出
			If Not setting.Inject Then Return

			' 记录参数
			SyncLock _Lock
				Dim type = setting.GetType
				If Instance.ContainsKey(type) Then
					Instance = Instance.SetItem(type, setting)
				Else
					Instance = Instance.Add(type, setting)
				End If
			End SyncLock
		End Sub

		''' <summary>加载并更新本地参数</summary>
		Public Sub Load(settings As IEnumerable(Of ISetting)) Implements ISettingProvider.Load
			If settings?.Count > 0 Then
				For Each S In settings
					Load(S)
				Next
			End If
		End Sub

		''' <summary>重载指定类型，需要开启自动加载的项目才允许重载</summary>
		Public Sub Reload(typeName As String) Implements ISettingProvider.Reload
			Dim setting = _Instance.
				Where(Function(x) CommonHelper.UpdateName(x.Key.FullName, "setting").Equals(typeName, StringComparison.OrdinalIgnoreCase)).
				Select(Function(x) x.Value).
				FirstOrDefault

			If setting?.AutoUpdate Then Load(setting)
		End Sub
	End Class

End Namespace

