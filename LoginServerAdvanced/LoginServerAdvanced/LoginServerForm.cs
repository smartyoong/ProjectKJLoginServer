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
            InfoVersionViewListBox.Items.Add("DB 연결전");
            InfoVersionViewListBox.Items.Add("Gate Server 연결전");
        }
        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
                MessageBox.Show("이미 서버가 실행중입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("서버를 시작하시겠습니까?", "시작", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if(string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
                    LogItemAddTime("서버를 시작합니다.");
                    if (!LoginServerCore.InitLoginServer())
                    {
                        LogItemAddTime("서버를 시작중에 오류가 발생했습니다.");
                        return;
                    }
                    LoginServerCore.Run();
                    LogItemAddTime("서버오픈 완료");
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
                if (MessageBox.Show("서버를 다시 시작하시겠습니까?", "재시작", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    SetAllConnectReset();
                    InfoVersionViewListBox.BackColor = Color.Red;
                    LoginServerCore.ShutDownServerCore();
                    LogItemAddTime("서버를 다시 시작하기 위해 준비합니다.");
                    InfoVersionViewListBox.BackColor = Color.Yellow;
                    // 비동기적으로 소켓이 정리되기도 전에 바로 재생성되는것을 방지
                    await Task.Delay(10000);
                    if (string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
                    LogItemAddTime("서버를 시작합니다.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("서버 버퍼 시작");
                    LogItemAddTime("서버오픈 완료");
                    InfoVersionViewListBox.BackColor = Color.Blue;
                }
            }
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("서버를 시작하시겠습니까?", "시작", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(Settings.Default.LogDirectory))
                        SetFirstLogDirectory();
                    else
                        UseDefaultLogDirectory();
                    LogItemAddTime("서버를 시작합니다.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("서버 버퍼 시작");
                    LogItemAddTime("서버오픈 완료");
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
                InfoVersionViewListBox.Items[0] = "DB 연결 성공";
            }
            else
            {
                InfoVersionViewListBox.Items[0] = "DB 연결 실패";
            }
        }
        public void SetGateServerSuccess( bool IsSuccess)
        {
            if(IsSuccess)
            {
                InfoVersionViewListBox.Items[1] = "GateServer 연결 성공";
            }
            else
            {
                InfoVersionViewListBox.Items[1] = "GateServer 연결 실패";
            }
        }
        public void SetAllConnectReset()
        {
            InfoVersionViewListBox.Items[0] = "DB 연결전";
            InfoVersionViewListBox.Items[1] = "GateServer 연결전";
        }

    }
}


/*로그인 서버는 DB확인을 통해 유효한 로그인 요청인지 확인한 후, 메모리상에 로그인을한 유저들을 보유만하고 있는다.
 * 그리고 클라이언트를 게임서버와 통신하도록 유도하고, 게임 서버는 로그인 서버에게 해당 유저가 로그인을 했는지 요청이 들어오면,
 * 로그인 서버는 응답을 해주는 것으로 역할을 끝낸다.
 */

