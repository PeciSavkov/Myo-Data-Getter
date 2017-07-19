using System;
using System.Windows.Forms;

using MyoSharp.Device;
using MyoSharp.Communication;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System.IO;

namespace MyoTest
{
    public partial class Form1 : Form
    {
        IChannel myoChannel;
        IHub myoHub;
        string text;
        public int[] sen = new int[8];
        string rpy;
        string emg;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeMyo();
            timer1.Start();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            StopMyo();
            Application.Exit();
        }

        private void InitializeMyo()
        {
            myoChannel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(), MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            myoHub = Hub.Create(myoChannel);

            myoHub.MyoConnected += MyoHub_MyoConnected;
            myoHub.MyoDisconnected += MyoHub_MyoDisconnected;

            myoChannel.StartListening();
        }

        private void StopMyo()
        {
            myoChannel.StopListening();
            myoChannel.Dispose();
        }

        #region Event_Handlers
        private void MyoHub_MyoConnected(object sender, MyoEventArgs e)
        {
            MessageBox.Show("Myo е свързано!");
            
            e.Myo.Vibrate(VibrationType.Long);

            e.Myo.Unlock(UnlockType.Hold);

            var pose = HeldPose.Create(e.Myo, Pose.DoubleTap, Pose.FingersSpread, Pose.Fist, Pose.WaveIn, Pose.WaveOut);
            pose.Interval = TimeSpan.FromSeconds(2);
            pose.Start();
            
            pose.Triggered += Pose_Triggered;
            e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
            e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
            e.Myo.SetEmgStreaming(true);
        }

        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {
            for (int i = 0; i < 8; ++i)
            {
                sen[i] = e.EmgData.GetDataForSensor(i);
            }
            emg = "Sensor1: " + Convert.ToString( e.Myo.EmgData.GetDataForSensor(0)) + " Sensor2: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(1)) + " Sensor3: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(2))
                + " Sensor4: " + Environment.NewLine + Convert.ToString(e.Myo.EmgData.GetDataForSensor(3)) + " Sensor5: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(4) + " Sensor6: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(5))
                 + " Sensor7: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(6)) + " Sensor8: " + Convert.ToString(e.Myo.EmgData.GetDataForSensor(7)));
        }

        private void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
        {
            const float PI = (float)System.Math.PI;
            var roll = (e.Roll + PI) / (PI * 2.0f) * 100;
            var pitch = (e.Pitch + PI) / (PI * 2.0f) * 100;
            var yaw = (e.Yaw + PI) / (PI * 2.0f) * 100;
            rpy = Convert.ToString(roll) + " " + Convert.ToString(pitch) + " " + Convert.ToString(yaw);
            string data = "Roll: " + roll.ToString("#.##") + Environment.NewLine +
                           "Pitch: " + pitch.ToString("#.##") + Environment.NewLine +
                           "Yaw: " + yaw.ToString("#.##");
            InvokeDataRPY(data);
        }

        private void Pose_Triggered(object sender, PoseEventArgs e)
        {
            text += DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + " " + e.Pose.ToString() + Environment.NewLine + rpy + Environment.NewLine + emg + Environment.NewLine;
            InvokeData(e.Pose.ToString());
        }

        private void MyoHub_MyoDisconnected(object sender, MyoEventArgs e)
        {
            MessageBox.Show("Myo е изключено!");
        }
        #endregion
        void InvokeData(string Data)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(InvokeData), new object[] { Data });
                return;
            }
            rtbGestures.AppendText(Data + Environment.NewLine);
        }
        void InvokeDataRPY(string Data)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(InvokeDataRPY), new object[] { Data });
                return;
            }
            rtbRPY.Clear();
            rtbRPY.AppendText(Data + Environment.NewLine);
        }

        private void RefreshGraph()
        {
            chSensor1.Series["Сензор 1"].Points.AddY(sen[0]);
            chSensor2.Series["Сензор 2"].Points.AddY(sen[1]);
            chSensor3.Series["Сензор 3"].Points.AddY(sen[2]);
            chSensor4.Series["Сензор 4"].Points.AddY(sen[3]);
            chSensor5.Series["Сензор 5"].Points.AddY(sen[4]);
            chSensor6.Series["Сензор 6"].Points.AddY(sen[5]);
            chSensor7.Series["Сензор 7"].Points.AddY(sen[6]);
            chSensor8.Series["Сензор 8"].Points.AddY(sen[7]);
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            RefreshGraph();
        }

        private void запазиФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void изходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //saveFileDialog1.FileName = DateTime.Now.ToString();
            string Name = saveFileDialog1.FileName;
            File.WriteAllText(Name, text);
        }
    }
}
