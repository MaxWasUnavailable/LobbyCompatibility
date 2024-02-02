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

        var loadVarInstruction = new CodeInstruction(OpCodes.Ldloc_1);
        
        // Does the following:
        // - Runs FilterCompatibility on lobbyQuery2
        var codeMatcher = new CodeMatcher(instructions, ilGenerator)
            .SearchForward(inst => inst.Calls(searchCoroutineMethod))
            .ThrowIfInvalid("Unable to find StartCoroutine.")
            .Advance(1)
            .SetInstruction(new CodeInstruction(OpCodes.Nop))
            .CreateLabel(out var label)
            .Advance(-7)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Br, label))
            .SearchForward(inst => inst.opcode == OpCodes.Leave)
            .InsertAndAdvance([
                loadVarInstruction,
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, postfixMethod)
            ]);

        codeMatcher.Instruction.MoveLabelsTo(loadVarInstruction);
        
        

#if DEBUG
        LobbyCompatibilityPlugin.Logger!.LogDebugInstructionsFrom(codeMatcher);
#endif

        return codeMatcher.InstructionEnumeration();
    }

    internal static void FilterCompatibility(ref LobbyQuery lobbyQuery)
    {
        if (PluginHelper.GetRequiredPluginsChecksum() == "")
            return;
        
        lobbyQuery.WithKeyValue(LobbyMetadata.RequiredChecksum, PluginHelper.GetRequiredPluginsChecksum());
    }

    internal static async void LoadListPostfix(SteamLobbyManager steamLobbyManager, LobbyQuery lobbyQuery)
    {
        LobbyCompatibilityPlugin.Logger!.LogDebug("Running LoadServerList Postfix");

        var query = lobbyQuery;
        
        FilterCompatibility(ref query);

        var lobbyArray = await query.RequestAsync();//queryTask.Result;
        
        List<Lobby> allLobbies = [];

        if (lobbyArray != null)
            allLobbies.AddRange(lobbyArray);
        else
            LobbyCompatibilityPlugin.Logger!.LogDebug("No compatible modded lobbies found!");

        var nonDuplicates = steamLobbyManager.currentLobbyList.Where(lobby => !allLobbies.Any(check => lobby.Equals(check)));

        allLobbies.AddRange(nonDuplicates);
        
        allLobbies.Do(lobby => LobbyCompatibilityPlugin.Logger.LogWarning(lobby.GetData("name")));
        LobbyCompatibilityPlugin.Logger.LogError(allLobbies.Count);

        steamLobbyManager.currentLobbyList = allLobbies.ToArray();

        steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
    }
}