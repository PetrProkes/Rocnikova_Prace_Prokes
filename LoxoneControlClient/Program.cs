using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoxoneControlClient
{
    internal class Program
    {
        private static readonly string ProxyUrl = "http://localhost:8080/loxone";

        static async Task Main(string[] args)
        {
            using HttpClient client = new HttpClient();

            Console.WriteLine("LOXONE OVLÁDÁNÍ PŘES PROXY");

            bool running = true;

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("[1] Zapnout světlo");
                Console.WriteLine("[0] Vypnout světlo");
                Console.WriteLine("[T] Vypsat teplotu");
                Console.WriteLine("[Q] Konec");
                Console.Write("Zadej volbu: ");

                string? volba = Console.ReadLine()?.Trim().ToUpper();

                switch (volba)
                {
                    case "1":
                        await OdeslatPrikaz(client, "light/on");
                        break;

                    case "0":
                        await OdeslatPrikaz(client, "light/off");
                        break;

                    case "T":
                        await OdeslatPrikaz(client, "temperature");
                        break;

                    case "Q":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Neplatná volba.");
                        break;
                }
            }

            Console.WriteLine("Program ukončen.");
        }

        private static async Task OdeslatPrikaz(HttpClient client, string endpoint)
        {
            string url = $"{ProxyUrl}/{endpoint}";

            try
            {
                Console.WriteLine($"Odesílám požadavek na: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"HTTP Status: {response.StatusCode}");
                Console.WriteLine($"Odpověď: {content}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Požadavek byl úspěšně odeslán přes proxy.");
                }
                else
                {
                    Console.WriteLine("Požadavek se nepovedl.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
            }
        }
    }
}