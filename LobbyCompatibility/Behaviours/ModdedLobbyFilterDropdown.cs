using LobbyCompatibility.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours
{
    /// <summary>
    ///     Used to broadcast the <see cref="ModdedLobbyFilter"/> to use when searching for lobbies.
    /// </summary>
    internal class ModdedLobbyFilterDropdown : MonoBehaviour
    {
        public ModdedLobbyFilter LobbyFilter;
        private TMP_Dropdown? _dropdown; 

        public void Awake()
        {
            // TODO: Add defaults to a config file
            LobbyFilter = ModdedLobbyFilter.CompatibleFirst;

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
