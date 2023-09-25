namespace LoginServer
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.LoginLogView = new System.Windows.Forms.ListView();
            this.ServerStartButton = new System.Windows.Forms.Button();
            this.ServerStopButton = new System.Windows.Forms.Button();
            this.ServerReSetButton = new System.Windows.Forms.Button();
            this.InfoVersionViewListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // LoginLogView
            // 
            this.LoginLogView.HideSelection = false;
            this.LoginLogView.Location = new System.Drawing.Point(12, 12);
            this.LoginLogView.Name = "LoginLogView";
            this.LoginLogView.Size = new System.Drawing.Size(509, 287);
            this.LoginLogView.TabIndex = 0;
            this.LoginLogView.UseCompatibleStateImageBehavior = false;
            // 
            // ServerStartButton
            // 
            this.ServerStartButton.Font = new System.Drawing.Font("휴먼모음T", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ServerStartButton.ForeColor = System.Drawing.Color.RoyalBlue;
            this.ServerStartButton.Location = new System.Drawing.Point(553, 12);
            this.ServerStartButton.Name = "ServerStartButton";
            this.ServerStartButton.Size = new System.Drawing.Size(210, 65);
            this.ServerStartButton.TabIndex = 1;
            this.ServerStartButton.Text = "서버 시작";
            this.ServerStartButton.UseVisualStyleBackColor = true;
            this.ServerStartButton.Click += new System.EventHandler(this.ServerStartButton_Click);
            // 
            // ServerStopButton
            // 
            this.ServerStopButton.Font = new System.Drawing.Font("휴먼모음T", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ServerStopButton.ForeColor = System.Drawing.Color.Red;
            this.ServerStopButton.Location = new System.Drawing.Point(553, 121);
            this.ServerStopButton.Name = "ServerStopButton";
            this.ServerStopButton.Size = new System.Drawing.Size(210, 65);
            this.ServerStopButton.TabIndex = 2;
            this.ServerStopButton.Text = "서버 종료";
            this.ServerStopButton.UseVisualStyleBackColor = true;
            this.ServerStopButton.Click += new System.EventHandler(this.ServerStopButton_Click);
            // 
            // ServerReSetButton
            // 
            this.ServerReSetButton.Font = new System.Drawing.Font("휴먼모음T", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ServerReSetButton.ForeColor = System.Drawing.Color.Green;
            this.ServerReSetButton.Location = new System.Drawing.Point(553, 234);
            this.ServerReSetButton.Name = "ServerReSetButton";
            this.ServerReSetButton.Size = new System.Drawing.Size(210, 65);
            this.ServerReSetButton.TabIndex = 3;
            this.ServerReSetButton.Text = "서버 재시작";
            this.ServerReSetButton.UseVisualStyleBackColor = true;
            this.ServerReSetButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // InfoVersionViewListBox
            // 
            this.InfoVersionViewListBox.FormattingEnabled = true;
            this.InfoVersionViewListBox.ItemHeight = 12;
            this.InfoVersionViewListBox.Location = new System.Drawing.Point(12, 339);
            this.InfoVersionViewListBox.Name = "InfoVersionViewListBox";
            this.InfoVersionViewListBox.Size = new System.Drawing.Size(751, 88);
            this.InfoVersionViewListBox.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.InfoVersionViewListBox);
            this.Controls.Add(this.ServerReSetButton);
            this.Controls.Add(this.ServerStopButton);
            this.Controls.Add(this.ServerStartButton);
            this.Controls.Add(this.LoginLogView);
            this.Name = "MainForm";
            this.Text = "LoginServer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView LoginLogView;
        private System.Windows.Forms.Button ServerStartButton;
        private System.Windows.Forms.Button ServerStopButton;
        private System.Windows.Forms.Button ServerReSetButton;
        private System.Windows.Forms.ListBox InfoVersionViewListBox;
    }
}

