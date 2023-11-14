using Microsoft.Data.SqlClient;
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
        private static Dictionary<string, Socket>? LoginUsers = new Dictionary<string, Socket>(); // <닉네임,소켓>
        private bool IsServerRun = false;
        LoginDataBase? LoginDB = new LoginDataBase();
        GameSocket? GameSock = new GameSocket();
        LoginSocket? LoginSock;
        Task? LoginSocketTask;
        Task? MessageDataProcessTask;

        public void InitLoginServer()
        {
            LoginDB = new LoginDataBase();
            GameSock = new GameSocket();
            LoginSock = new LoginSocket();
            IsServerRun = true;
            ThreadPool.SetMaxThreads(4, 4);
            LoginSock.InitLoginSocket();
            LoginDB?.InitDataBase();
            GameSock?.InitGameSocket();
            if (LoginDB != null)
                MessageDataProcess.Init(LoginDB);
            else
                MessageBox.Show("서버 초기화 실패","서버 초기화 실패",MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageDataProcessTask = MessageDataProcess.Run();
                await Task.WhenAll(LoginSocketTask!, MessageDataProcessTask);
                LoginSock!.Dispose();
                MessageDataProcessTask!.Dispose();
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
        }

        public void ShutDownServerCore()
        {
            if(IsServerRun)
            {
                LoginSock?.Cancel();
                MessageDataProcess.Cancel();
            }
        }

        public static void AddLoginUsers(string NikcName, Socket Sock)
        {
            if(LoginUsers != null && NikcName != null && Sock != null)
            {
                lock (LoginUsers)
                {
                    LoginUsers.Add(NikcName, Sock);
                }
            }
        }
    }
}
