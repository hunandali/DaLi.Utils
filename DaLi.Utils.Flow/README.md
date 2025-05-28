# DaLi.Utils.Flow

## 项目概述

`DaLi.Utils.Flow` 是大沥网络科技有限公司开发的一款自动流程处理框架核心模块。它旨在提供一个灵活、可扩展的机制，用于定义、执行和管理各种业务流程。通过模块化的设计，该框架能够轻松集成不同的规则和操作，实现高度自动化的任务处理。

## 核心规则

当前模块默认包含以下基础规则：

-   **控制流规则**: `IfDebug`, `IfEmpty`, `IfInclude`, `IfValue`, `WhileBreak`, `WhileInterval`, `WhileObject`, `WhileTimes` 等，用于实现条件判断和循环控制。
-   **数据处理规则**: `Parameter`, `Parameters`, `JsonObject`, `JsonString`, `TextArray`, `TextReplace`, `TextTable`, `Math`, `Random`, `TimeNow` 等，用于数据的获取、转换和计算。
-   **系统操作规则**: `Console`, `Debug`, `Comment`, `Sleep`, `WatchTime`, `Exception` 等，用于日志输出、调试、注释、暂停和异常处理。
-   **验证规则**: `ValidateDate`, `ValidateDebug`, `ValidateEmpty`, `ValidateInclude`, `ValidateTime`, `ValidateValue` 等，用于各种数据格式和内容的验证。
-   **文件和网络规则**: `File`, `HttpDownload`, `HttpRequest` 等，用于文件操作和 HTTP 请求。

## 扩展性

框架设计充分考虑了扩展性，用户可以通过以下方式自定义和扩展功能：

-   **新增规则**: 继承 `RuleBase` 或其派生类，实现 `Execute` 方法，即可创建新的业务规则。
-   **自定义辅助类**: 在 `Helpers` 目录中添加新的辅助类，提供特定的数据处理或流程控制逻辑。
-   **扩展接口**: 根据需要定义新的接口，并在规则或流程中实现，以满足更复杂的业务场景。

## 许可证

本项目遵循 Mulan PSL v2 许可证。详情请参阅 `LICENSE` 文件或访问 [http://license.coscl.org.cn/MulanPSL2](http://license.coscl.org.cn/MulanPSL2)。

---

_Copyright © 2021 湖南大沥网络科技有限公司. All Rights Reserved._
