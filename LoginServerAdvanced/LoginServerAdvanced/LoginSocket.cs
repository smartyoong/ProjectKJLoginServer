using System.Net;
using System.Net.Sockets;

namespace LoginServerAdvanced
{
    public class LoginSocket
    {
        const int MaximunBufferSize = 1024;
        private Socket? ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource? CancelSocketCancel = new CancellationTokenSource();
        public void InitLoginSocket()
        {
            ListenSocket?.Bind(new IPEndPoint(IPAddress.Any, 11220));
            ListenSocket?.Listen(1000);
        }
        public async Task Run()
        {
            if (CancelSocketCancel == null) return;
            while (!CancelSocketCancel.IsCancellationRequested)
            {
                if (ListenSocket == null) return;
                Socket ClientSocket = await ListenSocket.AcceptAsync();
                Task.Run(() => RecvData(ClientSocket));
            }
        }
        public void SendData(Socket ClientSock)
        {
            byte[] data = null; // 추후 바꿔야함
            if (ClientSock == null) return;
            ClientSock.Send(data!);
        }
        public void Cancel()
        {
            if (CancelSocketCancel == null) return;
            CancelSocketCancel.Cancel();
        }

        private async Task RecvData(Socket Sock)
        {
            try
            {
                byte[] buffer = new byte[MaximunBufferSize];
                if (CancelSocketCancel == null) return;
                while (!CancelSocketCancel.IsCancellationRequested)
                {
                    int ReceivedLength = await Sock.ReceiveAsync(buffer, SocketFlags.None);
                    if (ReceivedLength <= 0)
                    {
                        return;
                    }
                    MessageDataProcess.BufferToMessageQueue(ref buffer);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Sock.Close();
            }
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
