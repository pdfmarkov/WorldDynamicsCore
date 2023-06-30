﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore.Tests
{
    public class DebugWalkerManager : MonoBehaviour, IWalkerManager
    {
        public event Action<Walker> WalkerRegistered;
        public event Action<Walker> WalkerDeregistered;

        private List<Walker> _walkers = new List<Walker>();

        private void Awake()
        {
            Dependencies.Register<IWalkerManager>(this);
        }

        public IEnumerable<Walker> GetWalkers() => _walkers;

        public void RegisterWalker(Walker walker)
        {
            _walkers.Add(walker);
            WalkerRegistered?.Invoke(walker);
        }
        public void DeregisterWalker(Walker walker)
        {
            _walkers.Remove(walker);
            WalkerDeregistered?.Invoke(walker);
        }
    }
}