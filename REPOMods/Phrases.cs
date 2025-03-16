using System;
using System.Collections.Generic;

namespace REPOMods
{
    public class Phrases
    {
        private static readonly List<string> phraseList = new List<string>
        {
            "Re re reeeeetard",
            "you are gay",
            "FUCK!",
            "SHIT FUCK",
            "Oooo Fuckk meeeee",
            "you're cute"
        };
        private static readonly Random rng = new Random();

        public static string GetRandomPhrase()
        {
            return phraseList[rng.Next(phraseList.Count)];
        }
    }
}
