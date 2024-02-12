using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace LobbyCompatibility.Pooling;

internal class PluginCategorySlotPool : MonoBehaviour
{
    private PluginCategorySlot? _template;
    private Transform? _container;
    private IObjectPool<PluginCategorySlot>? _pool;

    public void InitializeUsingTemplate(PluginCategorySlot template, Transform container)
    {
        _container = container;
        _template = template;

        _pool = new ObjectPool<PluginCategorySlot>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, 10);
    }

    public PluginCategorySlot? Spawn(PluginDiffResult pluginDiffResult)
    {
        var pluginCategorySlot = SpawnInternal();
        pluginCategorySlot?.SetPluginDiffResult(pluginDiffResult);
        return pluginCategorySlot;
    }
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

    public void Release(PluginCategorySlot pluginCategorySlot)
    {
        if (pluginCategorySlot == null || _pool == null)
            return;

        _pool.Release(pluginCategorySlot);
    }

    private PluginCategorySlot CreatePooledItem()
    {
        if (_template == null || _container == null)
            throw new NullReferenceException("Template is missing! Did it get destroyed?");

        var pluginCategorySlot = Instantiate(_template, _container);
        pluginCategorySlot.gameObject.SetActive(true);

        return pluginCategorySlot;
    }

    // Called when an item is returned to the pool using Release
    private void OnReturnedToPool(PluginCategorySlot pluginCategorySlot)
    {
        pluginCategorySlot.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    private void OnTakeFromPool(PluginCategorySlot pluginCategorySlot)
    {
        pluginCategorySlot.gameObject.SetActive(true);
        pluginCategorySlot.transform.SetAsLastSibling();
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private void OnDestroyPoolObject(PluginCategorySlot pluginCategorySlot)
    {
        Destroy(pluginCategorySlot.gameObject);
    }
}
