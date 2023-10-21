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
                MessageBox.Show("�̹� ������ �������Դϴ�.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SystemSounds.Beep.Play();
                if (MessageBox.Show("������ �����Ͻðڽ��ϱ�?", "����", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    LogItemAddTime("������ �����մϴ�.");
                    LoginServerCore.InitLoginServer();
                    LoginServerCore.Run();
                    LogItemAddTime("���� ���� ����");
                    LogItemAddTime("�������� �Ϸ�");
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


/*�α��� ������ DBȮ���� ���� ��ȿ�� �α��� ��û���� Ȯ���� ��, �޸𸮻� �α������� �������� �������ϰ� �ִ´�.
 * �׸��� Ŭ���̾�Ʈ�� ���Ӽ����� ����ϵ��� �����ϰ�, ���� ������ �α��� �������� �ش� ������ �α����� �ߴ��� ��û�� ������,
 * �α��� ������ ������ ���ִ� ������ ������ ������.
 */

class LoginCore
{

}
