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
using LoginServerAdvanced;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
                    LoginServerCore.InitDBServerConnect();
                    LoginServerLogList.Items.Add(LogItemAddTime("DB연결 성공"));
                    LoginServerCore.InitClientSocketServer();
                    LoginServerLogList.Items.Add(LogItemAddTime("클라이언트 오픈 준비 완료"));
                    LoginServerLogList.Items.Add(LogItemAddTime("서버오픈 완료"));
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
    private Dictionary<string, int> LoginUsers; // <ID,랜덤값>으로 유효성 검사 ID만 딱 보내서 위조 로그인 하는것을 방지
    private Socket ListenSocket;
    private Socket GameServerSocket;
    private SqlConnection AccountDBConnect;
    private Pipe LoginPipeLines;
    private bool IsServerRun = false;

    public void InitLoginServer()
    {
        IsServerRun = true;
    }
    public bool IsServerOn()
    {
        return IsServerRun;
    }
    public void InitClientSocketServer()
    {
        ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 11220));
        ListenSocket.Listen(1000);
        LoginPipeLines = new Pipe();

    }
    public void InitGameSocketServerConnect()
    {

    }
    public void InitDBServerConnect()
    {
        string SQLConnectString = string.Format("Server={0};Database={1};Integrated Security=SSPI;Encrypt=false;", "SMARTYOONG\\SQLEXPRESS", "AccountDB");
        AccountDBConnect = new SqlConnection(SQLConnectString);
        try
        {
            AccountDBConnect.Open();
        }
        catch(Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }
    // 사용법 연구중-----------------------------------------------------------------------------
    private async Task RunClientSocketServer()
    {
        while (true)
        {
            Socket clientSocket = await ListenSocket.AcceptAsync();

            Task writing = FillPipeAsync(clientSocket, LoginPipeLines.Writer);
            Task reading = ReadPipeAsync(LoginPipeLines.Reader);

            await Task.WhenAll(writing, reading);

        }
    }
    private async Task FillPipeAsync(Socket socket, PipeWriter writer)
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
            try
            {
                writer.Advance(bytesRead);
                FlushResult WriteResult = await writer.FlushAsync();

                if (WriteResult.IsCompleted)
                {
                    break;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        await writer.CompleteAsync();
    }
    private async Task ReadPipeAsync(PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;
            try
            {
                if (result.IsCanceled)
                {
                    break;
                }
                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    ProcessMessage(line);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        await reader.CompleteAsync();
    }
    // 여기부터 내가 커스터 마이징해야함
    private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        SequencePosition? position = buffer.PositionOf((byte)'\n');

        if (position == null)
        {
            line = default;
            return false;
        }

        line = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
        return true;
    }

    public void ShutDownServerCore()
    {
        //clientSocket.Shutdown(SocketShutdown.Both);
        //clientSocket.Close();
    }
    public void Abort()
    {
        LoginPipeLines.Reader.CancelPendingRead();
    }
    // 여기도 커스터마이징 해야함
    private void ProcessMessage(ReadOnlySequence<byte> ClientMessage)
    {

    }
}

// 파이프 라인은 데이터를 연속적으로 읽어오고, 커널에서 자동으로 메모리 풀링등을 해주는 역할이다.
// 즉, 나는 TryReadLine에서 먼저 메세지 Ident를 읽어오고, 각 변수에 맞게 라인을 조정한다.
// 그리고 Queue에 집어 넣어서 나중에 스레드 풀을 통해서 그 메세지들을 처리하는 ProcessMessage 함수를 만들자
// 데이터를 읽어오고, 버퍼에 저장하고, 메모리 관리는 PipeLines가 해준다.
// 나는 읽어온 버퍼를 파싱하고, 큐에 넣어서 메세지 처리하는 것을 구현해야한다.