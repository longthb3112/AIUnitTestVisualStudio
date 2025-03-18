using System;
using System.IO;
using System.Text.Json;

namespace AIUnittestExtension
{
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
       "AIUnitTestExtension",
       "config.json"
        );

        static ConfigManager()
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        }

        public static void SaveSettings(ToolWindowSetting settings)
        {
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(ConfigPath, json);
        }

        public static ToolWindowSetting LoadSettings()
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ToolWindowSetting>(json) ?? new ToolWindowSetting();
            }
            return new ToolWindowSetting();
        }
    }
}
