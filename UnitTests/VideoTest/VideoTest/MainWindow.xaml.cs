using Newtonsoft.Json;
using System.IO;
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
using static VideoTest.JsonHandler;

namespace VideoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int countDown = 3;
        private DispatcherTimer CountDownTimer;
        private DispatcherTimer SuccessTimer;

        private bool FullTestExecuted = false;

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

            SuccessTimer = new DispatcherTimer();
            SuccessTimer.Interval = TimeSpan.FromSeconds(3); // 3 segundos para fechar após sucesso
            SuccessTimer.Tick += SuccessTimer_Tick;

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
                Lbl_Main.Content = $"Iniciando teste de vídeo em...{countDown}";


                countDown--;
            }
            else
            {
                CountDownTimer.Stop();
                TestRGBValidation();
            }
        }

        private void TestRGBValidation()
        {
            Lbl_Main.FontSize = 18;
            Lbl_Main.Visibility = Visibility.Collapsed;
            Rgb_Red.Visibility = Visibility.Visible;
            Rgb_Blue.Visibility = Visibility.Visible;
            Rgb_Green.Visibility = Visibility.Visible;

            Lbl_Main.Content = "É possivel ver as cores: Vermelho, azul e verde, corretamente.";
            Lbl_Main.Visibility = Visibility.Visible;

            Btn_Yes.Visibility = Visibility.Visible;
            Btn_No.Visibility = Visibility.Visible;
            


        }

        private void Btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            
            if (FullTestExecuted)
            {
                TestValidation();
            }
            else
            {
                FullScreenTestAsync();
            }
        }

        private void Btn_No_Click(object sender, RoutedEventArgs e)
        {
            Btn_Try_Again.Visibility = Visibility.Hidden;
            FullScreenTestAsync();
        }

        private async Task FullScreenTestAsync()
        {
            Rgb_Blue.Visibility = Visibility.Hidden;
            Rgb_Green.Visibility = Visibility.Hidden;
            Rgb_Red.Visibility = Visibility.Hidden;

            Btn_Yes.Visibility = Visibility.Hidden;
            Btn_No.Visibility = Visibility.Hidden;


           

            Lbl_Main.Content = "Entrando em tela cheia";
            
            await Task.Delay(3000);

            Lbl_Main.Visibility = Visibility.Hidden;

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;

            await Task.Delay(TimeSpan.FromSeconds(5));

            // Retorna ao tamanho original
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;

            Lbl_Main.Visibility = Visibility.Visible;
            Lbl_Main.Content = "Foi retornado corretamente do modo tela cheia?";
            Btn_Yes.Visibility = Visibility.Visible;
            Btn_No.Visibility = Visibility.Visible;


            FullTestExecuted = true;
                      
            
        }

        private void TestValidation()
        {
            Btn_Yes.Visibility = Visibility.Hidden;
            Btn_No.Visibility = Visibility.Hidden;

            Lbl_Main.FontSize = 28;
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