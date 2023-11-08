using Microsoft.Data.SqlClient;
using System.Data;

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

                string[] lines = ex.StackTrace!.Split('\n');
                foreach (string line in lines)
                {
                    LoginServer.LogItemAddTime(line);
                }
                LoginServer.LogItemAddTime(ex.Message);
            }
        }
        public int SPCall(MS_SQL_SP_ID ID, LoginMessagePacket Packet)
        {
            int ReturnValue = 0;
            try
            {
                switch (ID)
                {
                    // 임시
                    case MS_SQL_SP_ID.SP_LOGIN:
                        Packet.GetHashCode();
                        break;
                }
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
            return ReturnValue;
        }
        // string output이 필요한 sp call
        public int SPCall(MS_SQL_SP_ID ID, LoginMessagePacket Packet, out string StringOutPutParameter)
        {
            int ReturnValue = 0;
            StringOutPutParameter = string.Empty;
            try
            {
                switch (ID)
                {
                    case MS_SQL_SP_ID.SP_LOGIN:
                        ReturnValue = Function_SP_Login(Packet, out StringOutPutParameter!);
                        break;
                }
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
            return ReturnValue;
        }
        private int Function_SP_Login(LoginMessagePacket Packet, out string? StringOuPutParameter)
        {
            int ReturnValue = 99999; // Error
            Object? NickNameNullCheck;
            SqlCommand Command = new SqlCommand("SP_Login", AccountDBConnect);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.AddWithValue("@ID", Packet.StringValue1);
            Command.Parameters.AddWithValue("@PW", Packet.StringValue2);
            SqlParameter NickNameParameter = new SqlParameter("@NickName", SqlDbType.NVarChar, 16);
            NickNameParameter.Direction = ParameterDirection.Output;
            Command.Parameters.Add(NickNameParameter);
            SqlParameter OutPutParameter = new SqlParameter("@result", SqlDbType.Int);
            OutPutParameter.Direction = ParameterDirection.ReturnValue;
            Command.Parameters.Add(OutPutParameter);
            Command.ExecuteNonQuery();
            NickNameNullCheck = NickNameParameter.Value;
            StringOuPutParameter = NickNameNullCheck != DBNull.Value ? (string)NickNameNullCheck : null;
            ReturnValue = (int)OutPutParameter.Value;
            return ReturnValue;
        }
    }
}
