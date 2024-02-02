using LobbyCompatibility.Enums;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours
{
    /// <summary>
    ///     Used to handle any changes to the <see cref="ModdedLobbyFilter"/> when searching for lobbies.
    /// </summary>
    internal class ModdedLobbyFilterDropdown : MonoBehaviour
    {
        public ModdedLobbyFilter LobbyFilter;
        private TMP_Dropdown? _dropdown; 

        public void Awake()
        {
            LobbyFilter = LobbyCompatibilityPlugin.Config?.DefaultModdedLobbyFilter.Value ?? ModdedLobbyFilter.CompatibleFirst;

            if (_dropdown != null)
                _dropdown.SetValueWithoutNotify((int)LobbyFilter);
        }

        public void SetDropdown(TMP_Dropdown dropdown)
        {
            _dropdown = dropdown;
        }

        public void ChangeFilterType(int newValue)
        {
            LobbyFilter = (ModdedLobbyFilter)newValue;
            LobbyCompatibilityPlugin.Logger?.LogInfo(LobbyFilter);
        }
    }
}
