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
        var searchRequestMethod = AccessTools.Method(typeof(LobbyQuery), nameof(LobbyQuery.RequestAsync));
        var searchCoroutineMethod = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new [] { typeof(IEnumerator) });
        
        var filterMethod = AccessTools.Method(typeof(LoadServerListTranspiler), nameof(FilterCompatibility));
        var postfixMethod = AccessTools.Method(typeof(LoadServerListTranspiler), nameof(LoadListPostfix));

        var loadVarInstruction = new CodeInstruction(OpCodes.Ldloc_1);
        var postfixInstruction = new CodeInstruction(OpCodes.Call, postfixMethod);
        
        // Does the following:
        // - Runs FilterCompatibility on lobbyQuery2
        var codeMatcher = new CodeMatcher(instructions, ilGenerator)
            .SearchForward(inst => inst.Calls(searchRequestMethod))
            .ThrowIfInvalid("Unable to find RequestAsync.")
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Call, filterMethod),
                new CodeInstruction(OpCodes.Ldloca_S, 3),
            ])
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
                postfixInstruction
            ]);

        codeMatcher.Instruction.MoveLabelsTo(loadVarInstruction);
        
        

#if DEBUG
        LobbyCompatibilityPlugin.Logger!.LogDebugInstructionsFrom(codeMatcher);
#endif

        return codeMatcher.InstructionEnumeration();
    }

    internal static void FilterCompatibility(ref LobbyQuery lobbyQuery)
    {
        if (_secondPass || PluginHelper.GetRequiredPluginsChecksum() == "")
        {
            _secondPass = false;
            return;
        }
        
        lobbyQuery.WithKeyValue(LobbyMetadata.RequiredChecksum, PluginHelper.GetRequiredPluginsChecksum());
    }

    internal static async void LoadListPostfix(SteamLobbyManager steamLobbyManager, LobbyQuery lobbyQuery)
    {
        LobbyCompatibilityPlugin.Logger!.LogDebug("Running LoadServerList Postfix");

        if (_secondPass)
        {
            _secondPass = false;
            
            if (steamLobbyManager.currentLobbyList != null)
                _allLobbies.UnionWith(steamLobbyManager.currentLobbyList);

            steamLobbyManager.currentLobbyList = _allLobbies.ToArray();
            
            LobbyCompatibilityPlugin.Logger.LogError("Finished second pass, loading lobby list now!");
        
            if (steamLobbyManager.currentLobbyList.Length != 0)
                steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
            
            return;
        }
        
        if (steamLobbyManager.currentLobbyList is { Length: >= 35 })
        {
            steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
            return;
        }
        
        LobbyCompatibilityPlugin.Logger.LogError("Attempting second pass!");

        _allLobbies = [];
        
        if (steamLobbyManager.currentLobbyList != null)
            _allLobbies.UnionWith(steamLobbyManager.currentLobbyList);

        _secondPass = true;
            
        steamLobbyManager.LoadServerList();
    }

    private static bool _secondPass;
    private static HashSet<Lobby> _allLobbies = [];
}