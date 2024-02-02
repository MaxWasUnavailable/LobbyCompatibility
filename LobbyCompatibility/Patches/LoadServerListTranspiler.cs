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
            .InsertAndAdvance([
                loadVarInstruction = new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, postfixMethod)
            ]);

        codeMatcher.Instruction.MoveLabelsTo(loadVarInstruction);

        return codeMatcher.InstructionEnumeration();
    }

    internal static async void LoadListPostfix(SteamLobbyManager steamLobbyManager, LobbyQuery lobbyQuery)
    {
        // If there is not a ModdedLobbyFilterDropdown Instance, treat as if we are doing no filtering
        var currentFilter = ModdedLobbyFilterDropdown.Instance != null ? ModdedLobbyFilterDropdown.Instance.LobbyFilter : ModdedLobbyFilter.All;
        
        if (currentFilter == ModdedLobbyFilter.All)
        {
            steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager
                .StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
            return;
        }
        
        // Create a local reference so the IDE doesn't complain about the param not being marked ref
        var query = lobbyQuery;
        
        // Add Modded filter if we are filtering for "vanilla" lobbies only,
        // otherwise add Checksum filter
        if (currentFilter == ModdedLobbyFilter.UnmoddedOnly)
            query.WithKeyValue(LobbyMetadata.Modded, "true");
        else if (PluginHelper.Checksum != "")
            query.WithKeyValue(LobbyMetadata.RequiredChecksum, PluginHelper.Checksum);

        var filteredLobbies = await query.RequestAsync();
        List<Lobby> allLobbies = [];
        var serverListBlankText = "";
        
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (currentFilter)
        {
            default:
            case ModdedLobbyFilter.CompatibleFirst:
                serverListBlankText = "No available servers to join.";
                
                // Add compatible lobbies first
                if (filteredLobbies != null)
                    allLobbies.AddRange(filteredLobbies);
                
                // Add the remaining incompatible lobbies
                allLobbies.AddRange(steamLobbyManager.currentLobbyList
                        .Where(lobby => !allLobbies.Any(check => lobby.Equals(check))));
                
                break;
            case ModdedLobbyFilter.CompatibleOnly:
                serverListBlankText = "No available compatible\nservers to join.";

                // Add compatible lobbies only
                if (filteredLobbies != null)
                    allLobbies.AddRange(filteredLobbies);
                
                break;
            case ModdedLobbyFilter.UnmoddedOnly:
                serverListBlankText = "No available vanilla\nservers to join.";
                
                // Add all lobbies
                allLobbies.AddRange(steamLobbyManager.currentLobbyList);
                
                // Remove lobbies marked as modded
                if (filteredLobbies != null)
                    allLobbies.RemoveAll(lobby => filteredLobbies.Any(check => lobby.Equals(check)));
                
                break;
        }

        if (allLobbies.Any())
        {
            // TODO: Add sorting of actual compatibility (server only version & client optional)
            steamLobbyManager.currentLobbyList = allLobbies.Take(50).ToArray();
        }
        else
        {
            steamLobbyManager.currentLobbyList = [];
            steamLobbyManager.serverListBlankText.text = serverListBlankText;
            return;
        }
        
        steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
    }
}