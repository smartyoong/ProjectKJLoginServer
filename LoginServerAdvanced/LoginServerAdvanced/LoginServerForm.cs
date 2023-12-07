using LoginServerAdvanced.Properties;
using System.Media;
using System.Runtime.CompilerServices;

namespace LoginServerAdvanced
{
    public partial class LoginServer : Form
    {
        private LoginCore LoginServerCore = new LoginCore();
        private static string LogFilePath = string.Empty;
        private static StreamWriter? LogFileStream;
        private int ConnectedUsers = 0;
        private event Action ConnectedUserChangeEvent;

        public LoginServer()
        {
            InitializeComponent();
            LoginServerLogList.HorizontalScrollbar = true;
            InfoVersionViewListBox.BackColor = Color.Red;
            RenewUserCount();
            ConnectedUserChangeEvent += RenewUserCount;
            LoginServerCore.MainForm = this;
            LogFilePath = Settings.Default.LogDirectory;
            InfoVersionViewListBox.Items.Add("DB ������");
            InfoVersionViewListBox.Items.Add("Gate Server ������");
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
                    if(string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
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
            LogFileStream!.WriteLine(Temp);
            LogFileStream.Flush();
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {
            LoginServerCore.ShutDownServerCore();
            SetAllConnectReset();
            InfoVersionViewListBox.BackColor = Color.Red;
        }

        private async void ServerReSetButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �ٽ� �����Ͻðڽ��ϱ�?", "�����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    SetAllConnectReset();
                    InfoVersionViewListBox.BackColor = Color.Red;
                    LoginServerCore.ShutDownServerCore();
                    LogItemAddTime("������ �ٽ� �����ϱ� ���� �غ��մϴ�.");
                    InfoVersionViewListBox.BackColor = Color.Yellow;
                    // �񵿱������� ������ �����Ǳ⵵ ���� �ٷ� ������Ǵ°��� ����
                    await Task.Delay(10000);
                    if (string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
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
                    if (string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
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
            FolderBrowserDialog FolderBrowserDialog = new FolderBrowserDialog();

            if (FolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string FolderPath = FolderBrowserDialog.SelectedPath;
                DateTime CurrentTime = DateTime.Now;
                string FormattedTime = CurrentTime.ToString("yyyy-MM-dd-HH-mm-ss");
                LogFilePath = Path.Combine(FolderPath, $"LOG{FormattedTime}.txt");
                if (!File.Exists(LogFilePath))
                {
                    File.Create(LogFilePath).Close();
                    if (LogFileStream != null)
                    {
                        LogFileStream.Close();
                        LogFileStream = null;
                    }
                    LogFileStream = new StreamWriter(LogFilePath, true);
                }
                Settings.Default.LogDirectory = LogFilePath;
                Settings.Default.Save();
            }
        }
        private void SetFirstLogDirectory()
        {
            string EXEPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string EXEDirectory = Path.GetDirectoryName(EXEPath)!;
            DateTime CurrentTime = DateTime.Now;
            string FormattedTime = CurrentTime.ToString("yyyy-MM-dd-HH-mm-ss");
            string LogDirectory = Path.Combine(EXEDirectory, $"Logs");
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            LogFilePath = Path.Combine(LogDirectory, $"LOG{FormattedTime}.txt");
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
                if (LogFileStream != null)
                {
                    LogFileStream.Close();
                    LogFileStream = null;
                }
                LogFileStream = new StreamWriter(LogFilePath, true);
            }
        }
        private void UseDefaultLogDirectory()
        {
            LogFilePath = Settings.Default.LogDirectory;
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
                if (LogFileStream != null)
                {
                    LogFileStream.Close();
                    LogFileStream = null;
                }
            }
            LogFileStream = new StreamWriter(LogFilePath, true);
        }

        private void FileLogPathOpen(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Notepad.exe", LogFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open folder: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        private void RenewUserCount()
        {
            ConnectUserTextBox.Text = ConnectedUsers.ToString();
        }
        public void IncreaseUserCount()
        {
            ConnectedUsers++;
            ConnectedUserChangeEvent.Invoke();
        }
        public void DecreaseUserCount()
        {
            ConnectedUsers--;
            ConnectedUserChangeEvent.Invoke();
        }
        public void SetDBConnectSucces(bool IsSuccess)
        {
            if(IsSuccess)
            {
                InfoVersionViewListBox.Items[0] = "DB ���� ����";
            }
            else
            {
                InfoVersionViewListBox.Items[0] = "DB ���� ����";
            }
        }
        public void SetGateServerSuccess( bool IsSuccess)
        {
            if(IsSuccess)
            {
                InfoVersionViewListBox.Items[1] = "GateServer ���� ����";
            }
            else
            {
                InfoVersionViewListBox.Items[1] = "GateServer ���� ����";
            }
        }
        public void SetAllConnectReset()
        {
            InfoVersionViewListBox.Items[0] = "DB ������";
            InfoVersionViewListBox.Items[1] = "GateServer ������";
        }

    }
}


/*�α��� ������ DBȮ���� ���� ��ȿ�� �α��� ��û���� Ȯ���� ��, �޸𸮻� �α������� �������� �������ϰ� �ִ´�.
 * �׸��� Ŭ���̾�Ʈ�� ���Ӽ����� ����ϵ��� �����ϰ�, ���� ������ �α��� �������� �ش� ������ �α����� �ߴ��� ��û�� ������,
 * �α��� ������ ������ ���ִ� ������ ������ ������.
 */

