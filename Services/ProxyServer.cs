using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FunAiGateway.Models;

namespace FunAiGateway.Services
{
    public class ProxyServer
    {
        private HttpListener? _listener;
        private readonly ConfigService _configService;
        private CancellationTokenSource? _cts;
        private readonly HttpClient _httpClient;
        // 按代理设置缓存 HttpClient，避免重复创建
        private readonly ConcurrentDictionary<string, HttpClient> _proxyClients = new();

        public event Action<RequestLog>? OnRequestLogged;
        public event Action<string>? OnLog;
        public bool IsRunning { get; private set; }

        public ProxyServer(ConfigService configService)
        {
            _configService = configService;
            _httpClient = new HttpClient(new SocketsHttpHandler
            {
                // 不勾选代理时走直连，不使用系统代理
                UseProxy = false,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 100
            })
            {
                // 超时由 CancellationToken 控制，HttpClient 自身不设超时限制
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
        }

        // 根据渠道代理设置获取 HttpClient，未启用代理时返回共享实例
        private HttpClient GetHttpClient(ChannelConfig channel)
        {
            // 未启用代理或未填写代理地址时，使用默认 HttpClient
            if (!channel.ProxyEnabled || string.IsNullOrWhiteSpace(channel.ProxyHost))
                return _httpClient;

            var key = $"{channel.ProxyType}|{channel.ProxyHost}|{channel.ProxyUsername}|{channel.ProxyPassword}";
            return _proxyClients.GetOrAdd(key, _ => CreateProxyClient(channel));
        }

        // 创建带渠道超时的 CancellationToken，避免修改共享 HttpClient 的 Timeout 属性
        private CancellationTokenSource CreateTimeoutCts(CancellationToken ct, int timeoutSeconds)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return cts;
        }

