using BepInEx.Logging;
using OpJosModREPO.IAmEnemy.Networking;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OpJosModREPO.IAmEnemy.UI
{
    public static class EnemySelector
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }
        private static EnemyTypes[] _values;
        private static int _selectedIndex = 0;
        private static bool IsChoosingEnemy = false;
        private static float lastShownNoteTime = -1;
        private static float lastSwappedSelection = -1;

        public static Vector3 PendingSpawnPosition = Vector3.zero;

        public static void BeginChooseEnemy(Vector3 spawnPosition)
        {
            IsChoosingEnemy = true;
            var all = (EnemyTypes[])Enum.GetValues(typeof(EnemyTypes));
            _values = Array.FindAll(all, e => e != EnemyTypes.ChooseEnemy && e != EnemyTypes.Random);

            RefreshHighlight();
            PendingSpawnPosition = spawnPosition;
        }

        public static void Update()
        {
            if (!IsChoosingEnemy) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.upArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _values.Length - 1);
                RefreshHighlight();
                lastSwappedSelection = Time.time;
            }
            else if (kb.downArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex + 1, 0, _values.Length - 1);
                RefreshHighlight();
                lastSwappedSelection = Time.time;
            }
            else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                var chosen = _values[_selectedIndex];
                mls.LogInfo($"Player chose {chosen}");
                CompleteEnemySelection(chosen);
                RestoreOriginalTextAndCleanup();
                IsChoosingEnemy = false;
            }
            else if (kb.escapeKey.wasPressedThisFrame)
            {
                mls.LogInfo("Selection cancelled");
                IsChoosingEnemy = false;
                RestoreOriginalTextAndCleanup();
            }
            else if (Mathf.FloorToInt(Time.time) % 4 == 0 && Mathf.FloorToInt(Time.time) != lastShownNoteTime && 
                Time.time - lastSwappedSelection >= 4)//nothign pressed in a bit
            {
                lastShownNoteTime = Mathf.FloorToInt(Time.time);
                SpectateNameUI.instance.SetName("Arrow Keys To Select Enemy. Enter to confirm.");
            }
        }

        public static void CompleteEnemySelection(EnemyTypes selected)
        {
            PublicVars.NextSpawnType = selected;
            IsChoosingEnemy = false;

            // Enforce limits and perform spawn using the same logic as PlayerAvatarPatch
            if (ConfigVariables.limitEnemiesPerLevel && PublicVars.TimesSpawnedEnemy >= ConfigVariables.maxEnemiesPerLevel)
            {
                mls.LogInfo("Can't spawn enemy again, set to spectate");
                GeneralUtil.ReleaseEnemyControlToSpectate();
                return;
            }

            PublicVars.TimesSpawnedEnemy += 1;

            if (PhotonNetwork.IsMasterClient)
            {
                mls.LogInfo("Confirmed selection, spawning enemy as host");
                GeneralUtil.SpawnEnemyAt(PendingSpawnPosition, 1, PublicVars.NextSpawnType);
            }
            else
            {
                mls.LogInfo("Confirmed selection, sending spawn enemy request to host");
                mls.LogInfo($"Sending enemy spawn request. My actor number: {PhotonNetwork.LocalPlayer.ActorNumber}");
                EnemySpawnerNetwork.Instance.RequestSpawnEnemy(PendingSpawnPosition, PublicVars.NextSpawnType);
            }
        }

        private static void RestoreOriginalTextAndCleanup()
        {
            try
            {
                if (SpectateNameUI.instance != null)
                {
                    SpectateNameUI.instance.SetName(null ?? string.Empty);
                }
            }
            catch { }
        }

        private static void RefreshHighlight()
        {
            var display = "[Select: " + _values[_selectedIndex].ToString() + "]";
            try
            {
                if (SpectateNameUI.instance != null)
                {
                    SpectateNameUI.instance.SetName(display);
                    return;
                }
            }
            catch { }

            mls.LogInfo($"Selecting: {_values[_selectedIndex]}");
        }
    }
}
