using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json;
using static PingTest.JsonHandler;

namespace PingTest
{
    public partial class MainWindow : Window
    {
        private int countDown = 3;
        private DispatcherTimer CountDownTimer;
        private DispatcherTimer SuccessTimer; // Novo timer para fechamento após sucesso
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

        private async void CountDownTimer_Tick(object sender, EventArgs e)
        {
            if (countDown > 0)
            {
                Lbl_Main.Content = $"Iniciando teste de Ping em...{countDown}";
                Img_Init.Visibility = Visibility.Visible;
                countDown--;
            }
            else
            {
                CountDownTimer.Stop();
                await TestValidationAsync();
            }
        }

        private async void SuccessTimer_Tick(object sender, EventArgs e)
        {
            SuccessTimer.Stop();
            Close();
        }

        public async Task<bool> TestExecutionAsync()
        {
            string hostname = "www.google.com";
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = $"Enviando ping ao host {hostname}";
                });

                await Task.Delay(3000);

                bool success = await Task.Run(() =>
                {
                    try
                    {
                        Ping pingSender = new Ping();
                        PingReply reply = pingSender.Send(hostname);

                        return reply.Status == IPStatus.Success;
                    }
                    catch (PingException)
                    {
                        return false;
                    }
                });

                return success;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = "Não foi possível fazer o teste de ping";
                });

                return false;
            }
        }

        public async Task TestValidationAsync()
        {
            if (await TestExecutionAsync())
            {
                Img_Init.Visibility = Visibility.Hidden;
                Lbl_Main.Content = "Teste finalizado com sucesso";
                string caminhoImagem = "success.png";
                Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);
                Img_result.Source = imagemSource;
                Img_result.Visibility = Visibility.Visible;

                StatusJson status = new StatusJson
                {
                    result = "Aprovado",
                    msg = "Computador enviou com sucesso pacote via ping"
                };

                string jsonString = JsonConvert.SerializeObject(status, Formatting.Indented);
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
                    result = "Reprovado",
                    msg = "Computador não foi capaz de enviar pacote"
                };

                Btn_Try_Again.Visibility = Visibility.Visible;
                Btn_Try_Again.Content = $"Tentar Novamente {Tries}";
                string jsonString = JsonConvert.SerializeObject(status, Formatting.Indented);
                JsonList.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);
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
            public string result { get; set; }
            public string msg { get; set; }
        }
    }
}
