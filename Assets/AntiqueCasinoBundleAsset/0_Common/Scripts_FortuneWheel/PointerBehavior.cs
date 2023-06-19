﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mkey;

namespace MkeyFW
{
    public class PointerBehavior : MonoBehaviour
    {
        [SerializeField]
        private AudioClip pointerHit;
        private AudioSource audioSource;
        private SoundMaster MSound { get { return SoundMaster.Instance; } }

        #region regular
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource && pointerHit)
            {
                audioSource.volume =(MSound)? MSound.Volume:1;
                audioSource.clip = pointerHit;
            }
        }
        #endregion regular

        /// <summary>
        /// Used as animation curve event handler
        /// </summary>
        public void Hithandler()
        {
            bool soundOn =(MSound)? MSound.SoundOn : true;
            if (pointerHit && audioSource && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}