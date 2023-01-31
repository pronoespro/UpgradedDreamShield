using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Upgraded_Dream_Shield
{
    public class Upgraded_Dream_Shield : Mod
    {
        internal static Upgraded_Dream_Shield Instance;

        //public override List<ValueTuple<string, string>> GetPreloadNames()
        //{
        //    return new List<ValueTuple<string, string>>
        //    {
        //        new ValueTuple<string, string>("White_Palace_18", "White Palace Fly")
        //    };
        //}

        //public Upgraded_Dream_Shield() : base("Upgraded_Dream_Shield")
        //{
        //    Instance = this;
        //}

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;

            Log("Initialized");
        }
    }
}