using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="SteamLobbyManager.LoadServerList" />.
///     This sorts out certainly incompatible lobbies to prevent taking up
///     some of the maximum 50 lobbies that load.
/// </summary>
[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class LoadServerListTranspiler
{
    [HarmonyTargetMethod]
    private static MethodBase? TargetMethod()
    {
        // Target async method using the HarmonyHelper
        return HarmonyHelper.GetAsyncInfo(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList));
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
    {
        var searchCoroutineMethod = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new [] { typeof(IEnumerator) });
        var postfixMethod = AccessTools.Method(typeof(LoadServerListTranspiler), nameof(LoadListPostfix));

        CodeInstruction loadVarInstruction;
        
        // Does the following:
        // - Sets final instruction of the coroutine line to Nop
        // - Sets the first instruction of the coroutine line to unconditionally branch to Nop
        // - Moves to the end of the "try" section of the MoveNext method
        // - Load steamLobbyManager variable
        // - Load lobbyQuery2 variable
        // - Call Postfix method
        var codeMatcher = new CodeMatcher(instructions, ilGenerator)
            .SearchForward(inst => inst.Calls(searchCoroutineMethod))
            .ThrowIfInvalid("Unable to find StartCoroutine.")
            .Advance(1)
            .SetInstruction(new CodeInstruction(OpCodes.Nop))
            .CreateLabel(out var label)
            .Advance(-7)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Br, label))
            .SearchForward(inst => inst.opcode == OpCodes.Leave)
            .ThrowIfInvalid("Unable to find leave instruction.")
            .InsertAndAdvance(new[] {
                loadVarInstruction = new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, postfixMethod)
            });

        codeMatcher.Instruction.MoveLabelsTo(loadVarInstruction);

        return codeMatcher.InstructionEnumeration();
    }

    internal static async void LoadListPostfix(SteamLobbyManager steamLobbyManager, LobbyQuery lobbyQuery)
    {
        // If there is not a ModdedLobbyFilterDropdown Instance, treat as if we are doing no filtering
        var currentFilter = ModdedLobbyFilterDropdown.Instance != null ? ModdedLobbyFilterDropdown.Instance.LobbyFilter : ModdedLobbyFilter.All;

        // Always apply no filtering when the user is entering a custom tag, as they're likely searching for a specific lobby
        if (steamLobbyManager.serverTagInputField.text != string.Empty)
            currentFilter = ModdedLobbyFilter.All;

        // Create a local reference so the IDE doesn't complain about the param not being marked ref
        var query = lobbyQuery;

        Lobby[]? filteredLobbies = null;

        // we only need to run the hashfilter if we're specifically looking for compatible lobbies
        if (PluginHelper.Checksum != "" && (currentFilter == ModdedLobbyFilter.CompatibleFirst || currentFilter == ModdedLobbyFilter.CompatibleOnly))
        {
            // Re-add distance filter since it gets removed for some reason
            query = steamLobbyManager.sortByDistanceSetting switch
            {
                0 => query.FilterDistanceClose(),
                1 => query.FilterDistanceFar(),
                2 => query.FilterDistanceWorldwide(),
                _ => query,
            };

            // Add Checksum filter
            query.WithKeyValue(LobbyMetadata.RequiredChecksum, PluginHelper.Checksum);

            // Make an additional search for lobbies that match the checksum
            filteredLobbies = await query.RequestAsync();
        }

        // Get the final lobby array based on the user's ModdedLobbyFilter.
        // IMPORTANT: steamLobbyManager.currentLobbyList WILL return null if steam is inaccessible, so we need initialize a default empty value just in case
        var allLobbies = LobbyHelper.FilterLobbies(steamLobbyManager.currentLobbyList ?? new Lobby[0], filteredLobbies, currentFilter);
        steamLobbyManager.currentLobbyList = allLobbies.ToArray();

        if (!allLobbies.Any())
        {
            steamLobbyManager.serverListBlankText.text = LobbyHelper.GetEmptyLobbyListString(currentFilter);
            return;
        }

        steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
    }
}