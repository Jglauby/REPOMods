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
            "you're cute",
            "poop gee ha ha",
            "what are you talking about?",
            "your mom gay",
            "i love to goon",
            "i just gooned all over myself",
            "i am gay",
            "i love massive milkers",
            "i fear i might be a homosexual",
            "go away i'm looks maxing",
            "booo",
            "haha boobies",
            "eueueueueeueueue",
            "yyyyyyyyyyyyyyyy",
            "GAYyyybeeeeee",
            "i'm coming",
            "this week at jeremy franklin mitsubishi",
            "its ya boi, uhhhhh. skinny penis",
            "i sure hope it does!",
            "a child!",
            "i was hiddddiiiiinnngggggg",
            "sup chubby",
            "beeepp booopp beeep",
            "I SAIDDDDD, whoever threw that paperrrr. your moms a HOE!",
            "Bonk",
            "Yeet",
            "DEEENOOOOO STOOOPPPSSS",
            "kaboom",
            "later nerd",
            "nahhh imma do my own thing",
            "ass",
            "cunt",
            "aaaaachhhhoooooooo",
            "sneezes in robot",
            "no one is here"
        };
        private static readonly Random rng = new Random();

        public static string GetRandomPhrase()
        {
            return phraseList[rng.Next(phraseList.Count)];
        }
    }
}
