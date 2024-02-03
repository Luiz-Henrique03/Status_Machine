using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using static USBPortTest.JsonHandler;

namespace USBPortTest
{
    public partial class MainWindow : Window
    {
        private int countDown = 3;
        private DispatcherTimer countDownTimer;
        private DispatcherTimer successTimer;
        private int Tries = 3;
        List<string> jsonlist = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            countDownTimer = new DispatcherTimer();
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownTimer.Tick += CountDownTimer_Tick;

            successTimer = new DispatcherTimer();
            successTimer.Interval = TimeSpan.FromSeconds(3); 
            successTimer.Tick += SuccessTimer_Tick;

            StartCountdown();
        }

        private void Init()
        {
            countDown = 3;
            countDownTimer = new DispatcherTimer();
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownTimer.Tick += CountDownTimer_Tick;
            Img_Init.Visibility = Visibility.Visible;
            Img_Result.Visibility = Visibility.Hidden;
            Btn_TentarNovamente.Visibility = Visibility.Hidden;
            Lbl_Main.Content = "";
            StartCountdown();
        }

        private void StartCountdown()
        {
            countDownTimer.Start();
        }

        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            if (countDown > 0)
            {
                Lbl_Main.Content = $"Iniciando testes de portas USB em... {countDown}";
                countDown--;
            }
            else
            {
                countDownTimer.Stop();
                Img_Init.Visibility = Visibility.Hidden;
                TestExecution();
            }
        }

        private void SuccessTimer_Tick(object sender, EventArgs e)
        {
            successTimer.Stop();
            Close();
        }

        public void TestExecution()
        {
            string result = "";
            bool testResult = true;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub");

                foreach (ManagementObject device in searcher.Get())
                {
                    result += $"Device ID: {device["DeviceID"]}\n";
                    result += $"Caption: {device["Caption"]}\n";
                    result += $"Description: {device["Description"]}\n";
                    result += $"Status: {device["Status"]}\n";
                    result += "----------------------------------------\n";
                }

                Lbl_Main.FontSize = 14;

                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = result;
                });

                Task.Delay(3000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        TestValidation(result);
                    });
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = $"Não foi possível obter informações das portas USB {ex.Message}";
                });
            }
        }

        public void TestValidation(string result)
        {
            if (result.Contains("Error"))
            {
                StatusJson status = new StatusJson
                {
                    Resultado = "Reprovado",
                    Mensagem = "Porta USB com defeito"
                };

                string jsonString = JsonConvert.SerializeObject(status);
                jsonlist.Add(jsonString);
                Lbl_Main.FontSize = 32;
                Lbl_Main.Content = "Teste finalizado com falha";
                string caminhoImagem = "png-clipart-computer-icons-error-information-error-angle-triangle-thumbnail-removebg-preview.png";
                Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);

                Btn_TentarNovamente.Visibility = Visibility.Visible;

                Btn_TentarNovamente.Content = $"Tentar Novamente {Tries}";

                Img_Result.Source = imagemSource;
                Img_Result.Visibility = Visibility.Visible;

                JsonHandler.CreateStatusJson(jsonString);
            }
            else
            {
                StatusJson status = new StatusJson
                {
                    Resultado = "Aprovado",
                    Mensagem = "Todas as portas USBs conectadas funcionando."
                };

                string jsonString = JsonConvert.SerializeObject(status, Formatting.Indented);
                jsonlist.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);

                Lbl_Main.FontSize = 32;
                Lbl_Main.Content = "Teste finalizado com sucesso";
                string caminhoImagem = "success.png";
                Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);

                Img_Result.Source = imagemSource;
                Img_Result.Visibility = Visibility.Visible;

                // Inicia o timer para fechar após 3 segundos
                successTimer.Start();
            }
        }

        private void Btn_TentarNovamente_Click(object sender, RoutedEventArgs e)
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
            public string Resultado { get; set; }
            public string Mensagem { get; set; }
        }
    }
}
