using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LoxoneControlApp
{
    internal class Program
    {
        // Nastavení připojení k Loxone Miniserveru
        private static readonly string MiniserverIp = "192.168.5.105";
        private static readonly string Uzivatel = "admin";
        private static readonly string Heslo = "Miky2088Loxone123";
        private static readonly string LightControllerUuid = "204db7aa-006c-8fa2-ffff42f1439d4ef5";

        static async Task Main(string[] args)
        {
            using HttpClient client = new HttpClient();

            // Basic Auth
            string authData = $"{Uzivatel}:{Heslo}";
            byte[] authBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(authData);
            string base64Auth = Convert.ToBase64String(authBytes);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64Auth);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
            );

            Console.WriteLine("LOXONE OVLÁDÁNÍ SVĚTLA");

            bool running = true;

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("[1] Zapnout světlo");
                Console.WriteLine("[0] Vypnout světlo");
                Console.WriteLine("[Q] Konec");
                Console.Write("Zadej volbu: ");

                string? volba = Console.ReadLine()?.Trim().ToUpper();

                switch (volba)
                {
                    case "1":
                        await OdeslatPrikaz(client, "on");
                        break;

                    case "0":
                        await OdeslatPrikaz(client, "off");
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

        private static async Task OdeslatPrikaz(HttpClient client, string prikaz)
        {
            string url = $"http://{MiniserverIp}/dev/sps/io/{LightControllerUuid}/{prikaz}";

            try
            {
                Console.WriteLine($"Odesílám příkaz: {prikaz}");

                HttpResponseMessage response = await client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"HTTP Status: {response.StatusCode}");
                Console.WriteLine($"Odpověď: {content}");

                if (response.IsSuccessStatusCode && content.Contains("Code=\"200\""))
                {
                    Console.WriteLine("Příkaz úspěšně proveden.");
                }
                else
                {
                    Console.WriteLine("Loxone příkaz nepřijal.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
            }
        }
    }
}