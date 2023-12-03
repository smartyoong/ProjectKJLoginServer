namespace LoginServerAdvanced
{
    partial class LoginServer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LoginServerLogList = new ListBox();
            InfoVersionViewListBox = new ListBox();
            ServerStartButton = new Button();
            ServerStopButton = new Button();
            ServerReSetButton = new Button();
            menuStrip1 = new MenuStrip();
            파일로그ToolStripMenuItem = new ToolStripMenuItem();
            디렉토리설정ToolStripMenuItem = new ToolStripMenuItem();
            로그파일열기ToolStripMenuItem = new ToolStripMenuItem();
            label1 = new Label();
            ConnectUserTextBox = new TextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // LoginServerLogList
            // 
            LoginServerLogList.Font = new Font("휴먼모음T", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            LoginServerLogList.FormattingEnabled = true;
            LoginServerLogList.ItemHeight = 14;
            LoginServerLogList.Location = new Point(12, 26);
            LoginServerLogList.Name = "LoginServerLogList";
            LoginServerLogList.Size = new Size(620, 312);
            LoginServerLogList.TabIndex = 0;
            // 
            // InfoVersionViewListBox
            // 
            InfoVersionViewListBox.FormattingEnabled = true;
            InfoVersionViewListBox.ItemHeight = 15;
            InfoVersionViewListBox.Location = new Point(12, 407);
            InfoVersionViewListBox.Name = "InfoVersionViewListBox";
            InfoVersionViewListBox.Size = new Size(834, 64);
            InfoVersionViewListBox.TabIndex = 1;
            // 
            // ServerStartButton
            // 
            ServerStartButton.Font = new Font("휴먼모음T", 18F, FontStyle.Regular, GraphicsUnit.Point);
            ServerStartButton.ForeColor = Color.Blue;
            ServerStartButton.Location = new Point(656, 26);
            ServerStartButton.Name = "ServerStartButton";
            ServerStartButton.Size = new Size(190, 78);
            ServerStartButton.TabIndex = 2;
            ServerStartButton.Text = "서버 시작";
            ServerStartButton.UseVisualStyleBackColor = true;
            ServerStartButton.Click += ServerStartButton_Click;
            // 
            // ServerStopButton
            // 
            ServerStopButton.Font = new Font("휴먼모음T", 18F, FontStyle.Regular, GraphicsUnit.Point);
            ServerStopButton.ForeColor = Color.Red;
            ServerStopButton.Location = new Point(656, 143);
            ServerStopButton.Name = "ServerStopButton";
            ServerStopButton.Size = new Size(190, 86);
            ServerStopButton.TabIndex = 3;
            ServerStopButton.Text = "서버 종료";
            ServerStopButton.UseVisualStyleBackColor = true;
            ServerStopButton.Click += ServerStopButton_Click;
            // 
            // ServerReSetButton
            // 
            ServerReSetButton.Font = new Font("휴먼모음T", 18F, FontStyle.Regular, GraphicsUnit.Point);
            ServerReSetButton.ForeColor = Color.FromArgb(0, 192, 0);
            ServerReSetButton.Location = new Point(656, 260);
            ServerReSetButton.Name = "ServerReSetButton";
            ServerReSetButton.Size = new Size(190, 78);
            ServerReSetButton.TabIndex = 4;
            ServerReSetButton.Text = "서버 재시작";
            ServerReSetButton.UseVisualStyleBackColor = true;
            ServerReSetButton.Click += ServerReSetButton_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { 파일로그ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(858, 24);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // 파일로그ToolStripMenuItem
            // 
            파일로그ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { 디렉토리설정ToolStripMenuItem, 로그파일열기ToolStripMenuItem });
            파일로그ToolStripMenuItem.Name = "파일로그ToolStripMenuItem";
            파일로그ToolStripMenuItem.Size = new Size(67, 20);
            파일로그ToolStripMenuItem.Text = "파일로그";
            // 
            // 디렉토리설정ToolStripMenuItem
            // 
            디렉토리설정ToolStripMenuItem.Name = "디렉토리설정ToolStripMenuItem";
            디렉토리설정ToolStripMenuItem.Size = new Size(154, 22);
            디렉토리설정ToolStripMenuItem.Text = "디렉토리 설정";
            디렉토리설정ToolStripMenuItem.Click += SetFileLogDirectory;
            // 
            // 로그파일열기ToolStripMenuItem
            // 
            로그파일열기ToolStripMenuItem.Name = "로그파일열기ToolStripMenuItem";
            로그파일열기ToolStripMenuItem.Size = new Size(154, 22);
            로그파일열기ToolStripMenuItem.Text = "로그 파일 열기";
            로그파일열기ToolStripMenuItem.Click += FileLogPathOpen;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 368);
            label1.Name = "label1";
            label1.Size = new Size(70, 15);
            label1.TabIndex = 6;
            label1.Text = "동접자 수 : ";
            // 
            // ConnectUserTextBox
            // 
            ConnectUserTextBox.Location = new Point(88, 365);
            ConnectUserTextBox.Name = "ConnectUserTextBox";
            ConnectUserTextBox.ReadOnly = true;
            ConnectUserTextBox.Size = new Size(100, 23);
            ConnectUserTextBox.TabIndex = 7;
            ConnectUserTextBox.TextAlign = HorizontalAlignment.Right;
            // 
            // LoginServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(858, 498);
            Controls.Add(ConnectUserTextBox);
            Controls.Add(label1);
            Controls.Add(ServerReSetButton);
            Controls.Add(ServerStopButton);
            Controls.Add(ServerStartButton);
            Controls.Add(InfoVersionViewListBox);
            Controls.Add(LoginServerLogList);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "LoginServer";
            Text = "LoginServer";
            FormClosing += LoginServerFormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListBox InfoVersionViewListBox;
        private Button ServerStartButton;
        private Button ServerStopButton;
        private Button ServerReSetButton;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 파일로그ToolStripMenuItem;
        private ToolStripMenuItem 디렉토리설정ToolStripMenuItem;
        private ToolStripMenuItem 로그파일열기ToolStripMenuItem;
        private Label label1;
        private TextBox ConnectUserTextBox;
        private static ListBox LoginServerLogList;
    }
}