using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoginServerAdvanced
{
    public class MessageDataProcess
    {
        private ConcurrentQueue<LoginMessagePacket>? LoginMessageQueue;
        private readonly CancellationTokenSource CancelProgress = new CancellationTokenSource();
        private readonly AutoResetEvent QueueEvent = new AutoResetEvent(false);

        public void InitDataProcess()

        {
            LoginMessageQueue = new ConcurrentQueue<LoginMessagePacket>();
        }
        public void BufferToMessageQueue(ref ReadOnlySequence<byte> buffer)
        {

            // 데이터 읽기
            byte[] ReceivedData = buffer.ToArray();
            LoginMessagePacket Msg;
            Msg = SocketDataSerializer.DeSerialize<LoginMessagePacket>(ReceivedData);
            if (Msg != null)
            {
                if (LoginMessageQueue == null) return;
                LoginMessageQueue.Enqueue(Msg);
                QueueEvent.Set();
            }
            else
            {
                Console.WriteLine("Msg is null");
            }
        }
        private void ProcessMessage()
        {
            if (LoginMessageQueue == null) return;
            while (!CancelProgress.IsCancellationRequested)
            {
                LoginMessagePacket? TempPacket;
                if (LoginMessageQueue.TryDequeue(out TempPacket))
                {
                    if (TempPacket == null) return;

                    switch(TempPacket.IDNum)
                    {
                        

                    }
                    
                } 
                else
                {
                    QueueEvent.WaitOne();
                }
            }
        }

        public async void Run()
        {

        }

        public void Cancel()
        {
            CancelProgress.Cancel();
            QueueEvent.Set();
        }

    }
}
