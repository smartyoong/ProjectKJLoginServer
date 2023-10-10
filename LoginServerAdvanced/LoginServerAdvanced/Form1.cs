using System.Buffers;
using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipelines;
using Microsoft.Data.SqlClient;

namespace LoginServerAdvanced
{
    public partial class LoginServer : Form
    {
        public LoginServer()
        {
            InitializeComponent();
        }
        private LoginCore LoginServerCore = new LoginCore();
        private void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
                MessageBox.Show("이미 서버가 실행중입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("서버를 시작하시겠습니까?", "시작", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LoginServerLogList.Items.Add(LogItemAddTime("서버를 시작합니다."));
                    LoginServerCore.InitLoginServer();
                }
            }
        }
        public string LogItemAddTime(string LogContext)
        {

            string Temp = string.Format("{0,-40}{1}", LogContext, DateTime.Now.ToString());
            return Temp;
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {

        }

        private void ServerReSetButton_Click(object sender, EventArgs e)
        {

        }
    }
}


/*로그인 서버는 DB확인을 통해 유효한 로그인 요청인지 확인한 후, 메모리상에 로그인을한 유저들을 보유만하고 있는다.
 * 그리고 클라이언트를 게임서버와 통신하도록 유도하고, 게임 서버는 로그인 서버에게 해당 유저가 로그인을 했는지 요청이 들어오면,
 * 로그인 서버는 응답을 해주는 것으로 역할을 끝낸다.
 */

class LoginCore
{
    private Dictionary<string, int> LoginUsers; // ID,랜덤값으로 유효성 검사
    private Socket ListenSocket;
    private Socket GameServerSocket;
    private SqlConnection AccountDBConnect;
    private Pipe LoginPipeLines;
    private bool IsServerRun = false;

    public void InitLoginServer()
    {
        IsServerRun = true;
        InitClientSocketServer();
    }
    public bool IsServerOn()
    {
        return IsServerRun;
    }
    private void InitClientSocketServer()
    {
        ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 11220));
        ListenSocket.Listen(1000);
        LoginPipeLines = new Pipe();

    }
    private void InitGameSocketServerConnect()
    {

    }
    private void InitDBServerConnect()
    {

    }
    private async Task RunClientSocketServer()
    {
        while (true)
        {
            Socket clientSocket = await ListenSocket.AcceptAsync();

            Task writing = FillPipeAsync(clientSocket, LoginPipeLines.Writer);
            Task reading = ReadPipeAsync(LoginPipeLines.Reader);

            await Task.WhenAll(writing, reading);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
    private static async Task FillPipeAsync(Socket socket, PipeWriter writer)
    {
        const int minimumBufferSize = 512;

        while (true)
        {
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);

            int bytesRead = await socket.ReceiveAsync(memory);

            if (bytesRead == 0)
            {
                break;
            }

            writer.Advance(bytesRead);
            await writer.FlushAsync();
        }

        writer.Complete();
    }
    private static async Task ReadPipeAsync(PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                // 데이터 처리
            }

            reader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        reader.Complete();
    }
}