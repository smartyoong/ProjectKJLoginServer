using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;


// 파이프 라인은 데이터를 연속적으로 읽어오고, 커널에서 자동으로 메모리 풀링등을 해주는 역할이다.(내부에선 스레드풀도 사용한다)
// 즉, 나는 BufferToMessage에서 먼저 메세지길이를 파악해서 읽어오고
// 그리고 Queue에 집어 넣어서 나중에 스레드 풀을 통해서 그 메세지들을 처리하는 ProcessMessage 함수를 만들자
// 데이터를 읽어오고, 버퍼에 저장하고, 메모리 관리는 PipeLines가 해준다.
// 나는 읽어온 버퍼를 파싱하고, 큐에 넣어서 메세지 처리하는 것을 구현해야한다.


// 안쓰임

namespace LoginServerAdvanced
{
    public class DataPipeLines
    {
        const int MinimumBufferSize = 1024;
        private Pipe? LoginPipeLines = new Pipe();

        public async Task RecvData(Socket ReceivedSocket)
        {
            //Task Fill = FillPipeAsync(ReceivedSocket);
            //Task Read = ReadPipeAsync();
            //await Task.WhenAll(Fill, Read);
            await ProcessLinesAsync(ReceivedSocket);
        }
        private async Task FillPipeAsync(Socket ClientSocket)
        {

            try
            {
                while (true)
                {
                    if (LoginPipeLines == null) return;
                    Memory<byte> MemorySpace = LoginPipeLines.Writer.GetMemory(MinimumBufferSize);
                    int ReceivedLength = await ClientSocket.ReceiveAsync(MemorySpace, SocketFlags.None);
                    if (ReceivedLength <= 0)
                    {
                        return;
                    }
                    LoginPipeLines.Writer.Advance(ReceivedLength);
                    FlushResult WriteResult = await LoginPipeLines.Writer.FlushAsync();
                    if (WriteResult.IsCompleted)
                    {
                        break;
                    }
                }
                await LoginPipeLines.Writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                string[] lines = ex.StackTrace!.Split('\n');
                foreach (string line in lines)
                {
                    LoginServer.LogItemAddTime(line);
                }
                LoginServer.LogItemAddTime(ex.Message);
            }
            finally
            {
                ClientSocket.Close();
            }
        }

        private async Task ReadPipeAsync()
        {
            while (true)
            {
                if (LoginPipeLines == null) return;
                ReadResult Result = await LoginPipeLines.Reader.ReadAsync();
                ReadOnlySequence<byte> Buffer = Result.Buffer;
                try
                {
                    if (Result.IsCanceled)
                    {
                        break;
                    }
                    //MessageDataProcess.BufferToMessageQueue(ref Buffer);
                    LoginPipeLines.Reader.AdvanceTo(Buffer.Start, Buffer.End);
                    if (Result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string[] lines = ex.StackTrace!.Split('\n');
                    foreach (string line in lines)
                    {
                        LoginServer.LogItemAddTime(line);
                    }
                    LoginServer.LogItemAddTime(ex.Message);
                }
                await LoginPipeLines.Reader.CompleteAsync();
            }
        }
        private static async Task ProcessLinesAsync(Socket socket)
        {

            // Create a PipeReader over the network stream
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                //MessageDataProcess.BufferToMessageQueue(ref buffer);

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();

        }
        public void Cancel()
        {
            if (LoginPipeLines == null) return;
            LoginPipeLines.Reader.CancelPendingRead();
        }
    }
}
