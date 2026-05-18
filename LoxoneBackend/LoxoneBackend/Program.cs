using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string MiniserverIp = "192.168.5.105";
string Uzivatel = "admin";
string Heslo = "Miky2088Loxone123";

string LightControllerUuid = "204db7aa-006c-8fa2-ffff42f1439d4ef5";
string TemperatureUuid = "204db4c1-01b6-1226-ffff42f1439d4ef5";

app.MapGet("/", () =>
{
    return "Loxone backend běží. Použij /light/on, /light/off nebo /temperature";
});

app.MapGet("/light/on", async () =>
{
    return await OdeslatPrikazSvetlo("on");
});

app.MapGet("/light/off", async () =>
{
    return await OdeslatPrikazSvetlo("off");
});

app.MapGet("/temperature", async () =>
{
    return await ZiskatTeplotu();
});

async Task<IResult> OdeslatPrikazSvetlo(string prikaz)
{
    using HttpClient client = VytvorClient();

    string url = $"http://{MiniserverIp}/dev/sps/io/{LightControllerUuid}/{prikaz}";

    try
    {
        HttpResponseMessage response = await client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();

        bool uspech = response.IsSuccessStatusCode && content.Contains("Code=\"200\"");

        if (uspech)
        {
            return Results.Ok(new
            {
                zprava = $"Příkaz '{prikaz}' byl úspěšně proveden.",
                httpStatus = response.StatusCode.ToString(),
                odpovedLoxone = content
            });
        }

        return Results.BadRequest(new
        {
            zprava = $"Loxone příkaz '{prikaz}' nepřijal.",
            httpStatus = response.StatusCode.ToString(),
            odpovedLoxone = content
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Chyba při komunikaci s Loxone Miniserverem",
            detail: ex.Message
        );
    }
}

async Task<IResult> ZiskatTeplotu()
{
    using HttpClient client = VytvorClient();

    string url = $"http://{MiniserverIp}/dev/sps/io/{TemperatureUuid}/state";

    try
    {
        HttpResponseMessage response = await client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return Results.BadRequest(new
            {
                zprava = "Nepodařilo se načíst teplotu z Loxone.",
                httpStatus = response.StatusCode.ToString(),
                odpovedLoxone = content
            });
        }

        string? teplota = NajdiHodnotuVLoxoneOdpovedi(content);

        if (teplota == null)
        {
            return Results.Ok(new
            {
                zprava = "Odpověď z Loxone byla přijata, ale teplota nebyla automaticky rozpoznána.",
                odpovedLoxone = content
            });
        }

        return Results.Ok(new
        {
            zprava = "Teplota načtena.",
            teplota = teplota,
            jednotka = "°C",
            odpovedLoxone = content
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Chyba při čtení teploty z Loxone",
            detail: ex.Message
        );
    }
}

HttpClient VytvorClient()
{
    HttpClient client = new HttpClient();

    string authData = $"{Uzivatel}:{Heslo}";
    byte[] authBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(authData);
    string base64Auth = Convert.ToBase64String(authBytes);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", base64Auth);

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
    );

    return client;
}

string? NajdiHodnotuVLoxoneOdpovedi(string content)
{
    Match match = Regex.Match(content, "value=\"([^\"]+)\"");

    if (match.Success)
    {
        return match.Groups[1].Value;
    }

    match = Regex.Match(content, "value='([^']+)'");

    if (match.Success)
    {
        return match.Groups[1].Value;
    }

    match = Regex.Match(content, "\"value\"\\s*:\\s*\"?([0-9.,-]+)\"?");

    if (match.Success)
    {
        return match.Groups[1].Value.Replace(",", ".");
    }

    match = Regex.Match(content, "<LL[^>]*value=\"([^\"]+)\"");

    if (match.Success)
    {
        return match.Groups[1].Value;
    }

    return null;
}

app.Run("http://0.0.0.0:5000");