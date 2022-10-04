using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System;
using Return.Agents;
using TNet;
using DarkTonic.MasterAudio;
using Return.Audios;

namespace Return.Items.Weapons
{

    public class FireControlPreset : FirearmsModulePreset<FireControlModule>
    {
        [SerializeField][Tooltip("Gameplay packageOption")]
        public FireModeBinding[] FireModes;

        //public m_Location m_Trigger;
        //public m_Location m_ModeToggle;

        [Title("Event Definition")]
        public FirearmsEvent Event;


        // audio performer without animation
        [TitleGroup("Independent Audio Config",Alignment =TitleAlignments.Centered)]

        [Tooltip("Play via audio manager.")]
        [BoxGroup("Independent Audio Config/AudioBundle", ShowLabel = false)]
        public BundleSelecter SwitchModeSound;

        [Tooltip("Play via audio manager.")]
        [BoxGroup("Independent Audio Config/AudioBundle")]
        public BundleSelecter MuzzleSounds;

        [Tooltip("Play via audio manager.")]
        [BoxGroup("Independent Audio Config/AudioBundle")]
        public BundleSelecter EmptyTriggerSound;




        // anim data 
        [TitleGroup("Animation Config", Alignment = TitleAlignments.Centered)]


        [BoxGroup("Animation Config/Safety")]
        public TimelinePreset Safety;

        [BoxGroup("Animation Config/IdleState")]
        public TimelinePreset Idle;

        [Obsolete]
        [BoxGroup("Animation Config/Trigger")]
        public TimelinePreset Trigger;

        [BoxGroup("Animation Config/Fire")]
        public TimelinePreset Firing;

        [BoxGroup("Animation Config/SwitchMode")]
        public TimelinePreset SwitchMode;

        //[BoxGroup("Animation Config/Loaded")]
        //public TimelinePreset Loaded;
    }
}