        // 创建带代理的 HttpClient
        private HttpClient CreateProxyClient(ChannelConfig channel)
        {
            // 解析代理地址 host:port
            var (host, port) = ParseHostPort(channel.ProxyHost, channel.ProxyType == "SOCKS5" ? 1080 : 7890);

            SocketsHttpHandler handler;
            if (channel.ProxyType.Equals("SOCKS5", StringComparison.OrdinalIgnoreCase))
            {
                // SOCKS5 代理：通过自定义连接回调实现握手
                var pHost = host;
                var pPort = port;
                var pUser = channel.ProxyUsername;
                var pPass = channel.ProxyPassword;
                handler = new SocketsHttpHandler
                {
                    ConnectCallback = async (sctx, ct) =>
                    {
                        return await ConnectViaSocks5Async(pHost, pPort, sctx.DnsEndPoint.Host, sctx.DnsEndPoint.Port, pUser, pPass, ct);
                    },
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    MaxConnectionsPerServer = 100
                };
            }
            else
            {
                // HTTP 代理
                var proxyUrl = host.Contains("://") ? channel.ProxyHost : $"http://{channel.ProxyHost}";
                var proxy = new WebProxy(proxyUrl);
                if (!string.IsNullOrEmpty(channel.ProxyUsername) || !string.IsNullOrEmpty(channel.ProxyPassword))
                    proxy.Credentials = new NetworkCredential(channel.ProxyUsername, channel.ProxyPassword);

                handler = new SocketsHttpHandler
                {
                    UseProxy = true,
                    Proxy = proxy,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    MaxConnectionsPerServer = 100
                };
            }

            return new HttpClient(handler)
            {
                // 超时由 CancellationToken 控制，HttpClient 自身不设超时限制
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
        }

        // 解析 host:port 格式
        private static (string host, int port) ParseHostPort(string address, int defaultPort)
        {
            address = address.Trim();
            if (address.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                address = address[7..];
            else if (address.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                address = address[8..];

            var idx = address.LastIndexOf(':');
            if (idx > 0 && int.TryParse(address[(idx + 1)..], out var p))
                return (address[..idx].Trim(), p);
            return (address, defaultPort);
        }

        // 通过 SOCKS5 代理建立到目标服务器的连接
        private static async Task<Stream> ConnectViaSocks5Async(string proxyHost, int proxyPort, string targetHost, int targetPort, string username, string password, CancellationToken ct)
        {
            var tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(proxyHost, proxyPort, ct);
            }
            catch
            {
                tcp.Dispose();
                throw;
            }

            var stream = tcp.GetStream();

            // 1. 握手：协商认证方式
            bool hasAuth = !string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password);
            byte[] greeting = hasAuth ? new byte[] { 0x05, 0x02, 0x00, 0x02 } : new byte[] { 0x05, 0x01, 0x00 };
            await stream.WriteAsync(greeting, ct);
            await stream.FlushAsync(ct);

            byte[] resp = new byte[2];
            await ReadExactAsync(stream, resp, 2, ct);
            if (resp[0] != 0x05)
            {
                tcp.Dispose();
                throw new Exception($"SOCKS5 代理握手失败：无效的版本号 {resp[0]}");
            }
            byte method = resp[1];

            // 2. 用户名/密码认证
            if (method == 0x02)
            {
                var userBytes = System.Text.Encoding.UTF8.GetBytes(username ?? "");
                var passBytes = System.Text.Encoding.UTF8.GetBytes(password ?? "");
                var authReq = new byte[3 + userBytes.Length + passBytes.Length];
                authReq[0] = 0x01; // 子协议版本
                authReq[1] = (byte)userBytes.Length;
                Array.Copy(userBytes, 0, authReq, 2, userBytes.Length);
                authReq[2 + userBytes.Length] = (byte)passBytes.Length;
                Array.Copy(passBytes, 0, authReq, 3 + userBytes.Length, passBytes.Length);

                await stream.WriteAsync(authReq, ct);
                await stream.FlushAsync(ct);

                byte[] authResp = new byte[2];
                await ReadExactAsync(stream, authResp, 2, ct);
                if (authResp[1] != 0x00)
                {
                    tcp.Dispose();
                    throw new Exception("SOCKS5 代理认证失败：用户名或密码错误");
                }
            }
            else if (method != 0x00)
            {
                tcp.Dispose();
                throw new Exception($"SOCKS5 代理不支持此认证方式: {method}");
            }

            // 3. 发送连接请求
            using var ms = new System.IO.MemoryStream();
            ms.WriteByte(0x05); // SOCKS 版本
            ms.WriteByte(0x01); // CONNECT 命令
            ms.WriteByte(0x00); // 保留

            // 目标地址：优先按 IP 解析，否则按域名
            if (IPAddress.TryParse(targetHost, out var ip))
            {
                var bytes = ip.GetAddressBytes();
                ms.WriteByte(ip.AddressFamily == AddressFamily.InterNetworkV6 ? (byte)0x04 : (byte)0x01);
                ms.Write(bytes, 0, bytes.Length);
            }
            else
            {
                var domainBytes = System.Text.Encoding.UTF8.GetBytes(targetHost);
                ms.WriteByte(0x03); // 域名类型
                ms.WriteByte((byte)domainBytes.Length);
                ms.Write(domainBytes, 0, domainBytes.Length);
            }

            // 目标端口（网络字节序）
            ms.WriteByte((byte)(targetPort >> 8));
            ms.WriteByte((byte)(targetPort & 0xFF));

            var connectReq = ms.ToArray();
            await stream.WriteAsync(connectReq, ct);
            await stream.FlushAsync(ct);

            // 4. 读取连接响应
            byte[] connectRespHeader = new byte[4];
            await ReadExactAsync(stream, connectRespHeader, 4, ct);
            if (connectRespHeader[0] != 0x05)
            {
                tcp.Dispose();
                throw new Exception("SOCKS5 代理响应无效");
            }
            if (connectRespHeader[1] != 0x00)
            {
                tcp.Dispose();
                throw new Exception($"SOCKS5 代理连接失败，错误码: {connectRespHeader[1]}");
            }

            // 跳过绑定地址和端口
            byte addrType = connectRespHeader[3];
            int skipLen = addrType switch
            {
                0x01 => 4,  // IPv4
                0x04 => 16, // IPv6
                _ => 0      // 域名：需先读取长度字节
            };
            if (addrType == 0x03)
            {
                byte[] lenBuf = new byte[1];
                await ReadExactAsync(stream, lenBuf, 1, ct);
                skipLen = lenBuf[0];
            }
            byte[] skip = new byte[skipLen + 2]; // 地址 + 端口
            await ReadExactAsync(stream, skip, skip.Length, ct);

            return stream;
        }

        // 从流中精确读取指定字节数
        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int count, CancellationToken ct)
        {
            int total = 0;
            while (total < count)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(total, count - total), ct);
                if (read == 0) throw new Exception("代理连接已断开");
                total += read;
            }
        }

        public void Start(int port, ListenMode listenMode = ListenMode.Local)
        {
            if (IsRunning) Stop();

            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            var prefix = listenMode == ListenMode.Broadcast
                ? $"http://+:{port}/"
                : $"http://127.0.0.1:{port}/";
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            IsRunning = true;

            _ = Task.Run(() => ListenAsync(_cts.Token));
            Log($"代理服务器已启动，监听端口: {port}");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            IsRunning = false;
            Log("代理服务器已停止");
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener!.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context, ct), ct);
                }
                catch (HttpListenerException) when (ct.IsCancellationRequested) { break; }
                catch (ObjectDisposedException) when (ct.IsCancellationRequested) { break; }
                catch (Exception ex) { Log($"监听异常: {ex.Message}"); }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;

            var requestLog = new RequestLog
            {
                Time = DateTime.Now,
                RequestProtocol = "Unknown"
            };

            try
            {
                // CORS预检
                if (request.HttpMethod == "OPTIONS")
                {
                    SetCorsHeaders(response);
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                SetCorsHeaders(response);

                var path = request.Url?.AbsolutePath ?? "";
                var body = await ReadRequestBodyAsync(request);

                // 路由分发
                if (path.StartsWith("/v1/chat/completions") || path.StartsWith("/v1/completions"))
                {
                    requestLog.RequestProtocol = "OpenAI";
                    await HandleOpenAIChatAsync(context, body, requestLog, ct);
                }
                else if (path.StartsWith("/v1/messages"))
                {
                    requestLog.RequestProtocol = "Anthropic";
                    await HandleAnthropicMessagesAsync(context, body, requestLog, ct);
                }
                else if (path.StartsWith("/v1/models"))
                {
                    await HandleModelsAsync(context);
                }
                else
                {
                    response.StatusCode = 404;
                    await WriteJsonAsync(response, new { error = new { message = "Not Found", type = "invalid_request_error" } });
                }

                sw.Stop();
                requestLog.DurationMs = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                sw.Stop();
                requestLog.Success = false;
                requestLog.ErrorMessage = ex.Message;
                requestLog.DurationMs = sw.ElapsedMilliseconds;

                try
                {
                    response.StatusCode = 500;
                    await WriteJsonAsync(response, new { error = new { message = ex.Message, type = "internal_error" } });
                }
                catch { }
            }
            finally
            {
                // 未知协议（404等）不记录请求次数和日志
                if (requestLog.RequestProtocol != "Unknown")
                {
                    OnRequestLogged?.Invoke(requestLog);
                }
                try { response.Close(); } catch { }
            }
        }

        // 处理OpenAI协议请求
        private async Task HandleOpenAIChatAsync(HttpListenerContext context, string body, RequestLog requestLog, CancellationToken ct)
        {
            var response = context.Response;
            var reqBody = JObject.Parse(body);
            var modelName = reqBody["model"]?.ToString() ?? "";
            var isStream = reqBody["stream"]?.Value<bool>() ?? false;

            requestLog.ModelName = modelName;
            requestLog.IsStream = isStream;

            // system_model 路由到默认模型
            if (modelName.Equals("system_model", StringComparison.OrdinalIgnoreCase))
            {
                var defaultModel = _configService.Config.DefaultModel;
                if (string.IsNullOrEmpty(defaultModel))
                {
                    var available = _configService.GetAllModelNames().FirstOrDefault();
                    if (string.IsNullOrEmpty(available))
                    {
                        response.StatusCode = 404;
                        await WriteJsonAsync(response, new { error = new { message = "No available model. Please add channels first.", type = "invalid_request_error" } });
                        requestLog.Success = false;
                        requestLog.ErrorMessage = "system_model: no available model";
               return;
                    }
                    defaultModel = available;
                }
                reqBody["model"] = defaultModel;
                modelName = defaultModel;
                requestLog.ModelName = $"system_model→{defaultModel}";
            }

            // API Key验证
            if (_configService.Config.RequireApiKey)
            {
                var authHeader = context.Request.Headers["Authorization"]?.Replace("Bearer ", "");
                if (authHeader != _configService.Config.ApiKey)
                {
                    response.StatusCode = 401;
                    await WriteJsonAsync(response, new { error = new { message = "Invalid API Key", type = "auth_error" } });
                    requestLog.Success = false;
                    requestLog.ErrorMessage = "Invalid API Key";
                    return;
                }
            }

            // 查找渠道
            var found = _configService.FindChannelForModel(modelName);
            if (found == null)
            {
                response.StatusCode = 404;
                await WriteJsonAsync(response, new { error = new { message = $"Model '{modelName}' not found", type = "invalid_request_error" } });
                requestLog.Success = false;
                requestLog.ErrorMessage = $"Model '{modelName}' not found";
                return;
            }

            var (channel, model) = found.Value;
            requestLog.ChannelName = channel!.Name;
            Log($"渠道: {channel.Name} | 模型: {model!.RealModelName ?? model.ModelName} | {context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}");

            // 替换实际模型名
            if (!string.IsNullOrEmpty(model!.RealModelName))
                reqBody["model"] = model.RealModelName;

            if (channel!.Type == ChannelType.OpenAI)
            {
                // OpenAI -> OpenAI 直接转发
                requestLog.TargetProtocol = "OpenAI";
                await ForwardOpenAIAsync(context, channel, reqBody, isStream, requestLog, ct);
            }
            else
            {
                // OpenAI -> Anthropic 协议转换
                requestLog.TargetProtocol = "Anthropic";
                await ForwardOpenAIToAnthropicAsync(context, channel, reqBody, isStream, requestLog, ct);
            }
        }

        // 处理Anthropic协议请求
        private async Task HandleAnthropicMessagesAsync(HttpListenerContext context, string body, RequestLog requestLog, CancellationToken ct)
        {
            var response = context.Response;
            var reqBody = JObject.Parse(body);
            var modelName = reqBody["model"]?.ToString() ?? "";
            var isStream = reqBody["stream"]?.Value<bool>() ?? false;

            requestLog.ModelName = modelName;
            requestLog.IsStream = isStream;

            // system_model 路由到默认模型
            if (modelName.Equals("system_model", StringComparison.OrdinalIgnoreCase))
            {
                var defaultModel = _configService.Config.DefaultModel;
                if (string.IsNullOrEmpty(defaultModel))
                {
                    var available = _configService.GetAllModelNames().FirstOrDefault();
                    if (string.IsNullOrEmpty(available))
                    {
                        response.StatusCode = 404;
                        await WriteJsonAsync(response, new { type = "error", error = new { type = "not_found_error", message = "No available model" } });
                        requestLog.Success = false;
                        requestLog.ErrorMessage = "system_model: no available model";
                        return;
                    }
                    defaultModel = available;
                }
                reqBody["model"] = defaultModel;
                modelName = defaultModel;
                requestLog.ModelName = $"system_model→{defaultModel}";
            }

            // API Key验证 (Anthropic用x-api-key头)
            if (_configService.Config.RequireApiKey)
            {
                var apiKey = context.Request.Headers["x-api-key"];
                var authHeader = context.Request.Headers["Authorization"]?.Replace("Bearer ", "");
                var providedKey = apiKey ?? authHeader;
                if (providedKey != _configService.Config.ApiKey)
                {
                    response.StatusCode = 401;
                    await WriteJsonAsync(response, new { error = new { message = "Invalid API Key", type = "auth_error" } });
                    requestLog.Success = false;
                    requestLog.ErrorMessage = "Invalid API Key";
                    return;
                }
            }

            var found = _configService.FindChannelForModel(modelName);
            if (found == null)
            {
                response.StatusCode = 404;
                await WriteJsonAsync(response, new { type = "error", error = new { type = "not_found_error", message = $"Model '{modelName}' not found" } });
                requestLog.Success = false;
                requestLog.ErrorMessage = $"Model '{modelName}' not found";
                return;
            }

            var (channel, model) = found.Value;
            requestLog.ChannelName = channel!.Name;
            Log($"渠道: {channel.Name} | 模型: {model!.RealModelName ?? model.ModelName} | {context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}");

            if (!string.IsNullOrEmpty(model!.RealModelName))
                reqBody["model"] = model.RealModelName;

            if (channel!.Type == ChannelType.Anthropic)
            {
                // Anthropic -> Anthropic 直接转发
                requestLog.TargetProtocol = "Anthropic";
                await ForwardAnthropicAsync(context, channel, reqBody, isStream, requestLog, ct);
            }
            else
            {
                // Anthropic -> OpenAI 协议转换
                requestLog.TargetProtocol = "OpenAI";
                await ForwardAnthropicToOpenAIAsync(context, channel, reqBody, isStream, requestLog, ct);
            }
        }

        // OpenAI -> OpenAI 直接转发
        private async Task ForwardOpenAIAsync(HttpListenerContext context, ChannelConfig channel, JObject reqBody, bool isStream, RequestLog requestLog, CancellationToken ct)
        {
            var url = $"{channel.BaseUrl.TrimEnd('/')}/chat/completions";
            await ForwardRequestAsync(context, url, channel, reqBody, isStream, requestLog, ct, null);
        }

        // OpenAI -> Anthropic 转换转发
        private async Task ForwardOpenAIToAnthropicAsync(HttpListenerContext context, ChannelConfig channel, JObject openaiReq, bool isStream, RequestLog requestLog, CancellationToken ct)
        {
            var (anthropicReq, _) = ProtocolConverter.OpenAIToAnthropic(openaiReq);
            var url = $"{channel.BaseUrl.TrimEnd('/')}/messages";

            if (isStream)
            {
                await ForwardAnthropicStreamAsOpenAIAsync(context, url, channel, anthropicReq, openaiReq["model"]?.ToString() ?? "", requestLog, ct);
            }
            else
            {
                var (respBody, statusCode) = await SendAnthropicRequestAsync(url, channel, anthropicReq, ct);
                // 记录上游响应内容（根据配置的状态码范围决定是否记录）
                LogResponseContent(channel, statusCode, openaiReq["model"]?.ToString() ?? "", respBody.ToString(Formatting.None));
                var openaiResp = ProtocolConverter.AnthropicToOpenAIResponse(respBody, openaiReq["model"]?.ToString() ?? "");
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await WriteJsonAsync(context.Response, openaiResp);

                requestLog.InputTokens = respBody["usage"]?["input_tokens"]?.Value<int>() ?? 0;
                requestLog.OutputTokens = respBody["usage"]?["output_tokens"]?.Value<int>() ?? 0;
            }
        }

        // Anthropic -> Anthropic 直接转发
        private async Task ForwardAnthropicAsync(HttpListenerContext context, ChannelConfig channel, JObject reqBody, bool isStream, RequestLog requestLog, CancellationToken ct)
        {
            var url = $"{channel.BaseUrl.TrimEnd('/')}/messages";
            await ForwardAnthropicRequestAsync(context, url, channel, reqBody, isStream, requestLog, ct);
        }

        // Anthropic -> OpenAI 转换转发
        private async Task ForwardAnthropicToOpenAIAsync(HttpListenerContext context, ChannelConfig channel, JObject anthropicReq, bool isStream, RequestLog requestLog, CancellationToken ct)
        {
            // 将Anthropic请求转为OpenAI请求
            var openaiReq = AnthropicToOpenAIRequest(anthropicReq);
            var url = $"{channel.BaseUrl.TrimEnd('/')}/chat/completions";

            if (isStream)
            {
                await ForwardOpenAIStreamAsAnthropicAsync(context, url, channel, openaiReq, anthropicReq["model"]?.ToString() ?? "", requestLog, ct);
            }
            else
            {
                var (respBody, statusCode) = await SendOpenAIRequestAsync(url, channel, openaiReq, ct);
                // 记录上游响应内容（根据配置的状态码范围决定是否记录）
                LogResponseContent(channel, statusCode, anthropicReq["model"]?.ToString() ?? "", respBody.ToString(Formatting.None));
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await WriteJsonAsync(context.Response, respBody);

                requestLog.InputTokens = respBody["usage"]?["prompt_tokens"]?.Value<int>() ?? 0;
                requestLog.OutputTokens = respBody["usage"]?["completion_tokens"]?.Value<int>() ?? 0;
            }
        }

        // Anthropic请求格式转OpenAI请求格式
        private JObject AnthropicToOpenAIRequest(JObject anthropicReq)
        {
            var openaiReq = new JObject();

            if (anthropicReq["model"] != null) openaiReq["model"] = anthropicReq["model"];
            if (anthropicReq["max_tokens"] != null) openaiReq["max_tokens"] = anthropicReq["max_tokens"];
            if (anthropicReq["temperature"] != null) openaiReq["temperature"] = anthropicReq["temperature"];
            if (anthropicReq["top_p"] != null) openaiReq["top_p"] = anthropicReq["top_p"];
            if (anthropicReq["stream"] != null) openaiReq["stream"] = anthropicReq["stream"];
            if (anthropicReq["stop_sequences"] != null) openaiReq["stop"] = anthropicReq["stop_sequences"];

            // system -> messages
            var messages = new JArray();
            if (anthropicReq["system"] != null)
            {
                messages.Add(new JObject { ["role"] = "system", ["content"] = anthropicReq["system"]!.ToString() });
            }

            // messages转换
            var anthropicMessages = anthropicReq["messages"] as JArray ?? new JArray();
            foreach (var msg in anthropicMessages)
            {
                var role = msg["role"]?.ToString();
                var content = msg["content"];

                if (role == "user")
                {
                    // 检查是否包含tool_result
                    if (content is JArray arr)
                    {
                        var toolResults = arr.Where(b => b["type"]?.ToString() == "tool_result").ToList();
                        if (toolResults.Count > 0)
                        {
                            foreach (var tr in toolResults)
                            {
                                messages.Add(new JObject
                                {
                                    ["role"] = "tool",
                                    ["tool_call_id"] = tr["tool_use_id"]?.ToString() ?? "",
                                    ["content"] = tr["content"]?.ToString() ?? ""
                                });
                            }
                            continue;
                        }
                    }
                    messages.Add(new JObject { ["role"] = "user", ["content"] = ConvertAnthropicContent(content) });
                }
                else if (role == "assistant")
                {
                    if (content is JArray arr2)
                    {
                        var toolUses = arr2.Where(b => b["type"]?.ToString() == "tool_use").ToList();
                        var textParts = arr2.Where(b => b["type"]?.ToString() == "text").Select(b => b["text"]?.ToString() ?? "").ToList();

                        if (toolUses.Count > 0)
                        {
                            var msg2 = new JObject
                            {
                                ["role"] = "assistant",
                                ["content"] = string.Join("", textParts),
                                ["tool_calls"] = new JArray(toolUses.Select(tu => new JObject
                                {
                                    ["id"] = tu["id"]?.ToString() ?? "",
                                    ["type"] = "function",
                                    ["function"] = new JObject
                                    {
                                        ["name"] = tu["name"]?.ToString() ?? "",
                                        ["arguments"] = tu["input"]?.ToString(Formatting.None) ?? "{}"
                                    }
                                }))
                            };
                            messages.Add(msg2);
                            continue;
                        }
                    }
                    messages.Add(new JObject { ["role"] = "assistant", ["content"] = ConvertAnthropicContent(content) });
                }
            }

            openaiReq["messages"] = messages;

            // tools转换
            var anthropicTools = anthropicReq["tools"] as JArray;
            if (anthropicTools != null && anthropicTools.Count > 0)
            {
                openaiReq["tools"] = new JArray(anthropicTools.Select(t => new JObject
                {
                    ["type"] = "function",
                    ["function"] = new JObject
                    {
                        ["name"] = t["name"]?.ToString() ?? "",
                        ["description"] = t["description"]?.ToString() ?? "",
                        ["parameters"] = t["input_schema"] ?? new JObject { ["type"] = "object" }
                    }
                }));
            }

            return openaiReq;
        }

        private JToken ConvertAnthropicContent(JToken? content)
        {
            if (content is JArray arr)
            {
                var converted = new JArray();
                foreach (var item in arr)
                {
                    var type = item["type"]?.ToString();
                    if (type == "text")
                        converted.Add(new JObject { ["type"] = "text", ["text"] = item["text"]?.ToString() ?? "" });
                    else if (type == "image")
                    {
                        var source = item["source"] as JObject;
                        if (source?["type"]?.ToString() == "base64")
                        {
                            converted.Add(new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject
                                {
                                    ["url"] = $"data:{source["media_type"]};base64,{source["data"]}"
                                }
                            });
                        }
                        else if (source?["type"]?.ToString() == "url")
                        {
                            converted.Add(new JObject
                            {
                                ["type"] = "image_url",
                                ["image_url"] = new JObject { ["url"] = source["url"]?.ToString() ?? "" }
                            });
                        }
                    }
                }
                return converted;
            }
            return content ?? "";
        }

        // 通用请求转发（OpenAI格式）
        private async Task ForwardRequestAsync(HttpListenerContext context, string url, ChannelConfig channel, JObject reqBody, bool isStream, RequestLog requestLog, CancellationToken ct, string? contentType)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("Authorization", $"Bearer {channel.ApiKey}");
                ForwardUserAgent(req, context);
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(reqBody.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, isStream, timeoutCts.Token);

            if (!isStream)
            {
                var respBody = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(respBody), ct);

                // 记录上游响应内容（根据配置的状态码范围决定是否记录）
                LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", respBody);

                try
                {
                    var jResp = JObject.Parse(respBody);
                    requestLog.InputTokens = jResp["usage"]?["prompt_tokens"]?.Value<int>() ?? 0;
                    requestLog.OutputTokens = jResp["usage"]?["completion_tokens"]?.Value<int>() ?? 0;
                }
                catch { }
            }
            else
            {
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "text/event-stream";
                context.Response.Headers.Add("Cache-Control", "no-cache");
                context.Response.Headers.Add("Connection", "keep-alive");

                // 流式请求若上游返回非成功状态码，读取并记录错误响应内容
                if ((int)resp.StatusCode >= 400)
                {
                    var errBody = await TryReadStreamErrorAsync(resp, timeoutCts.Token);
                    LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", errBody);
                    // 已读取的错误响应也需要转发给客户端
                    context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes(errBody));
                    await context.Response.OutputStream.FlushAsync(ct);
                    return;
                }

                // 流式成功响应：收集内容用于日志记录
                var sbOpenAI = new System.Text.StringBuilder();
                using var stream = await resp.Content.ReadAsStreamAsync(timeoutCts.Token);
                await StreamAndExtractUsageAsync(stream, context.Response.OutputStream, requestLog, false, ct, sbOpenAI);
                LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", sbOpenAI.ToString());
            }
        }

        // Anthropic格式请求转发
        private async Task ForwardAnthropicRequestAsync(HttpListenerContext context, string url, ChannelConfig channel, JObject reqBody, bool isStream, RequestLog requestLog, CancellationToken ct)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("x-api-key", channel.ApiKey);
                req.Headers.Add("anthropic-version", "2023-06-01");
                ForwardUserAgent(req, context);
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(reqBody.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, isStream, timeoutCts.Token);

            if (!isStream)
            {
                var respBody = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(respBody), ct);

                // 记录上游响应内容（根据配置的状态码范围决定是否记录）
                LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", respBody);

                try
                {
                    var jResp = JObject.Parse(respBody);
                    requestLog.InputTokens = jResp["usage"]?["input_tokens"]?.Value<int>() ?? 0;
                    requestLog.OutputTokens = jResp["usage"]?["output_tokens"]?.Value<int>() ?? 0;
                }
                catch { }
            }
            else
            {
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "text/event-stream";
                context.Response.Headers.Add("Cache-Control", "no-cache");

                // 流式请求若上游返回非成功状态码，读取并记录错误响应内容
                if ((int)resp.StatusCode >= 400)
                {
                    var errBody = await TryReadStreamErrorAsync(resp, timeoutCts.Token);
                    LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", errBody);
                    context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes(errBody));
                    await context.Response.OutputStream.FlushAsync(ct);
                    return;
                }

                // 流式成功响应：收集内容用于日志记录
                var sbAnthropic = new System.Text.StringBuilder();
                using var stream = await resp.Content.ReadAsStreamAsync(timeoutCts.Token);
                await StreamAndExtractUsageAsync(stream, context.Response.OutputStream, requestLog, true, ct, sbAnthropic);
                LogResponseContent(channel, (int)resp.StatusCode, reqBody["model"]?.ToString() ?? "", sbAnthropic.ToString());
            }
        }

        // Anthropic流式响应转OpenAI流式
        private async Task ForwardAnthropicStreamAsOpenAIAsync(HttpListenerContext context, string url, ChannelConfig channel, JObject anthropicReq, string modelName, RequestLog requestLog, CancellationToken ct)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("x-api-key", channel.ApiKey);
                req.Headers.Add("anthropic-version", "2023-06-01");
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(anthropicReq.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, true, timeoutCts.Token);

            // 流式请求若上游返回非成功状态码，读取并记录错误响应内容
            if ((int)resp.StatusCode >= 400)
            {
                var errBody = await TryReadStreamErrorAsync(resp, timeoutCts.Token);
                LogResponseContent(channel, (int)resp.StatusCode, modelName, errBody);
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errBody), ct);
                await context.Response.OutputStream.FlushAsync(ct);
                return;
            }

            context.Response.StatusCode = (int)resp.StatusCode;
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Connection", "keep-alive");

            // 流式成功响应：收集内容用于日志记录
            var sbA2O = new System.Text.StringBuilder();

            var requestId = $"chatcmpl-{Guid.NewGuid():N}";

            using var stream = await resp.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var converted = ProtocolConverter.ConvertAnthropicStreamEvent(line, modelName, requestId);
                if (converted != null)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(converted);
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length, ct);
                    await context.Response.OutputStream.FlushAsync(ct);

                    // 收集转换后的内容用于日志（限制大小）
                    if (sbA2O.Length < 104857600)
                    {
                        sbA2O.AppendLine(converted);
                    }
                }
            }

            LogResponseContent(channel, (int)resp.StatusCode, modelName, sbA2O.ToString());
        }

        // OpenAI流式响应转Anthropic流式
        private async Task ForwardOpenAIStreamAsAnthropicAsync(HttpListenerContext context, string url, ChannelConfig channel, JObject openaiReq, string modelName, RequestLog requestLog, CancellationToken ct)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("Authorization", $"Bearer {channel.ApiKey}");
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(openaiReq.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, true, timeoutCts.Token);

            // 流式请求若上游返回非成功状态码，读取并记录错误响应内容
            if ((int)resp.StatusCode >= 400)
            {
                var errBody = await TryReadStreamErrorAsync(resp, timeoutCts.Token);
                LogResponseContent(channel, (int)resp.StatusCode, modelName, errBody);
                context.Response.StatusCode = (int)resp.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errBody), ct);
                await context.Response.OutputStream.FlushAsync(ct);
                return;
            }

            context.Response.StatusCode = (int)resp.StatusCode;
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Add("Cache-Control", "no-cache");

            // 流式成功响应：收集内容用于日志记录
            var sbO2A = new System.Text.StringBuilder();

            // 先发送message_start事件
            var msgStart = $"event: message_start\ndata: {JsonConvert.SerializeObject(new JObject
            {
                ["type"] = "message_start",
                ["message"] = new JObject
                {
                    ["id"] = $"msg_{Guid.NewGuid():N}",
                    ["type"] = "message",
                    ["role"] = "assistant",
                    ["model"] = modelName,
                    ["content"] = new JArray(),
                    ["stop_reason"] = null as JToken,
                    ["usage"] = new JObject { ["input_tokens"] = 0, ["output_tokens"] = 0 }
                }
            })}\n\n";
            await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(msgStart), 0, msgStart.Length, ct);
            if (sbO2A.Length < 104857600) { sbO2A.Append(msgStart); }

            // content_block_start
            var blockStart = "event: content_block_start\ndata: {\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}\n\n";
            await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(blockStart), 0, blockStart.Length, ct);
            if (sbO2A.Length < 104857600) { sbO2A.Append(blockStart); }

            using var stream = await resp.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var converted = ProtocolConverter.ConvertOpenAIStreamToAnthropic(line, modelName, null);
                if (converted != null)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(converted);
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length, ct);
                    await context.Response.OutputStream.FlushAsync(ct);
                    if (sbO2A.Length < 104857600) { sbO2A.AppendLine(converted); }
                }
            }

            // 结束事件
            var blockStop = "event: content_block_stop\ndata: {\"type\":\"content_block_stop\",\"index\":0}\n\n";
            await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(blockStop), 0, blockStop.Length, ct);
            if (sbO2A.Length < 104857600) { sbO2A.Append(blockStop); }

            var msgDelta = "event: message_delta\ndata: {\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":0}}\n\n";
            await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(msgDelta), 0, msgDelta.Length, ct);
            if (sbO2A.Length < 104857600) { sbO2A.Append(msgDelta); }

            var msgStop = "event: message_stop\ndata: {\"type\":\"message_stop\"}\n\n";
            await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(msgStop), 0, msgStop.Length, ct);
            await context.Response.OutputStream.FlushAsync(ct);
            if (sbO2A.Length < 104857600) { sbO2A.Append(msgStop); }

            LogResponseContent(channel, (int)resp.StatusCode, modelName, sbO2A.ToString());
        }

        // 发送OpenAI格式请求
        private async Task<(JObject body, int statusCode)> SendOpenAIRequestAsync(string url, ChannelConfig channel, JObject reqBody, CancellationToken ct)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("Authorization", $"Bearer {channel.ApiKey}");
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(reqBody.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, false, timeoutCts.Token);
            var body = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
            return (JObject.Parse(body), (int)resp.StatusCode);
        }

        // 发送Anthropic格式请求
        private async Task<(JObject body, int statusCode)> SendAnthropicRequestAsync(string url, ChannelConfig channel, JObject reqBody, CancellationToken ct)
        {
            using var timeoutCts = CreateTimeoutCts(ct, channel.Timeout);

            // 通过工厂委托创建请求，便于 429 时自动重试（HttpRequestMessage 不可重用）
            Func<HttpRequestMessage> createRequest = () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("x-api-key", channel.ApiKey);
                req.Headers.Add("anthropic-version", "2023-06-01");
                AddCustomHeaders(req, channel);
                req.Content = new StringContent(reqBody.ToString(Formatting.None), System.Text.Encoding.UTF8, "application/json");
                return req;
            };

            using var resp = await SendWithRetryAsync(createRequest, channel, false, timeoutCts.Token);
            var body = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
            return (JObject.Parse(body), (int)resp.StatusCode);
        }

        // 模型列表接口
        private async Task HandleModelsAsync(HttpListenerContext context)
        {
            var models = _configService.GetAllModelNames();
            // 在模型列表中加入虚拟的 system_model
            models.Insert(0, "system_model");
            var result = new JObject
            {
                ["object"] = "list",
                ["data"] = new JArray(models.Select(m => new JObject
                {
                    ["id"] = m,
                    ["object"] = "model",
                    ["created"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ["owned_by"] = "funai-gateway"
                }))
            };
            context.Response.ContentType = "application/json";
            await WriteJsonAsync(context.Response, result);
        }

        private void ForwardUserAgent(HttpRequestMessage req, HttpListenerContext context)
        {
            var clientUA = context.Request.Headers["User-Agent"];
            if (!string.IsNullOrWhiteSpace(clientUA))
                req.Headers.TryAddWithoutValidation("User-Agent", clientUA);
            else
                req.Headers.TryAddWithoutValidation("User-Agent", "OpenAI/Python 1.82.0");
        }

        private void AddCustomHeaders(HttpRequestMessage req, ChannelConfig channel)
        {
            if (string.IsNullOrWhiteSpace(channel.CustomHeaders)) return;
            try
            {
                var headers = JObject.Parse(channel.CustomHeaders);
                foreach (var prop in headers)
                    req.Headers.TryAddWithoutValidation(prop.Key, prop.Value?.ToString());
            }
            catch { }
        }

        private static void SetCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
        }

        private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            return await reader.ReadToEndAsync();
        }

        private async Task WriteJsonAsync(HttpListenerResponse response, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        // 流式请求遇到错误时，尝试读取响应体（最多 2000 字符）用于日志记录和错误转发
        private static async Task<string> TryReadStreamErrorAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrEmpty(body)) { return "{\"error\":{\"message\":\"上游返回错误且无响应内容\"}}"; }
                return body.Length > 2000 ? body.Substring(0, 2000) + "...(truncated)" : body;
            }
            catch
            {
                return "{\"error\":{\"message\":\"无法读取上游错误响应内容\"}}";
            }
        }

        // 逐行转发 SSE 流，同时解析 usage 字段提取 token 数
        // isAnthropicUsage: true 时使用 input_tokens/output_tokens，false 时使用 prompt_tokens/completion_tokens
        // contentCollector：可选，传入时收集流式内容用于日志记录（最多 maxCollectChars 字符）
        private async Task StreamAndExtractUsageAsync(Stream input, System.IO.Stream output, RequestLog requestLog, bool isAnthropicUsage, CancellationToken ct, System.Text.StringBuilder? contentCollector = null, int maxCollectChars = 104857600)
        {
            using var reader = new StreamReader(input);
            string? line;
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                line = await reader.ReadLineAsync(ct);
                if (line == null) break;

                var bytes = System.Text.Encoding.UTF8.GetBytes(line + "\n");
                await output.WriteAsync(bytes, 0, bytes.Length, ct);
                await output.FlushAsync(ct);

                // 收集流式内容用于日志（限制大小）
                if (contentCollector != null && contentCollector.Length < maxCollectChars)
                {
                    contentCollector.AppendLine(line);
                }

                // 尝试从 data: 行中提取 usage
                if (line.StartsWith("data:") && line.Contains("\"usage\""))
                {
                    TryExtractUsage(line, requestLog, isAnthropicUsage);
                }
            }
        }

        // 从 SSE 数据行解析 usage token
        private static void TryExtractUsage(string line, RequestLog requestLog, bool isAnthropicUsage)
        {
            try
            {
                var jsonPart = line["data:".Length..].Trim();
                if (jsonPart == "[DONE]") return;
                var obj = JObject.Parse(jsonPart);
                var usage = obj["usage"];
                if (usage == null) return;

                if (isAnthropicUsage)
                {
                    requestLog.InputTokens = usage["input_tokens"]?.Value<int>() ?? requestLog.InputTokens;
                    requestLog.OutputTokens = usage["output_tokens"]?.Value<int>() ?? requestLog.OutputTokens;
                }
                else
                {
                    requestLog.InputTokens = usage["prompt_tokens"]?.Value<int>() ?? requestLog.InputTokens;
                    requestLog.OutputTokens = usage["completion_tokens"]?.Value<int>() ?? requestLog.OutputTokens;
                }
            }
            catch { }
        }

        // 记录上游响应内容到文件（需开启 EnableResponseLog 且状态码匹配配置的记录范围）
        private void LogResponseContent(ChannelConfig channel, int statusCode, string modelName, string responseBody)
        {
            if (!_configService.Config.EnableResponseLog) { return; }
            if (!ShouldLogStatusCode(statusCode)) { return; }
            _configService.WriteResponseLog(channel.Name, statusCode, modelName, responseBody);
        }

        // 根据配置判断是否需要记录该状态码的响应内容
        private bool ShouldLogStatusCode(int statusCode)
        {
            var cfg = _configService.Config.ResponseLog;
            if (statusCode >= 200 && statusCode < 300) { return cfg.Log2xx; }
            if (statusCode >= 400 && statusCode < 500) { return cfg.Log4xx; }
            if (statusCode >= 500 && statusCode < 600) { return cfg.Log5xx; }
            return cfg.LogOther;
        }

        // 带自动重试的请求发送：当上游返回 429/500/502/503 或网络异常时，延迟 5 秒后重试
        // createRequest：每次尝试时调用以创建新的 HttpRequestMessage（请求实例不可重用）
        // streamResponse：是否以流式方式读取响应（ResponseHeadersRead）
        // linkedCt：已绑定渠道超时的 CancellationToken
        // 返回的 HttpResponseMessage 由调用方负责 Dispose
        private async Task<HttpResponseMessage> SendWithRetryAsync(
            Func<HttpRequestMessage> createRequest,
            ChannelConfig channel,
            bool streamResponse,
            CancellationToken linkedCt)
        {
            // 总尝试次数 = 自动重试次数 + 首次请求；至少 1 次
            var maxAttempts = Math.Max(1, channel.RetryCount + 1);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                // 首次不延迟；后续重试前固定延迟 5 秒
                if (attempt > 1)
                {
                    Log($"渠道 [{channel.Name}] 5 秒后进行第 {attempt - 1}/{channel.RetryCount} 次自动重试");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), linkedCt);
                    }
                    catch (OperationCanceledException) { throw; }
                }

                var req = createRequest();
                HttpResponseMessage? resp = null;
                try
                {
                    resp = await GetHttpClient(channel).SendAsync(
                        req,
                        streamResponse ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead,
                        linkedCt);

                    var statusCode = (int)resp.StatusCode;
                    // 命中可重试状态码（429/500/502/503）且仍有重试机会：读取错误内容、释放资源后重试
                    if ((statusCode == 429 || statusCode == 500 || statusCode == 502 || statusCode == 503) && attempt < maxAttempts)
                    {
                        try
                        {
                            var errSnippet = await TryReadShortErrorAsync(resp, linkedCt);
                            Log($"渠道 [{channel.Name}] 上游返回 {statusCode}：{errSnippet}");
                        }
                        catch { }
                        resp.Dispose();
                        resp = null;
                        continue;
                    }

                    // 成功 / 非重试错误 / 最后一次仍为错误：交由调用方按原逻辑处理
                    return resp;
                }
                catch (OperationCanceledException)
                {
                    // 超时/取消不重试，直接抛出
                    resp?.Dispose();
                    req.Dispose();
                    throw;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    // 网络异常（连接失败、DNS解析失败、上游断开等）：释放资源后重试
                    Log($"渠道 [{channel.Name}] 请求异常：{ex.Message}");
                    resp?.Dispose();
                    req.Dispose();
                    continue;
                }
            }

            // 理论上不会到达这里
            throw new InvalidOperationException("请求重试逻辑异常");
        }

        // 读取响应内容的前 200 字符用于日志展示
        private static async Task<string> TryReadShortErrorAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrEmpty(body)) { return "无响应内容"; }
                return body.Length > 200 ? body.Substring(0, 200) + "..." : body;
            }
            catch
            {
                return "无法读取错误内容";
            }
        }
    }
}
