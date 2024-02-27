using Newtonsoft.Json;
using System.IO;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static FanTest.JsonHandler;


namespace FanTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                Lbl_Main.Content = $"Iniciando teste de funcionamento de fans em...{countDown}";
                Img_Init.Visibility = Visibility.Visible;
                countDown--;
            }
            else
            {
                CountDownTimer.Stop();
                TestValidationAsync();
            }
        }

        private async Task<bool> TestExecutionTest()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Fan");

            int totalFans = 0;
            int workingFans = 0;
            string message = "";

      
            foreach (ManagementObject queryObj in searcher.Get())
            {
                string hardwareType = queryObj["SystemCreationClassName"].ToString();
                string fanName = queryObj["Name"].ToString();
                // Verifica se a ventoinha está presente
                bool isFanPresent = Convert.ToBoolean(queryObj["Availability"]);

                if (isFanPresent)
                {
                    totalFans++;

                    // Verifica se a ventoinha está ligada
                    bool isFanRunning = Convert.ToBoolean(queryObj["ActiveCooling"]);

                    if (isFanRunning)
                    {
                        workingFans++;
                    }
                }
                else
                {
                    message = "Não há fans";
                }
            }

          

            Dispatcher.Invoke(() =>
            {
                Lbl_Main.Content = message;
            });

            return workingFans == totalFans;
        }


        private async void TestValidationAsync()
        {
            Img_Init.Visibility = Visibility.Hidden;
            if (await TestExecutionTest())
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
                    Mensagem = "Computador enviou com sucesso pacote via ping"
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
                    Mensagem = "Computador não foi capaz de enviar pacote"
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
            string filePath = System.IO.Path.Combine(directoryPath, "status.json");

            File.WriteAllText(filePath, jsonString);
        }

        public class StatusJson
        {
            public string Resultado { get; set; }
            public string Mensagem { get; set; }
        }
    }
}