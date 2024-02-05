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
        var searchRequestMethod = AccessTools.Method(typeof(LobbyQuery), nameof(LobbyQuery.RequestAsync));
        var searchCoroutineMethod = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new [] { typeof(IEnumerator) });
        
        var cloneMethod = AccessTools.Method(typeof(LoadServerListTranspiler), nameof(CreateClone));
        var postfixMethod = AccessTools.Method(typeof(LoadServerListTranspiler), nameof(LoadListPostfix));
        
        // Create a new local variable in the method & store the index
        var cloneVarIndex = ilGenerator.DeclareLocal(typeof(LobbyQuery)).LocalIndex;
        
        CodeMatcher codeMatcher = new(instructions, ilGenerator);
        CodeInstruction loadVarInstruction;
            
        // Does the following:
        // - Finds the RequestAsync call
        // - Calls CloneLobby
        // - Stores the returned query
        // - Loads lobbyQuery2's address
        codeMatcher
            .SearchForward(inst => inst.Calls(searchRequestMethod))
            .ThrowIfInvalid("Unable to find RequestAsync.")
            .InsertAndAdvance(new[] {
                new CodeInstruction(OpCodes.Callvirt, cloneMethod),
                new CodeInstruction(OpCodes.Stloc_S, cloneVarIndex),
                new CodeInstruction(OpCodes.Ldloca_S, 3)
            });
        
        // Does the following:
        // - Finds the StartCoroutine call
        // - Sets final instruction of the coroutine line to Nop
        // - Sets first instruction of the coroutine line to unconditionally branch to Nop
        codeMatcher
            .SearchForward(inst => inst.Calls(searchCoroutineMethod))
            .ThrowIfInvalid("Unable to find StartCoroutine.")
            .Advance(1)
            .SetInstruction(new CodeInstruction(OpCodes.Nop))
            .CreateLabel(out var label)
            .Advance(-7)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Br, label));
            
        // Does the following:
        // - Finds the end of the final state of the MoveNext method
        // - Loads steamLobbyManager variable
        // - Loads cloned lobby query variable
        // - Calls Postfix
        codeMatcher
            .SearchForward(inst => inst.opcode == OpCodes.Leave)
            .ThrowIfInvalid("Unable to find leave instruction.")
            .InsertAndAdvance(new[] {
                loadVarInstruction = new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_S, cloneVarIndex),
                new CodeInstruction(OpCodes.Call, postfixMethod)
            });

        // Does the following:
        // - Move the Branch label from the leave instruction to the start of our postfix instructions
        codeMatcher.Instruction.MoveLabelsTo(loadVarInstruction);

        return codeMatcher.InstructionEnumeration();
    }

    private static LobbyQuery CreateClone(ref LobbyQuery lobbyQuery) => new()
        {
            distance = lobbyQuery.distance,
            stringFilters = lobbyQuery.stringFilters ?? new Dictionary<string, string>()
        };

    internal static async void LoadListPostfix(SteamLobbyManager steamLobbyManager, LobbyQuery lobbyQuery)
    {
        // Create a new LobbyQuery using the previous request's values
        var query = lobbyQuery;

        // If there is not a ModdedLobbyFilterDropdown Instance, treat as if we are doing no filtering
        var currentFilter = ModdedLobbyFilterDropdown.Instance != null ? ModdedLobbyFilterDropdown.Instance.LobbyFilter : ModdedLobbyFilter.All;

        // Always apply no filtering when the user is entering a custom tag, as they're likely searching for a specific lobby
        if (query.stringFilters.ContainsKey(LobbyMetadata.Tag))
            currentFilter = ModdedLobbyFilter.All;

        Lobby[]? filteredLobbies = null;

        // we only need to run the hashfilter if we're specifically looking for compatible lobbies
        if (PluginHelper.Checksum != "" && (currentFilter == ModdedLobbyFilter.CompatibleFirst || currentFilter == ModdedLobbyFilter.CompatibleOnly))
        {
            // Add checksum filter to query
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