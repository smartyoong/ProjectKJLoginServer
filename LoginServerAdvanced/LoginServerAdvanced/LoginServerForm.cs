using System.Media;

namespace LoginServerAdvanced
{
    public partial class LoginServer : Form
    {
        private LoginCore LoginServerCore = new LoginCore();
        private static string LogFilePath = string.Empty;

        public LoginServer()
        {
            InitializeComponent();
            LoginServerLogList.HorizontalScrollbar = true;
            InfoVersionViewListBox.BackColor = Color.Red;
            SetFirstLogDirectory();
        }
        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
                MessageBox.Show("�̹� ������ �������Դϴ�.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �����Ͻðڽ��ϱ�?", "����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LogItemAddTime("������ �����մϴ�.");
                    if (!LoginServerCore.InitLoginServer())
                    {
                        LogItemAddTime("������ �����߿� ������ �߻��߽��ϴ�.");
                        return;
                    }
                    LoginServerCore.Run();
                    LogItemAddTime("�������� �Ϸ�");
                    InfoVersionViewListBox.BackColor = Color.Blue;
                }
            }
        }
        public static void LogItemAddTime(string LogContext)
        {
            string Temp = string.Format("{0,-25}{1}", DateTime.Now.ToString(), LogContext);
            if (LoginServerLogList.InvokeRequired)
            {
                LoginServerLogList.Invoke(new Action<string>(LogItemAddTime), LogContext);
            }
            else
                LoginServerLogList.Items.Add(Temp);
            File.AppendAllText(LogFilePath, Temp + Environment.NewLine);
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {
            LoginServerCore.ShutDownServerCore();
            InfoVersionViewListBox.BackColor = Color.Red;
        }

        private async void ServerReSetButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �ٽ� �����Ͻðڽ��ϱ�?", "�����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    InfoVersionViewListBox.BackColor = Color.Red;
                    LoginServerCore.ShutDownServerCore();
                    LogItemAddTime("������ �ٽ� �����ϱ� ���� �غ��մϴ�.");
                    InfoVersionViewListBox.BackColor = Color.Yellow;
                    // �񵿱������� ������ �����Ǳ⵵ ���� �ٷ� ������Ǵ°��� ����
                    await Task.Delay(10000);
                    LogItemAddTime("������ �����մϴ�.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("���� ���� ����");
                    LogItemAddTime("�������� �Ϸ�");
                    InfoVersionViewListBox.BackColor = Color.Blue;
                }
            }
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �����Ͻðڽ��ϱ�?", "����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LogItemAddTime("������ �����մϴ�.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("���� ���� ����");
                    LogItemAddTime("�������� �Ϸ�");
                    InfoVersionViewListBox.BackColor = Color.Blue;
                }
            }
        }

        private void LoginServerFormClosing(object sender, FormClosingEventArgs e)
        {
            if (LoginServerCore.IsServerOn())
            {
                LoginServerCore.ShutDownServerCore();
            }
        }

        private void SetFileLogDirectory(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (FolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string FolderPath = FolderBrowserDialog.SelectedPath;
                LogFilePath = Path.Combine(FolderPath, "log.txt");
                if (!File.Exists(LogFilePath))
                {
                    File.Create(LogFilePath);
                }

            }
        }
        private void SetFirstLogDirectory()
        {
            string EXEPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string EXEDirectory = Path.GetDirectoryName(EXEPath)!;
            string LogDirectory = Path.Combine(EXEDirectory, "logs");
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            LogFilePath = Path.Combine(LogDirectory, "log.txt");
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath);
            }
        }

        private void FileLogPathOpen(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(LogFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open folder: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}


/*�α��� ������ DBȮ���� ���� ��ȿ�� �α��� ��û���� Ȯ���� ��, �޸𸮻� �α������� �������� �������ϰ� �ִ´�.
 * �׸��� Ŭ���̾�Ʈ�� ���Ӽ����� ����ϵ��� �����ϰ�, ���� ������ �α��� �������� �ش� ������ �α����� �ߴ��� ��û�� ������,
 * �α��� ������ ������ ���ִ� ������ ������ ������.
 */
