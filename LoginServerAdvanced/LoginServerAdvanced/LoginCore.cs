using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public class LoginCore
    {
        private Dictionary<string, int>? LoginUsers; // <ID,랜덤값>으로 유효성 검사 ID만 딱 보내서 위조 로그인 하는것을 방지
        private bool IsServerRun = false;
        MessageDataProcess? MessageProcessor;
        DataPipeLines? LoginPipe;
        LoginSocket? LoginSock;
        LoginDataBase? LoginDB;
        GameSocket? GameSock;

        public void InitLoginServer()
        {
            IsServerRun = true;
            ThreadPool.SetMaxThreads(4, 4);
            MessageProcessor = new MessageDataProcess();
            LoginPipe = new DataPipeLines();
            LoginPipe.InitPipe(ref MessageProcessor);
            LoginSock = new LoginSocket();
            LoginSock.InitLoginSocket();
            LoginDB = new LoginDataBase();
            LoginDB.InitDataBase();
            GameSock = new GameSocket();
            GameSock.InitGameSocket();
        }
        public bool IsServerOn()
        {
            return IsServerRun;
        }
        public async Task Run()
        {

        }

        public void ShutDownServerCore()
        {

        }
    }
}
