using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REPOMods
{
    public class Phrases
    {
        private static readonly List<string> phraseList = new List<string>
        {
            "Hello, world!",
            "Stay positive!",
            "Keep moving forward!",
            "Never give up!"
        };
        private static readonly Random rng = new Random();

        public static string GetRandomPhrase()
        {
            return phraseList[rng.Next(phraseList.Count)];
        }
    }
}
