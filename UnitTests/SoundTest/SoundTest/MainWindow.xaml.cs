using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static SoundTest.JsonHandler;

namespace SoundTest
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
        private int randomNumber;
        private int userNumber;
        private List<string> JsonList = new List<string>();
        private TaskCompletionSource<int> _userInputTaskSource;

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
            Img_Result.Visibility = Visibility.Hidden;
            Btn_Try_Again.Visibility = Visibility.Hidden;
            Lbl_Main.Content = "";
            Txt_Result.Visibility = Visibility.Hidden;
            StartCountDown();
        }

        private void StartCountDown()
        {
            CountDownTimer.Start();
        }

        private void SuccessTimer_Tick(object sender, EventArgs e)
        {
            SuccessTimer.Stop();
            Close();
        }

        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            if (countDown > 0)
            {
                countDown--;
                Lbl_Main.Content = $"Iniciando teste de Áudio em...{countDown}";
                Img_Init.Visibility = Visibility.Visible;
            }
            else
            {
                CountDownTimer.Stop();
                TestValidationAsync();
            }
        }

        public async Task<int> TestExecutionAsync()
        {
            Random random = new Random();
            randomNumber = random.Next(3, 9);

            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.Speak("O número aleatório é " + randomNumber);
            }

            IncreaseSystemVolume();

            return randomNumber;
        }

        private void IncreaseSystemVolume()
        {
            const int HWND_BROADCAST = 0xffff;
            const uint WM_APPCOMMAND = 0x319;
            const int APPCOMMAND_VOLUME_UP = 0x0a;
            SendMessage(HWND_BROADCAST, WM_APPCOMMAND, 0, APPCOMMAND_VOLUME_UP * 0x10000);
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        private async void TestValidationAsync()
        {
            Img_Init.Visibility = Visibility.Hidden;
            Lbl_Main.Content = "";
            try
            {
                randomNumber = await TestExecutionAsync();
                Txt_Result.Visibility = Visibility.Visible;
                Lbl_Main.Content = "Informe o número que você escutou";

                Txt_Result.Text = "";
                Tries = 3;

                await WaitForUserInputAsync();

                if (userNumber == randomNumber)
                {
                    Lbl_Main.Content = "Teste finalizado com sucesso";
                    Txt_Result.Visibility = Visibility.Hidden;
                    string caminhoImagem = "success.png";
                    Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                    BitmapImage imagemSource = new BitmapImage(uriImagem);
                    Img_Result.Source = imagemSource;
                    Img_Result.Visibility = Visibility.Visible;

                    StatusJson status = new StatusJson
                    {
                        Resultado = "Aprovado",
                        Mensagem = "Saída de som está funcionando corretamente"
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
                    Txt_Result.Visibility = Visibility.Hidden;
                    Uri uriImagem = new Uri(caminhoImagem, UriKind.Relative);
                    BitmapImage imagemSource = new BitmapImage(uriImagem);
                    Img_Result.Source = imagemSource;
                    Img_Result.Visibility = Visibility.Visible;
                    Lbl_Main.Content = "Teste finalizado com falha";

                    StatusJson status = new StatusJson
                    {
                        Resultado = "Reprovado",
                        Mensagem = "Saída de som não está funcionando corretamente"
                    };

                    Btn_Try_Again.Visibility = Visibility.Visible;
                    Btn_Try_Again.Content = $"Tentar Novamente {Tries}";
                    string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
                    JsonList.Add(jsonString);
                    JsonHandler.CreateStatusJson(jsonString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro durante o teste de áudio: " + ex.Message);
            }
        }

        private async Task<int> WaitForUserInputAsync()
        {
            _userInputTaskSource = new TaskCompletionSource<int>();

            Txt_Result.PreviewTextInput += Txt_Result_PreviewTextInput;

            return await _userInputTaskSource.Task;
        }

        private void Txt_Result_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out int digit))
            {
                Txt_Result.Text += digit.ToString();
                userNumber = int.Parse(Txt_Result.Text);

                if (Txt_Result.Text.Length >= randomNumber.ToString().Length)
                {
                    Txt_Result.PreviewTextInput -= Txt_Result_PreviewTextInput;
                    _userInputTaskSource.SetResult(userNumber);
                }
            }
            e.Handled = true;
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
