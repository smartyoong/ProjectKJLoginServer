using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LoginServerAdvanced
{
    public static class MessageDataProcess
    {
        private static BlockingCollection<LoginMessagePacket>? LoginMessageQueue = new BlockingCollection<LoginMessagePacket>();
        private static CancellationTokenSource CancelProgress = new CancellationTokenSource();
        private static LoginDataBase? LoginDBSocket;

        public static void Init(LoginDataBase LoginDB)
        {
            if (LoginDB == null) return;
            LoginDBSocket = LoginDB;
            if (LoginMessageQueue!.IsCompleted)
            {
                LoginMessageQueue.Dispose();
                LoginMessageQueue = new BlockingCollection<LoginMessagePacket>();
                CancelProgress.Dispose();
                CancelProgress = new CancellationTokenSource();
            }

        }
        public static void BufferToMessageQueue(ref byte[] ReceivedData, Socket Sock)
        {

            LoginMessagePacket Msg;
            Msg = SocketDataSerializer.DeSerialize<LoginMessagePacket>(ReceivedData);
            if (Msg != null)
            {
                Msg.ResponeSocket = Sock;
                if (LoginMessageQueue == null) return;
                LoginMessageQueue.Add(Msg);
            }
            else
            {
                MessageBox.Show("Msg is null");
            }
        }
        private static void ProcessMessage()
        {
            if (LoginMessageQueue == null) return;
            try
            {
                while (!LoginMessageQueue.IsCompleted)
                {
                    LoginMessagePacket? TempPacket = new LoginMessagePacket();
                    TempPacket = LoginMessageQueue.Take(CancelProgress.Token);
                    if (TempPacket == null) return;
                    switch (TempPacket.IDNum)
                    {
                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_TRY_LOGIN:
                            Callback_SP_Login(TempPacket);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                if(ex is not OperationCanceledException)
                {
                    string[] lines = ex.StackTrace!.Split('\n');
                    foreach (string line in lines)
                    {
                        LoginServer.LogItemAddTime(line);
                    }
                    LoginServer.LogItemAddTime(ex.Message);
                }
                else
                {
                    LoginServer.LogItemAddTime("ProcessMessage가 정상적으로 종료되었습니다.");
                }
            }
        }

        public static async Task Run()
        {
            try
            {
                await Task.Run(() =>
                {
                    while (!CancelProgress.Token.IsCancellationRequested)
                    {
                        ProcessMessage();
                    }
                }, CancelProgress.Token);
            }
            catch (OperationCanceledException ex)
            {
                LoginServer.LogItemAddTime($"MessageDataProcess Run이 정상적으로 종료되었습니다. {ex.Message}");
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
        }

        public static void Cancel()
        {
            CancelProgress.Cancel();
            LoginMessageQueue?.CompleteAdding();
        }

        private static void Callback_SP_Login(LoginMessagePacket Packet)
        {
            int ErrorValue = (int)ERROR_CODE.ERR_NULL_VALUE;
            string NickName = string.Empty;
            if(LoginDBSocket != null)
                ErrorValue = LoginDBSocket.SPCall(MS_SQL_SP_ID.SP_LOGIN, Packet, out NickName);
            LoginSendToClientMessagePacket? TempPacket = new LoginSendToClientMessagePacket();
            TempPacket.IDNum = LOGIN_SERVER_PACKET_ID.LOGIN_SERVER_LOGIN_RESULT;
            TempPacket.IntegerValue1 = ErrorValue;
            byte[] DataBytes;
            DataBytes = SocketDataSerializer.Serialize(TempPacket);
            Packet.ResponeSocket?.Send(DataBytes);
            if(ErrorValue == 2)
            {
                LoginCore.AddLoginUsers(NickName, Packet.ResponeSocket!);
                IPEndPoint? RemoteEndPoint = Packet.ResponeSocket!.RemoteEndPoint as IPEndPoint;
                if (RemoteEndPoint != null)
                {
                    LoginServer.LogItemAddTime($"{NickName}님이 접속하셨습니다. {RemoteEndPoint.Address}");
                }
            }
        }
    }
}
