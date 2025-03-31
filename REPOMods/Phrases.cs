using OpJosModREPO.Tourettes.Patches;
using System;
using System.Collections.Generic;

namespace REPOMods
{
    public class Phrases
    {
        private static readonly List<string> phraseList = new List<string>
        {
            "Gosh Darn it",
            "you are gay",
            "FUCK!",
            "SHIT FUCK",
            "Oooo Fuckk meeeee",
            "you're cute",
            "ah shoot!",
            "ballocks",
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
            "These boots have seen everything..",
            "i was hiddddiiiiinnngggggg",
            "sup chubby",
            "beeepp booopp beeep",
            "I SAIDDDDD, whoever threw that paperrrr. your moms a HOE!",
            "I shouldn't have wished to live in more interesting times",
            "still me, despite everything",
            "DEEENOOOOO STOOOPPPSSS",
            "kaboom",
            "later nerd",
            "nahhh imma do my own thing",
            "stand ready for my arrival worm",
            "Is that blood? No... nevermind",
            "I've got a lot on my mind... and, well, in it.",
            "I wish I had a bag of holding",
            "Better not be cursed",
            "Still alive... so that's progress",
            "All's well that ends... not as bad as it could've",
            "Cursed to put my hands on everything",
            "No traps, please ...",
            "Do you know what else is massive?",
            "Blood comes easier these days....",
        };
        private static readonly Random rng = new Random();

        public static void SpeakRandomPhrase(PlayerAvatar __instance)
        {
            int beePosition = rng.Next(phraseList.Count + 1);
            if (beePosition == phraseList.Count)
            {
                //play whole bee script
                __instance.StartCoroutine(BeeMovie.PlayBeeMovie(__instance));
            }
            else
            {
                //just play random phrase
                PlayerAvatarPatch.isSpeakingBee = false;
                string phrase = phraseList[rng.Next(phraseList.Count)];
                __instance.ChatMessageSend(phrase, false);
            }
        }
    }
}
