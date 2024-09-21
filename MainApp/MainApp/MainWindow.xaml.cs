﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Management;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.Net.Sockets;
using System.Net;

namespace MainApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://app-management-api.onrender.com/");  

            Task.Run(() => SendInitialInfo());

            Task.Run(() => CheckServerForTasks());
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

        private static string GetLocalIPAddress()
        {
            string localIP = "Não foi possível obter o endereço IP";
            try
            {
                foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = address.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao obter o endereço IP: " + ex.Message);
            }

            return localIP;
        }

        private async Task SendInitialInfo()
        {
            try
            {
                var initialInfo = new JObject
                {
                    {"IP", GetLocalIPAddress() },
                    { "model", GetComputerModel() },
                    { "SN", GetSerial() },
                    { "online", true }
                };

                var content = new StringContent(initialInfo.ToString(), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/computer", content);

                response.EnsureSuccessStatusCode();

                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText("Informações iniciais enviadas ao servidor.\n");
                });
            }
            catch (HttpRequestException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro ao enviar informações iniciais para o servidor: {ex.Message}\n");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro inesperado ao enviar informações iniciais para o servidor: {ex.Message}\n");
                });
            }
        }

        private async Task CheckServerForTasks()
        {
            while (true)
            {
                try
                {
                    var response = await httpClient.GetAsync("/queue?status=1&SN="+GetSerial());

                    if (response.IsSuccessStatusCode)
                    {
                        var taskData = await response.Content.ReadAsStringAsync();

                        if (taskData.Trim().StartsWith("["))
                        {
                            // É um array JSON
                            var jsonArray = JArray.Parse(taskData);

                            foreach (var jsonObject in jsonArray)
                            {
                                await HandleTask(jsonObject);
                            }
                        }
                        else if (taskData.Trim().StartsWith("{"))
                        {
                            var jsonObject = JObject.Parse(taskData);
                            await HandleTask(jsonObject);
                        }
                        else
                        {
                            Dispatcher.Invoke(() => status_info.AppendText("Formato inesperado de resposta do servidor.\n"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => status_info.AppendText($"Erro ao consultar servidor: {ex.Message}\n"));
                }

                await Task.Delay(5000);  // Aguarda 5 segundos antes de checar novamente
            }
        }

        private async Task HandleTask(JToken jsonObject)
        {
            try
            {
                int status = (int)jsonObject["status"];
                string method = jsonObject["method"].ToString();
                string taskId = jsonObject["_id"].ToString();  // Pega o ID da tarefa

                if (status == 1)
                {
                    Dispatcher.Invoke(() => status_info.AppendText("Tarefa pendente recebida.\n"));

                    await RunTests(method, taskId);  // Passa o método e o ID da tarefa
                }
                else if (status == 2)
                {
                    Dispatcher.Invoke(() => status_info.AppendText("Falha na tarefa anterior.\n"));
                }
                else if (status == 3)
                {
                    Dispatcher.Invoke(() => status_info.AppendText("Tarefa concluída com sucesso.\n"));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => status_info.AppendText($"Erro ao processar tarefa: {ex.Message}\n"));
            }
        }

        private async Task UpdateTaskStatus(string taskId, int status)
        {
            try
            {
                var statusUpdate = new JObject
        {
            { "status", status }
        };

                var content = new StringContent(statusUpdate.ToString(), Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"/queue/{taskId}", content);

                response.EnsureSuccessStatusCode();

                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Status da tarefa {taskId} atualizado para {status}.\n");
                });
            }
            catch (HttpRequestException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro ao atualizar o status da tarefa {taskId}: {ex.Message}\n");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro inesperado ao atualizar o status da tarefa {taskId}: {ex.Message}\n");
                });
            }
        }

       


        private async Task RunTests(string method, string taskId)
        {
            string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string TestPath = Path.Combine(BasePath, "UnitTests");

            if (Directory.Exists(TestPath))
            {
                string testDirectory = Path.Combine(TestPath, method);  // Busca a pasta baseada no método do servidor
                if (Directory.Exists(testDirectory))
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

                                    Dispatcher.Invoke(() =>
                                    {
                                        status_info.Text += "----------------------------------------------\n";
                                        status_info.Text += statusContent.Replace("{", "").Replace("}", "").Trim() + "\n";
                                        status_info.Foreground = Brushes.Blue;
                                    });

                                    var jsonObject = JObject.Parse(statusContent);
                                    var jsonString = jsonObject.ToString();

                                    await SendStatusToServer(jsonString);

                                    // Após enviar o status, atualizar a tarefa para status 3 (concluído)
                                    await UpdateTaskStatus(taskId, 3);  // Aqui fazemos o PUT com o ID e status 3
                                }
                                else
                                {
                                    await Task.Delay(1);

                                    Dispatcher.Invoke(() =>
                                    {
                                        status_info.AppendText($"O arquivo status.json não foi encontrado em {testDirectory}.\n\n");
                                    });

                                    // Atualizar a tarefa para status 2 (falha) se o arquivo status.json não for encontrado
                                    await UpdateTaskStatus(taskId, 2);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        status_info.AppendText($"O diretório do método {method} não foi encontrado.\n\n");
                    });

                    // Atualizar a tarefa para status 2 (falha) se o diretório não for encontrado
                    await UpdateTaskStatus(taskId, 2);
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText("O diretório de testes não foi encontrado.\n\n");
                });

                // Atualizar a tarefa para status 2 (falha) se o diretório de testes não for encontrado
                await UpdateTaskStatus(taskId, 2);
            }

            Dispatcher.Invoke(() =>
            {
                Lbl_Info.Content = $"Todos os testes do método {method} foram concluídos.\n\n";
            });
        }



        private async Task SendStatusToServer(string statusContent)
        {
            try
            {
                var content = new StringContent(statusContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/test", content);

                response.EnsureSuccessStatusCode();

                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText("Status enviado ao servidor\n");

                    // Verifica se o JSON contém resultado aprovado
                    var jsonObject = JObject.Parse(statusContent);
                    if (jsonObject["resultado"]?.ToString() == "aprovado")
                    {
                        // Fecha a aplicação em 3 segundos
                        var timer = new System.Timers.Timer(3000);
                        timer.Elapsed += (sender, e) => Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                        timer.Start();
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro ao enviar status para o servidor: {ex.Message}\n");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    status_info.AppendText($"Erro inesperado ao enviar status para o servidor: {ex.Message}\n");
                });
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
