using SimpleModManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleModManager.Services
{
    public class SettingsService
    {
        private string _settingsFile = "settings.json";
        private JsonSerializerOptions _serializeOptions = new JsonSerializerOptions() { WriteIndented = true };

        public SettingsModel Read()
        {
            if (!File.Exists(_settingsFile))
            {
                Write(new SettingsModel());
            }

            var read = File.ReadAllText(_settingsFile);
            return JsonSerializer.Deserialize<SettingsModel>(read);
        }

        public void Write(SettingsModel model)
        {
            var serialized = JsonSerializer.Serialize<SettingsModel>(model, _serializeOptions);
            File.WriteAllText(_settingsFile, serialized);
        }
    }
}
