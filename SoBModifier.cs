using System.Collections.Generic;
using System;
using Modding;
using Satchel.BetterMenus;
using SFCore.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using UnityEngine;

namespace SoBModifier {
    public class SoBModifier : Mod, ICustomMenuMod, ILocalSettings<LocalSettings> {
        private Menu menuRef = null;
        public static SoBModifier instance;

        public SoBModifier() : base("SoB Modifier") => instance = this;

        public static LocalSettings localSettings { get; private set; } = new();
        public void OnLoadLocal(LocalSettings s) => localSettings = s;
        public LocalSettings OnSaveLocal() => localSettings;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public bool ToggleButtonInsideMenu => false;

        public override void Initialize() {
            Log("Initializing");
            instance = this;
            ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.NewGameHook += AddModifierComponent;

            Log("Initialized");
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) {    
            menuRef ??= new Menu(
                name: "SoB Modifier",
                elements: new Element[] {
                    new CustomSlider(
                        name: "First Wave Mantis Count",
                        storeValue: val => localSettings.numMantisesFirstWave = (int)val,
                        loadValue: () => localSettings.numMantisesFirstWave,
                        minValue: 1,
                        maxValue: 40,
                        wholeNumbers: true,
                        Id: "numMantisesFirstWave"
                    ),
                    new CustomSlider(
                        name: "Second Wave Mantis Count",
                        storeValue: val => localSettings.numMantisesSecondWave = (int)val,
                        loadValue: () => localSettings.numMantisesSecondWave,
                        minValue: 3,
                        maxValue: 40,
                        wholeNumbers: true,
                        Id: "numMantisesSecondWave"
                    ),
                    new MenuButton(
                        name: "Reset To Defaults",
                        description: "",
                        submitAction: _ => ResetToDefaults()
                    )
                }
            );
            
            return menuRef.GetMenuScreen(modListMenu);
        }

        public void ResetToDefaults() {
            localSettings.numMantisesFirstWave = 1;
            localSettings.numMantisesSecondWave = 3;

            CustomSlider minNumMinionsPerWaveSlider = menuRef.Find("numMantisesFirstWave") as CustomSlider;
            CustomSlider maxNumMinionsPerWaveSlider = menuRef.Find("numMantisesSecondWave") as CustomSlider;
            minNumMinionsPerWaveSlider.Update();
            maxNumMinionsPerWaveSlider.Update();
        }

        private void AfterSaveGameLoad(SaveGameData _) {
            AddModifierComponent();
        }

        private void AddModifierComponent() {
            GameManager.instance.gameObject.AddComponent<Modifier>();
        }
    }

    public class LocalSettings {
        public int numMantisesFirstWave = 1;
        public int numMantisesSecondWave = 3;
    }
}
