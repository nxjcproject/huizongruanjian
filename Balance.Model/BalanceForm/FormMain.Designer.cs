namespace BalanceForm
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.button1 = new System.Windows.Forms.Button();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnMonth = new System.Windows.Forms.Button();
            this.textBox_LocalDigital = new System.Windows.Forms.TextBox();
            this.textBox_LocalAnalog = new System.Windows.Forms.TextBox();
            this.textBox_QueryThreadTime = new System.Windows.Forms.TextBox();
            this.groupBox_RealtimeDataTransport = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_Remot = new System.Windows.Forms.Label();
            this.label_Local = new System.Windows.Forms.Label();
            this.label_Analog = new System.Windows.Forms.Label();
            this.label_Digital = new System.Windows.Forms.Label();
            this.textBox_RemoteDigital = new System.Windows.Forms.TextBox();
            this.textBox_RemoteAnalog = new System.Windows.Forms.TextBox();
            this.pictureBox_Logo = new System.Windows.Forms.PictureBox();
            this.label_ = new System.Windows.Forms.Label();
            this.textBox_RemoteString = new System.Windows.Forms.TextBox();
            this.textBox_LocalString = new System.Windows.Forms.TextBox();
            this.groupBox_RealtimeDataTransport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Logo)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(54, 252);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "日数据插入";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(123, 225);
            this.txtDate.Name = "txtDate";
            this.txtDate.Size = new System.Drawing.Size(101, 21);
            this.txtDate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(82, 228);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "时间:";
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon1";
            this.notifyIcon.Visible = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 60000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnMonth
            // 
            this.btnMonth.Location = new System.Drawing.Point(202, 252);
            this.btnMonth.Name = "btnMonth";
            this.btnMonth.Size = new System.Drawing.Size(75, 23);
            this.btnMonth.TabIndex = 3;
            this.btnMonth.Text = "月数据插入";
            this.btnMonth.UseVisualStyleBackColor = true;
            this.btnMonth.Visible = false;
            this.btnMonth.Click += new System.EventHandler(this.btnMonth_Click);
            // 
            // textBox_LocalDigital
            // 
            this.textBox_LocalDigital.Location = new System.Drawing.Point(55, 41);
            this.textBox_LocalDigital.Name = "textBox_LocalDigital";
            this.textBox_LocalDigital.ReadOnly = true;
            this.textBox_LocalDigital.Size = new System.Drawing.Size(62, 21);
            this.textBox_LocalDigital.TabIndex = 4;
            // 
            // textBox_LocalAnalog
            // 
            this.textBox_LocalAnalog.Location = new System.Drawing.Point(55, 68);
            this.textBox_LocalAnalog.Name = "textBox_LocalAnalog";
            this.textBox_LocalAnalog.ReadOnly = true;
            this.textBox_LocalAnalog.Size = new System.Drawing.Size(62, 21);
            this.textBox_LocalAnalog.TabIndex = 5;
            // 
            // textBox_QueryThreadTime
            // 
            this.textBox_QueryThreadTime.Location = new System.Drawing.Point(235, 41);
            this.textBox_QueryThreadTime.Name = "textBox_QueryThreadTime";
            this.textBox_QueryThreadTime.ReadOnly = true;
            this.textBox_QueryThreadTime.Size = new System.Drawing.Size(75, 21);
            this.textBox_QueryThreadTime.TabIndex = 6;
            // 
            // groupBox_RealtimeDataTransport
            // 
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label_);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_RemoteString);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_LocalString);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label2);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label_Remot);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label_Local);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label_Analog);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.label_Digital);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_RemoteDigital);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_LocalDigital);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_RemoteAnalog);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_QueryThreadTime);
            this.groupBox_RealtimeDataTransport.Controls.Add(this.textBox_LocalAnalog);
            this.groupBox_RealtimeDataTransport.Location = new System.Drawing.Point(12, 12);
            this.groupBox_RealtimeDataTransport.Name = "groupBox_RealtimeDataTransport";
            this.groupBox_RealtimeDataTransport.Size = new System.Drawing.Size(316, 136);
            this.groupBox_RealtimeDataTransport.TabIndex = 7;
            this.groupBox_RealtimeDataTransport.TabStop = false;
            this.groupBox_RealtimeDataTransport.Text = "实时数据传输";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(233, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "线程最大时间";
            // 
            // label_Remot
            // 
            this.label_Remot.AutoSize = true;
            this.label_Remot.Location = new System.Drawing.Point(139, 23);
            this.label_Remot.Name = "label_Remot";
            this.label_Remot.Size = new System.Drawing.Size(29, 12);
            this.label_Remot.TabIndex = 8;
            this.label_Remot.Text = "远程";
            // 
            // label_Local
            // 
            this.label_Local.AutoSize = true;
            this.label_Local.Location = new System.Drawing.Point(68, 23);
            this.label_Local.Name = "label_Local";
            this.label_Local.Size = new System.Drawing.Size(29, 12);
            this.label_Local.TabIndex = 8;
            this.label_Local.Text = "本地";
            // 
            // label_Analog
            // 
            this.label_Analog.AutoSize = true;
            this.label_Analog.Location = new System.Drawing.Point(8, 71);
            this.label_Analog.Name = "label_Analog";
            this.label_Analog.Size = new System.Drawing.Size(41, 12);
            this.label_Analog.TabIndex = 7;
            this.label_Analog.Text = "模拟量";
            // 
            // label_Digital
            // 
            this.label_Digital.AutoSize = true;
            this.label_Digital.Location = new System.Drawing.Point(8, 44);
            this.label_Digital.Name = "label_Digital";
            this.label_Digital.Size = new System.Drawing.Size(41, 12);
            this.label_Digital.TabIndex = 7;
            this.label_Digital.Text = "开关量";
            // 
            // textBox_RemoteDigital
            // 
            this.textBox_RemoteDigital.Location = new System.Drawing.Point(123, 41);
            this.textBox_RemoteDigital.Name = "textBox_RemoteDigital";
            this.textBox_RemoteDigital.ReadOnly = true;
            this.textBox_RemoteDigital.Size = new System.Drawing.Size(62, 21);
            this.textBox_RemoteDigital.TabIndex = 4;
            // 
            // textBox_RemoteAnalog
            // 
            this.textBox_RemoteAnalog.Location = new System.Drawing.Point(123, 68);
            this.textBox_RemoteAnalog.Name = "textBox_RemoteAnalog";
            this.textBox_RemoteAnalog.ReadOnly = true;
            this.textBox_RemoteAnalog.Size = new System.Drawing.Size(62, 21);
            this.textBox_RemoteAnalog.TabIndex = 5;
            // 
            // pictureBox_Logo
            // 
            this.pictureBox_Logo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Logo.Image")));
            this.pictureBox_Logo.Location = new System.Drawing.Point(73, 154);
            this.pictureBox_Logo.Name = "pictureBox_Logo";
            this.pictureBox_Logo.Size = new System.Drawing.Size(184, 55);
            this.pictureBox_Logo.TabIndex = 8;
            this.pictureBox_Logo.TabStop = false;
            // 
            // label_
            // 
            this.label_.AutoSize = true;
            this.label_.Location = new System.Drawing.Point(8, 98);
            this.label_.Name = "label_";
            this.label_.Size = new System.Drawing.Size(41, 12);
            this.label_.TabIndex = 12;
            this.label_.Text = "字符串";
            // 
            // textBox_RemoteString
            // 
            this.textBox_RemoteString.Location = new System.Drawing.Point(123, 95);
            this.textBox_RemoteString.Name = "textBox_RemoteString";
            this.textBox_RemoteString.ReadOnly = true;
            this.textBox_RemoteString.Size = new System.Drawing.Size(62, 21);
            this.textBox_RemoteString.TabIndex = 10;
            // 
            // textBox_LocalString
            // 
            this.textBox_LocalString.Location = new System.Drawing.Point(55, 95);
            this.textBox_LocalString.Name = "textBox_LocalString";
            this.textBox_LocalString.ReadOnly = true;
            this.textBox_LocalString.Size = new System.Drawing.Size(62, 21);
            this.textBox_LocalString.TabIndex = 11;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(340, 287);
            this.Controls.Add(this.pictureBox_Logo);
            this.Controls.Add(this.groupBox_RealtimeDataTransport);
            this.Controls.Add(this.btnMonth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDate);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "恒拓数据服务程序";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox_RealtimeDataTransport.ResumeLayout(false);
            this.groupBox_RealtimeDataTransport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnMonth;
        private System.Windows.Forms.TextBox textBox_LocalDigital;
        private System.Windows.Forms.TextBox textBox_LocalAnalog;
        private System.Windows.Forms.TextBox textBox_QueryThreadTime;
        private System.Windows.Forms.GroupBox groupBox_RealtimeDataTransport;
        private System.Windows.Forms.Label label_Local;
        private System.Windows.Forms.Label label_Analog;
        private System.Windows.Forms.Label label_Digital;
        private System.Windows.Forms.TextBox textBox_RemoteDigital;
        private System.Windows.Forms.TextBox textBox_RemoteAnalog;
        private System.Windows.Forms.PictureBox pictureBox_Logo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_Remot;
        private System.Windows.Forms.Label label_;
        private System.Windows.Forms.TextBox textBox_RemoteString;
        private System.Windows.Forms.TextBox textBox_LocalString;
    }
}

