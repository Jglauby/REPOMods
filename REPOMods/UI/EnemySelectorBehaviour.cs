using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace OpJosModREPO.IAmEnemy.UI
{
    public class EnemySelectorBehaviour : MonoBehaviour
    {
        private EnemyTypes[] _values;
        private int _selectedIndex = 0;
        private string _originalText;

        void Awake()
        {
            // Build list of selectable enemy types (skip ChooseEnemy and Random)
            var all = (EnemyTypes[])Enum.GetValues(typeof(EnemyTypes));
            _values = Array.FindAll(all, e => e != EnemyTypes.ChooseEnemy && e != EnemyTypes.Random);

            _originalText = null;
            RefreshHighlight();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.upArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _values.Length - 1);
                RefreshHighlight();
            }
            else if (kb.downArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex + 1, 0, _values.Length - 1);
                RefreshHighlight();
            }
            else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                var chosen = _values[_selectedIndex];
                Debug.Log($"[IAmEnemy] Player chose {chosen}");
                OpJosModREPO.IAmEnemy.PublicVars.CompleteEnemySelection(chosen);
                RestoreOriginalTextAndCleanup();
                Destroy(gameObject);
            }
            else if (kb.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[IAmEnemy] Selection cancelled");
                OpJosModREPO.IAmEnemy.PublicVars.IsChoosingEnemy = false;
                RestoreOriginalTextAndCleanup();
                Destroy(gameObject);
            }
        }

        private void RestoreOriginalTextAndCleanup()
        {
            try
            {
                if (SpectateNameUI.instance != null)
                {
                    SpectateNameUI.instance.SetName(_originalText ?? string.Empty);
                }
            }
            catch { }
            _originalText = null;
        }

        private void RefreshHighlight()
        {
            var display = (_originalText ?? "") + "  [Select: " + _values[_selectedIndex].ToString() + "]";
            try
            {
                if (SpectateNameUI.instance != null)
                {
                    SpectateNameUI.instance.SetName(display);
                    return;
                }
            }
            catch { }

            Debug.Log($"[IAmEnemy] Selecting: {_values[_selectedIndex]}");
        }

        void OnDestroy()
        {
            OpJosModREPO.IAmEnemy.PublicVars.IsChoosingEnemy = false;
        }
    }
}
