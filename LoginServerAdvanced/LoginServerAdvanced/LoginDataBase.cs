using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public class LoginDataBase
    {
        private SqlConnection? AccountDBConnect;
        private string SQLConnectString = string.Format("Server={0};Database={1};Integrated Security=SSPI;Encrypt=false;", "SMARTYOONG\\SQLEXPRESS", "AccountDB");

        public void InitDataBase()
        {
            AccountDBConnect = new SqlConnection(SQLConnectString);
            try
            {
                AccountDBConnect.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
