using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myGUIApp4BBB
{
    public partial class Form1 : Form
    {

        int tx_count = 0;
        int rx_count = 0;
        int crx_count = 0;
        string storedAuthToken = "-1";

        public Form1()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisY.Maximum = 200;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Server Running...";
            button1.Enabled = false;
            startServer();
        }

        delegate void SetChartCallback(string text);
        TcpListener server;
        TcpClient connclient;
        NetworkStream ns;
        Thread t = null;
        WebClient client = null;

        Int32 port = 4020;
        IPAddress localAddr = IPAddress.Parse("192.168.137.1");

     

        private void startServer()
        {
            server = new TcpListener(localAddr, port);
            server.Start();
            connclient = server.AcceptTcpClient();
            ns = connclient.GetStream();
            t = new Thread(DoWork);
            t.Start();
        }

        public void DoWork()
        {
            byte[] bytes = new byte[256];

            try
            {

                // Read bytes from client
                int bytesRead = ns.Read(bytes, 0, bytes.Length);

                // Send back a response
                byte[] msg = System.Text.Encoding.ASCII.GetBytes("OK");
                ns.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", "OK");

                // Add the temperature point to the chart
                this.AddTempPoint(Encoding.ASCII.GetString(bytes, 0, bytesRead));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            connclient.Close();
            server.Stop();
            startServer();
        }

        private void AddTempPoint(string text)
        {
            if (this.chart1.InvokeRequired)
            {
                SetChartCallback d = new SetChartCallback(AddTempPoint);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (text.Length > 0)
                {
                    this.chart1.Series[0].Points.Add(double.Parse(text));
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                t.Abort();
            }
            catch (NullReferenceException ee) { }

            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            client.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        { 

            
            // Create web client simulating IE6.
            WebClient client = new WebClient();
            client.Headers["User-Agent"] =
            "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";

            // Download data.
            try
            {
                byte[] arr = client.DownloadData("http://192.168.137.9:4020/api/sensor");

                // Write values.
                string resp = Encoding.UTF8.GetString(arr);
                Console.WriteLine(resp);
                string[] parameters = resp.Split(',');
                foreach (string parameter in parameters)
                {
                    string[] keypair = parameter.Split(':');

                    if (keypair[0].Equals("temp"))
                    {
                        this.AddTempPoint(keypair[1]);
                        double num = Convert.ToDouble(keypair[1]);
                        client.UploadString("http://10.100.54.244:4020/thecloud/sensor?tempID=sensor_jlozano7&temp=" + num, "0");
                    }
                    else if (keypair[0].Equals("status"))
                    {
                        label1.Text = "Heat: " + keypair[1];

                        if (keypair[1].Equals("ON"))
                        {
                            label1.BackColor = Color.Red;
                            label1.ForeColor = Color.Black;
                        }
                        else
                        {
                            label1.BackColor = Color.LightBlue;
                            label1.ForeColor = Color.White;
                        }
                    }
                }
            }
            catch (WebException ee)
            {

            }
            
           
            

        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";

                // Download data.
                byte[] arr = client.DownloadData("http://192.168.137.9:4020/api/sensor/resetToken");

                // Write values.
                string resp = Encoding.UTF8.GetString(arr);
                Console.WriteLine(resp);
                if (resp.Equals("OK"))
                {
                    authserv.Text = "Token is reset.";
                }
            }
        }
    }
}
