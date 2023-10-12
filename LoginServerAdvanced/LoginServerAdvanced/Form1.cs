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
                MessageBox.Show("�̹� ������ �������Դϴ�.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �����Ͻðڽ��ϱ�?", "����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LoginServerLogList.Items.Add(LogItemAddTime("������ �����մϴ�."));
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.InitDBServerConnect();
                    LoginServerLogList.Items.Add(LogItemAddTime("DB���� ����"));
                    LoginServerCore.InitClientSocketServer();
                    LoginServerLogList.Items.Add(LogItemAddTime("Ŭ���̾�Ʈ ���� �غ� �Ϸ�"));
                    LoginServerLogList.Items.Add(LogItemAddTime("�������� �Ϸ�"));
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


/*�α��� ������ DBȮ���� ���� ��ȿ�� �α��� ��û���� Ȯ���� ��, �޸𸮻� �α������� �������� �������ϰ� �ִ´�.
 * �׸��� Ŭ���̾�Ʈ�� ���Ӽ����� ����ϵ��� �����ϰ�, ���� ������ �α��� �������� �ش� ������ �α����� �ߴ��� ��û�� ������,
 * �α��� ������ ������ ���ִ� ������ ������ ������.
 */

class LoginCore
{
    private Dictionary<string, int> LoginUsers; // <ID,������>���� ��ȿ�� �˻� ID�� �� ������ ���� �α��� �ϴ°��� ����
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
    // ���� ������-----------------------------------------------------------------------------
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
                // ������ ó��
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