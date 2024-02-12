using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LobbyCompatibility.Behaviours;
using Steamworks.Data;
using UnityEngine;

namespace LobbyCompatibility.Patches;

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
    [HarmonyPatch(typeof(SteamLobbyManager), nameof(SteamLobbyManager.loadLobbyListAndFilter), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var currentLobbyListField = 
            AccessTools.Field(typeof(SteamLobbyManager), nameof(SteamLobbyManager.currentLobbyList));
        var thisLobbyField = 
            AccessTools.Field(typeof(LobbySlot), nameof(LobbySlot.thisLobby));
        
        var levelListContainerField =
            AccessTools.Field(typeof(SteamLobbyManager), nameof(SteamLobbyManager.levelListContainer));
        var initializeLobbySlotMethod = 
            AccessTools.Method(typeof(LoadLobbyListAndFilterTranspiler), nameof(InitializeLobbySlot));
        
        // Does the following:
        // - Adds dup before last componentInChildren line to keep componentInChildren value on the stack
        // - Loads SteamLobbyManager.levelListContainer onto the stack
        // - Calls InitializeLobbySlot(lobbySlot, levelListContainer)
        return new CodeMatcher(instructions)
            .MatchForward(false, new [] {
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldfld, currentLobbyListField),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(inst => inst.opcode == OpCodes.Ldfld), // Compiler-generated field
                new CodeMatch(OpCodes.Ldelem, typeof(Lobby)),
                new CodeMatch(OpCodes.Stfld, thisLobbyField) })
            .ThrowIfNotMatch("Unable to find LobbySlot.thisLobby line.")
            .InsertAndAdvance(new [] {
                new CodeInstruction(OpCodes.Dup) })
            .Advance(6)
            .InsertAndAdvance(new [] {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, levelListContainerField),
                new CodeInstruction(OpCodes.Call, initializeLobbySlotMethod) })
            .InstructionEnumeration();
    }

    // Inject custom LobbySlot component for modded lobby data
    private static void InitializeLobbySlot(LobbySlot lobbySlot, Transform levelListContainer)
    {
        var moddedLobbySlot = lobbySlot.gameObject.AddComponent<ModdedLobbySlot>();

        moddedLobbySlot.Setup(lobbySlot);

        // Set container parent for hover tooltip position math
        moddedLobbySlot.SetParentContainer(levelListContainer.parent);
    }
}