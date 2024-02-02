using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="MenuManager.GetLeaderboardForChallenge" />.
///     Overrides the challenge moon leaderboard title text by appending (Modded) to the end
/// </summary>
/// <seealso cref="MenuManager.GetLeaderboardForChallenge" />
// ReSharper disable UnusedMember.Local
[HarmonyPatch]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class GetLeaderboardForChallengeTranspiler
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.GetLeaderboardForChallenge))
            .GetCustomAttribute<AsyncStateMachineAttribute>()
            .StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(instruction => instruction.OperandIs(" Results"))
            .ThrowIfInvalid("Could not find leaderboard results text")
            .SetOperandAndAdvance(" Results (Modded)")
            .InstructionEnumeration();
    }
}