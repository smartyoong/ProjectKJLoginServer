using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


// 파이프 라인은 데이터를 연속적으로 읽어오고, 커널에서 자동으로 메모리 풀링등을 해주는 역할이다.(내부에선 스레드풀도 사용한다)
// 즉, 나는 BufferToMessage에서 먼저 메세지길이를 파악해서 읽어오고
// 그리고 Queue에 집어 넣어서 나중에 스레드 풀을 통해서 그 메세지들을 처리하는 ProcessMessage 함수를 만들자
// 데이터를 읽어오고, 버퍼에 저장하고, 메모리 관리는 PipeLines가 해준다.
// 나는 읽어온 버퍼를 파싱하고, 큐에 넣어서 메세지 처리하는 것을 구현해야한다.

namespace LoginServerAdvanced
{
    public class DataPipeLines
    {
        private Pipe? LoginPipeLines;
        private Pipe? LoginSendPipeLines;
        private MessageDataProcess? MessageDataProcessor;

        public void InitPipe(ref MessageDataProcess MDP)
        {
            LoginPipeLines = new Pipe();
            LoginSendPipeLines = new Pipe();
            MessageDataProcessor = MDP;
        }
        public async Task FillPipeAsync(Socket socket)
        {
            const int minimumBufferSize = 1024;

            while (true)
            {
                if(LoginPipeLines == null) return;
                Memory<byte> memory = LoginPipeLines.Writer.GetMemory(minimumBufferSize);

                int bytesRead = await socket.ReceiveAsync(memory);

                if (bytesRead == 0)
                {
                    break;
                }
                try
                {
                    LoginPipeLines.Writer.Advance(bytesRead);
                    FlushResult WriteResult = await LoginPipeLines.Writer.FlushAsync();

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

            await LoginPipeLines.Writer.CompleteAsync();
        }

        public async Task ReadPipeAsync()
        {
            while (true)
            {
                if( LoginPipeLines == null) return;
                ReadResult result = await LoginPipeLines.Reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }
                    if (MessageDataProcessor == null) return;
                    MessageDataProcessor.BufferToMessageQueue(ref buffer);

                    LoginPipeLines.Reader.AdvanceTo(buffer.Start, buffer.End);

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

            await LoginPipeLines.Reader.CompleteAsync();
        }
        public async Task FillSendPipeAsync(byte[] data)
        {
            const int minimumBufferSize = 1024;
            while (true)
            {
                if (data.Length == 0)
                {
                    break;
                }
                if (LoginSendPipeLines == null) return;
                Memory<byte> memory = LoginSendPipeLines.Writer.GetMemory(minimumBufferSize);
                try
                {
                    data.CopyTo(memory.Span);
                    LoginSendPipeLines.Writer.Advance(data.Length);
                    FlushResult WriteResult = await LoginSendPipeLines.Writer.FlushAsync();

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


        public async Task ReadSendPipeAsync(Socket socket)
        {
            while (true)
            {
                if (LoginSendPipeLines == null) return;
                ReadResult result = await LoginSendPipeLines.Reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    await socket.SendAsync(buffer.ToArray(), SocketFlags.None);

                    LoginSendPipeLines.Reader.AdvanceTo(buffer.Start, buffer.End);

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
        public void Abort()
        {
            if(LoginSendPipeLines == null || LoginPipeLines == null) return;
            LoginPipeLines.Reader.CancelPendingRead();
            LoginSendPipeLines.Reader.CancelPendingRead();
        }
    }

    public class LoginSocket
    {
        private Socket? ListenSocket;
        private DataPipeLines? DataPipe;
        private DataPipeLines? DataSendPipe;
        public void InitLoginSocket()
        {
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 11220));
            ListenSocket.Listen(1000);
        }
        public async void Run()
        {
            try
            {
                await ProcessNetwork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task ProcessNetwork()
        {
            while (true)
            {
                if (ListenSocket == null) return;
                Socket ClientSocket = await ListenSocket.AcceptAsync();
                if(DataPipe == null) return;
                Task Writing = DataPipe.FillPipeAsync(ClientSocket);
                Task Reading = DataPipe.ReadPipeAsync();

                await Task.WhenAll(Writing, Reading);

            }
        }
        private async Task SendData(Socket ClientSock)
        {
            byte[] data = null; // 추후 바꿔야함
            if (DataSendPipe == null) return;
            Task writing = DataSendPipe.FillSendPipeAsync(data);
            Task reading = DataSendPipe.ReadSendPipeAsync(ClientSock); // 이것도 바꿔야함
            await Task.WhenAll(writing, reading); // 여기도 바꿔야함
        }
    }

    public class GameSocket
    {
        private Socket? GameConnectSocket;
        public void InitGameSocket()
        {
            GameConnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
