using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LobbyCompatibility.Patches;

public class LeaderboardPatch
{
    
    internal static IEnumerable<CodeInstruction> ModdedLeaderboards(IEnumerable<CodeInstruction> instructions)
    {
#if DEBUG
        LobbyCompatibilityPlugin.Logger?.LogDebug("-----START-----");
        instructions.Do(inst => LobbyCompatibilityPlugin.Logger?.LogDebug(inst));
        LobbyCompatibilityPlugin.Logger?.LogDebug("-----END-----");
#endif
        
        var newInstructions = new CodeMatcher(instructions)
            .SearchForward(i => i.Is(OpCodes.Ldstr, "challenge{0}"))
            .SetOperandAndAdvance("modded_challenge{0}")
            .InstructionEnumeration()
            .ToArray();

#if DEBUG
        LobbyCompatibilityPlugin.Logger?.LogDebug("-----START-----");
        newInstructions.Do(inst => LobbyCompatibilityPlugin.Logger?.LogDebug(inst));
        LobbyCompatibilityPlugin.Logger?.LogDebug("-----END-----");
#endif
        
        return newInstructions;
    }
}