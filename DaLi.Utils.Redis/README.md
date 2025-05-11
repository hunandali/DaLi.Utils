# DaLi.Utils.Redis

基于 FreeRedis 的 Redis 工具库,提供了更易用的 Redis 数据操作接口。

## 功能特性

- 支持 Redis JSON 模块,提供对象化的 JSON 数据操作
- 支持 Redis Hash 表操作
- 提供基于 Redis 的索引功能
  - 支持标签索引
  - 支持字典数据索引
- 支持状态缓存操作
- 支持大小写敏感/不敏感设置

## 依赖要求

- .NET 8.0+
- Redis 服务器需安装 RedisJSON 模块

## 快速开始

1. 安装 NuGet 包:

```bash
dotnet add package DaLi.Utils.Redis
```

2. 基本用法示例:

```vb
' 创建 Redis 客户端
Dim client As New RedisClient("localhost:6379")

' JSON 操作
Dim json As New RedisJson(client, "test-json")
json.Set("$.name", "test")
json.Get(Of String)("$.name") ' 返回 "test"

' Hash 表操作  
Dim hash As New RedisHash(Of String)(client, "test-hash") 
hash("key") = "value"
hash.Get("key") ' 返回 "value"

' 标签索引
Class TagIndex 
    Inherits TagIndexBase(Of Entity, Integer)
    
    Protected Overrides Function GetID(entity As Entity) As Integer
        Return entity.Id
    End Function
    
    Protected Overrides Function GetTags(entity As Entity) As String()
        Return entity.Tags
    End Function
End Class

' 状态缓存
Dim cache As New StatusCache(Of String)(client, "test-status")
cache.SET("key", "value")
```

## 主要类说明

- `RedisJson` - Redis JSON 数据操作
- `RedisHash` - Redis Hash 表操作 
- `IndexBase` - 索引基类
- `TagIndexBase` - 标签索引基类
- `DictionaryIndexBase` - 字典数据索引基类
- `StatusCache` - 状态缓存操作

## 许可证

本项目基于 [Mulan PSL v2](http://license.coscl.org.cn/MulanPSL2) 开源协议。

## 关于

由湖南大沥网络科技有限公司开发和维护。

- 作者: 木炭(WOODCOAL)
- 邮箱: i@woodcoal.cn
- 主页: http://www.hunandali.com/