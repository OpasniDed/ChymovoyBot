using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.games
{
    public class DiseGame
    {
        public int RollGet { get; set; }

        public DiseGame()
        {
            var random = new System.Random();
            int firstThrow = random.Next(1, 7);
            int secondThrow = random.Next(1, 7);
            RollGet = firstThrow + secondThrow;
        }
    }
}
