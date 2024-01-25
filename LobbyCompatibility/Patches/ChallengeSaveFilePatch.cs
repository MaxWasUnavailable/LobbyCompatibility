using HarmonyLib;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal class ChallengeSaveFilePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ES3Settings), MethodType.Constructor, typeof(string), typeof(ES3Settings))]
    private static void FileSettings(ref string path, ES3Settings settings)
    {
        path = path == "LCChallengeFile" ? "LCModdedChallengeFile" : path;
    }
}