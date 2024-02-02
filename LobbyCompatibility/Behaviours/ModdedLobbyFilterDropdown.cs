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
        public ModdedLobbyFilter LobbyFilter { get; private set; }
        private TMP_Dropdown? _dropdown; 

        public void Awake()
        {
            // Set default to whatever the user has configured
            UpdateLobbyFilter(LobbyCompatibilityPlugin.Config?.DefaultModdedLobbyFilter.Value ?? ModdedLobbyFilter.CompatibleFirst);

            if (_dropdown != null)
                _dropdown.SetValueWithoutNotify((int)LobbyFilter);
        }

        /// <summary>
        ///     Registers the <see cref="TMP_Dropdown"/>. Required for syncing the default config with UI.
        /// </summary>
        /// <param name="dropdown"> The dropdown. </param>
        public void SetDropdown(TMP_Dropdown dropdown)
        {
            _dropdown = dropdown;
        }

        /// <summary>
        ///     Called automatically when the <see cref="TMP_Dropdown"/> value is updated.
        /// </summary>
        /// <param name="index"> The index of the new filter type. </param>
        public void ChangeFilterType(int index)
        {
            UpdateLobbyFilter((ModdedLobbyFilter)index);
        }

        /// <summary>
        ///     Updates the <see cref="ModdedLobbyFilter"/> internally.
        ///     This will be called when the user changes the value manually, and when the menu scene is opened/reopened.
        /// </summary>
        /// <param name="lobbyFilter"> The new <see cref="ModdedLobbyFilter"/> to use. </param>
        private void UpdateLobbyFilter(ModdedLobbyFilter lobbyFilter)
        {
            LobbyFilter = lobbyFilter;

            // TODO: Hook this up. Use a singleton or action or whatever
        }
    }
}
