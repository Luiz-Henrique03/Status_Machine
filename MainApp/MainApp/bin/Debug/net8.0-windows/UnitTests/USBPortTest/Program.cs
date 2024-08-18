using System;
using System.IO;
using System.Management;

namespace USBPortTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Aguardando a conexão de um dispositivo USB...");

            // Verificar se a plataforma suporta System.Management
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Use o ManagementEventWatcher para monitorar a conexão de dispositivos USB
                try
                {
                    WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                    using (ManagementEventWatcher watcher = new ManagementEventWatcher(query))
                    {
                        watcher.EventArrived += (sender, eventArgs) =>
                        {
                            Console.WriteLine("Dispositivo USB conectado!");
                        };

                        watcher.Start();

                        // Mantém o programa em execução
                        Console.WriteLine("Pressione Enter para sair.");
                        Console.ReadLine();

                        // Para a escuta de eventos quando o usuário pressionar Enter
                        watcher.Stop();
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    Console.WriteLine("O monitoramento de eventos USB não é suportado nesta plataforma.");
                }
            }
            else
            {
                Console.WriteLine("O monitoramento de eventos USB não é suportado nesta plataforma.");
            }
        }
    }
}

