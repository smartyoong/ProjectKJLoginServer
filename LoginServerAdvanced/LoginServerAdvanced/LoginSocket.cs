﻿using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using LoginServerAdvanced.Properties;

namespace LoginServerAdvanced
{
    public class LoginSocket : IDisposable
    {
        private bool Disposed = false;
        const int MaximunBufferSize = 1024;
        private Socket? ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource? CancelSocketCancel = new CancellationTokenSource();
        public List<Task> SocketTasks = new List<Task>();
        private MessageDataProcess? PacketQueue; // 얘는 여기서 Dispose 시키면안됨
        public LoginServer? MainForm { get; set; }
        public bool InitLoginSocket(MessageDataProcess PacketQueuClass)
        {
            try
            {
                ListenSocket?.Bind(new IPEndPoint(IPAddress.Any, Settings.Default.ListenPort));
                ListenSocket?.Listen(1000);
                PacketQueue = PacketQueuClass;
                return true;
            }
            catch(SocketException ex) 
            {
                string[] lines = ex.StackTrace!.Split('\n');
                foreach (string line in lines)
                {
                    LoginServer.LogItemAddTime(line);
                }
                LoginServer.LogItemAddTime(ex.Message);
                return false;
            }
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
                    if(MainForm != null)
                        MainForm.IncreaseUserCount();
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
                    PacketQueue!.BufferToMessageQueue(ref buffer, Sock);
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
                    if(string.IsNullOrEmpty(LoginCore.FindNickNameBySocket(Sock)))
                        LoginServer.LogItemAddTime($"{LoginCore.FindNickNameBySocket(Sock)} {RemoteEndPoint.Address} 님이 연결 종료 하였습니다");
                    else
                        LoginServer.LogItemAddTime($"{RemoteEndPoint.Address} (로그인 혹은 회원가입하지 않음) 님이 연결 종료 하였습니다");
                }
                if (MainForm != null)
                    MainForm.DecreaseUserCount();
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

    public class GameGateSocket : IDisposable
    {
        private bool Disposed = false;
        private Socket? GameConnectSocket;
        private CancellationTokenSource? GateCancel;
        public LoginServer? MainForm { get; set; }
        public bool InitGameSocket()
        {
            try
            {
                GateCancel = new CancellationTokenSource();
                GameConnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return true;
            }
            catch (Exception ex) 
            {
                string[] lines = ex.StackTrace!.Split('\n');
                foreach (string line in lines)
                {
                    LoginServer.LogItemAddTime(line);
                }
                LoginServer.LogItemAddTime(ex.Message);
                return false;
            }
        }
        public async Task Run()
        {
            IPAddress GateAddr = IPAddress.Parse(Settings.Default.GateServerAddr);
            while(!GateCancel!.IsCancellationRequested)
            {
                try
                {
                    await GameConnectSocket!.ConnectAsync(GateAddr, Settings.Default.GateServerPort);
                    MainForm!.SetGateServerSuccess(true);
                    break;
                }
                catch (Exception ex)
                {
                    string[] lines = ex.StackTrace!.Split('\n');
                    foreach (string line in lines)
                    {
                        LoginServer.LogItemAddTime(line);
                    }
                    LoginServer.LogItemAddTime(ex.Message);
                    LoginServer.LogItemAddTime("게이트 서버와 재연결 중");
                }
                await Task.Delay(1000,GateCancel.Token);
            }
        }
        public void Cancel()
        {
            GateCancel!.Cancel();
            MainForm!.SetGateServerSuccess(false);
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
            GameConnectSocket?.Close();
            GateCancel?.Dispose();
            Disposed = true;
        }

        public int SendToGateServer(LOGIN_TO_GATE_PACKET_ID ID, object Data)
        {
            switch(ID)
            {
                case LOGIN_TO_GATE_PACKET_ID.ID_NEW_USER_TRY_CONNECT:
                    if(Data is LoginToGateServer)
                    {
                        int PacketSize = Marshal.SizeOf(typeof(LoginToGateServer)) + sizeof(LOGIN_TO_GATE_PACKET_ID);
                        byte[] Packet = new byte[PacketSize + sizeof(int)];
                        GameConnectSocket!.Send(Packet);
                    }
                    else
                    {
                        LoginServer.LogItemAddTime($"{ID}의 Data가  LoginToGateServer형태로 변환 불가능합니다");
                    }
                    break;
            }
            return 0;
        }
    }
}
