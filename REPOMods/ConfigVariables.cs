using BepInEx.Logging;
using System;
using System.Linq;
using UnityEngine.InputSystem;

namespace OPJosMod
{
    public static class ConfigVariables
    {
        public static Key flashBangKey;
        public static float flashBangTime;

        public static Key domainExpansionKey;
        public static float domainExpansionTime = 6f;

        public static Key heartEyesKey;
        public static float heartEyesTime = 1f;

        public static Key questionPingKey;
        public static float questionPingTime = 1f;
    }
}
