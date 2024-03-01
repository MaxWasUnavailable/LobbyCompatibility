using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Configuration;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility;

[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.Minor)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class LobbyCompatibilityPlugin : BaseUnityPlugin
{
    private bool _isPatched;
    private Harmony? Harmony { get; set; }
    internal new static ManualLogSource? Logger { get; private set; }
    public static LobbyCompatibilityPlugin? Instance { get; private set; }
    public new static Config? Config { get; private set; }

    private void Awake()
    {
        // Set instance
        Instance = this;

        // Init logger
        Logger = base.Logger;

        // Init config
        Config = new Config(base.Config);

        // Patch using Harmony
        PatchAll();

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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