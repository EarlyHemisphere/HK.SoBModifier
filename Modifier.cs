using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace SoBModifier {
    public class Modifier: MonoBehaviour {
        private bool fightStarted = false;
        private HashSet<GameObject> firstWaveMantises = new();
        private HashSet<GameObject> secondWaveMantises = new();
        private PlayMakerFSM battleSubFSM = null;
        private PlayMakerFSM battleControlFSM = null;
        private PlayMakerFSM mantisThroneMainFSM = null;
        public void Awake() {
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        public void Update() {
            if (fightStarted && mantisThroneMainFSM != null && firstWaveMantises.All(mantis => mantis == null)) {
                mantisThroneMainFSM.RemoveAction("Pause", 1);
                StartCoroutine(SendMantisDefeatedEvent());
                fightStarted = false;
            }
        }

        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);

            int numSecondWaveSpawns = SoBModifier.localSettings.numMantisesSecondWave - 3;
            int remainder = numSecondWaveSpawns % 3;

            if (self.gameObject.name == "Mantis Lord") {
                for (int _ = 0; _ < SoBModifier.localSettings.numMantisesFirstWave - 1; _++) {
                    firstWaveMantises.Add(Instantiate(self.gameObject));
                }
                firstWaveMantises.Add(self.gameObject);
                fightStarted = true;
            } else if (self.gameObject.name == "Mantis Lord S1") {
                secondWaveMantises.Add(self.gameObject);
                for (int _ = 0; _ < (remainder == 1 ? Math.Floor((double)numSecondWaveSpawns / 3) : Math.Ceiling((double)numSecondWaveSpawns / 3)); _++) {
                    GameObject newMantis = Instantiate(self.gameObject);
                    newMantis.GetComponent<HealthManager>().hp = self.gameObject.GetComponent<HealthManager>().hp;
                    newMantis.LocateMyFSM("Mantis Lord").FsmVariables.GetFsmBool("Sub").Value = false;
                    secondWaveMantises.Add(newMantis);
                }
            } else if (self.gameObject.name == "Mantis Lord S2") {
                secondWaveMantises.Add(self.gameObject);
                for (int _ = 0; _ < (remainder == 1 ? Math.Floor((double)numSecondWaveSpawns / 3) : Math.Ceiling((double)numSecondWaveSpawns / 3)); _++) {
                    GameObject newMantis = Instantiate(self.gameObject);
                    newMantis.GetComponent<HealthManager>().hp = self.gameObject.GetComponent<HealthManager>().hp;
                    newMantis.LocateMyFSM("Mantis Lord").FsmVariables.GetFsmBool("Sub").Value = false;
                    secondWaveMantises.Add(newMantis);
                }
            } else if (self.gameObject.name == "Mantis Lord S3") {
                secondWaveMantises.Add(self.gameObject);
                for (int _ = 0; _ < (remainder == 1 ? Math.Ceiling((double)numSecondWaveSpawns / 3) : Math.Floor((double)numSecondWaveSpawns / 3)); _++) {
                    GameObject newMantis = Instantiate(self.gameObject);
                    newMantis.GetComponent<HealthManager>().hp = self.gameObject.GetComponent<HealthManager>().hp;
                    newMantis.LocateMyFSM("Mantis Lord").FsmVariables.GetFsmBool("Sub").Value = false;
                    secondWaveMantises.Add(newMantis);
                }
                StartCoroutine(SetBattleEnemies());
            } else if (self.gameObject.name == "Battle Sub" && self.FsmName == "Start") {
                self.InsertAction("Amount", new CallMethod {
                    behaviour = this,
                    methodName = "SetMantisesActive",
                    parameters = new FsmVar[0],
                    everyFrame = false
                }, 0);
                battleSubFSM = self;
            } else if (self.FsmName == "Battle Control") {
                battleControlFSM = self;
            } else if (self.FsmName == "Mantis Throne Main") {
                self.AddAction("Pause", new CallMethod {
                    behaviour = this,
                    methodName = "PreventStageTransition",
                    parameters = new FsmVar[0],
                    everyFrame = false
                });
                mantisThroneMainFSM = self;
            }
        }

        private void SceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
            if (to.name != "GG_Mantis_Lords_V" || from.name == "GG_Mantis_Lords_V" && to.name == "GG_Mantis_Lords_V") {
                Modding.Logger.Log("Scene changed");
                fightStarted = false;
                firstWaveMantises = new();
                secondWaveMantises = new();
            }
        }

        public void SetMantisesActive() {
            Modding.Logger.Log("SetMantisesActive");
            int numMantisesActive = secondWaveMantises.Count(mantis => mantis != null && mantis.activeSelf == true);
            Modding.Logger.Log(numMantisesActive);
            if (battleSubFSM != null) {
                battleSubFSM.FsmVariables.GetFsmInt("Mantises Active").Value = numMantisesActive;
            }
        }

        public void PreventStageTransition() {
            if (mantisThroneMainFSM != null && FindObjectsOfType<GameObject>().Where(g => g.name.Contains("Mantis Lord") && !g.name.Contains("Throne")).ToList().Count != 0) {
                mantisThroneMainFSM.SetState("Wake");
            }
        }

        private IEnumerator SetBattleEnemies() {
            Modding.Logger.Log("SetBattleEnemies");
            yield return new WaitForSeconds(0.25f);
            if (battleControlFSM != null) {
                Modding.Logger.Log(secondWaveMantises.Count);
                battleControlFSM.FsmVariables.GetFsmInt("Battle Enemies").Value = secondWaveMantises.Count;
            }
        }

        private IEnumerator SendMantisDefeatedEvent() {
            yield return new WaitForSeconds(1.5f);
            mantisThroneMainFSM.SendEvent("MANTIS DEFEATED");
        }
    }
}
