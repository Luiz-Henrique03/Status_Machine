using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static CPUStressTest.JsonHandler;
using static System.Net.Mime.MediaTypeNames;

namespace CPUStressTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer CountDownTimer;
        private DispatcherTimer SuccessTimer;
        private int Tries = 3;
        private static bool stopThreads = false;
        private int countDown = 3;
        private List<string> JsonList = new List<string>();

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
                Lbl_Main.Content = $"Iniciando teste de CPU Stress em...{countDown}";
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
            int numThreads = Environment.ProcessorCount;
            Thread[] threads = new Thread[numThreads];

            bool success = true;

            try
            {
                for (int i = 0; i < numThreads; i++)
                {
                    threads[i] = new Thread(CPULoad);
                    threads[i].Start();
                }

                // Atualize a interface do usuário usando Dispatcher.Invoke
                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = "Estressando CPU...";
                });

                Thread.Sleep(15000);
                stopThreads = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro durante o teste de estresse: {ex.Message}");
                success = false;
            }
            finally
            {
                foreach (Thread thread in threads)
                {
                    if (thread != null && thread.IsAlive)
                        thread.Join(); // Aguarda a thread terminar
                }

                Dispatcher.Invoke(() =>
                {
                    Lbl_Main.Content = "Teste de estresse de CPU concluído.";
                });
            }

            return success;
        }


        static void CPULoad()
        {
            try
            {
                while (!stopThreads)
                {
                    // Este loop irá consumir CPU
                    double value = 1.0;
                    for (int i = 0; i < 100000; i++)
                    {
                        value += Math.Sqrt(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro durante a execução da thread: {ex.Message}");
            }
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
                    Resultado = "Aprovado",
                    Mensagem = "CPU aprovada"
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
                    Resultado = "Reprovado",
                    Mensagem = "CPU ão foi capaz de liidar com o teste"
                };

                Btn_Try_Again.Visibility = Visibility.Visible;
                Btn_Try_Again.Content = $"Tentar Novamente {Tries}";
                string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
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
            public string Resultado { get; set; }
            public string Mensagem { get; set; }
        }
    }

}



    

   
