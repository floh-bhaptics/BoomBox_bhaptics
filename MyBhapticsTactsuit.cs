﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();
        public List<string> myEffectStrings = new List<string> { };

        private static bHaptics.RotationOption defaultRotationOption = new bHaptics.RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                bHaptics.SubmitRegistered("HeartBeat");
                Thread.Sleep(600);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }



        void RegisterAllTactFiles()
        {
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    bHaptics.RegisterFeedbackFromTactFile(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
                if (prefix.StartsWith("LightEffect"))
                {
                    myEffectStrings.Add(prefix);
                    LOG("Light effect pattern added: " + prefix);
                }
            }
            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            //LOG("Trying to play");
            if (FeedbackMap.ContainsKey(key))
            {
                //LOG("ScaleOption");
                bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(intensity, duration);
                //LOG("Submit");
                bHaptics.SubmitRegistered(key, key, scaleOption, defaultRotationOption);
                // LOG("Playing back: " + key);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void DrumHit(bool isRightHand, float intensity = 1.0f)
        {
            // weaponName is a parameter that will go into the vest feedback pattern name
            // isRightHand is just which side the feedback is on
            // intensity should usually be between 0 and 1

            float duration = 1.0f;
            var scaleOption = new bHaptics.ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            var rotationFront = new bHaptics.RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyArm = "DrumstickArm" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "DrumstickVest" + postfix;
            bHaptics.SubmitRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHaptics.SubmitRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }


        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public bool IsPlaying(String effect)
        {
            return bHaptics.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            bHaptics.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHaptics.TurnOff(key);
            }
        }

        public void PlaySpecialEffect(string effect)
        {
            foreach (string myEffect in myEffectStrings)
            {
                if (IsPlaying(myEffect)) return;
            }
            if (IsPlaying(effect)) return;
            PlaybackHaptics(effect);
        }

        public void StopThreads()
        {
            // Yes, looks silly here, but if you have several threads like this, this is
            // very useful when the player dies or starts a new level
            StopHeartBeat();
        }


    }
}
