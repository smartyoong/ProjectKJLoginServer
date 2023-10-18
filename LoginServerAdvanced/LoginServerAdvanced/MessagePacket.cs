﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public static class SocketDataSerializer
    {
        public static T DeSerialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public static byte[] Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }
    }

    [Serializable]
    public class LoginMessagePacket
    {
        // 변수는 아무렇게나 추가 가능
        public LOGIN_CLIENT_PACKET_ID IDNum { get; set; }
        public string StringValue1 { get; set; } = string.Empty;
        public string StringValue2 { get; set; } = string.Empty;
        public int IntegerValue1 { get; set; } = 0;
    }
}
