using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace LobbyCompatibility.Pooling;

/// <summary>
///     Pools <see cref="PluginCategorySlot"/> objects.
/// </summary>
/// <seealso cref="PluginCategorySlot" />
internal class PluginCategorySlotPool : MonoBehaviour
{
    private PluginCategorySlot? _template;
    private Transform? _container;
    private IObjectPool<PluginCategorySlot>? _pool;

    /// <summary>
    ///     Initializes the pool using a template <see cref="PluginCategorySlot"/>.
    /// </summary>
    /// <param name="template"> The template <see cref="PluginCategorySlot"/> to pool. </param>
    /// <param name="container"> The container to Instantiate the template into by default. </param>
    public void InitializeUsingTemplate(PluginCategorySlot template, Transform container)
    {
        _container = container;
        _template = template;

        _pool = new ObjectPool<PluginCategorySlot>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, 10);
    }

    /// <summary>
    ///     Spawns a <see cref="PluginCategorySlot"/> using the pool, and sets the text to match a <see cref="PluginDiffResult"/>.
    /// </summary>
    /// <param name="pluginDiffResult"> The <see cref="PluginDiffResult"/> to set the category header text to. </param>
    public PluginCategorySlot? Spawn(PluginDiffResult pluginDiffResult)
    {
        var pluginCategorySlot = SpawnInternal();
        pluginCategorySlot?.SetPluginDiffResult(pluginDiffResult);
        return pluginCategorySlot;
    }

    /// <summary>
    ///     Despawns a <see cref="PluginCategorySlot"/> using the pool.
    /// </summary>
    /// <param name="pluginCategorySlot"> The <see cref="PluginCategorySlot"/> to despawn. </param>
    public void Release(PluginCategorySlot pluginCategorySlot)
    {
        if (pluginCategorySlot == null || _pool == null)
            return;

        _pool.Release(pluginCategorySlot);
    }

    /// <summary>
    ///     Spawns a <see cref="PluginCategorySlot"/> using the pool.
    /// </summary>
    /// <remarks>
    ///     Identical to <see cref="ObjectPool{T}.Get"/>, except with null checking
    /// </remarks>
    private PluginCategorySlot? SpawnInternal()
    {
        // This shouldn't happen unless you call Spawn *right* before scene change
        if (_template == null || _container == null || _pool == null)
        {
            LobbyCompatibilityPlugin.Logger?.LogInfo("PluginCategorySlotPool tried to spawn an item, but was not initialized properly.");
            return null;
        }

        var pluginCategorySlot = _pool.Get();
        return pluginCategorySlot;
    }

    /// <summary>
    ///     Creates a new pooled <see cref="PluginCategorySlot"/>.
    ///     Called when the pool is exhausted and needs a new item.
    /// </summary>
    private PluginCategorySlot CreatePooledItem()
    {
        if (_template == null || _container == null)
            throw new NullReferenceException("Template is missing! Did it get destroyed?");

        var pluginCategorySlot = Instantiate(_template, _container);
        pluginCategorySlot.gameObject.SetActive(true);

        return pluginCategorySlot;
    }

    /// <summary>
    ///     Activates a pooled item's gameObject and sets it to appear last in the hierarchy.
    ///     Called when an item is taken from the pool using Get.
    /// </summary>
    private void OnTakeFromPool(PluginCategorySlot pluginCategorySlot)
    {
        pluginCategorySlot.gameObject.SetActive(true);
        pluginCategorySlot.transform.SetAsLastSibling();
    }

    /// <summary>
    ///     Deactivates a pooled item's gameObject.
    ///     Called when an item is returned to the pool using Release.
    /// </summary>
    private void OnReturnedToPool(PluginCategorySlot pluginCategorySlot)
    {
        pluginCategorySlot.gameObject.SetActive(false);
    }

    /// <summary>
    ///     Destroys a pooled item.
    ///     Called when the pool capacity is reached and an item is returned.
    /// </summary>
    private void OnDestroyPoolObject(PluginCategorySlot pluginCategorySlot)
    {
        Destroy(pluginCategorySlot.gameObject);
    }
}
