using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;

namespace BoomBox_bhaptics
{
    public class BoomBox_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(GameplayManager), "OnNoteMissed", new Type[] { typeof(object), typeof(boombox_api.Scoring.ScoreEventArgs) })]
        public class bhaptics_NoteMissed
        {
            [HarmonyPostfix]
            public static void Postfix(GameplayManager __instance, bool ____playerIsPlaying)
            {
                if (!____playerIsPlaying) return;
                if (__instance.GivenUp) return;
                if (__instance.Failed) return;
                tactsuitVr.PlaybackHaptics("MissedNote");
            }
        }

        [HarmonyPatch(typeof(GameplayManager), "OnNoteHit", new Type[] { typeof(object), typeof(boombox_api.Scoring.ScoreEventArgs) })]
        public class bhaptics_NoteHit
        {
            [HarmonyPostfix]
            public static void Postfix(GameplayManager __instance, boombox_api.Scoring.ScoreEventArgs args, bool ____playerIsPlaying)
            {
                if (!____playerIsPlaying) return;
                if (__instance.GivenUp) return;
                if (__instance.Failed) return;
                bool isRightHand = (args.Data.Hand == boombox_api.Maps.DataTypes.HandType.Right);
                float intensity = args.VelocityScore;
                tactsuitVr.DrumHit(isRightHand, intensity);
                if (args.Judgement == boombox_api.Scoring.Structures.Judgement.Perfect) tactsuitVr.PlaybackHaptics("BellyDrum");
            }
        }

        [HarmonyPatch(typeof(GameplayManager), "OnObstacleHit", new Type[] { typeof(object), typeof(boombox_api.Maps.DataTypes.ObjectPair<boombox_api.Maps.DataTypes.BoomboxObject>) } )]
        public class bhaptics_ObstacleHit
        {
            [HarmonyPostfix]
            public static void Postfix(GameplayManager __instance, bool ____playerIsPlaying)
            {
                if (!____playerIsPlaying) return;
                tactsuitVr.PlaybackHaptics("HitByWall");
                tactsuitVr.PlaybackHaptics("HitByWallHead");
            }
        }

        [HarmonyPatch(typeof(GameplayManager), "OnFailed", new Type[] { typeof(object), typeof(EventArgs) })]
        public class bhaptics_LevelFailed
        {
            [HarmonyPostfix]
            public static void Postfix(GameplayManager __instance, bool ____playerIsPlaying)
            {
                //if (!____playerIsPlaying) return;
                tactsuitVr.PlaybackHaptics("LevelFailed");
            }
        }

        [HarmonyPatch(typeof(GameplayManager), "EndGame", new Type[] { typeof(bool) })]
        public class bhaptics_LevelEnds
        {
            [HarmonyPostfix]
            public static void Postfix(GameplayManager __instance, bool ____playerIsPlaying)
            {
                //if (!____playerIsPlaying) return;
                if (__instance.Failed) return;
                tactsuitVr.PlaybackHaptics("LevelSuccess");

            }
        }

        [HarmonyPatch(typeof(PurrController), "OnPurringStart", new Type[] {  })]
        public class bhaptics_StartPurring
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (tactsuitVr.IsPlaying("PupaPurr")) return;
                tactsuitVr.PlaybackHaptics("PupaPurr");
            }
        }

    }
}
