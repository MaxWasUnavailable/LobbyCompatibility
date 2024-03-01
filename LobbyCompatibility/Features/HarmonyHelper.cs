using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LobbyCompatibility.Features;

/// <summary>
///     Helper class related to Harmony Patching.
/// </summary>
internal static class HarmonyHelper
{
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
            .StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}