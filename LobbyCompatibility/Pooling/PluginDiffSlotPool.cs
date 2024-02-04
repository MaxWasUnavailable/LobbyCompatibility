using HarmonyLib;
using LobbyCompatibility;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace LobbyCompatibility.Pooling;

internal class PluginDiffSlotPool : MonoBehaviour
{
    private PluginDiffSlot? _template;
    private Transform? _container;
    private IObjectPool<PluginDiffSlot>? _pool;

    public void InitializeUsingTemplate(PluginDiffSlot template, Transform container)
    {
        _container = container;
        _template = template;

        _pool = new ObjectPool<PluginDiffSlot>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, 10);
    }

    public PluginDiffSlot? Spawn(PluginDiff pluginDiff)
    {
        // This shouldn't happen unless you call Spawn *right* before scene change
        if (_template == null || _container == null || _pool == null)
        {
            LobbyCompatibilityPlugin.Logger?.LogInfo("PluginDiffSlotPool tried to spawn an item, but was not initialized properly.");
            return null;
        }

        var pluginDiffSlot = _pool.Get();
        pluginDiffSlot.SetPluginDiff(pluginDiff);
        return pluginDiffSlot;
    }

    public void Release(PluginDiffSlot pluginDiffSlot)
    {
        if (pluginDiffSlot == null || _pool == null)
            return;

        _pool.Release(pluginDiffSlot);
    }

    private PluginDiffSlot CreatePooledItem()
    {
        if (_template == null || _container == null)
            throw new NullReferenceException("Template is missing! Did it get destroyed?");

        var pluginDiffSlot = Instantiate(_template, _container);
        pluginDiffSlot.gameObject.SetActive(true);

        return pluginDiffSlot;
    }

    // Called when an item is returned to the pool using Release
    private void OnReturnedToPool(PluginDiffSlot pluginDiffSlot)
    {
        pluginDiffSlot.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    private void OnTakeFromPool(PluginDiffSlot pluginDiffSlot)
    {
        pluginDiffSlot.gameObject.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private void OnDestroyPoolObject(PluginDiffSlot pluginDiffSlot)
    {
        Destroy(pluginDiffSlot.gameObject);
    }
}
