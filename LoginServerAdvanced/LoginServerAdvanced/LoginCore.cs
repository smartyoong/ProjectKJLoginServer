﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public class LoginCore
    {
        private Dictionary<string, int>? LoginUsers; // <ID,랜덤값>으로 유효성 검사 ID만 딱 보내서 위조 로그인 하는것을 방지
        private bool IsServerRun = false;
        LoginDataBase? LoginDB = new LoginDataBase();
        GameSocket? GameSock = new GameSocket();

        public void InitLoginServer()
        {
            IsServerRun = true;
            ThreadPool.SetMaxThreads(4, 4);
            LoginSocket.InitLoginSocket();
            LoginDB?.InitDataBase();
            GameSock?.InitGameSocket();
        }
        public bool IsServerOn()
        {
            return IsServerRun;
        }
        public async Task Run()
        {
            try
            {
                LoginSocket.Run();
                MessageDataProcess.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ShutDownServerCore()
        {
            LoginSocket.Cancel();
            DataPipeLines.Cancel();
            MessageDataProcess.Cancel();
        }
    }
}