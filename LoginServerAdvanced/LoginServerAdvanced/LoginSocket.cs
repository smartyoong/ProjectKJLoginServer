using System.Net;
using System.Net.Sockets;

namespace LoginServerAdvanced
{
    public class LoginSocket : IDisposable
    {
        private bool Disposed = false;
        const int MaximunBufferSize = 1024;
        private Socket? ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource? CancelSocketCancel = new CancellationTokenSource();
        public List<Task> SocketTasks = new List<Task>();
        public void InitLoginSocket()
        {
            ListenSocket?.Bind(new IPEndPoint(IPAddress.Any, 11220));
            ListenSocket?.Listen(1000);
        }
        public async Task Run()
        {
            try
            {
                if (CancelSocketCancel == null) return;
                while (!CancelSocketCancel.Token.IsCancellationRequested)
                {
                    if (ListenSocket == null) return;
                    Socket ClientSocket = await ListenSocket.AcceptAsync(CancelSocketCancel.Token);
                    SocketTasks.Add(Task.Run(() => RecvData(ClientSocket), CancelSocketCancel.Token));
                }
            }
            catch (OperationCanceledException ex)
            {
                LoginServer.LogItemAddTime($"LoginSocket Run이 정상적으로 종료되었습니다. {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                {
                    string[] lines = ex.StackTrace!.Split('\n');
                    foreach (string line in lines)
                    {
                        LoginServer.LogItemAddTime(line);
                    }
                    LoginServer.LogItemAddTime(ex.Message);
                }
            }
            finally
            {
                await Task.WhenAll(SocketTasks);
                LoginServer.LogItemAddTime("모든 소켓 작업 취소 완료");
            }
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
                while (!CancelSocketCancel.Token.IsCancellationRequested)
                {
                    int ReceivedLength = await Sock.ReceiveAsync(buffer, SocketFlags.None,CancelSocketCancel.Token);
                    if (ReceivedLength <= 0)
                    {
                        return;
                    }
                    MessageDataProcess.BufferToMessageQueue(ref buffer, Sock);
                }
            }
            catch(SocketException ex)
            {
                if(ex.SocketErrorCode == SocketError.ConnectionAborted || ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.Shutdown)
                {
                    IPEndPoint? RemoteEndPoint = Sock.RemoteEndPoint as IPEndPoint;
                    if (RemoteEndPoint != null)
                    {
                        LoginServer.LogItemAddTime($"{RemoteEndPoint.Address} 유저가 연결을 끊었습니다.");
                    }
                }
            }
            catch(OperationCanceledException ex)
            {
                LoginServer.LogItemAddTime($"RecvData가 정상적으로 종료되었습니다. {ex.Message}");
            }
            catch(Exception ex)
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
                IPEndPoint? RemoteEndPoint = Sock.RemoteEndPoint as IPEndPoint;
                if (RemoteEndPoint != null)
                {
                    if(LoginCore.FindNickNameBySocket(Sock) != string.Empty)
                        LoginServer.LogItemAddTime($"{LoginCore.FindNickNameBySocket(Sock)} {RemoteEndPoint.Address} 님이 로그아웃 하였습니다");
                    else
                        LoginServer.LogItemAddTime($"{RemoteEndPoint.Address} (로그인 혹은 회원가입하지 않음) 님이 로그아웃 하였습니다");
                }
                Sock.Close();

            }
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
                CancelSocketCancel!.Dispose();
                for(int i = 0;i< SocketTasks.Count;i++) 
                {
                    SocketTasks[i].Dispose();
                }
                SocketTasks.Clear();
                CancelSocketCancel?.Dispose();
            }
            ListenSocket?.Close();
            Disposed = true;
        }
    }

    public class GameSocket : IDisposable
    {
        private bool Disposed = false;
        private Socket? GameConnectSocket;
        public void InitGameSocket()
        {
            GameConnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                // 관리 리소스 해제
            }
            // 비관리 리소스 해제
            GameConnectSocket?.Close();

            Disposed = true;
        }
    }
}
