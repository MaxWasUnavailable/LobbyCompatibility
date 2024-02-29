using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Models;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace LobbyCompatibility.Pooling;

/// <summary>
///     Pools <see cref="PluginDiffSlot"/> objects.
/// </summary>
/// <seealso cref="PluginDiffSlot" />
internal class PluginDiffSlotPool : MonoBehaviour
{
    private PluginDiffSlot? _template;
    private Transform? _container;
    private IObjectPool<PluginDiffSlot>? _pool;

    /// <summary>
    ///     Initializes the pool using a template <see cref="PluginDiffSlot"/>.
    /// </summary>
    /// <param name="template"> The template <see cref="PluginDiffSlot"/> to pool. </param>
    /// <param name="container"> The container to Instantiate the template into by default. </param>
    public void InitializeUsingTemplate(PluginDiffSlot template, Transform container)
    {
        _container = container;
        _template = template;

        _pool = new ObjectPool<PluginDiffSlot>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, 20);
    }

    /// <summary>
    ///     Spawns a <see cref="PluginDiffSlot"/> using the pool, and sets the text to match a <see cref="PluginDiff"/>.
    /// </summary>
    /// <param name="pluginDiff"> The <see cref="PluginDiff"/> to set the diff text to. </param>
    /// <param name="lobbyCompatibilityPresent"> LobbyCompatibility is installed on the lobby. </param>
    public PluginDiffSlot? Spawn(PluginDiff pluginDiff, bool lobbyCompatibilityPresent)
    {
        var pluginDiffSlot = SpawnInternal();
        pluginDiffSlot?.SetPluginDiff(pluginDiff, lobbyCompatibilityPresent);
        return pluginDiffSlot;
    }

    /// <summary>
    ///     Spawns a <see cref="PluginDiffSlot"/> using the pool, and sets the text manually.
    /// </summary>
    /// <param name="pluginNameText"> The plugin's name to display. </param>
    /// <param name="clientVersionText"> The client's plugin version to display. </param>
    /// <param name="serverVersionText"> The server's plugin version to display. </param>
    /// <param name="color"> Color to display. </param>
    public PluginDiffSlot? Spawn(string pluginNameText, string clientVersionText, string serverVersionText, Color color)
    {
        var pluginDiffSlot = SpawnInternal();
        pluginDiffSlot?.SetText(pluginNameText, clientVersionText, serverVersionText, color);
        return pluginDiffSlot;
    }

    /// <summary>
    ///     Despawns a <see cref="PluginDiffSlot"/> using the pool.
    /// </summary>
    /// <param name="pluginDiffSlot"> The <see cref="PluginDiffSlot"/> to despawn. </param>
    public void Release(PluginDiffSlot pluginDiffSlot)
    {
        if (pluginDiffSlot == null || _pool == null)
            return;

        _pool.Release(pluginDiffSlot);
    }

    /// <summary>
    ///     Spawns a <see cref="PluginDiffSlot"/> using the pool.
    /// </summary>
    /// <remarks>
    ///     Identical to <see cref="ObjectPool{T}.Get"/>, except with null checking
    /// </remarks>
    private PluginDiffSlot? SpawnInternal()
    {
        // This shouldn't happen unless you call Spawn *right* before scene change
        if (_template == null || _container == null || _pool == null)
        {
            LobbyCompatibilityPlugin.Logger?.LogInfo("PluginDiffSlotPool tried to spawn an item, but was not initialized properly.");
            return null;
        }

        var pluginDiffSlot = _pool.Get();
        return pluginDiffSlot;
    }

    /// <summary>
    ///     Creates a new pooled <see cref="PluginDiffSlot"/>.
    ///     Called when the pool is exhausted and needs a new item.
    /// </summary>
    private PluginDiffSlot CreatePooledItem()
    {
        if (_template == null || _container == null)
            throw new NullReferenceException("Template is missing! Did it get destroyed?");

        var pluginDiffSlot = Instantiate(_template, _container);
        pluginDiffSlot.gameObject.SetActive(true);

        return pluginDiffSlot;
    }

    /// <summary>
    ///     Activates a pooled item's gameObject and sets it to appear last in the hierarchy.
    ///     Called when an item is taken from the pool using Get.
    /// </summary>
    private void OnTakeFromPool(PluginDiffSlot pluginDiffSlot)
    {
        pluginDiffSlot.gameObject.SetActive(true);
        pluginDiffSlot.transform.SetAsLastSibling();
    }

    /// <summary>
    ///     Deactivates a pooled item's gameObject.
    ///     Called when an item is returned to the pool using Release.
    /// </summary>
    private void OnReturnedToPool(PluginDiffSlot pluginDiffSlot)
    {
        pluginDiffSlot.gameObject.SetActive(false);
    }

    /// <summary>
    ///     Destroys a pooled item.
    ///     Called when the pool capacity is reached and an item is returned.
    /// </summary>
    private void OnDestroyPoolObject(PluginDiffSlot pluginDiffSlot)
    {
        Destroy(pluginDiffSlot.gameObject);
    }
}
