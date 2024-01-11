using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal class LeaderboardPatch
{
    [HarmonyTranspiler]
    // Normal Menu Manager methods to patch
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.SetIfChallengeMoonHasBeenCompleted))]
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.EnableLeaderboardDisplay))]
    // Save File UI Slot methods to patch
    [HarmonyPatch(typeof(SaveFileUISlot), nameof(SaveFileUISlot.Awake))]
    [HarmonyPatch(typeof(SaveFileUISlot), nameof(SaveFileUISlot.SetChallengeFileSettings))]
    // Game Network Manager methods to patch
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SetLobbyJoinable))]
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveGameValues))]
    // Start of Round method to patch
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetTimeAndPlanetToSavedSettings))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
#if DEBUG
        LobbyCompatibilityPlugin.Logger!.LogDebug("Attempting to change save file string");
#endif
        
        // For all instructions, do this: (for each)
        var newInstructions = instructions.Select(codeInstruction =>
            {
                // If the operand is "LCChallengeFile", replace it with "LCModdedChallengeFile", otherwise leave as is
                codeInstruction.operand = codeInstruction.Is(OpCodes.Ldstr, "LCChallengeFile")
                    ? "LCModdedChallengeFile"
                    : codeInstruction.operand;
                return codeInstruction;
            }
        );

        return newInstructions;
    }
    
    internal static IEnumerable<CodeInstruction> ChangeToModdedLeaderboard(IEnumerable<CodeInstruction> instructions)
    {
#if DEBUG
        LobbyCompatibilityPlugin.Logger!.LogDebug("Attempting to change leaderboard string");
#endif
        
        // Create CodeMatcher from instructions with all instances of "LCChallengeFile" replaced
        return new CodeMatcher(Transpiler(instructions))
            // Find the instance of "challenge{0}"
            .SearchForward(i => i.Is(OpCodes.Ldstr, "challenge{0}"))
            // Replace the leaderboard name operand with the modded version
            .SetOperandAndAdvance("modded_challenge{0}")
            // Turn CodeMatcher to IEnumerable<CodeInstruction> so it can be returned
            .InstructionEnumeration();
    }
}