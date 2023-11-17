using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public enum ERROR_CODE : int
    {
        ERR_NULL_VALUE = 99999
    }
    public enum LOGIN_CLIENT_PACKET_ID : uint
    {
        LOGIN_CLIENT_TRY_LOGIN = 0,
        LOGIN_CLIENT_TRY_LOGOUT = 1
    }
    public enum LOGIN_SERVER_PACKET_ID  : uint
    {
        LOGIN_SERVER_LOGIN_RESULT = 0,
        LOGIN_SERVER_LOGOUT_RESULT = 1
    }

    public enum MS_SQL_SP_ID  : uint
    {
        SP_LOGIN   = 0,
        SP_REGIST_ACCOUNT = 1
    }
}
