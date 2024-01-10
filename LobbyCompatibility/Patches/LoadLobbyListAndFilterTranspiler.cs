using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyCompatibility.Patches
{
    /// <summary>
    ///     Patches <see cref="SteamLobbyManager.loadLobbyListAndFilter" />.
    ///     Adds custom ModdedLobbySlot component to add additional behaviour to Lobby slots
    /// </summary>
    /// <seealso cref="SteamLobbyManager.loadLobbyListAndFilter" />
    // ReSharper disable UnusedMember.Local
    // [HarmonyPatch(typeof(SteamLobbyManager), nameof(SteamLobbyManager.loadLobbyListAndFilter))]
    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    [HarmonyWrapSafe]
    internal class LoadLobbyListAndFilterTranspiler
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(SteamLobbyManager), nameof(SteamLobbyManager.loadLobbyListAndFilter)));
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // The method used to get the LobbySlot after instantiation (yes, it's accessed through GetComponentInChildren)
            var lobbySlotMethod = AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponentInChildren), new Type[0], new Type[] { typeof(LobbySlot) });

            return new CodeMatcher(instructions)
                .SearchForward(instruction => instruction.Calls(lobbySlotMethod))
                .ThrowIfInvalid("Could not find LobbySlot method")
                .RemoveInstructions(1)
                .Insert(new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(LoadLobbyListAndFilterTranspiler), nameof(InitializeLobbySlot))))
                .InstructionEnumeration();
        }

        // Inject custom LobbySlot component for modded lobby data
        private static LobbySlot InitializeLobbySlot(GameObject gameObject)
        {
            var lobbySlot = gameObject.GetComponentInChildren<LobbySlot>();
            lobbySlot.gameObject.AddComponent<ModdedLobbySlot>();
            return lobbySlot;
        }
    }
}
