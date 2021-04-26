using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;


//Bibliotecas do mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace automação_sef
{
    public partial class form_base : Form
    {
        string l1 = "sfinfo/l1";
        string l2 = "sfinfo/l2";  //#####TOPICO DE RECEBIMENTO
        string p1 = "sfinfo/p1";  //#####TOPICO DE RECEBIMENTO
        string p2 = "sfinfo/p2";  //#####TOPICO DE RECEBIMENTO
        string c1 = "sfinfo/c1";  //#####TOPICO DE RECEBIMENTO
        string c2 = "sfinfo/c2";  //#####TOPICO DE RECEBIMENTO
        string Recebido;
        int Recnum,freq=1,recp1,recp2;
        float Recnum2;
        int contl1 = 0;
        int contl2 = 0;
        int i = 0;
        bool conn=false;
        string broker = "192.168.18.2";
        //string broker = "168.205.178.139";
        //string broker = "dvdmqtt.ddns.net";
        //string broker = "test.mosquitto.org";
        //string broker = "iot.eclipse.org";
        MqttClient client;
        int mqttPort =1883;
        string clientId;
        int upt = 0;
        // string Recebido;
      
        public form_base()
        {
            InitializeComponent();
        }
        public void Conectar()
        {
            //client = new MqttClient(broker);    //###COM endereço###########
            //client = new MqttClient("iot.eclipse.org");
            //client = new MqttClient(IPAddress.Parse(broker));    //###COM IP###########
            //client = new MqttClient(IPAddress.Parse(broker),mqttPort,,);
            //client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            // clientId = Guid.NewGuid().ToString();
            client = new MqttClient(broker, mqttPort, false, null, null, null); //funcionou
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            clientId = "Comando";
            client.Connect(clientId);
            conn = client.IsConnected;
            if (conn == true)
            {
                client.Subscribe(new string[] { l1, l2, p1, p2, c1, c2 }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                Atualizar();
            }
            else
            {
                 Application.Restart();
            }
        }
        public void Atualizar()
        {
            client.Publish("sfinfo", Encoding.UTF8.GetBytes("atualizar"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Bt_conectar_Click(object sender, EventArgs e)
        {
            Conectar();
            if (conn==true)
            {
                Bt_conectar.BackgroundImage = Properties.Resources.conectado_removebg_preview4; 
                
            }
            else
            {
                Conectar();
                Bt_conectar.BackgroundImage = Properties.Resources.desconectar_removebg_preview;
            }

        }
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            
            if (e.Topic.ToString() == p1){
                Recebido = Encoding.UTF8.GetString(e.Message);
                recp1 = int.Parse(Recebido);
                if(recp1==1)
                {
                    Lb_porta1.Invoke((MethodInvoker)(() => Lb_porta1.Text = "Aberta"));
                    Lb_porta1.ForeColor = System.Drawing.Color.Red;
                }
                else 
                {
                    Lb_porta1.Invoke((MethodInvoker)(() => Lb_porta1.Text = "Fechada"));
                    Lb_porta1.ForeColor = System.Drawing.Color.Black;
                }

            }
            if (e.Topic.ToString() == p2){
                Recebido = Encoding.UTF8.GetString(e.Message);
                recp2 = int.Parse(Recebido);
                if (recp2 == 1)
                {
                    Lb_porta2.Invoke((MethodInvoker)(() => Lb_porta2.Text = "Aberta"));
                    Lb_porta2.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    Lb_porta2.Invoke((MethodInvoker)(() => Lb_porta2.Text = "Fechada"));
                    Lb_porta2.ForeColor = System.Drawing.Color.Black;
                }

            }
            if (e.Topic.ToString() == c1){
                Recebido = Encoding.UTF8.GetString(e.Message);
                Recnum2 = float.Parse(Recebido);
                if (Recnum2/100 < 4.0) 
                {
                    Lb_c1.Invoke((MethodInvoker)(() => Lb_c1.Text = "0cm"));
                    Pb_caixa1.Invoke((MethodInvoker)(() => Pb_caixa1.Value = 0));
                }
                else if (Recnum2/100 > 400)
                {
                    Lb_c1.Invoke((MethodInvoker)(() => Lb_c1.Text = "400cm")); 
                    Pb_caixa1.Invoke((MethodInvoker)(() => Pb_caixa1.Value =100));
                }
                else
                {
                    Lb_c1.Invoke((MethodInvoker)(() => Lb_c1.Text = Recebido + "cm"));
                    Pb_caixa1.Invoke((MethodInvoker)(() => Pb_caixa1.Value = Convert.ToInt32(Recnum2) / 400));
                }
            }
            if (e.Topic.ToString() == c2){
                Recebido = Encoding.UTF8.GetString(e.Message);
                Lb_c2.Invoke((MethodInvoker)(() => Lb_c2.Text = Recebido));
                Recnum = int.Parse(Recebido);
                Pb_caixa2.Invoke((MethodInvoker)(() => Pb_caixa2.Value = Recnum*100/1024));
                
            }

        }

        private void Bt_fechar_Click(object sender, EventArgs e)
        {
            base.OnClosed(e);
            if (conn==true) client.Disconnect();
            Close();
        }  
        private void Bt_ls1_Click(object sender, EventArgs e)
        {
            if (contl1==0 && conn==true)
            {
                client.Publish("sfinfo/l1", Encoding.UTF8.GetBytes("on"));
                Bt_ls1.BackgroundImage = Properties.Resources.lamp1acesa_removebg_preview;
                contl1 = 1;
            }
            else if(conn==true)
            {
                client.Publish("sfinfo/l1", Encoding.UTF8.GetBytes("off"));
                Bt_ls1.BackgroundImage =Properties.Resources.lamp1apagada_removebg_preview;
                contl1 = 0;
            }
        }
        private void Bt_ls2_Click(object sender, EventArgs e)
        {
            if (contl2==0 && conn==true)
            {
                client.Publish("sfinfo/l2", Encoding.UTF8.GetBytes("on"));
                Bt_ls2.BackgroundImage = Properties.Resources.lamp2acesa_removebg_preview;
                contl2 = 1;
            }
            else if(conn==true)
            {
                client.Publish("sfinfo/l2", Encoding.UTF8.GetBytes("off"));
                Bt_ls2.BackgroundImage = Properties.Resources.lamp2apagada_removebg_preview;
                contl2 = 0;
            }
        }

        private void Bt_atualizar_Click(object sender, EventArgs e)
        {
            client.Publish("sfinfo", Encoding.UTF8.GetBytes("atualizar"));
        }

        private void Lb_porta1_Click(object sender, EventArgs e)
        {
            
        }

        private void Lb_porta2_Click(object sender, EventArgs e)
        {

        }

        private void Cb_autoupdate_CheckedChanged(object sender, EventArgs e)
        {
            if (Cb_autoupdate.Checked)
            {
                if (conn == true)
                {
                    this.backgroundWorker1.RunWorkerAsync();
                    Lb_autoupdate.Text = "ON";
                }
                else
                {
                    Cb_autoupdate.Checked = false;
                }
            }
            else
            {
                backgroundWorker1.CancelAsync();
                Lb_autoupdate.Text = "OFF";
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           
            BackgroundWorker bw = sender as BackgroundWorker;
            while (true){
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                Thread.Sleep(freq*1000);
                Atualizar();               
            }


        }

        private void Lb_c1_Click(object sender, EventArgs e)
        {

        }

        private void Lb_freq_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Cb_freq_SelectedIndexChanged(object sender, EventArgs e)
        {
            freq = Convert.ToInt32(Cb_freq.SelectedItem);
        }

        private void Lb_c2_Click(object sender, EventArgs e)
        {

        }
    }
}
