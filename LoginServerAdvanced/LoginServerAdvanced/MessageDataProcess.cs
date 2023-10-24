using Microsoft.Identity.Client;
using System.Buffers;
using System.Collections.Concurrent;

namespace LoginServerAdvanced
{
    public static class MessageDataProcess
    {
        private static BlockingCollection<LoginMessagePacket>? LoginMessageQueue = new BlockingCollection<LoginMessagePacket>();
        private readonly static CancellationTokenSource CancelProgress = new CancellationTokenSource();

        public static void BufferToMessageQueue(ref byte[] ReceivedData)
        {

            LoginMessagePacket Msg;
            Msg = SocketDataSerializer.DeSerialize<LoginMessagePacket>(ReceivedData);
            if (Msg != null)
            {
                if (LoginMessageQueue == null) return;
                LoginMessageQueue.Add(Msg);
            }
            else
            {
                MessageBox.Show("Msg is null");
            }
        }
        private static void ProcessMessage()
        {
            if (LoginMessageQueue == null) return;
            try
            {
                while (!LoginMessageQueue.IsCompleted)
                {
                    LoginMessagePacket? TempPacket = new LoginMessagePacket();
                    TempPacket = LoginMessageQueue.Take(CancelProgress.Token);
                    if (TempPacket == null) return;
                    switch (TempPacket.IDNum)
                    {

                        case LOGIN_CLIENT_PACKET_ID.LOGIN_CLIENT_TRY_LOGIN:
                            Console.WriteLine(TempPacket.StringValue1);
                            Console.WriteLine(TempPacket.StringValue2);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
    }

    public static async Task Run()
    {
        await Task.Run(() =>
        {
            while (!CancelProgress.IsCancellationRequested)
            {
                ProcessMessage();
            }
        });
    }

    public static void Cancel()
    {
        CancelProgress.Cancel();
        LoginMessageQueue?.CompleteAdding();
    }

}
}
