using FunAiGateway.Forms;
using FunAiGateway.Models;
using FunAiGateway.Services;
using Sunny.UI;

namespace FunAiGateway
{
    public partial class MainForm : UIForm
    {
        private readonly ConfigService _configService;
        private readonly ProxyServer _proxyServer;
        private int _requestCount = 0;
        private bool _initialized = false;
        // 当前会话的内存日志缓冲（启动时不读取历史文件，仅保留本次运行期间的日志）
        private readonly System.Collections.Concurrent.ConcurrentQueue<RequestLog> _sessionLogs = new();

        public MainForm()
        {
            InitializeComponent();

            _configService = new ConfigService();
            _proxyServer = new ProxyServer(_configService);

            // 首次启动自动生成API Key
            if (string.IsNullOrWhiteSpace(_configService.Config.ApiKey))
            {
                _configService.Config.ApiKey = "fk-" + Guid.NewGuid().ToString("N");
                _configService.Save();
            }

            // 事件绑定
            btnAddChannel.Click += BtnAddChannel_Click;
            btnEditChannel.Click += BtnEditChannel_Click;
            btnDeleteChannel.Click += BtnDeleteChannel_Click;
            btnToggleChannel.Click += BtnToggleChannel_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnSaveSettings.Click += BtnSaveSettings_Click;
            btnClearLogs.Click += BtnClearLogs_Click;

            // 日志上限变化时保存并裁剪已有日志
            dgvChannels.DoubleClick += BtnEditChannel_Click;

            // 日志表格颜色格式化
            dgvLogs.CellFormatting += DgvLogs_CellFormatting;

            // 切换到日志页时补设列宽（非激活页上设置列宽会崩溃）
            tabControl.SelectedIndexChanged += (_, _) =>
            {
                if (tabControl.SelectedTab == tabLogs) { ApplyLogColumnWidths(); }
            };

            // 日志显示设置按钮
            btnLogSettings.Click += BtnLogSettings_Click;

            // 复制按钮
            btnCopyOpenAI.Click += (_, _) => CopyToClipboard(txtOpenAIUrl.Text);
            btnCopyAnthropic.Click += (_, _) => CopyToClipboard(txtAnthropicUrl.Text);
            btnCopyModels.Click += (_, _) => CopyToClipboard(txtModelsUrl.Text);

            // 监听模式切换时保存并更新连接信息
            rdoLocal.ValueChanged += (_, _) => { AutoSaveSettings(); UpdateConnectionInfo(); };
            rdoBroadcast.ValueChanged += (_, _) => { AutoSaveSettings(); UpdateConnectionInfo(); };

            // API Key验证切换时自动保存
            chkRequireApiKey.ValueChanged += (_, _) => AutoSaveSettings();

            // 自动启动切换时自动保存
            chkAutoStart.ValueChanged += (_, _) => AutoSaveSettings();

            // API Key显示/隐藏切换
            btnToggleKeyVisibility.Click += BtnToggleKeyVisibility_Click;

            // 自定义域名变化时保存并更新连接信息
            txtCustomHost.TextChanged += (_, _) => { AutoSaveSettings(); UpdateConnectionInfo(); };

            // 默认模型变化时自动保存
            cmbDefaultModel.SelectedIndexChanged += (_, _) => AutoSaveSettings();

            _proxyServer.OnRequestLogged += OnRequestLogged;
            _proxyServer.OnLog += OnServerLog;

            // 加载设置
            LoadSettings();
            RefreshChannels();
            UpdateConnectionInfo();
            _initialized = true;

            // 启动时自动启动中转服务
            if (_configService.Config.AutoStartOnLaunch)
            {
                BtnStart_Click(null, EventArgs.Empty);
            }
        }

        private ListenMode GetCurrentListenMode() => rdoBroadcast.Checked ? ListenMode.Broadcast : ListenMode.Local;

        private void BtnToggleKeyVisibility_Click(object? sender, EventArgs e)
        {
            txtApiKey.UseSystemPasswordChar = !txtApiKey.UseSystemPasswordChar;
            btnToggleKeyVisibility.Text = txtApiKey.UseSystemPasswordChar ? "显示" : "隐藏";
        }

        private string GetDisplayHost()
        {
            var port = (int)numPort.Value;
            var portSuffix = port == 80 ? "" : $":{port}";

            // 1. 用户自定义域名/IP优先
            var customHost = txtCustomHost.Text.Trim();
            if (!string.IsNullOrEmpty(customHost))
                return $"{customHost}{portSuffix}";

            // 2. 本地模式
            if (rdoLocal.Checked)
                return $"127.0.0.1{portSuffix}";

            // 3. 广播模式自动检测本机IP
            var localIp = GetLocalIPAddress();
            return $"{localIp}{portSuffix}";
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                using var socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Dgram,
                    System.Net.Sockets.ProtocolType.Udp);
                socket.Connect("8.8.8.8", 53);
                var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
                if (endPoint != null)
                    return endPoint.Address.ToString();
            }
            catch { }

