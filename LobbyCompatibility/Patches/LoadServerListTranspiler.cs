using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
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
        // If we are not filtering & sorting, run the coroutine and skip postfix
        if (false)
        {
            steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager
                .StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));

            return;
        }
        
        // Create local copy so IDE doesn't complain about the param not being ref
        var query = lobbyQuery;
        
        // Add Checksum filter if we are not filtering for "vanilla" lobbies only, otherwise add Modded filter 
        if (false)
            query.WithKeyValue(LobbyMetadata.Modded, "true");
        else
            query.WithKeyValue(LobbyMetadata.RequiredChecksum, PluginHelper.GetRequiredPluginsChecksum());


        var filteredLobbies = await query.RequestAsync();
        
        List<Lobby> allLobbies = [];
        
        if (filteredLobbies != null)
            allLobbies.AddRange(filteredLobbies);
        else
            LobbyCompatibilityPlugin.Logger!.LogDebug("No compatible modded lobbies found!");

        var nonDuplicates = steamLobbyManager.currentLobbyList.Where(lobby => !allLobbies.Any(check => lobby.Equals(check)));

        allLobbies.AddRange(nonDuplicates);

        steamLobbyManager.currentLobbyList = allLobbies.ToArray();

        steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
    }
}