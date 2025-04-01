using System.IO;
using System.Text.Json;
using AliciasSoftwareSim.Model;

namespace AliciasSoftwareSim;

public static class SaveService
{
    private const string SaveFile = "savegame.json";

    public static void SaveGame(Company company)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(company, options);
        File.WriteAllText(SaveFile, json);
    }

    public static Company LoadGame()
    {
        if (!File.Exists(SaveFile)) return null;

        string json = File.ReadAllText(SaveFile);
        return JsonSerializer.Deserialize<Company>(json);
    }
}