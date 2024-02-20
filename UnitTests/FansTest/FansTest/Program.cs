using System;
using System.Management;

class Program
{
    static void Main(string[] args)
    {
        // Query para buscar informações sobre as ventoinhas
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Fan");

        foreach (ManagementObject queryObj in searcher.Get())
        {
            string hardwareType = queryObj["SystemCreationClassName"].ToString();
            string fanName = queryObj["Name"].ToString();
            // Verifica se a ventoinha está presente
            bool isFanPresent = Convert.ToBoolean(queryObj["Availability"]);

            if (isFanPresent)
            {
                // Verifica se a ventoinha está ligada
                bool isFanRunning = Convert.ToBoolean(queryObj["ActiveCooling"]);

                // Exibe o resultado
                Console.WriteLine($"A ventoinha do hardware '{hardwareType}' ({fanName}) está presente.");
                Console.WriteLine("A ventoinha está ligada: " + isFanRunning);
            }
            else
            {
                Console.WriteLine($"Não foi possível detectar a ventoinha do hardware '{hardwareType}'.");
            }
        }
    }
}
