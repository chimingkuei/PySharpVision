﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static PySharpVision.BaseLogRecord;
using System.Diagnostics;
using System.Web;

namespace PySharpVision
{
    #region Config Class
    public class SerialNumber
    {
        [JsonProperty("Parameter1_val")]
        public string Parameter1_val { get; set; }
        [JsonProperty("Parameter2_val")]
        public string Parameter2_val { get; set; }
    }

    public class Model
    {
        [JsonProperty("SerialNumbers")]
        public SerialNumber SerialNumbers { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("Models")]
        public List<Model> Models { get; set; }
    }
    #endregion

    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Config
        private SerialNumber SerialNumberClass()
        {
            SerialNumber serialnumber_ = new SerialNumber
            {
                Parameter1_val = Parameter1.Text,
                Parameter2_val = Parameter2.Text
            };
            return serialnumber_;
        }

        private void LoadConfig(int model, int serialnumber, bool encryption = false)
        {
            List<RootObject> Parameter_info = Config.Load(encryption);
            if (Parameter_info != null)
            {
                Parameter1.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter1_val;
                Parameter2.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter2_val;
            }
            else
            {
                // 結構:2個Models、Models下在各2個SerialNumbers
                SerialNumber serialnumber_ = SerialNumberClass();
                List<Model> models = new List<Model>
                {
                    new Model { SerialNumbers = serialnumber_ },
                    new Model { SerialNumbers = serialnumber_ }
                };
                List<RootObject> rootObjects = new List<RootObject>
                {
                    new RootObject { Models = models },
                    new RootObject { Models = models }
                };
                Config.InitSave(rootObjects, encryption);
            }
        }
       
        private void SaveConfig(int model, int serialnumber, bool encryption = false)
        {
            Config.Save(model, serialnumber, SerialNumberClass(), encryption);
        }
        #endregion

        private void CameraInit()
        {
            BC.Display = Display_Windows;
            bool state;
            BC.Init(out state);
            if (state)
            {
                BC.OpenCamera();
            }
        }

        private void DisTcpConnect(TcpClient client, NetworkStream stream)
        {
            if (cts.Token.IsCancellationRequested)
            {
                Do.Sendmsg(stream, "DisTcpConnect!");
                client.Close();
                return;
            }
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CameraInit();
            LoadConfig(0, 0);
        }
        BaseConfig<RootObject> Config = new BaseConfig<RootObject>();
        CommunicationHandler Do = new CommunicationHandler();
        CancellationTokenSource cts;
        Basler BC = new Basler();
        #region Log
        BaseLogRecord Logger = new BaseLogRecord();
        //Logger.WriteLog("儲存參數!", LogLevel.General, richTextBoxGeneral);
        #endregion
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Start):
                    {
                        cts = new CancellationTokenSource();
                        Task.Run(() =>
                        {
                            try
                            {
                                Byte[] bytes = new Byte[256];
                                (TcpListener server, TcpClient client, NetworkStream stream) = Do.TcpConnect();
                                while (true)
                                {
                                    string[] files = Directory.GetFiles(@"D:\Chimingkuei\repos\PySharpVision\Original Image");
                                    int filenum1 = files.Length;
                                    DisTcpConnect(client, stream);
                                    while (filenum1 != 0)
                                    {
                                        DisTcpConnect(client, stream);
                                        Do.Getmsg(bytes, stream.Read(bytes, 0, bytes.Length));
                                        Do.SaveMemory(files[filenum1-1], "5MImage");
                                        //Do.SaveMemory(BC.result, "5MImage");
                                        Do.Sendmsg(stream, "Transfer images to memory!");
                                        File.Move(System.IO.Path.Combine(@"D:\Chimingkuei\repos\PySharpVision\Original Image", System.IO.Path.GetFileName(files[filenum1 - 1])), System.IO.Path.Combine(@"D:\Chimingkuei\repos\PySharpVision\Output Image", System.IO.Path.GetFileName(files[filenum1 - 1])));
                                        Thread.Sleep(200);
                                        filenum1 -= 1;
                                    }
                                    Thread.Sleep(200);
                                }
                            }
                            catch (SocketException ex)
                            {
                                Console.WriteLine("SocketException: {0}", ex);
                            }
                            finally
                            {
                                Do.DisTcpConnect();
                            }
                        }, cts.Token);
                        Do.RunPython();
                        break;
                    }
                case nameof(Stop):
                    {
                        cts.Cancel();
                        break;
                    }
                case nameof(GetImageBytesLength):
                    {
                        if (!string.IsNullOrEmpty(Image_Path.Text))
                        {
                            Console.WriteLine(Do.SaveMemory(Image_Path.Text, "ImageShareMemory"));
                        }
                        else
                        {
                            MessageBox.Show("請輸入影像路徑!", "確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
                case nameof(Continue_Acquisition):
                    {
                        BC.ContinueAcquisition();
                        break;
                    }
                case nameof(Stop_Continue_Acquisition):
                    {
                        BC.StopAcquisition();
                        break;
                    }
            }
        }
        #endregion





    }
}
