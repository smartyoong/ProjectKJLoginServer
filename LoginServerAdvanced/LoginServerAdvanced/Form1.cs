using System.Buffers;
using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipelines;
using Microsoft.Data.SqlClient;
using LoginServerAdvanced;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace LoginServerAdvanced
{
    public partial class LoginServer : Form
    {
        private LoginCore LoginServerCore = new LoginCore();
        public LoginServer()
        {
            InitializeComponent();
        }
        private async void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (LoginServerCore.IsServerOn())
                MessageBox.Show("이미 서버가 실행중입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("서버를 시작하시겠습니까?", "시작", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LogItemAddTime("서버를 시작합니다.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("서버 버퍼 시작");
                    LogItemAddTime("서버오픈 완료");
                }
            }
        }
        public static void LogItemAddTime(string LogContext)
        {
            string Temp = string.Format("{0,-40}{1}", LogContext, DateTime.Now.ToString());
            LoginServerLogList.Items.Add(Temp);
        }

        private void ServerStopButton_Click(object sender, EventArgs e)
        {
            LoginServerCore.ShutDownServerCore();
        }

        private void ServerReSetButton_Click(object sender, EventArgs e)
        {

        }
    }
}


/*로그인 서버는 DB확인을 통해 유효한 로그인 요청인지 확인한 후, 메모리상에 로그인을한 유저들을 보유만하고 있는다.
 * 그리고 클라이언트를 게임서버와 통신하도록 유도하고, 게임 서버는 로그인 서버에게 해당 유저가 로그인을 했는지 요청이 들어오면,
 * 로그인 서버는 응답을 해주는 것으로 역할을 끝낸다.
 */

class LoginCore
{

}
