using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


// 파이프 라인은 데이터를 연속적으로 읽어오고, 커널에서 자동으로 메모리 풀링등을 해주는 역할이다.(내부에선 스레드풀도 사용한다)
// 즉, 나는 BufferToMessage에서 먼저 메세지길이를 파악해서 읽어오고
// 그리고 Queue에 집어 넣어서 나중에 스레드 풀을 통해서 그 메세지들을 처리하는 ProcessMessage 함수를 만들자
// 데이터를 읽어오고, 버퍼에 저장하고, 메모리 관리는 PipeLines가 해준다.
// 나는 읽어온 버퍼를 파싱하고, 큐에 넣어서 메세지 처리하는 것을 구현해야한다.

namespace LoginServerAdvanced
{
    public static class DataPipeLines
    {
        const int MinimumBufferSize = 1024;
        private static Pipe? LoginPipeLines = new Pipe();
        //private Pipe? LoginSendPipeLines;

        public static async Task RecvData(Socket ReceivedSocket)
        {
            Task Fill = FillPipeAsync(ReceivedSocket);
            Task Read = ReadPipeAsync();
            await Task.WhenAll(Fill, Read);
        }
        private async static Task FillPipeAsync(Socket ClientSocket)
        {

            while (true)
            {
                if(LoginPipeLines == null) return;
                Memory<byte> MemorySpace = LoginPipeLines.Writer.GetMemory(MinimumBufferSize);
                int ReceivedLength = await ClientSocket.ReceiveAsync(MemorySpace);
                if (ReceivedLength <= 0) 
                {
                    return;
                }
                try
                {
                    LoginPipeLines.Writer.Advance(ReceivedLength);
                    FlushResult WriteResult = await LoginPipeLines.Writer.FlushAsync();

                    if (WriteResult.IsCompleted)
                    {
                        break;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            await LoginPipeLines.Writer.CompleteAsync();
        }

        private async static Task ReadPipeAsync()
        {
            while (true)
            {
                if( LoginPipeLines == null) return;
                ReadResult Result = await LoginPipeLines.Reader.ReadAsync();
                ReadOnlySequence<byte> Buffer = Result.Buffer;
                try
                {
                    if (Result.IsCanceled)
                    {
                        break;
                    }
                    MessageDataProcess.BufferToMessageQueue(ref Buffer);

                    LoginPipeLines.Reader.AdvanceTo(Buffer.Start, Buffer.End);

                    if (Result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            await LoginPipeLines.Reader.CompleteAsync();
        }
        //public async Task FillSendPipeAsync(byte[] data)
        //{
        //    const int minimumBufferSize = 1024;
        //    while (true)
        //    {
        //        if (data.Length == 0)
        //        {
        //            break;
        //        }
        //        if (LoginSendPipeLines == null) return;
        //        Memory<byte> memory = LoginSendPipeLines.Writer.GetMemory(minimumBufferSize);
        //        try
        //        {
        //            data.CopyTo(memory.Span);
        //            LoginSendPipeLines.Writer.Advance(data.Length);
        //            FlushResult WriteResult = await LoginSendPipeLines.Writer.FlushAsync();

        //            if (WriteResult.IsCompleted)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }

        //    }
        //}


        //public async Task ReadSendPipeAsync(Socket socket)
        //{
        //    while (true)
        //    {
        //        if (LoginSendPipeLines == null) return;
        //        ReadResult result = await LoginSendPipeLines.Reader.ReadAsync();
        //        ReadOnlySequence<byte> buffer = result.Buffer;
        //        try
        //        {
        //            if (result.IsCanceled)
        //            {
        //                break;
        //            }

        //            await socket.SendAsync(buffer.ToArray(), SocketFlags.None);

        //            LoginSendPipeLines.Reader.AdvanceTo(buffer.Start, buffer.End);

        //            if (result.IsCompleted)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}
        public static void Cancel()
        {
            if(LoginPipeLines == null) return;
            LoginPipeLines.Reader.CancelPendingRead();
        }

        private static void MoveData(byte[] Source, ref Memory<byte> Destination)
        {
            if (Source == null)
            {
                throw new ArgumentNullException(nameof(Source));
            }

            if (Destination.Length < Source.Length)
            {
                throw new ArgumentException("Destination memory is too small.", nameof(Destination));
            }

            GCHandle handle = GCHandle.Alloc(Source, GCHandleType.Pinned);
            try
            {
                IntPtr SourcePtr = handle.AddrOfPinnedObject();
                Memory<byte> SourceMemory = MemoryMarshal.CreateFromPinnedArray(Source, 0, Source.Length);
                SourceMemory.CopyTo(Destination);
            }
            finally
            {
                handle.Free();
            }
        }
    }

    public static class LoginSocket
    {
        private static Socket? ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static CancellationTokenSource? CancelSocketCancel= new CancellationTokenSource();
        public static void InitLoginSocket()
        {
            ListenSocket?.Bind(new IPEndPoint(IPAddress.Any, 11220));
            ListenSocket?.Listen(1000);
        }
        public async static Task Run()
        {
            if(CancelSocketCancel == null) return;
            while (!CancelSocketCancel.IsCancellationRequested)
            {
                if (ListenSocket == null) return;
                Socket ClientSocket = await ListenSocket.AcceptAsync();
                DataPipeLines.RecvData(ClientSocket);
            }
        }
        public static void SendData(Socket ClientSock)
        {
            byte[] data = null; // 추후 바꿔야함
            if (ClientSock == null) return;
            ClientSock.Send(data);
        }
        public static void Cancel()
        {
            if (CancelSocketCancel == null) return;
            CancelSocketCancel.Cancel();
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
