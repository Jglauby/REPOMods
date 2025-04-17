using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpJosModREPO.Tourettes.Patches;

namespace OpJosModREPO.Tourettes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosModBase : BaseUnityPlugin
    {
        private const string modGUID = "OpJosModREPO.Tourettes";
        private const string modName = "Tourettes";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmoy = new Harmony(modGUID);
        private static OpJosModBase Instance;
        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modName} has started!");
            setupConfig();

            PlayerAvatarPatch.SetLogSource(mls);
            harmoy.PatchAll();
        }

        private void setupConfig()
        {
            var configPhrase1 = Config.Bind("Phrase 1",
                                        "Phrase1",
                                        "Gosh Darn it",
                                        "one of the phrases you may blurt out");

            var configPhrase2 = Config.Bind("Phrase 2",
                                        "Phrase2",
                                        "you are gay",
                                        "one of the phrases you may blurt out");

            var configPhrase3 = Config.Bind("Phrase 3",
                                        "Phrase3",
                                        "FUCK!",
                                        "one of the phrases you may blurt out");

            var configPhrase4 = Config.Bind("Phrase 4",
                                        "Phrase4",
                                        "SHIT FUCK",
                                        "one of the phrases you may blurt out");

            var configPhrase5 = Config.Bind("Phrase 5",
                                        "Phrase5",
                                        "Oooo Fuckk meeeee",
                                        "one of the phrases you may blurt out");

            var configPhrase6 = Config.Bind("Phrase 6",
                                        "Phrase6",
                                        "you're cute",
                                        "one of the phrases you may blurt out");

            var configPhrase7 = Config.Bind("Phrase 7",
                                        "Phrase7",
                                        "ah shoot!",
                                        "one of the phrases you may blurt out");

            var configPhrase8 = Config.Bind("Phrase 8",
                                        "Phrase8",
                                        "ballocks",
                                        "one of the phrases you may blurt out");

            var configPhrase9 = Config.Bind("Phrase 9",
                                        "Phrase9",
                                        "poop gee ha ha",
                                        "one of the phrases you may blurt out");

            var configPhrase10 = Config.Bind("Phrase 10",
                                        "Phrase10",
                                        "what are you talking about?",
                                        "one of the phrases you may blurt out");

            var configPhrase11 = Config.Bind("Phrase 11",
                                        "Phrase11",
                                        "your mom gay",
                                        "one of the phrases you may blurt out");

            var configPhrase12 = Config.Bind("Phrase 12",
                                        "Phrase12",
                                        "i love to goon",
                                        "one of the phrases you may blurt out");

            var configPhrase13 = Config.Bind("Phrase 13",
                                        "Phrase13",
                                        "i just gooned all over myself",
                                        "one of the phrases you may blurt out");

            var configPhrase14 = Config.Bind("Phrase 14",
                                        "Phrase14",
                                        "i am gay",
                                        "one of the phrases you may blurt out");

            var configPhrase15 = Config.Bind("Phrase 15",
                                        "Phrase15",
                                        "i love massive milkers",
                                        "one of the phrases you may blurt out");

            var configPhrase16 = Config.Bind("Phrase 16",
                                        "Phrase16",
                                        "i fear i might be a homosexual",
                                        "one of the phrases you may blurt out");

            var configPhrase17 = Config.Bind("Phrase 17",
                                        "Phrase17",
                                        "go away i'm looks maxing",
                                        "one of the phrases you may blurt out");

            var configPhrase18 = Config.Bind("Phrase 18",
                                        "Phrase18",
                                        "booo",
                                        "one of the phrases you may blurt out");

            var configPhrase19 = Config.Bind("Phrase 19",
                                        "Phrase19",
                                        "haha boobies",
                                        "one of the phrases you may blurt out");

            var configPhrase20 = Config.Bind("Phrase 20",
                                        "Phrase20",
                                        "eueueueueeueueue",
                                        "one of the phrases you may blurt out");

            var configPhrase21 = Config.Bind("Phrase 21",
                                        "Phrase21",
                                        "yyyyyyyyyyyyyyyy",
                                        "one of the phrases you may blurt out");

            var configPhrase22 = Config.Bind("Phrase 22",
                                        "Phrase22",
                                        "GAYyyybeeeeee",
                                        "one of the phrases you may blurt out");

            var configPhrase23 = Config.Bind("Phrase 23",
                                        "Phrase23",
                                        "i'm coming",
                                        "one of the phrases you may blurt out");

            var configPhrase24 = Config.Bind("Phrase 24",
                                        "Phrase24",
                                        "this week at jeremy franklin mitsubishi",
                                        "one of the phrases you may blurt out");

            var configPhrase25 = Config.Bind("Phrase 25",
                                        "Phrase25",
                                        "its ya boi, uhhhhh. skinny penis",
                                        "one of the phrases you may blurt out");

            var configPhrase26 = Config.Bind("Phrase 26",
                                        "Phrase26",
                                        "i sure hope it does!",
                                        "one of the phrases you may blurt out");

            var configPhrase27 = Config.Bind("Phrase 27",
                                        "Phrase27",
                                        "These boots have seen everything..",
                                        "one of the phrases you may blurt out");

            var configPhrase28 = Config.Bind("Phrase 28",
                                        "Phrase28",
                                        "i was hiddddiiiiinnngggggg",
                                        "one of the phrases you may blurt out");

            var configPhrase29 = Config.Bind("Phrase 29",
                                        "Phrase29",
                                        "sup chubby",
                                        "one of the phrases you may blurt out");

            var configPhrase30 = Config.Bind("Phrase 30",
                                        "Phrase30",
                                        "beeepp booopp beeep",
                                        "one of the phrases you may blurt out");

            var configPhrase31 = Config.Bind("Phrase 31",
                                        "Phrase31",
                                        "I SAIDDDDD, whoever threw that paperrrr. your moms a HOE!",
                                        "one of the phrases you may blurt out");

            var configPhrase32 = Config.Bind("Phrase 32",
                                        "Phrase32",
                                        "I shouldn't have wished to live in more interesting times",
                                        "one of the phrases you may blurt out");

            var configPhrase33 = Config.Bind("Phrase 33",
                                        "Phrase33",
                                        "still me, despite everything",
                                        "one of the phrases you may blurt out");

            var configPhrase34 = Config.Bind("Phrase 34",
                                        "Phrase34",
                                        "DEEENOOOOO STOOOPPPSSS",
                                        "one of the phrases you may blurt out");

            var configPhrase35 = Config.Bind("Phrase 35",
                                        "Phrase35",
                                        "kaboom",
                                        "one of the phrases you may blurt out");

            var configPhrase36 = Config.Bind("Phrase 36",
                                        "Phrase36",
                                        "later nerd",
                                        "one of the phrases you may blurt out");

            var configPhrase37 = Config.Bind("Phrase 37",
                                        "Phrase37",
                                        "nahhh imma do my own thing",
                                        "one of the phrases you may blurt out");

            var configPhrase38 = Config.Bind("Phrase 38",
                                        "Phrase38",
                                        "stand ready for my arrival worm",
                                        "one of the phrases you may blurt out");

            var configPhrase39 = Config.Bind("Phrase 39",
                                        "Phrase39",
                                        "Is that blood? No... nevermind",
                                        "one of the phrases you may blurt out");

            var configPhrase40 = Config.Bind("Phrase 40",
                                        "Phrase40",
                                        "I've got a lot on my mind... and, well, in it.",
                                        "one of the phrases you may blurt out");

            var configPhrase41 = Config.Bind("Phrase 41",
                                        "Phrase41",
                                        "I wish I had a bag of holding",
                                        "one of the phrases you may blurt out");

            var configPhrase42 = Config.Bind("Phrase 42",
                                        "Phrase42",
                                        "Better not be cursed",
                                        "one of the phrases you may blurt out");

            var configPhrase43 = Config.Bind("Phrase 43",
                                        "Phrase43",
                                        "Still alive... so that's progress",
                                        "one of the phrases you may blurt out");

            var configPhrase44 = Config.Bind("Phrase 44",
                                        "Phrase44",
                                        "All's well that ends... not as bad as it could've",
                                        "one of the phrases you may blurt out");

            var configPhrase45 = Config.Bind("Phrase 45",
                                        "Phrase45",
                                        "Cursed to put my hands on everything",
                                        "one of the phrases you may blurt out");

            var configPhrase46 = Config.Bind("Phrase 46",
                                        "Phrase46",
                                        "No traps, please ...",
                                        "one of the phrases you may blurt out");

            var configPhrase47 = Config.Bind("Phrase 47",
                                        "Phrase47",
                                        "Do you know what else is massive?",
                                        "one of the phrases you may blurt out");

            var configPhrase48 = Config.Bind("Phrase 48",
                                        "Phrase48",
                                        "Blood comes easier these days....",
                                        "one of the phrases you may blurt out");

            var configPhrase49 = Config.Bind("Phrase 49",
                                        "Phrase49",
                                        "me needs to poopy",
                                        "one of the phrases you may blurt out");

            var configPhrase50 = Config.Bind("Phrase 50",
                                        "Phrase50",
                                        "I smell like beef",
                                        "one of the phrases you may blurt out");

            ConfigVariables.phrase1 = configPhrase1.Value;
            ConfigVariables.phrase2 = configPhrase2.Value;
            ConfigVariables.phrase3 = configPhrase3.Value;
            ConfigVariables.phrase4 = configPhrase4.Value;
            ConfigVariables.phrase5 = configPhrase5.Value;
            ConfigVariables.phrase6 = configPhrase6.Value;
            ConfigVariables.phrase7 = configPhrase7.Value;
            ConfigVariables.phrase8 = configPhrase8.Value;
            ConfigVariables.phrase9 = configPhrase9.Value;
            ConfigVariables.phrase10 = configPhrase10.Value;
            ConfigVariables.phrase11 = configPhrase11.Value;
            ConfigVariables.phrase12 = configPhrase12.Value;
            ConfigVariables.phrase13 = configPhrase13.Value;
            ConfigVariables.phrase14 = configPhrase14.Value;
            ConfigVariables.phrase15 = configPhrase15.Value;
            ConfigVariables.phrase16 = configPhrase16.Value;
            ConfigVariables.phrase17 = configPhrase17.Value;
            ConfigVariables.phrase18 = configPhrase18.Value;
            ConfigVariables.phrase19 = configPhrase19.Value;
            ConfigVariables.phrase20 = configPhrase20.Value;
            ConfigVariables.phrase21 = configPhrase21.Value;
            ConfigVariables.phrase22 = configPhrase22.Value;
            ConfigVariables.phrase23 = configPhrase23.Value;
            ConfigVariables.phrase24 = configPhrase24.Value;
            ConfigVariables.phrase25 = configPhrase25.Value;
            ConfigVariables.phrase26 = configPhrase26.Value;
            ConfigVariables.phrase27 = configPhrase27.Value;
            ConfigVariables.phrase28 = configPhrase28.Value;
            ConfigVariables.phrase29 = configPhrase29.Value;
            ConfigVariables.phrase30 = configPhrase30.Value;
            ConfigVariables.phrase31 = configPhrase31.Value;
            ConfigVariables.phrase32 = configPhrase32.Value;
            ConfigVariables.phrase33 = configPhrase33.Value;
            ConfigVariables.phrase34 = configPhrase34.Value;
            ConfigVariables.phrase35 = configPhrase35.Value;
            ConfigVariables.phrase36 = configPhrase36.Value;
            ConfigVariables.phrase37 = configPhrase37.Value;
            ConfigVariables.phrase38 = configPhrase38.Value;
            ConfigVariables.phrase39 = configPhrase39.Value;
            ConfigVariables.phrase40 = configPhrase40.Value;
            ConfigVariables.phrase41 = configPhrase41.Value;
            ConfigVariables.phrase42 = configPhrase42.Value;
            ConfigVariables.phrase43 = configPhrase43.Value;
            ConfigVariables.phrase44 = configPhrase44.Value;
            ConfigVariables.phrase45 = configPhrase45.Value;
            ConfigVariables.phrase46 = configPhrase46.Value;
            ConfigVariables.phrase47 = configPhrase47.Value;
            ConfigVariables.phrase48 = configPhrase48.Value;
            ConfigVariables.phrase49 = configPhrase49.Value;
            ConfigVariables.phrase50 = configPhrase50.Value;
        }
    }
}
