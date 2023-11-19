using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LoginServerAdvanced
{
    public class MessageDataProcess : IDisposable
    {
        private BlockingCollection<LoginMessagePacket>? LoginMessageQueue;
        private CancellationTokenSource? CancelProgress;
        private LoginDataBase? LoginDBSocket; // 얘는 Dispose 여기서 시키면 안됨
        private bool Disposed = false;

        public bool Init(LoginDataBase LoginDB)
        {
            if (LoginDB == null) return false;
            LoginDBSocket = LoginDB;
            LoginMessageQueue = new BlockingCollection<LoginMessagePacket>();
            CancelProgress = new CancellationTokenSource();
            return true;
        }
        public void BufferToMessageQueue(ref byte[] ReceivedData, Socket Sock)
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
        private void ProcessMessage()
        {
            if (LoginMessageQueue == null) return;
            try
            {
                while (!LoginMessageQueue.IsCompleted)
                {
                    LoginMessagePacket? TempPacket = new LoginMessagePacket();
                    TempPacket = LoginMessageQueue.Take(CancelProgress!.Token);
                    if (TempPacket == null) return;
                    switch (TempPacket.IDNum)
                    {
                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_TRY_LOGIN:
                            Callback_SP_Login(TempPacket);
                            break;
                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_TRY_LOGOUT:
                            Callback_LogOut(TempPacket);
                            break;
                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_TRY_REGIST:
                            Callback_SP_Regist(TempPacket);
                            break;
                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_CHECK_ID_UNIQUE: 
                            Callback_SP_CheckID(TempPacket); 
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

        public async Task Run()
        {
            try
            {
                await Task.Run(() =>
                {
                    while (!CancelProgress!.Token.IsCancellationRequested)
                    {
                        ProcessMessage();
                    }
                }, CancelProgress!.Token);
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

        public void Cancel()
        {
            CancelProgress!.Cancel();
            LoginMessageQueue?.CompleteAdding();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool IsDisposing)
        {
            // 중복 실행 방지
            if (Disposed)
                return;
            if (IsDisposing)
            {
                CancelProgress?.Dispose();
            }
            LoginMessageQueue!.Dispose();
            Disposed = true;
        }
        private void Callback_SP_Login(LoginMessagePacket Packet)
        {
            int ErrorValue = (int)ERROR_CODE.ERR_NULL_VALUE;
            string NickName = string.Empty;
            if(LoginDBSocket != null)
                ErrorValue = LoginDBSocket.SPCall(MS_SQL_SP_ID.SP_LOGIN, Packet, out NickName);
            LoginSendToClientMessagePacket? TempPacket = new LoginSendToClientMessagePacket();
            TempPacket.IDNum = LOGIN_SERVER_PACKET_ID.LOGIN_SERVER_LOGIN_RESULT;
            TempPacket.IntegerValue1 = ErrorValue;
            TempPacket.StringValue1 = NickName;
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
        private void Callback_LogOut(LoginMessagePacket Packet)
        {
            LoginSendToClientMessagePacket? TempPacket = new LoginSendToClientMessagePacket();
            TempPacket.IDNum = LOGIN_SERVER_PACKET_ID.LOGIN_SERVER_LOGOUT_RESULT;
            TempPacket.IntegerValue1 = LoginCore.DeleteUserOnDictionary(Packet.StringValue1);
            byte[] DataBytes;
            DataBytes = SocketDataSerializer.Serialize(TempPacket);
            Packet.ResponeSocket?.Send(DataBytes);
        }
        private void Callback_SP_Regist(LoginMessagePacket Packet)
        {
            int ErrorValue = (int)ERROR_CODE.ERR_NULL_VALUE;
            string NickName = string.Empty;
            if (LoginDBSocket != null)
                ErrorValue = LoginDBSocket.SPCall(MS_SQL_SP_ID.SP_LOGIN, Packet, out NickName);
            LoginSendToClientMessagePacket? TempPacket = new LoginSendToClientMessagePacket();
            TempPacket.IDNum = LOGIN_SERVER_PACKET_ID.LOGIN_SERVER_LOGIN_RESULT;
            TempPacket.IntegerValue1 = ErrorValue;
            TempPacket.StringValue1 = NickName;
            byte[] DataBytes;
            DataBytes = SocketDataSerializer.Serialize(TempPacket);
            Packet.ResponeSocket?.Send(DataBytes);
            if (ErrorValue == 2)
            {
                LoginCore.AddLoginUsers(NickName, Packet.ResponeSocket!);
                IPEndPoint? RemoteEndPoint = Packet.ResponeSocket!.RemoteEndPoint as IPEndPoint;
                if (RemoteEndPoint != null)
                {
                    LoginServer.LogItemAddTime($"{NickName}님이 접속하셨습니다. {RemoteEndPoint.Address}");
                }
            }
        }
        private void Callback_SP_CheckID(LoginMessagePacket Packet)
        {
            int ErrorValue = (int)ERROR_CODE.ERR_NULL_VALUE;
            if (LoginDBSocket != null)
                ErrorValue = LoginDBSocket.SPCall(MS_SQL_SP_ID.SP_ID_UNIQUE_CHECK, Packet);
            LoginSendToClientMessagePacket? TempPacket = new LoginSendToClientMessagePacket();
            TempPacket.IDNum = LOGIN_SERVER_PACKET_ID.LOGIN_SERVER_CHECK_ID_UNIQUE_RESULT;
            TempPacket.IntegerValue1 = ErrorValue;
            byte[] DataBytes;
            DataBytes = SocketDataSerializer.Serialize(TempPacket);
            Packet.ResponeSocket?.Send(DataBytes);
        }
    }
}
