using OpJosModREPO.Tourettes.Patches;
using System;
using System.Collections.Generic;

namespace OpJosModREPO
{
    public class Phrases
    {
        private static readonly List<string> phraseList = new List<string>();
        private static readonly Random rng = new Random();

        private static void setupPhrases()
        {
            phraseList.Add(ConfigVariables.phrase1);
            phraseList.Add(ConfigVariables.phrase2);
            phraseList.Add(ConfigVariables.phrase3);
            phraseList.Add(ConfigVariables.phrase4);
            phraseList.Add(ConfigVariables.phrase5);
            phraseList.Add(ConfigVariables.phrase6);
            phraseList.Add(ConfigVariables.phrase7);
            phraseList.Add(ConfigVariables.phrase8);
            phraseList.Add(ConfigVariables.phrase9);
            phraseList.Add(ConfigVariables.phrase10);
            phraseList.Add(ConfigVariables.phrase11);
            phraseList.Add(ConfigVariables.phrase12);
            phraseList.Add(ConfigVariables.phrase13);
            phraseList.Add(ConfigVariables.phrase14);
            phraseList.Add(ConfigVariables.phrase15);
            phraseList.Add(ConfigVariables.phrase16);
            phraseList.Add(ConfigVariables.phrase17);
            phraseList.Add(ConfigVariables.phrase18);
            phraseList.Add(ConfigVariables.phrase19);
            phraseList.Add(ConfigVariables.phrase20);
            phraseList.Add(ConfigVariables.phrase21);
            phraseList.Add(ConfigVariables.phrase22);
            phraseList.Add(ConfigVariables.phrase23);
            phraseList.Add(ConfigVariables.phrase24);
            phraseList.Add(ConfigVariables.phrase25);
            phraseList.Add(ConfigVariables.phrase26);
            phraseList.Add(ConfigVariables.phrase27);
            phraseList.Add(ConfigVariables.phrase28);
            phraseList.Add(ConfigVariables.phrase29);
            phraseList.Add(ConfigVariables.phrase30);
            phraseList.Add(ConfigVariables.phrase31);
            phraseList.Add(ConfigVariables.phrase32);
            phraseList.Add(ConfigVariables.phrase33);
            phraseList.Add(ConfigVariables.phrase34);
            phraseList.Add(ConfigVariables.phrase35);
            phraseList.Add(ConfigVariables.phrase36);
            phraseList.Add(ConfigVariables.phrase37);
            phraseList.Add(ConfigVariables.phrase38);
            phraseList.Add(ConfigVariables.phrase39);
            phraseList.Add(ConfigVariables.phrase40);
            phraseList.Add(ConfigVariables.phrase41);
            phraseList.Add(ConfigVariables.phrase42);
            phraseList.Add(ConfigVariables.phrase43);
            phraseList.Add(ConfigVariables.phrase44);
            phraseList.Add(ConfigVariables.phrase45);
            phraseList.Add(ConfigVariables.phrase46);
            phraseList.Add(ConfigVariables.phrase47);
            phraseList.Add(ConfigVariables.phrase48);
            phraseList.Add(ConfigVariables.phrase49);
            phraseList.Add(ConfigVariables.phrase50);
        }

        public static void SpeakRandomPhrase(PlayerAvatar __instance)
        {
            if (phraseList.Count == 0)
                setupPhrases();           

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
