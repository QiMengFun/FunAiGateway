using Sunny.UI;

namespace FunAiGateway
{
    partial class MainForm : UIForm
    {
        private System.ComponentModel.IContainer components = null;

        private UITabControl tabControl;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabChannels;
        private System.Windows.Forms.TabPage tabLogs;

        // 设置页 - 网络设置
        private UIGroupBox grpNetwork;
        private UILabel lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private UIRadioButton rdoLocal;
        private UIRadioButton rdoBroadcast;
        private UILabel lblCustomHost;
        private UITextBox txtCustomHost;
        private UILabel lblDefaultModel;
        private UIComboBox cmbDefaultModel;

        // 设置页 - 认证设置
        private UIGroupBox grpAuth;
        private UICheckBox chkRequireApiKey;
        private UILabel lblApiKey;
        private System.Windows.Forms.TextBox txtApiKey;
        private UIButton btnToggleKeyVisibility;

        // 设置页 - 服务控制
        private UIButton btnStart;
        private UIButton btnStop;
        private UIButton btnSaveSettings;
        private UICheckBox chkAutoStart;

        // 设置页 - 连接信息
        private UIGroupBox grpConnectionInfo;
        private UILabel lblOpenAIInfo;
        private UITextBox txtOpenAIUrl;
        private UIButton btnCopyOpenAI;
        private UILabel lblAnthropicInfo;
        private UITextBox txtAnthropicUrl;
        private UIButton btnCopyAnthropic;
        private UILabel lblModelsInfo;
        private UITextBox txtModelsUrl;
        private UIButton btnCopyModels;

        // 渠道页
        private System.Windows.Forms.DataGridView dgvChannels;
        private UIButton btnAddChannel;
        private UIButton btnEditChannel;
        private UIButton btnDeleteChannel;
        private UIButton btnToggleChannel;

        // 日志页
        private System.Windows.Forms.DataGridView dgvLogs;
        private UITextBox txtLogOutput;
        private UIButton btnClearLogs;
        private UILabel lblMaxLogs;
        private System.Windows.Forms.NumericUpDown numMaxLogs;

