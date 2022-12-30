using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleModManager.Models
{
    public class SettingsModel
    {
        public class Game
        {
            public string Name { get; set; } = "New Game";
            public string GameDirectory { get; set; }
            public string ModDirectory { get; set; }
        }

        public List<Game> Games { get; set; } = new List<Game>();
    }
}
