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
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace LoginServerAdvanced
{
    public partial class LoginServer : Form
    {
        private LoginCore LoginServerCore = new LoginCore();
        public LoginServer()
        {
            InitializeComponent();
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
                    LoginServerLogList.Items.Add(LogItemAddTime("������ �����մϴ�."));
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.InitDBServerConnect();
                    LoginServerLogList.Items.Add(LogItemAddTime("DB���� ����"));
                    LoginServerCore.InitClientSocketServer();
                    LoginServerLogList.Items.Add(LogItemAddTime("Ŭ���̾�Ʈ ���� �غ� �Ϸ�"));
                    LoginServerCore.Run();
                    LoginServerLogList.Items.Add(LogItemAddTime("���� ���� ����"));
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
    private Pipe LoginSendPipeLines;
    private bool IsServerRun = false;
    private ConcurrentQueue<LoginMessagePacket> LoginMessageQueue;

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
        LoginMessageQueue = new ConcurrentQueue<LoginMessagePacket>();
        ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 11220));
        ListenSocket.Listen(1000);
        LoginPipeLines = new Pipe();
        LoginSendPipeLines = new Pipe();
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

        }
    }
    private async Task RunSendSocketServer()
    {
        while(true)
        {
            byte[] data = null; // ���� �ٲ����
            Task writing = FillSendPipeAsync(LoginSendPipeLines.Writer, data);
            Task reading = ReadSendPipeAsync(LoginSendPipeLines.Reader, ListenSocket); // �̰͵� �ٲ����
            await Task.WhenAll(writing, reading); // ���⵵ �ٲ����
        }
    }
    public async Task Run()
    {
        await RunClientSocketServer();
        await RunSendSocketServer();
    }
    //���Ͽ��� ���� �����͸� WritePipe�� ���� ���������ο� ����, ��� ũ�⸸ŭ ��ٴ� ���� ReadPipe����
    //�˷�����Ѵ�.
    private async Task FillPipeAsync(Socket socket, PipeWriter writer)
    {
        const int minimumBufferSize = 1024;

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
    // WritePipe�� �����͸� ���ٸ� �ش� �Լ��� �ߵ��ȴ�
    // ���� ���۸� �ް� ���� �ʿ信 ���� ���۸� �����Ѵ�
    // ���������� �о�帰 ���۸� �ǵ����ش�
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

                BufferToMessageQueue(ref buffer);
                ProcessMessage(); // �ӽ� �׽�Ʈ��

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
    private async Task FillSendPipeAsync(PipeWriter writer, byte[] data)
    {
        const int minimumBufferSize = 1024;
        while (true) 
        {
            if(data.Length == 0)
            {
                break;
            }
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            try
            {
                data.CopyTo(memory.Span);
                writer.Advance(data.Length);
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
    }

    private async Task ReadSendPipeAsync(PipeReader reader, Socket socket)
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

                await socket.SendAsync(buffer.ToArray(), SocketFlags.None);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    // ������ ����� �а� �ش� ũ�⸸ŭ �� �����͸� �߶� �޼���ť�� �����Ѵ�
    private void BufferToMessageQueue(ref ReadOnlySequence<byte> buffer)
    {

        // ������ �б�
        byte[] ReceivedData = buffer.ToArray();
        LoginMessagePacket Msg = new LoginMessagePacket();
        Msg = SocketDataSerializer.DeSerialize<LoginMessagePacket>(ReceivedData);
        Console.WriteLine(Msg.IDNum + " " +  Msg.StringValue1 + " " + Msg.StringValue2);
        if(Msg != null)
        {
            LoginMessageQueue.Enqueue(Msg);
        }
        else
        {
            Console.WriteLine("Msg is null");
        }
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
    // ������Ǯ���� �޼����� ó���ϴ� �Լ�
    private void ProcessMessage()
    {
        MessageBox.Show("������ �� ����!");
    }
}

// ������ ������ �����͸� ���������� �о����, Ŀ�ο��� �ڵ����� �޸� Ǯ������ ���ִ� �����̴�.(���ο��� ������Ǯ�� ����Ѵ�)
// ��, ���� BufferToMessage���� ���� �޼������̸� �ľ��ؼ� �о����
// �׸��� Queue�� ���� �־ ���߿� ������ Ǯ�� ���ؼ� �� �޼������� ó���ϴ� ProcessMessage �Լ��� ������
// �����͸� �о����, ���ۿ� �����ϰ�, �޸� ������ PipeLines�� ���ش�.
// ���� �о�� ���۸� �Ľ��ϰ�, ť�� �־ �޼��� ó���ϴ� ���� �����ؾ��Ѵ�.

public static class SocketDataSerializer
{
    public static T DeSerialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    public static byte[] Serialize<T>(T obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj);
    }
}

[Serializable]
public class LoginMessagePacket
{
    // ������ �ƹ����Գ� �߰� ����
    public LOGIN_CLIENT_PACKET_ID IDNum { get; set; }
    public string StringValue1 { get; set; } = string.Empty;
    public string StringValue2 { get; set; } = string.Empty;
    public int IntegerValue1 { get; set; } = 0;
}