        // 状态栏
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblRequestCount;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tabControl = new UITabControl();
            tabSettings = new TabPage();
            grpNetwork = new UIGroupBox();
            uiButton2 = new UIButton();
            uiButton1 = new UIButton();
            lblPort = new UILabel();
            numPort = new NumericUpDown();
            rdoLocal = new UIRadioButton();
            rdoBroadcast = new UIRadioButton();
            lblCustomHost = new UILabel();
            txtCustomHost = new UITextBox();
            lblDefaultModel = new UILabel();
            cmbDefaultModel = new UIComboBox();
            grpAuth = new UIGroupBox();
            chkRequireApiKey = new UICheckBox();
            lblApiKey = new UILabel();
            txtApiKey = new TextBox();
            btnToggleKeyVisibility = new UIButton();
            btnStart = new UIButton();
            btnStop = new UIButton();
            btnSaveSettings = new UIButton();
            chkAutoStart = new UICheckBox();
            grpConnectionInfo = new UIGroupBox();
            lblOpenAIInfo = new UILabel();
            txtOpenAIUrl = new UITextBox();
            btnCopyOpenAI = new UIButton();
            lblAnthropicInfo = new UILabel();
            txtAnthropicUrl = new UITextBox();
            btnCopyAnthropic = new UIButton();
            lblModelsInfo = new UILabel();
            txtModelsUrl = new UITextBox();
            btnCopyModels = new UIButton();
            tabChannels = new TabPage();
            dgvChannels = new DataGridView();
            btnAddChannel = new UIButton();
            btnEditChannel = new UIButton();
            btnDeleteChannel = new UIButton();
            btnToggleChannel = new UIButton();
            tabLogs = new TabPage();
            dgvLogs = new DataGridView();
            txtLogOutput = new UITextBox();
            btnClearLogs = new UIButton();
            lblMaxLogs = new UILabel();
            numMaxLogs = new NumericUpDown();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblRequestCount = new ToolStripStatusLabel();
            tabControl.SuspendLayout();
            tabSettings.SuspendLayout();
            grpNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            grpAuth.SuspendLayout();
            grpConnectionInfo.SuspendLayout();
            tabChannels.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChannels).BeginInit();
            tabLogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLogs).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabSettings);
            tabControl.Controls.Add(tabChannels);
            tabControl.Controls.Add(tabLogs);
            tabControl.Dock = DockStyle.Fill;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tabControl.ItemSize = new Size(150, 40);
            tabControl.Location = new Point(0, 35);
            tabControl.MainPage = "";
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(900, 552);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.TabIndex = 0;
            tabControl.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            tabControl.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabSettings
            // 
            tabSettings.BackColor = Color.FromArgb(243, 249, 255);
            tabSettings.Controls.Add(grpNetwork);
            tabSettings.Controls.Add(grpAuth);
            tabSettings.Controls.Add(btnStart);
            tabSettings.Controls.Add(btnStop);
            tabSettings.Controls.Add(btnSaveSettings);
            tabSettings.Controls.Add(chkAutoStart);
            tabSettings.Controls.Add(grpConnectionInfo);
            tabSettings.Location = new Point(0, 40);
            tabSettings.Name = "tabSettings";
            tabSettings.Size = new Size(900, 512);
            tabSettings.TabIndex = 0;
            tabSettings.Text = "设置";
            // 
            // grpNetwork
            // 
            grpNetwork.Controls.Add(uiButton2);
            grpNetwork.Controls.Add(uiButton1);
            grpNetwork.Controls.Add(lblPort);
            grpNetwork.Controls.Add(numPort);
            grpNetwork.Controls.Add(rdoLocal);
            grpNetwork.Controls.Add(rdoBroadcast);
            grpNetwork.Controls.Add(lblCustomHost);
            grpNetwork.Controls.Add(txtCustomHost);
            grpNetwork.Controls.Add(lblDefaultModel);
            grpNetwork.Controls.Add(cmbDefaultModel);
            grpNetwork.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpNetwork.Location = new Point(4, 5);
            grpNetwork.Margin = new Padding(4, 5, 4, 5);
            grpNetwork.MinimumSize = new Size(1, 1);
            grpNetwork.Name = "grpNetwork";
            grpNetwork.Padding = new Padding(0, 32, 0, 0);
            grpNetwork.Size = new Size(892, 110);
            grpNetwork.TabIndex = 0;
            grpNetwork.Text = "网络设置";
            grpNetwork.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // uiButton2
            // 
            uiButton2.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton2.Location = new Point(535, 68);
            uiButton2.MinimumSize = new Size(1, 1);
            uiButton2.Name = "uiButton2";
            uiButton2.Size = new Size(17, 27);
            uiButton2.TabIndex = 9;
            uiButton2.Text = "?";
            uiButton2.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton2.Click += uiButton2_Click;
            // 
            // uiButton1
            // 
            uiButton1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton1.Location = new Point(535, 31);
            uiButton1.MinimumSize = new Size(1, 1);
            uiButton1.Name = "uiButton1";
            uiButton1.Size = new Size(17, 27);
            uiButton1.TabIndex = 8;
            uiButton1.Text = "?";
            uiButton1.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            uiButton1.Click += uiButton1_Click;
            // 
            // lblPort
            // 
            lblPort.BackColor = Color.Transparent;
            lblPort.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblPort.ForeColor = Color.FromArgb(48, 48, 48);
            lblPort.Location = new Point(16, 34);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(93, 23);
            lblPort.TabIndex = 0;
            lblPort.Text = "监听端口:";
            // 
            // numPort
            // 
            numPort.Location = new Point(115, 31);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(80, 26);
            numPort.TabIndex = 1;
            numPort.Value = new decimal(new int[] { 80, 0, 0, 0 });
            // 
            // rdoLocal
            // 
            rdoLocal.BackColor = Color.Transparent;
            rdoLocal.Checked = true;
            rdoLocal.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            rdoLocal.Location = new Point(685, 65);
            rdoLocal.MinimumSize = new Size(1, 1);
            rdoLocal.Name = "rdoLocal";
            rdoLocal.Size = new Size(204, 29);
            rdoLocal.TabIndex = 2;
            rdoLocal.Text = "仅本地(127.0.0.1)";
            // 
            // rdoBroadcast
            // 
            rdoBroadcast.BackColor = Color.Transparent;
            rdoBroadcast.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            rdoBroadcast.Location = new Point(685, 30);
            rdoBroadcast.MinimumSize = new Size(1, 1);
            rdoBroadcast.Name = "rdoBroadcast";
            rdoBroadcast.Size = new Size(204, 29);
            rdoBroadcast.TabIndex = 3;
            rdoBroadcast.Text = "广播(0.0.0.0)";
            // 
            // lblCustomHost
            // 
            lblCustomHost.BackColor = Color.Transparent;
            lblCustomHost.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblCustomHost.ForeColor = Color.FromArgb(48, 48, 48);
            lblCustomHost.Location = new Point(16, 68);
            lblCustomHost.Name = "lblCustomHost";
            lblCustomHost.Size = new Size(93, 23);
            lblCustomHost.TabIndex = 4;
            lblCustomHost.Text = "对外地址:";
            // 
            // txtCustomHost
            // 
            txtCustomHost.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtCustomHost.Location = new Point(115, 66);
            txtCustomHost.Margin = new Padding(4, 5, 4, 5);
            txtCustomHost.MinimumSize = new Size(1, 16);
            txtCustomHost.Name = "txtCustomHost";
            txtCustomHost.Padding = new Padding(5);
            txtCustomHost.ShowText = false;
            txtCustomHost.Size = new Size(413, 29);
            txtCustomHost.TabIndex = 5;
            txtCustomHost.TextAlignment = ContentAlignment.MiddleLeft;
            txtCustomHost.Watermark = "留空自动，可填域名";
            // 
            // lblDefaultModel
            // 
            lblDefaultModel.BackColor = Color.Transparent;
            lblDefaultModel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDefaultModel.ForeColor = Color.FromArgb(48, 48, 48);
            lblDefaultModel.Location = new Point(211, 33);
            lblDefaultModel.Name = "lblDefaultModel";
            lblDefaultModel.Size = new Size(117, 23);
            lblDefaultModel.TabIndex = 6;
            lblDefaultModel.Text = "system_model:";
            // 
            // cmbDefaultModel
            // 
            cmbDefaultModel.DataSource = null;
            cmbDefaultModel.DropDownStyle = UIDropDownStyle.DropDownList;
            cmbDefaultModel.FillColor = Color.White;
            cmbDefaultModel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            cmbDefaultModel.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cmbDefaultModel.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cmbDefaultModel.Location = new Point(328, 31);
            cmbDefaultModel.Margin = new Padding(4, 5, 4, 5);
            cmbDefaultModel.MinimumSize = new Size(63, 0);
            cmbDefaultModel.Name = "cmbDefaultModel";
            cmbDefaultModel.Padding = new Padding(0, 0, 30, 2);
            cmbDefaultModel.Size = new Size(200, 29);
            cmbDefaultModel.SymbolSize = 24;
            cmbDefaultModel.TabIndex = 7;
            cmbDefaultModel.TextAlignment = ContentAlignment.MiddleLeft;
            cmbDefaultModel.Watermark = "";
            // 
            // grpAuth
            // 
            grpAuth.Controls.Add(chkRequireApiKey);
            grpAuth.Controls.Add(lblApiKey);
            grpAuth.Controls.Add(txtApiKey);
            grpAuth.Controls.Add(btnToggleKeyVisibility);
            grpAuth.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpAuth.Location = new Point(4, 125);
            grpAuth.Margin = new Padding(4, 5, 4, 5);
            grpAuth.MinimumSize = new Size(1, 1);
            grpAuth.Name = "grpAuth";
            grpAuth.Padding = new Padding(0, 32, 0, 0);
            grpAuth.Size = new Size(892, 65);
            grpAuth.TabIndex = 1;
            grpAuth.Text = "认证设置";
            grpAuth.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // chkRequireApiKey
            // 
            chkRequireApiKey.BackColor = Color.Transparent;
            chkRequireApiKey.Checked = true;
            chkRequireApiKey.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkRequireApiKey.ForeColor = Color.FromArgb(48, 48, 48);
            chkRequireApiKey.Location = new Point(16, 28);
            chkRequireApiKey.MinimumSize = new Size(1, 1);
            chkRequireApiKey.Name = "chkRequireApiKey";
            chkRequireApiKey.Size = new Size(150, 29);
            chkRequireApiKey.TabIndex = 0;
            chkRequireApiKey.Text = "启用API Key验证";
            // 
            // lblApiKey
            // 
            lblApiKey.BackColor = Color.Transparent;
            lblApiKey.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblApiKey.ForeColor = Color.FromArgb(48, 48, 48);
            lblApiKey.Location = new Point(189, 30);
            lblApiKey.Name = "lblApiKey";
            lblApiKey.Size = new Size(56, 23);
            lblApiKey.TabIndex = 1;
            lblApiKey.Text = "密钥:";
            // 
            // txtApiKey
            // 
            txtApiKey.Location = new Point(247, 27);
            txtApiKey.Name = "txtApiKey";
            txtApiKey.Size = new Size(571, 26);
            txtApiKey.TabIndex = 2;
            txtApiKey.UseSystemPasswordChar = true;
            // 
            // btnToggleKeyVisibility
            // 
            btnToggleKeyVisibility.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnToggleKeyVisibility.Location = new Point(824, 25);
            btnToggleKeyVisibility.MinimumSize = new Size(1, 1);
            btnToggleKeyVisibility.Name = "btnToggleKeyVisibility";
            btnToggleKeyVisibility.Size = new Size(50, 32);
            btnToggleKeyVisibility.TabIndex = 3;
            btnToggleKeyVisibility.Text = "显示";
            btnToggleKeyVisibility.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnStart
            // 
            btnStart.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStart.Location = new Point(4, 198);
            btnStart.MinimumSize = new Size(1, 1);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(130, 40);
            btnStart.TabIndex = 2;
            btnStart.Text = "启动服务";
            btnStart.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnStop.Location = new Point(144, 198);
            btnStop.MinimumSize = new Size(1, 1);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(130, 40);
            btnStop.TabIndex = 3;
            btnStop.Text = "停止服务";
            btnStop.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnSaveSettings
            // 
            btnSaveSettings.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSaveSettings.Location = new Point(766, 198);
            btnSaveSettings.MinimumSize = new Size(1, 1);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(130, 40);
            btnSaveSettings.TabIndex = 4;
            btnSaveSettings.Text = "保存设置";
            btnSaveSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // chkAutoStart
            // 
            chkAutoStart.BackColor = Color.Transparent;
            chkAutoStart.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkAutoStart.ForeColor = Color.FromArgb(48, 48, 48);
            chkAutoStart.Location = new Point(284, 198);
            chkAutoStart.MinimumSize = new Size(1, 1);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(170, 40);
            chkAutoStart.TabIndex = 6;
            chkAutoStart.Text = "启动时自动启动服务";
            chkAutoStart.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpConnectionInfo
            // 
            grpConnectionInfo.Controls.Add(lblOpenAIInfo);
            grpConnectionInfo.Controls.Add(txtOpenAIUrl);
            grpConnectionInfo.Controls.Add(btnCopyOpenAI);
            grpConnectionInfo.Controls.Add(lblAnthropicInfo);
            grpConnectionInfo.Controls.Add(txtAnthropicUrl);
            grpConnectionInfo.Controls.Add(btnCopyAnthropic);
            grpConnectionInfo.Controls.Add(lblModelsInfo);
            grpConnectionInfo.Controls.Add(txtModelsUrl);
            grpConnectionInfo.Controls.Add(btnCopyModels);
            grpConnectionInfo.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpConnectionInfo.Location = new Point(4, 246);
            grpConnectionInfo.Margin = new Padding(4, 5, 4, 5);
            grpConnectionInfo.MinimumSize = new Size(1, 1);
            grpConnectionInfo.Name = "grpConnectionInfo";
            grpConnectionInfo.Padding = new Padding(0, 32, 0, 0);
            grpConnectionInfo.Size = new Size(892, 261);
            grpConnectionInfo.TabIndex = 5;
            grpConnectionInfo.Text = "连接信息";
            grpConnectionInfo.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // lblOpenAIInfo
            // 
            lblOpenAIInfo.BackColor = Color.Transparent;
            lblOpenAIInfo.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            lblOpenAIInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblOpenAIInfo.Location = new Point(16, 25);
            lblOpenAIInfo.Name = "lblOpenAIInfo";
            lblOpenAIInfo.Size = new Size(250, 23);
            lblOpenAIInfo.TabIndex = 0;
            lblOpenAIInfo.Text = "OpenAI 兼容接口 (Chat):";
            // 
            // txtOpenAIUrl
            // 
            txtOpenAIUrl.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtOpenAIUrl.Location = new Point(16, 48);
            txtOpenAIUrl.Margin = new Padding(4, 5, 4, 5);
            txtOpenAIUrl.MinimumSize = new Size(1, 16);
            txtOpenAIUrl.Name = "txtOpenAIUrl";
            txtOpenAIUrl.Padding = new Padding(5);
            txtOpenAIUrl.ReadOnly = true;
            txtOpenAIUrl.ShowText = false;
            txtOpenAIUrl.Size = new Size(791, 29);
            txtOpenAIUrl.TabIndex = 1;
            txtOpenAIUrl.TextAlignment = ContentAlignment.MiddleLeft;
            txtOpenAIUrl.Watermark = "";
            // 
            // btnCopyOpenAI
            // 
            btnCopyOpenAI.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCopyOpenAI.Location = new Point(814, 45);
            btnCopyOpenAI.MinimumSize = new Size(1, 1);
            btnCopyOpenAI.Name = "btnCopyOpenAI";
            btnCopyOpenAI.Size = new Size(75, 32);
            btnCopyOpenAI.TabIndex = 2;
            btnCopyOpenAI.Text = "复制";
            btnCopyOpenAI.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // lblAnthropicInfo
            // 
            lblAnthropicInfo.BackColor = Color.Transparent;
            lblAnthropicInfo.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            lblAnthropicInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblAnthropicInfo.Location = new Point(16, 82);
            lblAnthropicInfo.Name = "lblAnthropicInfo";
            lblAnthropicInfo.Size = new Size(280, 23);
            lblAnthropicInfo.TabIndex = 3;
            lblAnthropicInfo.Text = "Anthropic 兼容接口 (Messages):";
            // 
            // txtAnthropicUrl
            // 
            txtAnthropicUrl.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtAnthropicUrl.Location = new Point(16, 105);
            txtAnthropicUrl.Margin = new Padding(4, 5, 4, 5);
            txtAnthropicUrl.MinimumSize = new Size(1, 16);
            txtAnthropicUrl.Name = "txtAnthropicUrl";
            txtAnthropicUrl.Padding = new Padding(5);
            txtAnthropicUrl.ReadOnly = true;
            txtAnthropicUrl.ShowText = false;
            txtAnthropicUrl.Size = new Size(791, 29);
            txtAnthropicUrl.TabIndex = 4;
            txtAnthropicUrl.TextAlignment = ContentAlignment.MiddleLeft;
            txtAnthropicUrl.Watermark = "";
            // 
            // btnCopyAnthropic
            // 
            btnCopyAnthropic.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCopyAnthropic.Location = new Point(814, 102);
            btnCopyAnthropic.MinimumSize = new Size(1, 1);
            btnCopyAnthropic.Name = "btnCopyAnthropic";
            btnCopyAnthropic.Size = new Size(75, 32);
            btnCopyAnthropic.TabIndex = 5;
            btnCopyAnthropic.Text = "复制";
            btnCopyAnthropic.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // lblModelsInfo
            // 
            lblModelsInfo.BackColor = Color.Transparent;
            lblModelsInfo.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            lblModelsInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblModelsInfo.Location = new Point(16, 139);
            lblModelsInfo.Name = "lblModelsInfo";
            lblModelsInfo.Size = new Size(120, 23);
            lblModelsInfo.TabIndex = 6;
            lblModelsInfo.Text = "模型列表接口:";
            // 
            // txtModelsUrl
            // 
            txtModelsUrl.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtModelsUrl.Location = new Point(16, 162);
            txtModelsUrl.Margin = new Padding(4, 5, 4, 5);
            txtModelsUrl.MinimumSize = new Size(1, 16);
            txtModelsUrl.Name = "txtModelsUrl";
            txtModelsUrl.Padding = new Padding(5);
            txtModelsUrl.ReadOnly = true;
            txtModelsUrl.ShowText = false;
            txtModelsUrl.Size = new Size(791, 29);
            txtModelsUrl.TabIndex = 7;
            txtModelsUrl.TextAlignment = ContentAlignment.MiddleLeft;
            txtModelsUrl.Watermark = "";
            // 
            // btnCopyModels
            // 
            btnCopyModels.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCopyModels.Location = new Point(814, 159);
            btnCopyModels.MinimumSize = new Size(1, 1);
            btnCopyModels.Name = "btnCopyModels";
            btnCopyModels.Size = new Size(75, 32);
            btnCopyModels.TabIndex = 8;
            btnCopyModels.Text = "复制";
            btnCopyModels.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabChannels
            // 
            tabChannels.Controls.Add(dgvChannels);
            tabChannels.Controls.Add(btnAddChannel);
            tabChannels.Controls.Add(btnEditChannel);
            tabChannels.Controls.Add(btnDeleteChannel);
            tabChannels.Controls.Add(btnToggleChannel);
            tabChannels.Location = new Point(0, 40);
            tabChannels.Name = "tabChannels";
            tabChannels.Size = new Size(200, 60);
            tabChannels.TabIndex = 1;
            tabChannels.Text = "渠道管理";
            // 
            // dgvChannels
            // 
            dgvChannels.AllowUserToAddRows = false;
            dgvChannels.AllowUserToDeleteRows = false;
            dgvChannels.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChannels.BackgroundColor = Color.White;
            dgvChannels.BorderStyle = BorderStyle.None;
            dgvChannels.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvChannels.ColumnHeadersHeight = 32;
            dgvChannels.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(64, 158, 255);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvChannels.DefaultCellStyle = dataGridViewCellStyle1;
            dgvChannels.GridColor = Color.FromArgb(230, 230, 230);
            dgvChannels.Location = new Point(7, 43);
            dgvChannels.MultiSelect = false;
            dgvChannels.Name = "dgvChannels";
            dgvChannels.ReadOnly = true;
            dgvChannels.RowHeadersVisible = false;
            dgvChannels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvChannels.Size = new Size(887, 466);
            dgvChannels.TabIndex = 0;
            // 
            // btnAddChannel
            // 
            btnAddChannel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnAddChannel.Location = new Point(10, 6);
            btnAddChannel.MinimumSize = new Size(1, 1);
            btnAddChannel.Name = "btnAddChannel";
            btnAddChannel.Size = new Size(100, 32);
            btnAddChannel.TabIndex = 1;
            btnAddChannel.Text = "添加渠道";
            btnAddChannel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnEditChannel
            // 
            btnEditChannel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnEditChannel.Location = new Point(120, 6);
            btnEditChannel.MinimumSize = new Size(1, 1);
            btnEditChannel.Name = "btnEditChannel";
            btnEditChannel.Size = new Size(100, 32);
            btnEditChannel.TabIndex = 2;
            btnEditChannel.Text = "编辑渠道";
            btnEditChannel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnDeleteChannel
            // 
            btnDeleteChannel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnDeleteChannel.Location = new Point(230, 6);
            btnDeleteChannel.MinimumSize = new Size(1, 1);
            btnDeleteChannel.Name = "btnDeleteChannel";
            btnDeleteChannel.Size = new Size(100, 32);
            btnDeleteChannel.TabIndex = 3;
            btnDeleteChannel.Text = "删除渠道";
            btnDeleteChannel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnToggleChannel
            // 
            btnToggleChannel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnToggleChannel.Location = new Point(340, 6);
            btnToggleChannel.MinimumSize = new Size(1, 1);
            btnToggleChannel.Name = "btnToggleChannel";
            btnToggleChannel.Size = new Size(100, 32);
            btnToggleChannel.TabIndex = 4;
            btnToggleChannel.Text = "启用/禁用";
            btnToggleChannel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // tabLogs
            // 
            tabLogs.Controls.Add(dgvLogs);
            tabLogs.Controls.Add(txtLogOutput);
            tabLogs.Controls.Add(btnClearLogs);
            tabLogs.Controls.Add(lblMaxLogs);
            tabLogs.Controls.Add(numMaxLogs);
            tabLogs.Location = new Point(0, 40);
            tabLogs.Name = "tabLogs";
            tabLogs.Size = new Size(200, 60);
            tabLogs.TabIndex = 2;
            tabLogs.Text = "请求日志";
            // 
            // dgvLogs
            // 
            dgvLogs.AllowUserToAddRows = false;
            dgvLogs.AllowUserToDeleteRows = false;
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLogs.BackgroundColor = Color.White;
            dgvLogs.BorderStyle = BorderStyle.None;
            dgvLogs.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvLogs.ColumnHeadersHeight = 26;
            dgvLogs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(64, 158, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvLogs.DefaultCellStyle = dataGridViewCellStyle2;
            dgvLogs.GridColor = Color.FromArgb(230, 230, 230);
            dgvLogs.Location = new Point(8, 37);
            dgvLogs.Name = "dgvLogs";
            dgvLogs.ReadOnly = true;
            dgvLogs.RowHeadersVisible = false;
            dgvLogs.RowTemplate.Height = 24;
            dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLogs.Size = new Size(887, 280);
            dgvLogs.TabIndex = 0;
            // 
            // txtLogOutput
            // 
            txtLogOutput.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtLogOutput.Location = new Point(8, 325);
            txtLogOutput.Margin = new Padding(4, 5, 4, 5);
            txtLogOutput.MinimumSize = new Size(1, 16);
            txtLogOutput.Multiline = true;
            txtLogOutput.Name = "txtLogOutput";
            txtLogOutput.Padding = new Padding(5);
            txtLogOutput.ReadOnly = true;
            txtLogOutput.ShowText = false;
            txtLogOutput.Size = new Size(887, 180);
            txtLogOutput.TabIndex = 1;
            txtLogOutput.TextAlignment = ContentAlignment.MiddleLeft;
            txtLogOutput.Watermark = "";
            // 
            // btnClearLogs
            // 
            btnClearLogs.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearLogs.Location = new Point(789, 8);
            btnClearLogs.MinimumSize = new Size(1, 1);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(100, 23);
            btnClearLogs.TabIndex = 2;
            btnClearLogs.Text = "清空日志";
            btnClearLogs.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // lblMaxLogs
            // 
            lblMaxLogs.Font = new Font("宋体", 10F);
            lblMaxLogs.ForeColor = Color.FromArgb(48, 48, 48);
            lblMaxLogs.Location = new Point(560, 12);
            lblMaxLogs.Name = "lblMaxLogs";
            lblMaxLogs.Size = new Size(75, 18);
            lblMaxLogs.TabIndex = 3;
            lblMaxLogs.Text = "日志上限:";
            lblMaxLogs.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numMaxLogs
            // 
            numMaxLogs.Location = new Point(635, 8);
            numMaxLogs.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
            numMaxLogs.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numMaxLogs.Name = "numMaxLogs";
            numMaxLogs.Size = new Size(80, 26);
            numMaxLogs.TabIndex = 4;
            numMaxLogs.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblRequestCount });
            statusStrip.Location = new Point(0, 587);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(900, 22);
            statusStrip.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 17);
            // 
            // lblRequestCount
            // 
            lblRequestCount.Name = "lblRequestCount";
            lblRequestCount.Size = new Size(0, 17);
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(900, 609);
            Controls.Add(tabControl);
            Controls.Add(statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            ShowShadow = false;
            Text = "Fun AI Gateway - AI聚合网关";
            ZoomScaleRect = new Rectangle(15, 15, 900, 560);
            tabControl.ResumeLayout(false);
            tabSettings.ResumeLayout(false);
            grpNetwork.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            grpAuth.ResumeLayout(false);
            grpAuth.PerformLayout();
            grpConnectionInfo.ResumeLayout(false);
            tabChannels.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvChannels).EndInit();
            tabLogs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLogs).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxLogs).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private UIButton uiButton1;
        private UIButton uiButton2;
    }
}
