using FunAiGateway.Models;
using Sunny.UI;

namespace FunAiGateway.Forms
{
    partial class LogSettingsDialog : UIForm
    {
        private System.ComponentModel.IContainer components = null;

        private UIGroupBox grpDuration;
        private UILabel lblDurationYellow;
        private System.Windows.Forms.NumericUpDown numDurationYellow;
        private UILabel lblDurationOrange;
        private System.Windows.Forms.NumericUpDown numDurationOrange;
        private UILabel lblDurationRed;
        private System.Windows.Forms.NumericUpDown numDurationRed;

        private UIGroupBox grpInputToken;
        private UILabel lblInputTokenOrange;
        private System.Windows.Forms.NumericUpDown numInputTokenOrange;
        private UILabel lblInputTokenRed;
        private System.Windows.Forms.NumericUpDown numInputTokenRed;

        private UIGroupBox grpOutputToken;
        private UILabel lblOutputTokenOrange;
        private System.Windows.Forms.NumericUpDown numOutputTokenOrange;
        private UILabel lblOutputTokenRed;
        private System.Windows.Forms.NumericUpDown numOutputTokenRed;

        private UIGroupBox grpGeneral;
        private UILabel lblMaxLogCount;
        private System.Windows.Forms.NumericUpDown numMaxLogCount;
        private UILabel lblLogRetentionDays;
        private System.Windows.Forms.NumericUpDown numLogRetentionDays;

        private UIGroupBox grpResponseLog;
        private System.Windows.Forms.CheckBox chkEnableResponseLog;
        private System.Windows.Forms.CheckBox chkLog2xx;
        private System.Windows.Forms.CheckBox chkLog4xx;
        private System.Windows.Forms.CheckBox chkLog5xx;
        private System.Windows.Forms.CheckBox chkLogOther;

        private UIButton btnOK;
        private UIButton btnCancel;

        public LogColorConfig LogColor { get; private set; }
        public int MaxLogCount { get; private set; }
        public int LogRetentionDays { get; private set; }
        public bool EnableResponseLog { get; private set; }
        public ResponseLogConfig ResponseLog { get; private set; }

