# DaLi.Utils.Flow

一个轻量级的工作流引擎，用于构建和执行自动化流程。

## 功能特性

- 基于规则的流程执行
- 灵活的参数配置
- 支持异步操作
- 内置多种常用规则
- 可扩展的规则系统

## 快速开始

### 安装

```bash
dotnet add package DaLi.Utils.Flow
```

### 基本使用

```csharp
// 创建一个简单的控制台输出规则
var consoleRule = new DaLi.Utils.Flow.Rules.Console
{
    Content = "Hello, DaLi Flow!",
    NewLine = true
};

// 执行规则
var context = new Dictionary<string, object>();
var result = await consoleRule.ExecuteAsync(context);
```

## 核心组件

### FlowRuleBase

所有规则的基类，提供了规则执行的基本框架。

### 内置规则

- Console：控制台输出规则
- 更多规则正在开发中...

## 扩展开发

您可以通过继承 `FlowRuleBase` 类来创建自定义规则：

```csharp
public class CustomRule : FlowRuleBase
{
    public override string Name => "自定义规则";

    protected override IDictionary<string, object> Execute(
        IDictionary<string, object> context,
        ExecuteStatus status,
        CancellationToken cancel)
    {
        // 实现您的规则逻辑
        return new Dictionary<string, object>();
    }
}
```

## 许可证

本项目基于 Mulan PSL v2 许可证开源。

## 贡献

欢迎提交问题和建议，一起改进这个项目！

## 关于

- 作者：木炭(WOODCOAL)
- 邮箱：i@woodcoal.cn
- 主页：http://www.hunandali.com/