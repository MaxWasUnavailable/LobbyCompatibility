using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
public class LeaderboardPatch
{
    [HarmonyPatch(typeof(MenuManager))]
    [HarmonyPatch(nameof(MenuManager.SetIfChallengeMoonHasBeenCompleted))]
    [HarmonyPatch(nameof(MenuManager.EnableLeaderboardDisplay))]
    [HarmonyPatch(nameof(MenuManager.EnableLeaderboardDisplay))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ChangeSaveFile(IEnumerable<CodeInstruction> instructions)
    {
        // For all instructions, do this: (for each)
        return instructions.Select(codeInstruction =>
            {
                // If the operand is "LCChallengeFile", replace it with "LCModdedChallengeFile", otherwise leave as is
                codeInstruction.operand = codeInstruction.Is(OpCodes.Ldstr, "LCChallengeFile")
                    ? "LCModdedChallengeFile"
                    : codeInstruction.operand;
                return codeInstruction;
            }
        );
    }
    
    internal static IEnumerable<CodeInstruction> ChangeToModdedLeaderboard(IEnumerable<CodeInstruction> instructions)
    {
        // Create CodeMatcher from instructions with all instances of "LCChallengeFile" replaced
        return new CodeMatcher(ChangeSaveFile(instructions))
            // Find the instance of "challenge{0}"
            .SearchForward(i => i.Is(OpCodes.Ldstr, "challenge{0}"))
            // Replace the leaderboard name operand with the modded version
            .SetOperandAndAdvance("modded_challenge{0}")
            // Turn CodeMatcher to IEnumerable<CodeInstruction> so it can be returned
            .InstructionEnumeration();
    }
}