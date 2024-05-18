using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using static MouseTest.JsonHandler;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using static MouseTest.JsonHandler;
using System.IO;

namespace MouseTest
{
    public partial class MainWindow : Window
    {
        
        private int CountDown = 10; 
        private bool Cursor_Enter;
        private bool LeftClick;
        private bool RightClick;
        private List<String> JsonList = new List<String>();
        private DispatcherTimer timerEnter;
        private DispatcherTimer SuccessTimer;
        private DispatcherTimer timerLClick;
        private DispatcherTimer timerRClick;
        private int Tries = 3;
        private void TestsHandler()
        {
            if (Cursor_Enter)
            {
                ConfigLeftButton();

            }
            else if (LeftClick)
            {
                ConfigRightButton();
            }
           
        }

        private void Init()
        {
            CountDown = 3;
            Img_result.Visibility = Visibility.Hidden;
            Lbl_Main.FontSize = 14;
            Btn_Try_Again.Visibility = Visibility.Hidden;
            Lbl_Main.Content = "";
        }

        public MainWindow()
        {
            InitializeComponent();

            
            // Inicializa o timer
            timerEnter = new DispatcherTimer();
            timerEnter.Interval = TimeSpan.FromSeconds(1);
            timerEnter.Tick += TimerEnterTick;

            // Inicia o timer
            timerEnter.Start();

            // Adiciona o evento MouseEnter ao Rectangle
            Cursor_Check.MouseEnter += Cursor_Check_MouseEnter;

            SuccessTimer = new DispatcherTimer();
            SuccessTimer.Interval = TimeSpan.FromSeconds(3); // 3 segundos para fechar após sucesso
            SuccessTimer.Tick += SuccessTimer_Tick;

        }

        private void TimerEnterTick(object sender, EventArgs e)
        {
            CountDown--;

            if (Cursor_Enter)
            {
               
                Lbl_Main.Content = "Cursor aprovado";
                Cursor_Check.Fill = Brushes.Green;
                timerEnter.Stop();
                
            }
            else if (CountDown <= 0 && !Cursor_Enter)
            {
                Lbl_Main.Content = "Cursor reprovado";
                Cursor_Enter = false;
                TestValidation();
            }
            else
            {
                
                Lbl_Main.Content = $"Passe o mouse em cima do quadrado cinza. Tempo restante: {CountDown} segundos";
            }

            TestsHandler();
        }

        private void Cursor_Check_MouseEnter(object sender, MouseEventArgs e)
        {
            // Quando o mouse entra na área do retângulo, define a flag como true
            Cursor_Enter = true;
        }

        private void Left_Click_Check_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            LeftClick = true;
        }

        private void Right_Click_Check_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            RightClick = true;
        }

         private void ConfigLeftButton()
         {
             Cursor_Check.Visibility = Visibility.Hidden;
             
             CountDown = 10;
             timerLClick = new DispatcherTimer();
             timerLClick.Interval = TimeSpan.FromSeconds(1);
             timerLClick.Tick += TimerLClickTick;
             Left_Click_Check.MouseLeftButtonDown += Left_Click_Check_MouseLeftButtonDown;

             // Inicia o timer
             timerLClick.Start();
         }


        private void TimerLClickTick(object sender, EventArgs e)
        {
            CountDown--;
            Left_Click_Check.Visibility = Visibility.Visible;
            if (LeftClick)
            {
                Lbl_Main.Content = "Botão esquerdo aprovado";
                Left_Click_Check.Fill = Brushes.Green;
                timerLClick.Stop();
                Dispatcher.Invoke(() =>
                {
                    ConfigRightButton();
                });
            }
            else if (CountDown <= 0 && !LeftClick)
            {
                Lbl_Main.Content = "Botão reprovado";
                LeftClick = false;
                TestValidation();
            }
            else
            {

                Lbl_Main.Content = $"Clique com o botão esquerdo em cima do quadrado cinza. Tempo restante: {CountDown} segundos";
            }
            
        }

        private void ConfigRightButton()
        {
            Left_Click_Check.Visibility = Visibility.Hidden;
            CountDown = 10;
            timerRClick = new DispatcherTimer();
            
            timerRClick.Interval = TimeSpan.FromSeconds(1);
            timerRClick.Tick += TimerRClickTick;
            Right_Click_Check.MouseRightButtonDown += Right_Click_Check_MouseRightButtonDown;

            // Inicia o timer
            timerRClick.Start();
        }

        private void TimerRClickTick(object sender, EventArgs e)
        {
            CountDown--;
            Right_Click_Check.Visibility = Visibility.Visible;

            if (RightClick)
            {
                Lbl_Main.Content = "Botão Direito aprovado";
                Right_Click_Check.Fill = Brushes.Green;
                timerRClick.Stop();

                Dispatcher.Invoke(() =>
                {
                    TestValidation();
                });

            }
            else if (CountDown <= 0 && !RightClick)
            {
                Lbl_Main.Content = "Botão reprovado";
                RightClick = false;
                TestValidation();
            }
            else
            {
                Lbl_Main.Content = $"Clique com o botão Direito em cima do quadrado cinza. Tempo restante: {CountDown} segundos";
            }

        }

        private void TestValidation()
        {
            Right_Click_Check.Visibility = Visibility.Hidden;
            Lbl_Main.FontSize = 24;

            Cursor_Check.Visibility = Visibility.Hidden;
            Left_Click_Check.Visibility = Visibility.Hidden;
            Right_Click_Check.Visibility = Visibility.Hidden;

            if (Cursor_Enter && LeftClick && RightClick)
            {
                Lbl_Main.Content = "Teste aprovado";
                string ImgPath = "success.png";
                Uri uriImagem = new Uri(ImgPath, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);
                Img_result.Source = imagemSource;
                Img_result.Visibility = Visibility.Visible;
                Img_result.Visibility = Visibility.Visible;
                StatusJson status = new StatusJson
                {
                    result = "Aprovado",
                    msg = "Mouse funcionando"
                };

                string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
                JsonList.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);

                SuccessTimer.Start();
            }else if(!Cursor_Enter || LeftClick || RightClick)
            {
                Lbl_Main.Content = "Teste reprovado";
                Btn_Try_Again.Visibility = Visibility.Visible;
                Btn_Try_Again.Content = $"Tentar Novamente {Tries}";

                string ImgPath = "Errors.png";
                Uri uriImagem = new Uri(ImgPath, UriKind.Relative);
                BitmapImage imagemSource = new BitmapImage(uriImagem);
                Img_result.Source = imagemSource;
                Img_result.Visibility = Visibility.Visible;
                Img_result.Visibility = Visibility.Visible;
                StatusJson status = new StatusJson
                {
                    result = "Reprovado",
                    msg = "Mouse não passou nos testes em relação a sua funcionalidade"
                };

                string jsonString = JsonConvert.SerializeObject(status, Newtonsoft.Json.Formatting.Indented);
                JsonList.Add(jsonString);
                JsonHandler.CreateStatusJson(jsonString);
            }
        }

        private async void SuccessTimer_Tick(object sender, EventArgs e)
        {
            SuccessTimer.Stop();
            Close();
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
