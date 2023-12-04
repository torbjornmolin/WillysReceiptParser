using System.Data;
using System.Text.Json;
using UglyToad.PdfPig.Fonts.TrueType;

namespace WillysReceiptParser.Helpers;

public class DbSettings
{
    public string? Server { get; set; }
    public string? Database { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public int Port { get;  set; }

    public void WriteConfig()
    {
        var json = JsonSerializer.Serialize(this);

        File.WriteAllText("dbconfig.json", json);
    }

    public static DbSettings ReadConfig()
    {
        var json = File.ReadAllText("dbconfig.json");

        return JsonSerializer.Deserialize<DbSettings>(json) ?? throw new InvalidOperationException("Unable to deserialize db settings");
    }
}