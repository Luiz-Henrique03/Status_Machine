using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Management;
using Newtonsoft.Json.Linq;
using System.Windows.Media;

namespace MainApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();

            // Inicialize o HttpClient
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private async void Btn_init_Click(object sender, RoutedEventArgs e)
        {
            Btn_Init.Visibility = Visibility.Hidden;
            Lbl_Info.Content = $"Executando testes no computador {GetComputerModel()}...";

            string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string TestPath = Path.Combine(BasePath, "UnitTests");

            if (Directory.Exists(TestPath))
            {
                string[] testDirectories = Directory.GetDirectories(TestPath);

                foreach (string testDirectory in testDirectories)
                {
                    string pathTests = Path.Combine(testDirectory, "bin");
                    string[] testExecutables = Directory.GetFiles(pathTests, "*.exe");

                    foreach (string executable in testExecutables)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = executable;

                        using (Process process = Process.Start(startInfo))
                        {
                            if (process != null)
                            {
                                await Task.Run(() => process.WaitForExit());

                                string statusFilePath = Path.Combine(testDirectory, "bin", "status.json");
                                if (File.Exists(statusFilePath))
                                {
                                    string statusContent = File.ReadAllText(statusFilePath);

                                    await Task.Delay(1);

                                    status_info.Text += "----------------------------------------------\n";
                                    status_info.Text += statusContent.Replace("{", "").Replace("}", "").Trim() + "\n";
                                    status_info.Foreground = Brushes.Blue;

                                    var jsonObject = JObject.Parse(statusContent);
                                    jsonObject["computerModel"] = GetComputerModel();

                                    var jsonString = jsonObject.ToString();

                                    await SendStatusToServer(jsonString);

                                }
                                else
                                {
                                    await Task.Delay(1);

                                    status_info.AppendText($"O arquivo status.json não foi encontrado em {testDirectory}.\n\n");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                status_info.AppendText("O diretório de testes não foi encontrado.\n\n");
            }

            Lbl_Info.Content = "Todos os testes foram concluídos.\n\n";
        }



        private async Task SendStatusToServer(string statusContent)
        {
            try
            {
                var content = new StringContent(statusContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/test", content);

                response.EnsureSuccessStatusCode();

                status_info.AppendText("Status enviado ao servidor");
            }
            catch (HttpRequestException ex)
            {
                status_info.AppendText($"Erro ao enviar status para o servidor: {ex.Message}\n");
            }
            catch (Exception ex)
            {
                status_info.AppendText($"Erro inesperado ao enviar status para o servidor: {ex.Message}\n");
            }
        }


        private string GetComputerModel()
        {
            string model = "Não disponível";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                model = obj["Model"].ToString();
                break;
            }

            return model;
        }

    }
}
