# FunAiGateway

一个轻量级的 AI API 中转网关，支持 OpenAI 和 Anthropic 协议互转，让你用一个统一接口访问不同的大模型服务商。

## 预览截图

![预览1](pic/1.png)
![预览2](pic/2.png)
![预览3](pic/3.png)

## 功能特性

### 协议转换
- **OpenAI ↔ Anthropic 双向转换**：客户端用 OpenAI 格式请求，网关自动转换为 Anthropic 格式调用上游，反之亦然
- 流式（SSE）和非流式响应均支持
- 支持视觉、函数调用等高级特性

### 渠道管理
- 添加/编辑/删除多个渠道
- 每个渠道可配置独立的模型、超时、重试次数
- 支持自定义请求头
- 支持模型名称映射（对外名称 ≠ 实际模型名）

### 代理支持
- 每个渠道可独立配置 HTTP / SOCKS5 代理
- 支持 HTTP 和 SOCKS5（含用户名/密码认证）
- 未启用代理时走直连，不使用系统代理

### 请求路由
- `system_model` 虚拟模型：请求可统一发到 `system_model`，在界面上随时切换它指向的真实渠道
- 自动匹配启用的渠道和模型

### 监听模式
- **本地模式**：仅监听 `127.0.0.1`
- **广播模式**：监听 `0.0.0.0`，支持外网访问

### 日志系统
- 请求日志记录：时间、渠道、模型、协议、Token 用量、耗时、状态
- 日志按天分割，单文件超过 50MB 自动滚动
- 启动时不回显历史日志，仅显示当前会话记录

### 其他
- 启动时自动启动服务（可选）
- API Key 验证保护
- 连接信息自动生成（OpenAI / Anthropic / 模型列表接口地址）

## 快速开始

### 环境要求
- .NET 8.0 SDK（开发/编译）
- Windows 操作系统


### 使用方法

1. 启动 `FunAiGateway.exe`
2. 在 **设置** 页配置监听端口、API Key、默认模型
3. 在 **渠道** 页添加 AI 服务商渠道（填入 Base URL 和 API Key）
4. 点击 **启动服务**
5. 客户端使用生成的接口地址发请求

### 接口地址

| 接口 | 地址 |
|------|------|
| OpenAI Chat | `http://<host>:<port>/v1/chat/completions` |
| OpenAI Completions | `http://<host>:<port>/v1/completions` |
| Anthropic Messages | `http://<host>:<port>/v1/messages` |
| 模型列表 | `http://<host>:<port>/v1/models` |

## 客户端配置示例

### OpenAI 客户端
```python
from openai import OpenAI

client = OpenAI(
    base_url="http://127.0.0.1:80/v1",
    api_key="your-gateway-key"
)

response = client.chat.completions.create(
    model="system_model",
    messages=[{"role": "user", "content": "Hello!"}]
)
```

### Anthropic 客户端
```python
import anthropic

client = anthropic.Anthropic(
    base_url="http://127.0.0.1:80",
    api_key="your-gateway-key"
)

message = client.messages.create(
    model="system_model",
    max_tokens=1024,
    messages=[{"role": "user", "content": "Hello!"}]
)
```

## 文件结构

```
FunAiGateway/
├── config.json          # 配置文件（自动生成）
├── logs/                # 请求日志（按天分割）
│   └── requests_YYYY-MM-DD.log
├── MainForm.cs          # 主窗体逻辑
├── MainForm.Designer.cs # 主窗体界面设计
├── Models/
│   └── Models.cs        # 数据模型定义
├── Services/
│   ├── ConfigService.cs # 配置与日志服务
│   ├── ProxyServer.cs   # HTTP 代理服务器核心
│   └── ProtocolConverter.cs # 协议转换器
├── Forms/
│   └── ChannelEditDialog.cs # 渠道编辑对话框
└── publish.bat          # 发布编译脚本
```

## License

GPL-3.0
