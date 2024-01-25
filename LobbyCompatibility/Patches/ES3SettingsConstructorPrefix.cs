using System;
using HarmonyLib;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="ES3Settings(string, ES3Settings)" />.
///     Creates a separate modded challenge save file.
/// </summary>
/// <seealso cref="ES3Settings(string, ES3Settings)" />
[HarmonyPatch(typeof(ES3Settings), MethodType.Constructor, new Type[] { typeof(string), typeof(ES3Settings) })]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal class ES3SettingsConstructorPrefix
{
    [HarmonyPrefix]
    private static void FileSettings(ref string path)
    {
        path = path == "LCChallengeFile" ? "LCModdedChallengeFile" : path;
    }
}