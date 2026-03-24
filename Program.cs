using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LoxoneControlApp
{
    class Program
    {
        private static readonly string MiniserverIp = "192.168.5.105";
        private static readonly string Uzivatel = "admin";
        private static readonly string Heslo = "Miky2088Loxone123";
        private static readonly string LightControllerUuid = "204db7aa-006c-8fa2-ffff42f1439d4ef5";

        static async Task Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("LOXONE - POKUS O RUČNÍ AUTORIZACI");
                string rawAuth = $"{Uzivatel}:{Heslo}";
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(rawAuth);
                string base64 = Convert.ToBase64String(bytes);

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                while (true)
                {
                    Console.WriteLine("\n[1] ZAPNOUT | [0] VYPNOUT | [Q] KONEC");
                    string volba = Console.ReadLine()?.ToUpper();

                    if (volba == "1" || volba == "0")
                    {
                        string prikaz = (volba == "1") ? "on" : "minus";
                        string url = $"http://{MiniserverIp}/dev/sps/io/{LightControllerUuid}/{prikaz}";

                        try
                        {
                            Console.WriteLine($"Odesílám na: {url}");
                            var response = await client.GetAsync(url);
                            string content = await response.Content.ReadAsStringAsync();

                            if (content.Contains("Code=\"200\""))
                                Console.WriteLine(">>> ÚSPĚCH: Světlo reaguje!");
                            else
                                Console.WriteLine($">>> CHYBA {response.StatusCode}: {content}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($">>> CHYBA: {ex.Message}");
                        }
                    }
                    else if (volba == "Q") break;
                }
            }
        }
    }
}