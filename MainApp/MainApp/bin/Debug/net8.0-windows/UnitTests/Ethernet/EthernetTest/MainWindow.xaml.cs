﻿using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json;
using System.Management;
using static EthernetTest.JsonHandler;

namespace EthernetTest
{

    public partial class MainWindow : Window
    {
        private int countDown = 3;
        private DispatcherTimer CountDownTimer;
        private DispatcherTimer SuccessTimer;
        private int Tries = 3;
        List<string> JsonList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            CountDownTimer = new DispatcherTimer();
            CountDownTimer.Interval = TimeSpan.FromSeconds(1);
            CountDownTimer.Tick += CountDownTimer_Tick;

            SuccessTimer = new DispatcherTimer();
            SuccessTimer.Interval = TimeSpan.FromSeconds(3); // 3 segundos para fechar após sucesso
            SuccessTimer.Tick += SuccessTimer_Tick;

            StartCountDown();
        }

        private void Init()
        {
            countDown = 3;
            Img_Init.Visibility = Visibility.Visible;
            Img_result.Visibility = Visibility.Hidden;
            Btn_Try_Again.Visibility = Visibility.Hidden;
            Lbl_Main.Content = "";
            StartCountDown();
        }

        private void StartCountDown()
        {
            CountDownTimer.Start();
        }

        private async void SuccessTimer_Tick(object sender, EventArgs e)
        {
            SuccessTimer.Stop();
            Close();
        }

        private async void CountDownTimer_Tick(object sender, EventArgs e)
        {
            if (countDown > 0)
            {
                Lbl_Main.Content = $"Iniciando teste de Conexão Ethernet em...{countDown}";
                Img_Init.Visibility = Visibility.Visible;
                countDown--;
            }
            else
            {
                CountDownTimer.Stop();
                TestValidationAsync();
            }
        }

        public async Task<bool> TestExecutionAsync()
        {
            bool IsPingSuccess = false;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Obtém todas as interfaces de rede

                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface nic in networkInterfaces)
                {
                    // Verifica se a interface é Ethernet e está operacional
                    string host = "www.google.com";
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                    {
                        using (Ping ping = new Ping())
                        {
                            PingReply reply = ping.Send(host);
                            if (reply.Status == IPStatus.Success)
                            {
                                IsPingSuccess = true;
                            }


                        }

                        Dispatcher.Invoke(() =>
                        {
                            Lbl_Main.FontSize = 20;
                            Lbl_Main.Content = $"Cabo Ethernet encontrado, validando conexão com internet";
                        });

                        await Task.Delay(3000);

                        if (IsPingSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            
                            Dispatcher.Invoke(() =>
                            {
                                Lbl_Main.Content = "Foi possível identificar a interface de rede, mas não foi possível fazer o Ping";
                            });
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Não foi possível encontrar conexão via cabo ethernet");
                        return false;
                    }
                }
            }

            return false;
        }

        private string GetSerial()
        {
            string serial = "Não disponível";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                serial = obj["SerialNumber"].ToString();
                break;
            }
            return serial;
        }

        public async Task TestValidationAsync()
        {
            if (await TestExecutionAsync())
            {
                Lbl_Main.FontSize = 28;
                Img_Init.Visibility = Visibility.Hidden;
                Lbl_Main.Content = "Teste finalizado com sucesso";
                string caminhoImagem = "success.png";
                Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);
                Img_result.Source = imagemSource;
                Img_result.Visibility = Visibility.Visible;

                StatusJson status = new StatusJson
                {
                    success = true,
                    msg = "Ethernet eprovada",
                    SN = GetSerial(),
                    type = "Ethernet"
                };

                string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
                JsonList.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);

                SuccessTimer.Start();
            }
            else
            {
                Img_Init.Visibility = Visibility.Hidden;
                string caminhoImagem = "Errors.png";
                Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);
                Img_result.Source = imagemSource;
                Img_result.Visibility = Visibility.Visible;
                Lbl_Main.Content = "Teste finalizado com falha";

                StatusJson status = new StatusJson
                {
                    success = false,
                    msg = "Ethernet reprovada",
                    SN = GetSerial(),
                    type = "Ethernet"
                };



                string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
                JsonList.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);
                await Task.Delay(2000);
                App.Current.Shutdown();
            }

        }

        private void Btn_Try_Again_Click(object sender, RoutedEventArgs e)
        {
            if (Tries > 0)
            {
                Init();
                Tries--;
            }
            else
            {
                Close();
            }
        }

        
    }

    public class JsonHandler
    {
        public static void CreateStatusJson(string jsonString)
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(directoryPath, "status.json");

            File.WriteAllText(filePath, jsonString);
        }

        public class StatusJson
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public string SN { get; set; }
            public string type { get; set; }
        }

    }
}   