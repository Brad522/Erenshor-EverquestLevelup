using System.Collections;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace Erenshor_EverquestLevelup
{
    [BepInPlugin(ModGUID, ModDescription, ModVersion)]
    public class EverquestLevelup : BaseUnityPlugin
    {
        internal const string ModName = "EverquestLevelup";
        internal const string ModVersion = "1.0.0";
        internal const string ModDescription = "Everquest Levelup";
        internal const string Author = "Brad522";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony harmony = new Harmony(ModGUID);

        public static AudioClip lvlupSFX;

        public void Awake()
        {
            harmony.PatchAll();

            Logger.LogMessage("Everquest Levelup Loaded");

            string sfxPath = Path.Combine(Path.GetDirectoryName(Info.Location), "everquest-level-up-ding.mp3");

            if (File.Exists(sfxPath))
            {
                StartCoroutine(GrabSFX(sfxPath));
            }
        }

        private IEnumerator GrabSFX(string path)
        {
            Logger.LogMessage("Attempting to load audio from: " + path);

            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Logger.LogMessage("UWR failed: "+ uwr.error);
                    yield break;
                }

                lvlupSFX = DownloadHandlerAudioClip.GetContent(uwr);
                Logger.LogMessage("Audioclip loaded: " + (lvlupSFX != null));
            }
        }

        [HarmonyPatch(typeof(Stats))]
        [HarmonyPatch("DoLevelUp")]
        class LevelUpSound
        {
            static bool Prefix(Stats __instance)
            {
                if(__instance.Level < 35 && !__instance.Myself.isNPC && lvlupSFX != null)
                {
                    GameData.PlayerAud.PlayOneShot(lvlupSFX, GameData.PlayerAud.volume * GameData.SFXVol);
                }
                return true;
            }
        }

    }
}
