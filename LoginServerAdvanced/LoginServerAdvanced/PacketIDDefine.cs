using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public enum LOGIN_CLIENT_PACKET_ID : uint
    {
        LOGIN_CLIENT_TRY_LOGIN = 0,
    }
    public enum LOGIN_SERVER_PACKET_ID  : uint
    {
        LOGIN_SERVER_LOGIN_RESULT = 0
    }
}
