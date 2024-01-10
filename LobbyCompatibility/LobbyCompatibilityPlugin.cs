using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Patches;

namespace LobbyCompatibility;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class LobbyCompatibilityPlugin : BaseUnityPlugin
{
    private bool _isPatched;
    private Harmony? Harmony { get; set; }
    internal new static ManualLogSource? Logger { get; set; }
    public static LobbyCompatibilityPlugin? Instance { get; private set; }
    
    private void Awake()
    {
        // Set instance
        Instance = this;

        // Init logger
        Logger = base.Logger;
        
        // Patch using Harmony
        PatchAll();
        
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
    
    /// <summary>
    /// Retrieves the MethodInfo of the compiler-generated async method.
    /// Async method content is not actually in the <c>async Method()</c>, but instead is in a separate <c>struct</c> under the method "MoveNext";
    /// this function retrieves that method info.
    /// </summary>
    /// <param name="type">(<see cref="Type"/>) The type of the class housing the method.</param>
    /// <param name="method">(<see cref="string"/>) The name of the method being patched.</param>
    /// <returns>(<see cref="MethodInfo"/>) The info of the async "MoveNext" method.</returns>
    public static MethodInfo? GetAsyncInfo(Type type, string method)
    {
        // Get the Method Info of the target Async Method
        return AccessTools.Method(type, method)
            // Find the AsyncStateMachine class from target method
            .GetCustomAttribute<AsyncStateMachineAttribute>()
            // Get the struct type (random compiler junk)
            .StateMachineType.GetMethod("MoveNext", (BindingFlags)60);
    }

    public void PatchAll()
    {
        if (_isPatched)
        {
            Logger?.LogWarning("Already patched!");
            return;
        }

        Logger?.LogDebug("Patching...");

        Harmony ??= new Harmony(PluginInfo.PLUGIN_GUID);
        
        Harmony.PatchAll();
        
        // Async Patches have to be done manually
        Harmony.Patch(GetAsyncInfo(typeof(MenuManager), nameof(MenuManager.GetLeaderboardForChallenge)), 
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(LeaderboardPatch), nameof(LeaderboardPatch.ChangeToModdedLeaderboard))));
        Harmony.Patch(GetAsyncInfo(typeof(MenuManager), nameof(MenuManager.RemoveLeaderboardScore)), 
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(LeaderboardPatch), nameof(LeaderboardPatch.ChangeToModdedLeaderboard))));
        
        
        _isPatched = true;

        Logger?.LogDebug("Patched!");
    }

    public void UnpatchAll()
    {
        if (!_isPatched)
        {
            Logger?.LogWarning("Already unpatched!");
            return;
        }

        Logger?.LogDebug("Unpatching...");

        Harmony!.UnpatchSelf();
        _isPatched = false;

        Logger?.LogDebug("Unpatched!");
    }
}