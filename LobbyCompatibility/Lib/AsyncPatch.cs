using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LobbyCompatibility.Lib;

public static class AsyncPatch
{
    internal static void Transpiler(Harmony harmony, Type targetType, string targetMethod, Type transpilerType, string transpilerMethod)
    {
        // Get the Method Info of the target Async Method; Can replace Type & Method with any async method
        var getLeaderboard = AccessTools.Method(targetType, targetMethod)
            // Find the AsyncStateMachine class from target method
            .GetCustomAttribute<AsyncStateMachineAttribute>()
            // Get the struct type (random compiler junk), and then get the <GetLeaderboardForChallenge>d__80::MoveNext
            .StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);

        // Use a manual patch to patch the MoveNext method - transpiler must be public
        harmony.Patch(getLeaderboard, transpiler: new HarmonyMethod(transpilerType.GetMethod(transpilerMethod, (BindingFlags)60)));
    }
}