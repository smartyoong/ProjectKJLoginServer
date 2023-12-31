﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public class LoginCore
    {
        private static Dictionary<string, Socket> LoginUsers = new Dictionary<string, Socket>(); // <닉네임,소켓>
        private bool IsServerRun = false;
        LoginDataBase? LoginDB;
        GameGateSocket? GameSock;
        LoginSocket? LoginSock;
        Task? LoginSocketTask;
        Task? MessageDataProcessTask;
        Task? GateServerTask;
        MessageDataProcess? PacketProccessor;
        public LoginServer? MainForm { get; set; }

        public bool InitLoginServer()
        {
            LoginDB = new LoginDataBase();
            GameSock = new GameGateSocket();
            LoginSock = new LoginSocket();
            PacketProccessor = new MessageDataProcess();
            ThreadPool.SetMaxThreads(4, 4);
            if(!LoginSock.InitLoginSocket(PacketProccessor))
                return false;
            if(!LoginDB.InitDataBase())
            {
                MainForm!.SetDBConnectSucces(false);
                return false;
            }
            if (!GameSock.InitGameSocket())
            {
                MainForm!.SetGateServerSuccess(false);
                return false;
            }
            if (LoginDB != null)
            {
                if (!PacketProccessor.Init(LoginDB))
                    return false;
            }
            else
                MessageBox.Show("서버 초기화 실패","서버 초기화 실패",MessageBoxButtons.OK, MessageBoxIcon.Error);
            IsServerRun = true;
            PacketProccessor.MainForm = this.MainForm;
            PacketProccessor.Owner = this;
            LoginSock.MainForm = this.MainForm;
            GameSock.MainForm = this.MainForm;
            MainForm!.SetDBConnectSucces(true);
            return true;
        }
        public bool IsServerOn()
        {
            return IsServerRun;
        }
        public async void Run()
        {
            try
            {
                LoginSocketTask = LoginSock?.Run();
                MessageDataProcessTask = PacketProccessor?.Run();
                GateServerTask = GameSock?.Run();
                await Task.WhenAll(LoginSocketTask!, MessageDataProcessTask!, GateServerTask!);
                LoginSock!.Dispose();
                GameSock!.Dispose();
                LoginDB?.Dispose();
                MessageDataProcessTask!.Dispose();
                PacketProccessor!.Dispose();
                LoginServer.LogItemAddTime("서버 객체 삭제 완료.");
                LoginServer.LogItemAddTime("서버가 종료되었습니다.");
                IsServerRun = false;
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
                else
                {
                    LoginServer.LogItemAddTime("LoginCore.Run 정상적으로 종료되었습니다.");
                }
            }
            finally
            {
                LoginUsers.Clear();
            }
        }

        public void ShutDownServerCore()
        {
            if(IsServerRun)
            {
                LoginSock?.Cancel();
                PacketProccessor?.Cancel();
                GameSock?.Cancel();
            }
        }

        public static void AddLoginUsers(string NickName, Socket Sock)
        {
            if(LoginUsers != null && NickName != null && Sock != null)
            {
                lock (LoginUsers)
                {
                    LoginUsers.Add(NickName, Sock);
                }
            }
        }
        public static Socket FindSocketByNickName(string NickName)
        {
            if (LoginUsers != null && NickName != null)
            {
                lock (LoginUsers)
                {
                    return LoginUsers[NickName];
                }
            }
            else
                return null!;
        }
        public static string FindNickNameBySocket(Socket Sock)
        {
            if (LoginUsers != null && Sock != null)
            {
                lock (LoginUsers)
                {
                    return LoginUsers.FirstOrDefault(x => x.Value == Sock).Key;
                }
            }
            else
                return string.Empty;
        }
        public static int DeleteUserOnDictionary(string NickName)
        {
            if (LoginUsers!.ContainsKey(NickName))
            {
                LoginUsers!.Remove(NickName);
                LoginServer.LogItemAddTime($"{NickName}님이 로그아웃 하셨습니다.");
                return 0;
            }
            else
            {
                return 1;
            }
        }
        public void SendToGateServer(LOGIN_TO_GATE_PACKET_ID ID ,LoginToGateServer Packet)
        {
            int SendData = GameSock!.SendToGateServer(ID, Packet);
            LoginServer.LogItemAddTime($"{SendData} byte 게이트 서버에 전송함");
        }
    }
}
