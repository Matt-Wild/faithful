﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore;

namespace Faithful
{
    internal class FaithfulCharacterBodyBehaviour : MonoBehaviour
    {
        // Store reference to Toolbox
        public Toolbox toolbox;

        // Store flags
        public Flags frameFlags = new Flags();
        public Flags stageFlags = new Flags();

        // Store reference to Character Body
        private CharacterBody character;

        private void Start()
        {
            // Get Character Body
            character = gameObject.GetComponent<CharacterBody>();
        }

        private void LateUpdate()
        {
            // Reset flags
            frameFlags.Reset();
        }
    }
}