            try
            {
                return System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                        && ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && !System.Net.IPAddress.IsLoopback(ua.Address)
                        && !ua.Address.ToString().StartsWith("169.254."))
                    .Select(ua => ua.Address.ToString())
                    .FirstOrDefault() ?? "0.0.0.0";
            }
            catch { return "0.0.0.0"; }
        }

        private void LoadSettings()
        {
            numPort.Value = _configService.Config.ListenPort;
            rdoLocal.Checked = _configService.Config.ListenMode == ListenMode.Local;
            rdoBroadcast.Checked = _configService.Config.ListenMode == ListenMode.Broadcast;
            txtCustomHost.Text = _configService.Config.CustomHost;
            chkRequireApiKey.Checked = _configService.Config.RequireApiKey;
            txtApiKey.Text = _configService.Config.ApiKey;
            chkAutoStart.Checked = _configService.Config.AutoStartOnLaunch;
            RefreshDefaultModelCombo();
        }

        private void UpdateConnectionInfo()
        {
            var host = GetDisplayHost();
            txtOpenAIUrl.Text = $"http://{host}/v1/chat/completions";
            txtAnthropicUrl.Text = $"http://{host}/v1/messages";
            txtModelsUrl.Text = $"http://{host}/v1/models";
        }

        private void CopyToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
                this.ShowInfoTip("已复制到剪贴板");
            }
        }

        private void AutoSaveSettings()
        {
            if (!_initialized) return;
            _configService.Config.ListenPort = (int)numPort.Value;
            _configService.Config.ListenMode = GetCurrentListenMode();
            _configService.Config.CustomHost = txtCustomHost.Text.Trim();
            _configService.Config.DefaultModel = cmbDefaultModel.SelectedItem?.ToString() ?? "";
            _configService.Config.RequireApiKey = chkRequireApiKey.Checked;
            _configService.Config.ApiKey = txtApiKey.Text.Trim();
            _configService.Config.AutoStartOnLaunch = chkAutoStart.Checked;
            _configService.Save();
        }

        private void BtnSaveSettings_Click(object? sender, EventArgs e)
        {
            AutoSaveSettings();
            UpdateConnectionInfo();
            this.ShowInfoTip("设置已保存");
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            try
            {
                var port = (int)numPort.Value;
                var mode = GetCurrentListenMode();
                _proxyServer.Start(port, mode);
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                numPort.Enabled = false;
                rdoLocal.Enabled = false;
                rdoBroadcast.Enabled = false;
                var modeText = mode == ListenMode.Broadcast ? "广播(0.0.0.0)" : "本地(127.0.0.1)";
                lblStatus.Text = $"服务运行中 - {modeText} 端口: {port}";
                lblStatus.ForeColor = Color.Green;

                var host = GetDisplayHost();
                AppendLog($"服务已启动，模式: {modeText}，端口: {port}");
                AppendLog($"OpenAI接口: http://{host}/v1/chat/completions");
                AppendLog($"Anthropic接口: http://{host}/v1/messages");
                AppendLog($"模型列表: http://{host}/v1/models");
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"启动失败: {ex.Message}\n\n提示: 广播模式可能需要管理员权限");
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _proxyServer.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            numPort.Enabled = true;
            rdoLocal.Enabled = true;
            rdoBroadcast.Enabled = true;
            lblStatus.Text = "服务已停止";
            lblStatus.ForeColor = Color.Red;
            AppendLog("服务已停止");
        }

        private void RefreshChannels()
        {
            var channels = _configService.Config.Channels.Select(c => new
            {
                c.Id,
                模型名称 = c.Name,
                实际模型 = c.Models.FirstOrDefault()?.RealModelName ?? c.Name,
                协议 = c.Type.ToString(),
                BaseUrl = c.BaseUrl,
                上下文 = c.Models.FirstOrDefault()?.ContextLength ?? 0,
                状态 = c.Enabled ? "启用" : "禁用"
            }).ToList();

            dgvChannels.DataSource = channels;
            if (dgvChannels.Columns["Id"] != null)
                dgvChannels.Columns["Id"]!.Visible = false;

            RefreshDefaultModelCombo();
        }

        private void RefreshDefaultModelCombo()
        {
            var current = _configService.Config.DefaultModel;
            var models = _configService.GetAllModelNames();
            cmbDefaultModel.Items.Clear();
            foreach (var m in models)
                cmbDefaultModel.Items.Add(m);
            if (!string.IsNullOrEmpty(current) && models.Contains(current))
                cmbDefaultModel.SelectedItem = current;
        }

        private void BtnAddChannel_Click(object? sender, EventArgs e)
        {
            using var dlg = new ChannelEditDialog(
                existingChannels: _configService.Config.Channels);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _configService.AddChannel(dlg.Channel);
                RefreshChannels();
            }
        }

        private void BtnEditChannel_Click(object? sender, EventArgs e)
        {
            if (dgvChannels.SelectedRows.Count == 0) return;
            var id = dgvChannels.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (id == null) return;

            var channel = _configService.Config.Channels.FirstOrDefault(c => c.Id == id);
            if (channel == null) return;

            using var dlg = new ChannelEditDialog(channel, _configService.Config.Channels);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _configService.UpdateChannel(dlg.Channel);
                RefreshChannels();
            }
        }

        private void BtnDeleteChannel_Click(object? sender, EventArgs e)
        {
            if (dgvChannels.SelectedRows.Count == 0) return;
            var id = dgvChannels.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (id == null) return;

            if (this.ShowAskDialog("确认删除", "确定删除该渠道？"))
            {
                _configService.DeleteChannel(id);
                RefreshChannels();
            }
        }

        private void BtnToggleChannel_Click(object? sender, EventArgs e)
        {
            if (dgvChannels.SelectedRows.Count == 0) return;
            var id = dgvChannels.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (id == null) return;

            var channel = _configService.Config.Channels.FirstOrDefault(c => c.Id == id);
            if (channel == null) return;

            channel.Enabled = !channel.Enabled;
            _configService.UpdateChannel(channel);
            RefreshChannels();
        }

        private void DgvLogs_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;
            var row = dgvLogs.Rows[e.RowIndex];
            var cfg = _configService.Config.LogColor;

            // 响应时间颜色：默认绿色，>黄色阈值黄色，>橙色阈值橙色，>红色阈值红色
            if (row.Cells["耗时s"]?.Value != null && double.TryParse(row.Cells["耗时s"].Value.ToString(), out var durationSec))
            {
                var durationMs = (long)(durationSec * 1000);
                Color durationColor;
                if (durationMs > cfg.DurationRed) { durationColor = Color.Red; }
                else if (durationMs > cfg.DurationOrange) { durationColor = Color.Orange; }
                else if (durationMs > cfg.DurationYellow) { durationColor = Color.YellowGreen; }
                else { durationColor = Color.Green; }
                row.Cells["耗时s"].Style.ForeColor = durationColor;
                row.Cells["耗时s"].Style.SelectionForeColor = durationColor;
            }

            // 输入Token颜色：默认绿色，>橙色阈值橙色，>红色阈值红色
            if (row.Cells["输入Token"]?.Value != null && int.TryParse(row.Cells["输入Token"].Value.ToString(), out var inputTokens))
            {
                Color inputColor;
                if (inputTokens > cfg.InputTokenRed) { inputColor = Color.Red; }
                else if (inputTokens > cfg.InputTokenOrange) { inputColor = Color.Orange; }
                else { inputColor = Color.Green; }
                row.Cells["输入Token"].Style.ForeColor = inputColor;
                row.Cells["输入Token"].Style.SelectionForeColor = inputColor;
            }

            // 输出Token颜色：默认绿色，>橙色阈值橙色，>红色阈值红色
            if (row.Cells["输出Token"]?.Value != null && int.TryParse(row.Cells["输出Token"].Value.ToString(), out var outputTokens))
            {
                Color outputColor;
                if (outputTokens > cfg.OutputTokenRed) { outputColor = Color.Red; }
                else if (outputTokens > cfg.OutputTokenOrange) { outputColor = Color.Orange; }
                else { outputColor = Color.Green; }
                row.Cells["输出Token"].Style.ForeColor = outputColor;
                row.Cells["输出Token"].Style.SelectionForeColor = outputColor;
            }
        }

        private void RefreshLogs()
        {
            // 窗体正在关闭或已释放时不再刷新
            if (IsDisposed || !IsHandleCreated || dgvLogs == null || dgvLogs.IsDisposed) { return; }

            try
            {
                var logs = _sessionLogs.ToList();
                // 先置空再赋值，强制 DataGridView 重新绑定并重绘
                dgvLogs.DataSource = null;
                dgvLogs.DataSource = logs.Select(l => new
                {
                    时间 = l.Time.ToString("HH:mm:ss"),
                    渠道 = l.ChannelName,
                    协议 = $"{l.RequestProtocol}→{l.TargetProtocol}",
                    输入Token = l.InputTokens,
                    输出Token = l.OutputTokens,
                    耗时s = Math.Round(l.DurationMs / 1000.0, 1),
                    状态 = l.Success ? "成功" : "失败"
                }).ToList();

                // 列宽设置：仅在日志页处于激活状态时执行（非激活页上 DataGridView 句柄未创建会崩溃）
                if (tabControl.SelectedTab == tabLogs)
                {
                    ApplyLogColumnWidths();
                }

                // 滚动到最后一行（最新记录）
                if (dgvLogs.Rows.Count > 0)
                {
                    dgvLogs.FirstDisplayedScrollingRowIndex = dgvLogs.Rows.Count - 1;
                }
                // 清除选中状态，避免出现焦点
                dgvLogs.ClearSelection();
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
        }

        // 设置日志表格列宽，仅在日志页激活时调用
        private void ApplyLogColumnWidths()
        {
            if (dgvLogs == null || dgvLogs.IsDisposed) { return; }
            try
            {
                if (dgvLogs.Columns["时间"] != null) { dgvLogs.Columns["时间"]!.Width = 95; dgvLogs.Columns["时间"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }
                if (dgvLogs.Columns["渠道"] != null) { dgvLogs.Columns["渠道"]!.Width = 215; dgvLogs.Columns["渠道"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }
                if (dgvLogs.Columns["协议"] != null) { dgvLogs.Columns["协议"]!.Width = 240; dgvLogs.Columns["协议"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }
                if (dgvLogs.Columns["输入Token"] != null) { dgvLogs.Columns["输入Token"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
                if (dgvLogs.Columns["输出Token"] != null) { dgvLogs.Columns["输出Token"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
                if (dgvLogs.Columns["耗时s"] != null) { dgvLogs.Columns["耗时s"]!.Width = 90; dgvLogs.Columns["耗时s"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }
                if (dgvLogs.Columns["状态"] != null) { dgvLogs.Columns["状态"]!.Width = 60; dgvLogs.Columns["状态"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
        }

        private void BtnClearLogs_Click(object? sender, EventArgs e)
        {
            // 仅清空当前会话内存缓冲（不影响已写入文件的日志）
            while (_sessionLogs.TryDequeue(out _)) { }
            RefreshLogs();
        }

        private void BtnLogSettings_Click(object? sender, EventArgs e)
        {
            using var dlg = new LogSettingsDialog(_configService.Config.LogColor, _configService.Config.MaxLogCount, _configService.Config.LogRetentionDays, _configService.Config.EnableResponseLog, _configService.Config.ResponseLog);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                // 保存颜色阈值配置
                _configService.Config.LogColor = dlg.LogColor;
                // 保存日志上限并裁剪
                var newMax = dlg.MaxLogCount;
                var oldMax = _configService.Config.MaxLogCount;
                _configService.Config.MaxLogCount = newMax;
                _configService.Config.LogRetentionDays = dlg.LogRetentionDays;
                // 保存响应日志配置
                _configService.Config.EnableResponseLog = dlg.EnableResponseLog;
                _configService.Config.ResponseLog = dlg.ResponseLog;
                _configService.Save();
                // 上限变小时立即裁剪已有日志（文件+内存缓冲）
                if (newMax < oldMax)
                {
                    _configService.TrimLogs(newMax);
                    while (_sessionLogs.Count > newMax)
                        _sessionLogs.TryDequeue(out _);
                }
                // 刷新日志显示以应用新的颜色配置
                RefreshLogs();
                this.ShowInfoTip("显示设置已保存");
            }
        }

        private void OnRequestLogged(RequestLog log)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnRequestLogged(log)));
                return;
            }

            // 持久化日志到文件（保留历史）
            _configService.AddLog(log);
            // 加入当前会话内存缓冲（用于界面显示）
            _sessionLogs.Enqueue(log);
            // 超过上限则移除最早的记录
            var maxLogs = _configService.Config.MaxLogCount;
            while (_sessionLogs.Count > maxLogs)
                _sessionLogs.TryDequeue(out _);

            _requestCount++;
            lblRequestCount.Text = $"请求: {_requestCount}";

            // 无论当前在哪个标签页都刷新日志表格，切换回来时数据即是最新的
            RefreshLogs();
        }

        private void OnServerLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnServerLog(message)));
                return;
            }
            AppendLog(message);
        }

        private void AppendLog(string message)
        {
            if (txtLogOutput.Lines.Length > 500)
            {
                var lines = txtLogOutput.Lines.Skip(200).ToArray();
                txtLogOutput.Lines = lines;
            }
            txtLogOutput.AppendText($"{message}{Environment.NewLine}");
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_proxyServer.IsRunning)
            {
                _proxyServer.Stop();
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("为了方便切换模型，可以指定使用system_model模型，然后在这个窗口里切换system_model到底调用哪个渠道.");
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("用于外网访问时填写域名以及下方链接自动生成.");
        }
    }
}
