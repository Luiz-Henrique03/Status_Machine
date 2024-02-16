using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private int userNumber; // Variável para armazenar o número digitado pelo usuário
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
            }
            else
            {
                CountDownTimer.Stop();
                TestValidationAsync();
            }
        }

        public async Task<int> TestExecutionAsync()
        {
            // Gerando um número aleatório entre 5 e 10
            Random random = new Random();
            randomNumber = random.Next(3, 9);

            // Iniciando a síntese de fala
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                // Emitindo a fala do número aleatório
                synth.Speak("O número aleatório é " + randomNumber);
            }

            // Aumentando o volume do sistema
            IncreaseSystemVolume();

            return randomNumber;
        }

        // Método para aumentar o volume do sistema
        private void IncreaseSystemVolume()
        {
            // Enviando uma mensagem para aumentar o volume do sistema
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

                // Limpa a caixa de texto antes de iniciar um novo teste
                Txt_Result.Text = "";

                // Reseta o número de tentativas
                Tries = 3;

                // Aguarda o usuário digitar o número
                await WaitForUserInput();

                // Verifica se o número digitado é igual ao número aleatório
                if (userNumber == randomNumber)
                {
                    MessageBox.Show("Você acertou!");
                }
                else
                {
                    MessageBox.Show("Você errou! Tente novamente.");
                    if (--Tries == 0)
                    {
                        MessageBox.Show("Você esgotou suas tentativas.");
                        Close();
                    }
                    else
                    {
                        // Reinicia o teste
                        TestValidationAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro durante o teste de áudio: " + ex.Message);
            }
        }

        private Task WaitForUserInput()
        {
            var tcs = new TaskCompletionSource<object>();

            // Adiciona o manipulador de eventos PreviewTextInput
            Txt_Result.PreviewTextInput += (sender, e) =>
            {
                if (int.TryParse(e.Text, out int digit))
                {
                    Txt_Result.Text += digit.ToString();
                    userNumber = int.Parse(Txt_Result.Text); // Atualiza o número digitado pelo usuário
                }
                e.Handled = true;

                
                if (Txt_Result.Text.Length >= randomNumber.ToString().Length)
                {
                    tcs.SetResult(null);
                }
            };

            return tcs.Task;
        }
    }
}
