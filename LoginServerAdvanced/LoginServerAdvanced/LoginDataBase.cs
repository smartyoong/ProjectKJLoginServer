using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;

namespace LoginServerAdvanced
{
    public class LoginDataBase : IDisposable
    {
        private SqlConnection? AccountDBConnect;
        private string SQLConnectString = string.Format("Server={0};Database={1};Integrated Security=SSPI;Encrypt=false;", "SMARTYOONG\\SQLEXPRESS", "AccountDB");
        private bool Disposed = false;

        public bool InitDataBase()
        {
            AccountDBConnect = new SqlConnection(SQLConnectString);
            try
            {
                AccountDBConnect.Open();
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
            AccountDBConnect?.Close();

            Disposed = true;
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
                        break;
                    case MS_SQL_SP_ID.SP_REGIST_ACCOUNT:
                        ReturnValue = Function_SP_RegistAccount(Packet);
                        break;
                    case MS_SQL_SP_ID.SP_ID_UNIQUE_CHECK:
                        ReturnValue = Function_SP_ID_UniqueCheck(Packet);
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
                    case MS_SQL_SP_ID.SP_REGIST_ACCOUNT:
                        Packet.GetHashCode();
                        break;
                    case MS_SQL_SP_ID.SP_ID_UNIQUE_CHECK:
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
        private int Function_SP_RegistAccount(LoginMessagePacket Packet)
        {
            int ReturnValue = 99999; // Error
            SqlCommand Command = new SqlCommand("SP_RegistAccount", AccountDBConnect);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.AddWithValue("@ID", Packet.StringValue1);
            Command.Parameters.AddWithValue("@PW", Packet.StringValue2);
            IPEndPoint? RemoteEndPoint = Packet.ResponeSocket!.RemoteEndPoint as IPEndPoint;
            if (RemoteEndPoint == null)
            {
                LoginServer.LogItemAddTime($"{Packet.StringValue1}회원 가입 실패, IP가 없음");
                return ReturnValue;
            }
            Command.Parameters.AddWithValue("@IP", RemoteEndPoint!.Address.ToString());
            SqlParameter OutPutParameter = new SqlParameter("@ErrorCode", SqlDbType.Int);
            OutPutParameter.Direction = ParameterDirection.ReturnValue;
            Command.Parameters.Add(OutPutParameter);
            Command.ExecuteNonQuery();
            ReturnValue = (int)OutPutParameter.Value;
            return ReturnValue;
        }
        private int Function_SP_ID_UniqueCheck(LoginMessagePacket Packet)
        {
            int ReturnValue = 99999; // Error
            SqlCommand Command = new SqlCommand("SP_ID_Unique_Check", AccountDBConnect);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.AddWithValue("@ID", Packet.StringValue1);
            SqlParameter OutPutParameter = new SqlParameter("@ErrorCode", SqlDbType.Int);
            OutPutParameter.Direction = ParameterDirection.ReturnValue;
            Command.Parameters.Add(OutPutParameter);
            Command.ExecuteNonQuery();
            ReturnValue = (int)OutPutParameter.Value;
            return ReturnValue;
        }
    }
}
