using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LobbyCompatibility.Enums;
using Steamworks.Data;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="GameNetworkManager.LobbyDataIsJoinable" />.
///     Overrides the joinable check to OR with __joinable. This is necessary in case there are required plugins in the
///     lobby, since the mod will then disable joining for vanilla clients.
/// </summary>
/// <seealso cref="GameNetworkManager.LobbyDataIsJoinable" />
// ReSharper disable UnusedMember.Local
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class LobbyDataIsJoinableTranspiler
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchForward(instruction => instruction.OperandIs(LobbyMetadata.Joinable))
            .ThrowIfInvalid("Could not find joinable")
            .RemoveInstructions(4)
            .Insert(new CodeInstruction(
                OpCodes.Call,
                AccessTools.Method(typeof(LobbyDataIsJoinableTranspiler), nameof(IsJoinable))))
            .InstructionEnumeration();
    }

    private static bool IsJoinable(ref Lobby lobby)
    {
        return lobby.GetData(LobbyMetadata.JoinableModded) == "true" || lobby.GetData(LobbyMetadata.Joinable) == "true";
    }
}