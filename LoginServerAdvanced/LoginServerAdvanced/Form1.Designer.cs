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
            SuspendLayout();
            // 
            // LoginServerLogList
            // 
            LoginServerLogList.Font = new Font("휴먼모음T", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            LoginServerLogList.FormattingEnabled = true;
            LoginServerLogList.ItemHeight = 14;
            LoginServerLogList.Location = new Point(12, 12);
            LoginServerLogList.Name = "LoginServerLogList";
            LoginServerLogList.Size = new Size(620, 326);
            LoginServerLogList.TabIndex = 0;
            // 
            // InfoVersionViewListBox
            // 
            InfoVersionViewListBox.FormattingEnabled = true;
            InfoVersionViewListBox.ItemHeight = 15;
            InfoVersionViewListBox.Location = new Point(12, 362);
            InfoVersionViewListBox.Name = "InfoVersionViewListBox";
            InfoVersionViewListBox.Size = new Size(834, 109);
            InfoVersionViewListBox.TabIndex = 1;
            // 
            // ServerStartButton
            // 
            ServerStartButton.Font = new Font("휴먼모음T", 18F, FontStyle.Regular, GraphicsUnit.Point);
            ServerStartButton.ForeColor = Color.Blue;
            ServerStartButton.Location = new Point(656, 12);
            ServerStartButton.Name = "ServerStartButton";
            ServerStartButton.Size = new Size(190, 86);
            ServerStartButton.TabIndex = 2;
            ServerStartButton.Text = "서버 시작";
            ServerStartButton.UseVisualStyleBackColor = true;
            ServerStartButton.Click += ServerStartButton_Click;
            // 
            // ServerStopButton
            // 
            ServerStopButton.Font = new Font("휴먼모음T", 18F, FontStyle.Regular, GraphicsUnit.Point);
            ServerStopButton.ForeColor = Color.Red;
            ServerStopButton.Location = new Point(656, 138);
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
            ServerReSetButton.Size = new Size(190, 86);
            ServerReSetButton.TabIndex = 4;
            ServerReSetButton.Text = "서버 재시작";
            ServerReSetButton.UseVisualStyleBackColor = true;
            ServerReSetButton.Click += ServerReSetButton_Click;
            // 
            // LoginServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(858, 498);
            Controls.Add(ServerReSetButton);
            Controls.Add(ServerStopButton);
            Controls.Add(ServerStartButton);
            Controls.Add(InfoVersionViewListBox);
            Controls.Add(LoginServerLogList);
            Name = "LoginServer";
            Text = "LoginServer";
            ResumeLayout(false);
        }

        #endregion

        private ListBox LoginServerLogList;
        private ListBox InfoVersionViewListBox;
        private Button ServerStartButton;
        private Button ServerStopButton;
        private Button ServerReSetButton;
    }
}