        public LogSettingsDialog(LogColorConfig config, int maxLogCount, int logRetentionDays, bool enableResponseLog, ResponseLogConfig responseLog)
        {
            InitializeComponent();

            LogColor = new LogColorConfig
            {
                DurationYellow = config.DurationYellow,
                DurationOrange = config.DurationOrange,
                DurationRed = config.DurationRed,
                InputTokenOrange = config.InputTokenOrange,
                InputTokenRed = config.InputTokenRed,
                OutputTokenOrange = config.OutputTokenOrange,
                OutputTokenRed = config.OutputTokenRed
            };
            MaxLogCount = maxLogCount;
            LogRetentionDays = logRetentionDays;
            EnableResponseLog = enableResponseLog;
            ResponseLog = new ResponseLogConfig
            {
                Log2xx = responseLog.Log2xx,
                Log4xx = responseLog.Log4xx,
                Log5xx = responseLog.Log5xx,
                LogOther = responseLog.LogOther
            };

            // 加载当前值
            numDurationYellow.Value = config.DurationYellow / 1000;
            numDurationOrange.Value = config.DurationOrange / 1000;
            numDurationRed.Value = config.DurationRed / 1000;
            numInputTokenOrange.Value = config.InputTokenOrange;
            numInputTokenRed.Value = config.InputTokenRed;
            numOutputTokenOrange.Value = config.OutputTokenOrange;
            numOutputTokenRed.Value = config.OutputTokenRed;
            numMaxLogCount.Value = Math.Clamp(maxLogCount, (int)numMaxLogCount.Minimum, (int)numMaxLogCount.Maximum);
            numLogRetentionDays.Value = Math.Clamp(logRetentionDays, (int)numLogRetentionDays.Minimum, (int)numLogRetentionDays.Maximum);

            // 响应日志设置
            chkEnableResponseLog.Checked = enableResponseLog;
            chkLog2xx.Checked = responseLog.Log2xx;
            chkLog4xx.Checked = responseLog.Log4xx;
            chkLog5xx.Checked = responseLog.Log5xx;
            chkLogOther.Checked = responseLog.LogOther;
            UpdateResponseLogCheckboxState(null, EventArgs.Empty);
            chkEnableResponseLog.CheckedChanged += UpdateResponseLogCheckboxState;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            // 校验阈值递增关系
            var dy = (int)numDurationYellow.Value;
            var dor = (int)numDurationOrange.Value;
            var dr = (int)numDurationRed.Value;
            if (dy >= dor || dor >= dr)
            {
                this.ShowErrorDialog("响应时间阈值必须满足：黄色 < 橙色 < 红色");
                return;
            }
            var ito = (int)numInputTokenOrange.Value;
            var itr = (int)numInputTokenRed.Value;
            if (ito >= itr)
            {
                this.ShowErrorDialog("输入Token阈值必须满足：橙色 < 红色");
                return;
            }
            var oto = (int)numOutputTokenOrange.Value;
            var otr = (int)numOutputTokenRed.Value;
            if (oto >= otr)
            {
                this.ShowErrorDialog("输出Token阈值必须满足：橙色 < 红色");
                return;
            }

            LogColor.DurationYellow = dy * 1000;
            LogColor.DurationOrange = dor * 1000;
            LogColor.DurationRed = dr * 1000;
            LogColor.InputTokenOrange = ito;
            LogColor.InputTokenRed = itr;
            LogColor.OutputTokenOrange = oto;
            LogColor.OutputTokenRed = otr;
            MaxLogCount = (int)numMaxLogCount.Value;
            LogRetentionDays = (int)numLogRetentionDays.Value;

            // 响应日志设置
            EnableResponseLog = chkEnableResponseLog.Checked;
            ResponseLog.Log2xx = chkLog2xx.Checked;
            ResponseLog.Log4xx = chkLog4xx.Checked;
            ResponseLog.Log5xx = chkLog5xx.Checked;
            ResponseLog.LogOther = chkLogOther.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        // 总开关关闭时，禁用状态码范围复选框
        private void UpdateResponseLogCheckboxState(object? sender, EventArgs e)
        {
            var enabled = chkEnableResponseLog.Checked;
            chkLog2xx.Enabled = enabled;
            chkLog4xx.Enabled = enabled;
            chkLog5xx.Enabled = enabled;
            chkLogOther.Enabled = enabled;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogSettingsDialog));
            grpDuration = new UIGroupBox();
            lblDurationYellow = new UILabel();
            numDurationYellow = new NumericUpDown();
            lblDurationOrange = new UILabel();
            numDurationOrange = new NumericUpDown();
            lblDurationRed = new UILabel();
            numDurationRed = new NumericUpDown();
            grpInputToken = new UIGroupBox();
            lblInputTokenOrange = new UILabel();
            numInputTokenOrange = new NumericUpDown();
            lblInputTokenRed = new UILabel();
            numInputTokenRed = new NumericUpDown();
            grpOutputToken = new UIGroupBox();
            lblOutputTokenOrange = new UILabel();
            numOutputTokenOrange = new NumericUpDown();
            lblOutputTokenRed = new UILabel();
            numOutputTokenRed = new NumericUpDown();
            grpGeneral = new UIGroupBox();
            lblMaxLogCount = new UILabel();
            numMaxLogCount = new NumericUpDown();
            lblLogRetentionDays = new UILabel();
            numLogRetentionDays = new NumericUpDown();
            grpResponseLog = new UIGroupBox();
            chkEnableResponseLog = new CheckBox();
            chkLog2xx = new CheckBox();
            chkLog4xx = new CheckBox();
            chkLog5xx = new CheckBox();
            chkLogOther = new CheckBox();
            btnOK = new UIButton();
            btnCancel = new UIButton();
            grpDuration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDurationYellow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDurationOrange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDurationRed).BeginInit();
            grpInputToken.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInputTokenOrange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numInputTokenRed).BeginInit();
            grpOutputToken.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOutputTokenOrange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOutputTokenRed).BeginInit();
            grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxLogCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLogRetentionDays).BeginInit();
            grpResponseLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpDuration
            // 
            grpDuration.Controls.Add(lblDurationYellow);
            grpDuration.Controls.Add(numDurationYellow);
            grpDuration.Controls.Add(lblDurationOrange);
            grpDuration.Controls.Add(numDurationOrange);
            grpDuration.Controls.Add(lblDurationRed);
            grpDuration.Controls.Add(numDurationRed);
            grpDuration.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpDuration.Location = new Point(12, 37);
            grpDuration.Margin = new Padding(4, 5, 4, 5);
            grpDuration.MinimumSize = new Size(1, 1);
            grpDuration.Name = "grpDuration";
            grpDuration.Padding = new Padding(0, 32, 0, 0);
            grpDuration.Size = new Size(380, 130);
            grpDuration.TabIndex = 0;
            grpDuration.Text = "响应时间阈值（秒）";
            grpDuration.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // lblDurationYellow
            // 
            lblDurationYellow.BackColor = Color.Transparent;
            lblDurationYellow.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDurationYellow.ForeColor = Color.YellowGreen;
            lblDurationYellow.Location = new Point(16, 36);
            lblDurationYellow.Name = "lblDurationYellow";
            lblDurationYellow.Size = new Size(100, 23);
            lblDurationYellow.TabIndex = 0;
            lblDurationYellow.Text = "黄色 >=";
            // 
            // numDurationYellow
            // 
            numDurationYellow.Location = new Point(120, 34);
            numDurationYellow.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numDurationYellow.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDurationYellow.Name = "numDurationYellow";
            numDurationYellow.Size = new Size(100, 26);
            numDurationYellow.TabIndex = 1;
            numDurationYellow.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblDurationOrange
            // 
            lblDurationOrange.BackColor = Color.Transparent;
            lblDurationOrange.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDurationOrange.ForeColor = Color.Orange;
            lblDurationOrange.Location = new Point(16, 66);
            lblDurationOrange.Name = "lblDurationOrange";
            lblDurationOrange.Size = new Size(100, 23);
            lblDurationOrange.TabIndex = 2;
            lblDurationOrange.Text = "橙色 >=";
            // 
            // numDurationOrange
            // 
            numDurationOrange.Location = new Point(120, 64);
            numDurationOrange.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            numDurationOrange.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDurationOrange.Name = "numDurationOrange";
            numDurationOrange.Size = new Size(100, 26);
            numDurationOrange.TabIndex = 3;
            numDurationOrange.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // lblDurationRed
            // 
            lblDurationRed.BackColor = Color.Transparent;
            lblDurationRed.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDurationRed.ForeColor = Color.Red;
            lblDurationRed.Location = new Point(16, 96);
            lblDurationRed.Name = "lblDurationRed";
            lblDurationRed.Size = new Size(100, 23);
            lblDurationRed.TabIndex = 4;
            lblDurationRed.Text = "红色 >=";
            // 
            // numDurationRed
            // 
            numDurationRed.Location = new Point(120, 94);
            numDurationRed.Maximum = new decimal(new int[] { 900, 0, 0, 0 });
            numDurationRed.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDurationRed.Name = "numDurationRed";
            numDurationRed.Size = new Size(100, 26);
            numDurationRed.TabIndex = 5;
            numDurationRed.Value = new decimal(new int[] { 90, 0, 0, 0 });
            // 
            // grpInputToken
            // 
            grpInputToken.Controls.Add(lblInputTokenOrange);
            grpInputToken.Controls.Add(numInputTokenOrange);
            grpInputToken.Controls.Add(lblInputTokenRed);
            grpInputToken.Controls.Add(numInputTokenRed);
            grpInputToken.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpInputToken.Location = new Point(12, 168);
            grpInputToken.Margin = new Padding(4, 5, 4, 5);
            grpInputToken.MinimumSize = new Size(1, 1);
            grpInputToken.Name = "grpInputToken";
            grpInputToken.Padding = new Padding(0, 32, 0, 0);
            grpInputToken.Size = new Size(380, 100);
            grpInputToken.TabIndex = 1;
            grpInputToken.Text = "输入Token阈值";
            grpInputToken.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // lblInputTokenOrange
            // 
            lblInputTokenOrange.BackColor = Color.Transparent;
            lblInputTokenOrange.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblInputTokenOrange.ForeColor = Color.Orange;
            lblInputTokenOrange.Location = new Point(16, 36);
            lblInputTokenOrange.Name = "lblInputTokenOrange";
            lblInputTokenOrange.Size = new Size(100, 23);
            lblInputTokenOrange.TabIndex = 0;
            lblInputTokenOrange.Text = "橙色 >=";
            // 
            // numInputTokenOrange
            // 
            numInputTokenOrange.Location = new Point(120, 34);
            numInputTokenOrange.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numInputTokenOrange.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numInputTokenOrange.Name = "numInputTokenOrange";
            numInputTokenOrange.Size = new Size(100, 26);
            numInputTokenOrange.TabIndex = 1;
            numInputTokenOrange.Value = new decimal(new int[] { 50000, 0, 0, 0 });
            // 
            // lblInputTokenRed
            // 
            lblInputTokenRed.BackColor = Color.Transparent;
            lblInputTokenRed.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblInputTokenRed.ForeColor = Color.Red;
            lblInputTokenRed.Location = new Point(16, 66);
            lblInputTokenRed.Name = "lblInputTokenRed";
            lblInputTokenRed.Size = new Size(100, 23);
            lblInputTokenRed.TabIndex = 2;
            lblInputTokenRed.Text = "红色 >=";
            // 
            // numInputTokenRed
            // 
            numInputTokenRed.Location = new Point(120, 64);
            numInputTokenRed.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numInputTokenRed.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numInputTokenRed.Name = "numInputTokenRed";
            numInputTokenRed.Size = new Size(100, 26);
            numInputTokenRed.TabIndex = 3;
            numInputTokenRed.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            // 
            // grpOutputToken
            // 
            grpOutputToken.Controls.Add(lblOutputTokenOrange);
            grpOutputToken.Controls.Add(numOutputTokenOrange);
            grpOutputToken.Controls.Add(lblOutputTokenRed);
            grpOutputToken.Controls.Add(numOutputTokenRed);
            grpOutputToken.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpOutputToken.Location = new Point(12, 269);
            grpOutputToken.Margin = new Padding(4, 5, 4, 5);
            grpOutputToken.MinimumSize = new Size(1, 1);
            grpOutputToken.Name = "grpOutputToken";
            grpOutputToken.Padding = new Padding(0, 32, 0, 0);
            grpOutputToken.Size = new Size(380, 100);
            grpOutputToken.TabIndex = 2;
            grpOutputToken.Text = "输出Token阈值";
            grpOutputToken.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // lblOutputTokenOrange
            // 
            lblOutputTokenOrange.BackColor = Color.Transparent;
            lblOutputTokenOrange.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblOutputTokenOrange.ForeColor = Color.Orange;
            lblOutputTokenOrange.Location = new Point(16, 36);
            lblOutputTokenOrange.Name = "lblOutputTokenOrange";
            lblOutputTokenOrange.Size = new Size(100, 23);
            lblOutputTokenOrange.TabIndex = 0;
            lblOutputTokenOrange.Text = "橙色 >=";
            // 
            // numOutputTokenOrange
            // 
            numOutputTokenOrange.Location = new Point(120, 34);
            numOutputTokenOrange.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numOutputTokenOrange.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numOutputTokenOrange.Name = "numOutputTokenOrange";
            numOutputTokenOrange.Size = new Size(100, 26);
            numOutputTokenOrange.TabIndex = 1;
            numOutputTokenOrange.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblOutputTokenRed
            // 
            lblOutputTokenRed.BackColor = Color.Transparent;
            lblOutputTokenRed.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblOutputTokenRed.ForeColor = Color.Red;
            lblOutputTokenRed.Location = new Point(16, 66);
            lblOutputTokenRed.Name = "lblOutputTokenRed";
            lblOutputTokenRed.Size = new Size(100, 23);
            lblOutputTokenRed.TabIndex = 2;
            lblOutputTokenRed.Text = "红色 >=";
            // 
            // numOutputTokenRed
            // 
            numOutputTokenRed.Location = new Point(120, 64);
            numOutputTokenRed.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numOutputTokenRed.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numOutputTokenRed.Name = "numOutputTokenRed";
            numOutputTokenRed.Size = new Size(100, 26);
            numOutputTokenRed.TabIndex = 3;
            numOutputTokenRed.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // grpGeneral
            // 
            grpGeneral.Controls.Add(lblMaxLogCount);
            grpGeneral.Controls.Add(numMaxLogCount);
            grpGeneral.Controls.Add(lblLogRetentionDays);
            grpGeneral.Controls.Add(numLogRetentionDays);
            grpGeneral.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpGeneral.Location = new Point(12, 372);
            grpGeneral.Margin = new Padding(4, 5, 4, 5);
            grpGeneral.MinimumSize = new Size(1, 1);
            grpGeneral.Name = "grpGeneral";
            grpGeneral.Padding = new Padding(0, 32, 0, 0);
            grpGeneral.Size = new Size(380, 105);
            grpGeneral.TabIndex = 3;
            grpGeneral.Text = "通用设置";
            grpGeneral.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // lblMaxLogCount
            // 
            lblMaxLogCount.BackColor = Color.Transparent;
            lblMaxLogCount.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblMaxLogCount.ForeColor = Color.FromArgb(48, 48, 48);
            lblMaxLogCount.Location = new Point(16, 38);
            lblMaxLogCount.Name = "lblMaxLogCount";
            lblMaxLogCount.Size = new Size(135, 23);
            lblMaxLogCount.TabIndex = 0;
            lblMaxLogCount.Text = "窗口日志行上限:";
            // 
            // numMaxLogCount
            // 
            numMaxLogCount.Location = new Point(165, 35);
            numMaxLogCount.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
            numMaxLogCount.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numMaxLogCount.Name = "numMaxLogCount";
            numMaxLogCount.Size = new Size(100, 26);
            numMaxLogCount.TabIndex = 1;
            numMaxLogCount.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // lblLogRetentionDays
            // 
            lblLogRetentionDays.BackColor = Color.Transparent;
            lblLogRetentionDays.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblLogRetentionDays.ForeColor = Color.FromArgb(48, 48, 48);
            lblLogRetentionDays.Location = new Point(14, 74);
            lblLogRetentionDays.Name = "lblLogRetentionDays";
            lblLogRetentionDays.Size = new Size(145, 23);
            lblLogRetentionDays.TabIndex = 2;
            lblLogRetentionDays.Text = "日志文件保留天数:";
            // 
            // numLogRetentionDays
            // 
            numLogRetentionDays.Location = new Point(165, 71);
            numLogRetentionDays.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            numLogRetentionDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLogRetentionDays.Name = "numLogRetentionDays";
            numLogRetentionDays.Size = new Size(100, 26);
            numLogRetentionDays.TabIndex = 3;
            numLogRetentionDays.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // grpResponseLog
            // 
            grpResponseLog.Controls.Add(chkEnableResponseLog);
            grpResponseLog.Controls.Add(chkLog2xx);
            grpResponseLog.Controls.Add(chkLog4xx);
            grpResponseLog.Controls.Add(chkLog5xx);
            grpResponseLog.Controls.Add(chkLogOther);
            grpResponseLog.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            grpResponseLog.Location = new Point(12, 477);
            grpResponseLog.Margin = new Padding(4, 5, 4, 5);
            grpResponseLog.MinimumSize = new Size(1, 1);
            grpResponseLog.Name = "grpResponseLog";
            grpResponseLog.Padding = new Padding(0, 32, 0, 0);
            grpResponseLog.Size = new Size(380, 97);
            grpResponseLog.TabIndex = 4;
            grpResponseLog.Text = "响应内容日志（写入 logs/responses 目录）";
            grpResponseLog.TextAlignment = ContentAlignment.MiddleLeft;
            // 
            // chkEnableResponseLog
            // 
            chkEnableResponseLog.AutoSize = true;
            chkEnableResponseLog.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkEnableResponseLog.Location = new Point(16, 36);
            chkEnableResponseLog.Name = "chkEnableResponseLog";
            chkEnableResponseLog.Size = new Size(90, 20);
            chkEnableResponseLog.TabIndex = 0;
            chkEnableResponseLog.Text = "启用记录";
            // 
            // chkLog2xx
            // 
            chkLog2xx.AutoSize = true;
            chkLog2xx.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkLog2xx.Location = new Point(121, 36);
            chkLog2xx.Name = "chkLog2xx";
            chkLog2xx.Size = new Size(82, 20);
            chkLog2xx.TabIndex = 1;
            chkLog2xx.Text = "2xx成功";
            // 
            // chkLog4xx
            // 
            chkLog4xx.AutoSize = true;
            chkLog4xx.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkLog4xx.Location = new Point(16, 62);
            chkLog4xx.Name = "chkLog4xx";
            chkLog4xx.Size = new Size(98, 20);
            chkLog4xx.TabIndex = 2;
            chkLog4xx.Text = "4xx客户端";
            // 
            // chkLog5xx
            // 
            chkLog5xx.AutoSize = true;
            chkLog5xx.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkLog5xx.Location = new Point(121, 62);
            chkLog5xx.Name = "chkLog5xx";
            chkLog5xx.Size = new Size(98, 20);
            chkLog5xx.TabIndex = 3;
            chkLog5xx.Text = "5xx服务端";
            // 
            // chkLogOther
            // 
            chkLogOther.AutoSize = true;
            chkLogOther.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            chkLogOther.Location = new Point(232, 62);
            chkLogOther.Name = "chkLogOther";
            chkLogOther.Size = new Size(82, 20);
            chkLogOther.TabIndex = 4;
            chkLogOther.Text = "其他3xx";
            // 
            // btnOK
            // 
            btnOK.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOK.Location = new Point(100, 605);
            btnOK.MinimumSize = new Size(1, 1);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(100, 35);
            btnOK.TabIndex = 5;
            btnOK.Text = "确定";
            btnOK.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCancel.Location = new Point(210, 605);
            btnCancel.MinimumSize = new Size(1, 1);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "取消";
            btnCancel.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            // 
            // LogSettingsDialog
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(404, 650);
            Controls.Add(grpDuration);
            Controls.Add(grpInputToken);
            Controls.Add(grpOutputToken);
            Controls.Add(grpGeneral);
            Controls.Add(grpResponseLog);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LogSettingsDialog";
            ShowShadow = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "日志显示设置";
            ZoomScaleRect = new Rectangle(15, 15, 404, 490);
            grpDuration.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numDurationYellow).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDurationOrange).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDurationRed).EndInit();
            grpInputToken.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numInputTokenOrange).EndInit();
            ((System.ComponentModel.ISupportInitialize)numInputTokenRed).EndInit();
            grpOutputToken.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numOutputTokenOrange).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOutputTokenRed).EndInit();
            grpGeneral.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numMaxLogCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLogRetentionDays).EndInit();
            grpResponseLog.ResumeLayout(false);
            grpResponseLog.PerformLayout();
            ResumeLayout(false);
        }
    }
}
