namespace FunAiGateway.Models
{
    // 渠道协议类型
    public enum ChannelType
    {
        OpenAI,      // OpenAI兼容协议
        Anthropic    // Anthropic协议
    }

    // 渠道配置
    public class ChannelConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public ChannelType Type { get; set; } = ChannelType.OpenAI;
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public List<ModelConfig> Models { get; set; } = new();
        public string CustomHeaders { get; set; } = ""; // 自定义请求头 JSON格式
        public int Timeout { get; set; } = 300; // 超时秒数
        public int RetryCount { get; set; } = 0;
        // 代理设置（用于访问国外模型）
        public bool ProxyEnabled { get; set; } = false;        // 是否启用代理
        public string ProxyType { get; set; } = "HTTP";        // 代理方法/协议 HTTP / SOCKS5
        public string ProxyHost { get; set; } = "";            // 代理IP:端口 如 127.0.0.1:7890
        public string ProxyUsername { get; set; } = "";        // 代理用户名（可选）
        public string ProxyPassword { get; set; } = "";        // 代理密码（可选）
    }

    // 模型配置
    public class ModelConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ModelName { get; set; } = "";        // 对外暴露的模型名
        public string RealModelName { get; set; } = "";     // 实际请求上游的模型名
        public int ContextLength { get; set; } = 4096;      // 上下文长度
        public int MaxOutputTokens { get; set; } = 4096;    // 最大输出token
        public double InputPrice { get; set; } = 0;         // 输入价格 每百万token
        public double OutputPrice { get; set; } = 0;        // 输出价格 每百万token
        public bool Enabled { get; set; } = true;
        public bool SupportStream { get; set; } = true;     // 是否支持流式
        public bool SupportVision { get; set; } = false;    // 是否支持视觉
        public bool SupportFunctionCalling { get; set; } = true; // 是否支持函数调用
    }

    // 请求日志
    public class RequestLog
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string ChannelName { get; set; } = "";
        public string ModelName { get; set; } = "";
        public string RequestProtocol { get; set; } = "";   // 请求来源协议
        public string TargetProtocol { get; set; } = "";    // 转发目标协议
        public int InputTokens { get; set; } = 0;
        public int OutputTokens { get; set; } = 0;
        public bool IsStream { get; set; } = false;
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public long DurationMs { get; set; } = 0;
    }

    // 监听模式
    public enum ListenMode
    {
        Local,      // 本地 127.0.0.1
        Broadcast   // 广播 0.0.0.0
    }

    // 应用配置
    public class AppConfig
    {
        public int ListenPort { get; set; } = 80;
        public ListenMode ListenMode { get; set; } = ListenMode.Local;
        public string CustomHost { get; set; } = "";         // 自定义域名/IP，留空则自动检测
        public string ApiKey { get; set; } = "";             // 网关自身的API Key
        public bool RequireApiKey { get; set; } = true;      // 默认需要验证API Key
        public string DefaultModel { get; set; } = "";       // system_model 默认路由到的模型名
        public List<ChannelConfig> Channels { get; set; } = new();
        public int LogRetentionDays { get; set; } = 7;
        public int MaxLogCount { get; set; } = 500; // 日志最大保留条数，超过自动删除最早记录
        public bool AutoStartOnLaunch { get; set; } = false; // 启动软件时自动启动中转服务
    }
}
