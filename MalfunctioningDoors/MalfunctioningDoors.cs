/*
    A Lethal Company Mod
    Copyright (C) 2024  TestAccount666 (Entity303 / Test-Account666)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MalfunctioningDoors.Dependencies;
using MalfunctioningDoors.Malfunctions;
using MalfunctioningDoors.Patches;
using UnityEngine;
using UnityEngine.Networking;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace MalfunctioningDoors;

[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class MalfunctioningDoors : BaseUnityPlugin {
    private const int GHOST_HAND_SOUNDS_SIZE = 3;
    internal static Object ghostHandPrefab = null!;

    internal static AudioClip? doorHitSfx;
    internal static AudioClip? doorBreakSfx;

    internal static readonly AudioClip[] GhostHandSfxList = new AudioClip[GHOST_HAND_SOUNDS_SIZE];
    public static MalfunctioningDoors Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            Logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize();
        }

        var modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Debug.Assert(modDirectory != null, nameof(modDirectory) + " != null");
        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(modDirectory, "ghosthand"));

        ghostHandPrefab = assetBundle.LoadAsset("ghosthand");

        Patch();

        DoorLockPatch.InitializeConfig(Config);
        MalfunctionalDoor.InitializeConfig(Config);

        FetchMalfunctions();

        //Make RCP methods work
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types) {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods) {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length <= 0)
                    continue;

                method.Invoke(null, null);
            }
        }

        StartCoroutine(LoadAudioClips());

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    private static void FetchMalfunctions() {
        MalfunctionGenerator.MalfunctionDictionary.Clear();

        var types = Assembly.GetExecutingAssembly().GetTypes();

        var markedTypes = types.Where(Predicate);

        foreach (var type in markedTypes) {
            var malfunction = (MalfunctionAttribute) type.GetCustomAttribute(typeof(MalfunctionAttribute), false);

            var weight = malfunction.weight;

            var overrideWeightMethod = type.GetMethod("OverrideWeight", BindingFlags.Public | BindingFlags.Static);

            if (overrideWeightMethod is not null)
                weight = (int) overrideWeightMethod.Invoke(null, [
                    Instance.Config,
                ]);

            var initializeConfigMethod = type.GetMethod("InitializeConfig", BindingFlags.Public | BindingFlags.Static);

            initializeConfigMethod?.Invoke(null, [
                Instance.Config,
            ]);

            MalfunctionGenerator.MalfunctionDictionary.Add(type, weight);
        }
    }

    private static bool Predicate(ICustomAttributeProvider type) =>
        type.GetCustomAttributes(typeof(MalfunctionAttribute), false).Length > 0;

    private static IEnumerator LoadAudioClips() {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Logger.LogInfo("Loading Sounds...");

        Debug.Assert(assemblyDirectory != null, nameof(assemblyDirectory) + " != null");
        var audioPath = Path.Combine(assemblyDirectory, "sounds");

        audioPath = Directory.Exists(audioPath)? audioPath : Path.Combine(assemblyDirectory);

        LoadGhostHandAudioClips(audioPath);

        LoadDoorAudioClips(audioPath);

        yield break;
    }

    private static void LoadGhostHandAudioClips(string audioPath) {
        Logger.LogInfo("Loading Ghost Hand Sounds...");

        var ghostHandAudioPath = Path.Combine(audioPath, "GhostHandSounds");

        ghostHandAudioPath = Directory.Exists(ghostHandAudioPath)? ghostHandAudioPath : Path.Combine(audioPath);

        for (var index = 1; index <= GHOST_HAND_SOUNDS_SIZE; index++) {
            var sound = index - 1;

            var ghostHandAudioClip =
                LoadAudioClipFromFile(new(Path.Combine(ghostHandAudioPath, $"GhostHand{index}.wav")), $"GhostHand{index}");

            if (ghostHandAudioClip is null) {
                Logger.LogInfo($"Failed to load clip 'GhostHand{index}'!");
                continue;
            }

            GhostHandSfxList[sound] = ghostHandAudioClip;

            Logger.LogInfo($"Loaded clip '{ghostHandAudioClip.name}'!");
        }
    }

    private static void LoadDoorAudioClips(string audioPath) {
        Logger.LogInfo("Loading Door Sounds...");

        var doorAudioPath = Path.Combine(audioPath, "DoorSfx");

        doorAudioPath = Directory.Exists(doorAudioPath)? doorAudioPath : Path.Combine(audioPath);

        doorHitSfx = LoadAudioClipFromFile(new(Path.Combine(doorAudioPath, "DoorHit.wav")), "DoorHit");

        Logger.LogInfo(doorHitSfx is null? "Failed to load clip 'DoorHit'!" : $"Loaded clip '{doorHitSfx.name}'!");

        doorBreakSfx = LoadAudioClipFromFile(new(Path.Combine(doorAudioPath, "DoorBreak.wav")), "DoorBreak");

        Logger.LogInfo(doorBreakSfx is null? "Failed to load clip 'DoorBreak'!" : $"Loaded clip '{doorBreakSfx.name}'!");
    }

    private static AudioClip? LoadAudioClipFromFile(Uri filePath, string name) {
        using var unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV);

        var asyncOperation = unityWebRequest.SendWebRequest();

        while (!asyncOperation.isDone)
            Thread.Sleep(100);

        if (unityWebRequest.result != UnityWebRequest.Result.Success) {
            Logger.LogError("Failed to load AudioClip: " + unityWebRequest.error);
            return null;
        }

        var clip = DownloadHandlerAudioClip.GetContent(unityWebRequest);

        clip.name = name;

        return clip;
    }
}