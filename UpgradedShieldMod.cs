using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding;
using SFCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using UObject = UnityEngine.Object;


namespace Upgraded_Dream_Shield
{

    [UsedImplicitly]
    class UpgradedShieldMod:Mod,ITogglableMod
    {

        #region variables
        public Dictionary<string, AssetBundle> GOBundle;
        public Dictionary<string, Transform> charmSpawns;
        public Dictionary<string, int> charmProjectileNums;
        public Transform ogDreamShield;

        public Dictionary<string, FsmState> ogCharmStates;

        public List<string> objectsToLoad = new List<string>() {"newattacks","sounds" };

        public void UnloadVariables()
        {

        }
        #endregion

        #region setup
        public UpgradedShieldMod() : base("Upgraded Dream Shield") { }

        internal static UpgradedShieldMod Instance;

        public override string GetVersion()
        {
            return "Light v0.0.1.03";
        }


        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {


            Log("Initializing");

            Instance = this;

            InitializeFSM();
            SetupBundles();

            Log("Finished Initializing");

        }

        public void Unload()
        {

            UnloadFSM();
            UnloadVariables();
            UnloadBundles();

            Instance = null;

        }
        #endregion

        #region Bundles
        public void SetupBundles()
        {


            GOBundle = new Dictionary<string, AssetBundle>();

            Assembly asm = Assembly.GetExecutingAssembly();
            Log("Searching for Levels");
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null)
                    {
                        continue;
                    }
                    Log("Found asset");

                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();
                    string bundleName = Path.GetExtension(res).Substring(1);
                    if (objectsToLoad.Contains(bundleName))
                    {
                        GOBundle.Add(bundleName, AssetBundle.LoadFromMemory(buffer));
                    }
                    else
                    {
                        continue;

                    }

                }
            }
        }

        public void UnloadBundles()
        {
            if (GOBundle != null)
            {
                GOBundle.Clear();
            }

            GOBundle = null;
        }
        #endregion

        #region ShieldUpgrade
        public void InitializeFSM()
        {
            On.HeroController.Update += HeroController_Update;
        }

        public void UnloadFSM()
        {
            On.HeroController.Update -= HeroController_Update;
        }

        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            // Call orig so the original OnEnable function happens - otherwise things will break
            orig(self);

            // Execute your code here - I like making it a separate function but you could just do it all here if you prefer
            if (self.gameObject.name == "Knight")
            {
                Instance.UpgradeDreamShield(self.fsm_orbitShield);
            }

        }

        public void UpgradeDreamShield(PlayMakerFSM fsm)
        {
            if (!BossSequenceController.BoundCharms)
            {
                if (ogCharmStates == null)
                {
                    ogCharmStates = new Dictionary<string, FsmState>();

                    ogCharmStates.Add("DreamShield_spawn", new FsmState(fsm.GetFsmState("Spawn")));
                    ogCharmStates.Add("DreamShield_idle", new FsmState(fsm.GetFsmState("Idle")));
                    ogCharmStates.Add("DreamShield_slash", new FsmState(fsm.GetFsmState("Send Slash Event")));
                }

                foreach (string key in ogCharmStates.Keys)
                {
                    RestoreAction(fsm, ogCharmStates[key]);
                }

                fsm.RemoveAction("Spawn", 0);

                fsm.AddMethod("Spawn", () =>
                {
                    //Log("Create Dee Shield");
                    CreateDeeShield();
                });

                fsm.AddMethod("Idle", () =>
                {
                    Log("Dream shield update");
                    CreateDeeShield();

                    if (charmSpawns == null)
                    {
                        charmSpawns = new Dictionary<string, Transform>();
                    }
                    if (charmSpawns.ContainsKey("DeeShield"))
                    {
                        charmSpawns["DeeShield"].position = HeroController.instance.transform.position;
                    }
                });

                fsm.AddMethod("Send Slash Event", () =>
                {

                    CreateDeeShield();

                    if (charmSpawns.ContainsKey("DeeShield"))
                    {
                        UpgradedDreamShield shield = charmSpawns["DeeShield"].GetComponent<UpgradedDreamShield>();
                        shield.Slash();
                    }
                });
            }
        }


        public void RestoreAction(PlayMakerFSM fsm, FsmState og)
        {
            int actionLength;
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                actionLength = fsm.FsmStates[i].Actions.Length;
                if (fsm.FsmStates[i].Name == og.Name)
                {
                    for (int a = actionLength - 1; a >= 0; a--)
                    {
                        fsm.RemoveAction(fsm.FsmStates[i].Name, a);
                    }
                    for (int a = 0; a < og.Actions.Length; a++)
                    {
                        fsm.AddAction(fsm.FsmStates[i].Name, og.Actions[a]);
                    }
                    //Log("Restored state " + og.Name);
                }
            }
        }

        public void CreateDeeShield()
        {
            if (ogDreamShield == null)
            {
                if (GameObject.Find("Orbit Shield(Clone)") != null){
                    ogDreamShield = GameObject.Find("Orbit Shield(Clone)").transform;
                }
            }
            if (ogDreamShield != null)
            {
                ogDreamShield.gameObject.SetActive(false);

                if (charmSpawns == null)
                {
                    charmSpawns = new Dictionary<string, Transform>();
                }
                if (!charmSpawns.ContainsKey("DeeShield"))
                {
                    if (GOBundle.ContainsKey("newattacks"))
                    {
                        Transform prefav = GOBundle["newattacks"].LoadAsset<GameObject>("DreamShieldUpgrade").transform;
                        Transform shield = GameObject.Instantiate(prefav);
                        shield.gameObject.AddComponent<UpgradedDreamShield>();
                        shield.gameObject.AddComponent<NonBouncer>();

                        shield.Find("LazerStopper").gameObject.AddComponent<NonBouncer>();

                        charmSpawns.Add("DeeShield", shield);
                        shield.transform.position = HeroController.instance.transform.position;
                        Log("Shield created");
                    }
                }
                else
                {
                    if (charmSpawns["DeeShield"] == null)
                    {
                        charmSpawns.Remove("DeeShield");
                        if (GOBundle.ContainsKey("newattacks"))
                        {
                            Transform prefav = GOBundle["newattacks"].LoadAsset<GameObject>("DreamShieldUpgrade").transform;
                            Transform shield = GameObject.Instantiate(prefav);
                            shield.gameObject.AddComponent<UpgradedDreamShield>();
                            shield.gameObject.AddComponent<NonBouncer>();

                            shield.Find("LazerStopper").gameObject.AddComponent<NonBouncer>();

                            charmSpawns.Add("DeeShield", shield);
                            shield.transform.position = HeroController.instance.transform.position;
                            Log("Shield created");
                        }
                    }
                    else
                    {
                        charmSpawns["DeeShield"].gameObject.SetActive(true);
                    }

                }
            }
        }

        public void CreateDeeShieldProjectile(Vector2 pos, Quaternion rot)
        {
            if (charmSpawns == null)
            {
                charmSpawns = new Dictionary<string, Transform>();
            }

            if (charmSpawns.ContainsKey("DeeShield_proj_0") && charmSpawns["DeeShield_proj_0"] == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    charmSpawns.Remove("DeeShield_proj_" + i.ToString());
                }
            }

            if (!charmSpawns.ContainsKey("DeeShield_proj_0"))
            {
                for (int i = 0; i < 3; i++)
                {
                    List<Collider2D> colliders = new List<Collider2D>();

                    Transform prefav = GOBundle["newattacks"].LoadAsset<GameObject>("DreamShieldProjectile").transform;
                    Transform shield = GameObject.Instantiate(prefav, pos, rot);

                    UpgradedDreamShieldProjectile proj = shield.gameObject.AddComponent<UpgradedDreamShieldProjectile>();
                    shield.gameObject.AddComponent<NonBouncer>();

                    DamageEnemies dmg = shield.Find("StrongAttack").gameObject.AddComponent<DamageEnemies>();
                    dmg.circleDirection = true;
                    dmg.attackType = AttackTypes.Nail;
                    dmg.damageDealt = 3;
                    dmg.direction = 180f;
                    dmg.moveDirection = true;

                    dmg.gameObject.AddComponent<NonBouncer>();

                    dmg.gameObject.AddComponent<DeactivateOnCollision>().target=shield;

                    colliders.Add(dmg.GetComponent<Collider2D>());


                    dmg = shield.Find("WeakAttack").gameObject.AddComponent<DamageEnemies>();
                    dmg.circleDirection = true;
                    dmg.attackType = AttackTypes.Nail;
                    dmg.damageDealt = 1;

                    dmg.gameObject.AddComponent<NonBouncer>();

                    DeactivateOnCollision col=dmg.gameObject.AddComponent<DeactivateOnCollision>();
                    col.target = shield;
                    col.mask = LayerMask.NameToLayer("Enemies");

                    colliders.Add(dmg.GetComponent<Collider2D>());

                    charmSpawns.Add("DeeShield_proj_" + i.ToString(), shield);
                    shield.gameObject.SetActive(i == 0);
                    if (charmProjectileNums == null)
                    {
                        charmProjectileNums = new Dictionary<string, int>();
                        charmProjectileNums.Add("DeeShield", 1);
                    }

                    proj.colliders = colliders.ToArray();

                }
            }
            else
            {
                if (charmProjectileNums == null)
                {
                    charmProjectileNums = new Dictionary<string, int>();
                    charmProjectileNums.Add("DeeShield", 0);
                }
                else
                {
                    if (charmProjectileNums.ContainsKey("DeeShield"))
                    {
                        charmProjectileNums["DeeShield"] = (charmProjectileNums["DeeShield"] + 1) % 3;
                    }
                    else
                    {
                        charmProjectileNums.Add("DeeShield", 0);
                    }
                }

                if (charmSpawns.ContainsKey("DeeShield_proj_" + (charmProjectileNums["DeeShield"].ToString())) && charmSpawns["DeeShield_proj_" + (charmProjectileNums["DeeShield"].ToString())] != null)
                {
                    charmSpawns["DeeShield_proj_" + (charmProjectileNums["DeeShield"].ToString())].gameObject.SetActive(true);
                    charmSpawns["DeeShield_proj_" + (charmProjectileNums["DeeShield"].ToString())].position = pos;
                    charmSpawns["DeeShield_proj_" + (charmProjectileNums["DeeShield"].ToString())].rotation = rot;
                }
            }
        }

        #endregion

    }
}
