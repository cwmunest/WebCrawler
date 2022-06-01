namespace WebCrawler
{
    partial class Form1
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.txtMainpage = new System.Windows.Forms.TextBox();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOpenSrcPath = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.fbDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.txtExcludepPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(89, 263);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(143, 43);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(261, 263);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(138, 43);
            this.btnStop.TabIndex = 12;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // txtMainpage
            // 
            this.txtMainpage.Location = new System.Drawing.Point(153, 29);
            this.txtMainpage.Name = "txtMainpage";
            this.txtMainpage.Size = new System.Drawing.Size(341, 21);
            this.txtMainpage.TabIndex = 29;
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(153, 71);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(341, 21);
            this.txtSavePath.TabIndex = 26;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(63, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 24);
            this.label5.TabIndex = 28;
            this.label5.Text = "目标网站首页：";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOpenSrcPath
            // 
            this.btnOpenSrcPath.Location = new System.Drawing.Point(500, 69);
            this.btnOpenSrcPath.Name = "btnOpenSrcPath";
            this.btnOpenSrcPath.Size = new System.Drawing.Size(86, 22);
            this.btnOpenSrcPath.TabIndex = 27;
            this.btnOpenSrcPath.Text = "选择文件夹";
            this.btnOpenSrcPath.Click += new System.EventHandler(this.btnOpenSrcPath_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(63, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 24);
            this.label6.TabIndex = 25;
            this.label6.Text = "文件保存路径：";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(434, 263);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(138, 43);
            this.btnClose.TabIndex = 30;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtExcludepPath
            // 
            this.txtExcludepPath.Location = new System.Drawing.Point(65, 132);
            this.txtExcludepPath.Multiline = true;
            this.txtExcludepPath.Name = "txtExcludepPath";
            this.txtExcludepPath.Size = new System.Drawing.Size(508, 108);
            this.txtExcludepPath.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(63, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 24);
            this.label1.TabIndex = 32;
            this.label1.Text = "排除路径（以逗号或回车分隔）：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 354);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtExcludepPath);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtMainpage);
            this.Controls.Add(this.txtSavePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOpenSrcPath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "网站爬虫";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TextBox txtMainpage;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOpenSrcPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.FolderBrowserDialog fbDialog;
        private System.Windows.Forms.TextBox txtExcludepPath;
        private System.Windows.Forms.Label label1;
    }
}

