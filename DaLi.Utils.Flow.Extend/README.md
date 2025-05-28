# DaLi.Utils.Flow.Extend

## 项目概述

`DaLi.Utils.Flow.Extend` 是 `DaLi.Utils.Flow` 自动流程框架的扩展包，旨在提供更丰富、更强大的规则和功能，以满足复杂业务场景的需求。它通过引入外部库和高级处理逻辑，扩展了核心框架的能力，使得流程自动化能够处理更多样化的数据格式和交互方式。

## 扩展规则

-   `Html2Markdown`: 用于将 HTML 内容转换为 Markdown 格式的规则，方便在不同文本环境中使用。
-   `HtmlParser`: 提供强大的 HTML 解析能力，能够从复杂的 HTML 结构中提取指定数据。
-   `JavaScript`: 允许在流程中执行 JavaScript 脚本，支持引入 `dayjs` 和 `lodash` 等常用库，极大地增强了流程的灵活性和可编程性。
-   `WebHook`: 用于发送 WebHook 通知，支持多种内容格式（文本、Markdown、JSON），实现流程与外部系统的集成。

## 扩展性

`DaLi.Utils.Flow.Extend` 遵循 `DaLi.Utils.Flow.Core` 的设计原则，具备良好的扩展性。开发者可以通过以下方式进行扩展：

-   **新增规则**：通过继承 `RuleBase` 类，可以轻松添加自定义规则，以处理特定的业务逻辑或数据类型。
-   **集成外部库**：项目结构允许方便地引入其他第三方库，以扩展功能，例如当前已集成了 `AngleSharp` 用于 HTML 解析，`Jint` 用于 JavaScript 执行，`ReverseMarkdown` 用于 Markdown 转换。

## 依赖

-   `DaLi.Utils.Flow.Core`: 本扩展包的核心依赖，提供了基础的流程框架和规则执行机制。
-   `AngleSharp`: 用于 HTML 解析。
-   `Jint`: 用于 JavaScript 脚本的执行。
-   `LiteDB`: (如果项目中使用) 轻量级 NoSQL 数据库，用于数据存储。
-   `ReverseMarkdown`: 用于 HTML 到 Markdown 的转换。

## 许可证

本项目遵循 Mulan PSL v2 许可证。详情请参阅 `LICENSE` 文件或访问 [http://license.coscl.org.cn/MulanPSL2](http://license.coscl.org.cn/MulanPSL2)。

---

_Copyright © 2021 湖南大沥网络科技有限公司. All Rights Reserved._
