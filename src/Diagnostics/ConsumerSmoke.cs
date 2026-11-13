using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Behaviour.UI.MainMenu;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KingdomFoundation;
using KingdomFoundation.Api;
using KingdomHotkeyCustomizer.Api;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using System.Collections;
using LightJson;
using Assets.Source.Player;
using Assets.Source.Util;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace HotkeyCustomizerConsumerSmoke
{
    // Compiled as a separate consumer; never included in the player DLL/package.
    [BepInPlugin("urakka.hotkeycustomizer.consumer.smoke", "Hotkey Consumer Smoke", "1.0.0")]
    [BepInDependency(HotkeyCustomizerApi.PluginId, "1.5.0")]
    public sealed class ConsumerSmoke : BaseUnityPlugin
    {
        private void Capture(string fileName)
        {
            Canvas.ForceUpdateCanvases();
            var camera = Camera.main;
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var oldCameras = canvases.Select(c => c.worldCamera).ToArray();
            var distances = canvases.Select(c => c.planeDistance).ToArray();
            var orders = canvases.Select(c => c.sortingOrder).ToArray();
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            var previous = RenderTexture.active;
            var oldTarget = camera.targetTexture;
            int oldMask = camera.cullingMask;
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                foreach (var canvas in canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 10;
                    canvas.sortingOrder = 30000;
                }

                camera.cullingMask = -1;
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(directory, fileName), ImageConversion.EncodeToPNG(texture));
            }
            finally
            {
                camera.targetTexture = oldTarget;
                camera.cullingMask = oldMask;
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceOverlay;
                    canvases[i].worldCamera = oldCameras[i];
                    canvases[i].planeDistance = distances[i];
                    canvases[i].sortingOrder = orders[i];
                }

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Destroy(texture);
            }
        }

        private const string AlphaId = "smoke.hotkeys.alpha", BetaId = "smoke.hotkeys.beta";
        private readonly List<string> checks = new List<string>();
        private readonly List<string> errors = new List<string>();
        private ModHotkeySection alpha, beta;
        private ModHotkey alphaKey, betaKey, second;
        private IModConfigSection alphaSettings;
        private ConfigFile alphaConfig, betaConfig;
        private Keyboard keyboard;
        private FoundationConfigMenu menu;
        private OptionsUI options;
        private int alphaCalls, betaCalls;
        private bool betaAllowed;
        private string directory, nativeBindings;
        private Harmony safety;
        private float delay;
        private ModHotkeySection fault;
        private void Awake()
        {
            if (!Environment.GetCommandLineArgs().Contains("-hotkeys-consumer-smoke"))
                return;
            directory = System.IO.Path.Combine(Paths.GameRootPath, "consumer-results");
            System.IO.Directory.CreateDirectory(directory);
            safety = new Harmony("urakka.hotkeycustomizer.consumer.smoke");
            foreach (var type in new[]
            {
                typeof(global::HotkeyCustomizerConsumerSmoke.NoSave),
                typeof(global::HotkeyCustomizerConsumerSmoke.NoAutosave),
                typeof(global::HotkeyCustomizerConsumerSmoke.NoLoad),
                typeof(global::HotkeyCustomizerConsumerSmoke.NoAchievement),
                typeof(global::HotkeyCustomizerConsumerSmoke.Offline),
                typeof(global::HotkeyCustomizerConsumerSmoke.SafeApply)
            }

            )
                safety.CreateClassProcessor(type).Patch();
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.logMessageReceived += WatchErrors;
            StartCoroutine(Run());
        }

        private void Check(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(name);
            checks.Add(name);
        }

        private void WatchErrors(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
                errors.Add(message + "\n" + stack);
        }

        private void Keys(params Key[] keys)
        {
            InputSystem.EnableDevice(keyboard);
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private void Open(string modId)
        {
            MainMenuUI.Instance.ShowOptions();
            options = Resources.FindObjectsOfTypeAll<OptionsUI>().First(ui => ui.gameObject.activeInHierarchy);
            menu = options.GetComponent<FoundationConfigMenu>();
            menu.ShowModsTab(modId);
        }

        private Button Button(string modId, string name)
        {
            var root = menu.GetComponentsInChildren<Transform>().Single(t => t.name == modId + ".hotkeys");
            return root.GetComponentsInChildren<Button>().Single(button => button.name == name);
        }

        private void ClickKey(string modId, string key)
        {
            Button(modId, "ModHotkey_Hotkeys_" + key).onClick.Invoke();
        }

        private void Reset(string modId)
        {
            var root = menu.GetComponentsInChildren<Transform>().Single(t => t.name == modId + ".hotkeys");
            root.GetComponentsInChildren<Button>().Single(b => b.GetComponentInChildren<TMPro.TMP_Text>().text == "Reset Hotkeys").onClick.Invoke();
        }

        private string Stored(ConfigFile config, string key)
        {
            return new ConfigFile(config.ConfigFilePath, false).Bind("Hotkeys", key, "missing").Value;
        }

        private void Reject(Action action, string name)
        {
            bool rejected = false;
            try
            {
                action();
            }
            catch (ArgumentException)
            {
                rejected = true;
            }

            Check(rejected, name);
        }

        private IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(5);
            string error = null;
            foreach (var step in Scenario())
            {
                delay = .08f;
                try
                {
                    step();
                }
                catch (Exception ex)
                {
                    error = ex.ToString();
                }

                if (error != null)
                    break;
                yield return new WaitForSecondsRealtime(delay);
            }

            Time.timeScale = 1;
            alpha?.Dispose();
            beta?.Dispose();
            fault?.Dispose();
            if (keyboard != null)
                InputSystem.RemoveDevice(keyboard);
            var result = new JsonObject
            {
                {
                    "passed",
                    error == null && errors.Count == 0
                },
                {
                    "version",
                    KingdomHotkeyCustomizer.HotkeyCustomizerPlugin.Version
                },
                {
                    "checks",
                    new JsonArray(checks.Select(c => (JsonValue)c).ToArray())
                },
                {
                    "count",
                    checks.Count
                },
                {
                    "errors",
                    new JsonArray(errors.Select(c => (JsonValue)c).ToArray())
                },
                {
                    "error",
                    error ?? ""
                },
                {
                    "dllSha256",
                    Hash(typeof(KingdomHotkeyCustomizer.HotkeyCustomizerPlugin).Assembly.Location)
                },
                {
                    "foundationSha256",
                    Hash(typeof(KingdomFoundation.Api.FoundationApi).Assembly.Location)
                },
                {
                    "gameVersion",
                    Application.version
                }
            };
            File.WriteAllText(Path.Combine(directory, "result.json"), result.ToString());
            Application.logMessageReceived -= WatchErrors;
            Application.Quit();
        }

        private static string Hash(string path)
        {
            using (var hash = System.Security.Cryptography.SHA256.Create())
            using (var file = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
        }

        private IEnumerable<Action> Scenario()
        {
            yield return () =>
            {
                Check(GamePlayer.Current == null && !SaveGame.LoadLatestSave(), "Main menu uses no saved kingdom; load guard installed");
                keyboard = InputSystem.AddDevice<Keyboard>();
                Keys();
                nativeBindings = GameInput.Asset.SaveBindingOverridesAsJson();
                alphaConfig = new ConfigFile(Path.Combine(directory, "alpha.cfg"), true);
                betaConfig = new ConfigFile(Path.Combine(directory, "beta.cfg"), true);
                var existing = alphaConfig.Bind("Hotkeys", "Action", "Ctrl+F6");
                alphaSettings = FoundationApi.ModConfig.GetOrCreateSection(AlphaId, "Consumer Alpha");
                alphaSettings.AddText("Existing consumer setting");
                alpha = HotkeyCustomizerApi.Register(AlphaId, "Consumer Alpha", alphaConfig);
                alphaKey = alpha.Add(existing, "Alpha action", () => alphaCalls++);
                second = alpha.Add("Second", "Settings-only binding");
                beta = HotkeyCustomizerApi.Register(BetaId, "Consumer Beta", betaConfig);
                betaKey = beta.Add("Action", "Beta action", () => betaCalls++, canExecute: () => betaAllowed);
                Check(Stored(alphaConfig, "Action") == "Ctrl+F6" && second.Binding == "" && betaKey.Binding == "", "Registration preserves saved entries and starts unspecified keys blank");
                Check(alphaSettings.Elements.Count == 2 && FoundationApi.ModConfig.TryGetSection(BetaId, out _), "Each consumer has its own settings section; existing controls retained");
                Reject(() => HotkeyCustomizerApi.Register(AlphaId, "Duplicate", alphaConfig), "Duplicate mod registration rejected");
                Reject(() => alpha.Add("Action", "Duplicate"), "Duplicate hotkey rejected");
                Reject(() => alpha.Add(betaKey.Entry, "Wrong owner"), "Cross-file entry registration rejected");
                Reject(() => alpha.Add("Bad", "Invalid", defaultBinding: "Ctrl+notakey"), "Malformed default rejected");
                Open(AlphaId);
            };
            yield return () =>
            {
                Check(menu.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Hotkeys"), "Consumer section renders native-themed Hotkeys controls");
                ClickKey(AlphaId, "Action");
            };
            yield return () => Keys(Key.RightCtrl, Key.RightShift, Key.F8);
            yield return () =>
            {
                Check(alphaKey.Binding == "Ctrl+F6" && Stored(alphaConfig, "Action") == "Ctrl+F6", "Capture remains pending without saving");
                Check(Button(AlphaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Ctrl + Shift + F8", "Capture handles multiple right-side modifiers");
                Keys();
                menu.SelectMod(BetaId);
            };
            yield return () => ClickKey(BetaId, "Action");
            yield return () => Keys(Key.LeftAlt, Key.F9);
            yield return () =>
            {
                Keys();
                menu.SelectMod(AlphaId);
            };
            yield return () =>
            {
                Check(Button(AlphaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Ctrl + Shift + F8", "Switching consumer sections preserves pending captures");
                var late = HotkeyCustomizerApi.Register("smoke.hotkeys.late", "Late consumer", new ConfigFile(Path.Combine(directory, "late.cfg"), true));
                late.Add("Later", "Late hotkey");
                Check(Button(AlphaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Ctrl + Shift + F8", "Late registration preserves open pending edits");
                late.Dispose();
                VerifySidebar();
                Check(GameInput.Asset.SaveBindingOverridesAsJson() == nativeBindings, "Consumer edits do not change native bindings");
                Capture("consumer-hotkeys.png");
                alphaConfig.SaveOnConfigSet = false;
                options.ApplyAndClose();
            };
            yield return () =>
            {
                Check(Stored(alphaConfig, "Action") == "Ctrl+Shift+F8" && Stored(betaConfig, "Action") == "Alt+F9", "Native Apply saves each consumer into its own file");
                Check(!alphaConfig.SaveOnConfigSet, "Apply preserves the consumer's automatic-save policy");
                alphaConfig.SaveOnConfigSet = true;
                Check(alphaCalls == 0 && betaCalls == 0, "Callbacks do not run while assigning in menus");
                Open(BetaId);
                ClickKey(BetaId, "Action");
            };
            yield return () => Keys(Key.Escape);
            yield return () =>
            {
                Keys();
                Check(options.gameObject.activeInHierarchy && Button(BetaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Alt + F9", "Escape cancels capture and keeps settings open");
                ClickKey(BetaId, "Action");
            };
            yield return () => Keys(Key.Backspace);
            yield return () =>
            {
                Check(Button(BetaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Unassigned" && betaKey.Binding == "Alt+F9", "Backspace clears only the pending consumer binding");
                Keys();
                menu.SelectMod(AlphaId);
                Reset(AlphaId);
                options.CancelOptions();
            };
            yield return () =>
            {
                Check(Stored(alphaConfig, "Action") == "Ctrl+Shift+F8" && Stored(betaConfig, "Action") == "Alt+F9", "Cancel discards clears and defaults in all consumer sections");
                Open(AlphaId);
                Reset(AlphaId);
                options.ApplyAndClose();
            };
            yield return () =>
            {
                Check(alphaKey.Binding == "Ctrl+F6" && betaKey.Binding == "Alt+F9", "Apply defaults affects only the selected consumer");
                second.Dispose();
                second.Dispose();
                Check(!second.IsRegistered, "A single binding can be removed idempotently");
                Open(BetaId);
                ClickKey(BetaId, "Action");
            };
            yield return () => Keys(Key.Delete);
            yield return () =>
            {
                Keys();
                options.ApplyAndClose();
            };
            yield return () =>
            {
                Check(betaKey.Binding == "" && Stored(betaConfig, "Action") == "", "Delete and Apply persist an unassigned binding");
                betaKey.Entry.Value = "Alt+F9";
                alphaKey.Entry.Value = "malformed";
                Open(AlphaId);
            };
            yield return () =>
            {
                Check(Button(AlphaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Unassigned", "Malformed saved binding safely displays Unassigned");
                alphaKey.Entry.Value = "Ctrl+F6";
            };
            yield return () =>
            {
                Check(Button(AlphaId, "ModHotkey_Hotkeys_Action").GetComponentInChildren<TMP_Text>().text == "Ctrl + F6", "External config changes update an unedited control");
                options.CancelOptions();
                GamePlayer.CreateNewPlayer();
                SceneManager.LoadScene("Game");
                delay = 4;
            };
            yield return () =>
            {
                Check(!GamePlayer.Current.HasTech(KingdomHotkeyCustomizer.HotkeyCustomizerPlugin.ResearchId), "Consumer hotkeys do not inherit this mod's building research gate");
                Keys(Key.F6);
            };
            yield return () =>
            {
                Check(alphaCalls == 0, "Plain key does not fire a modifier combination");
                Keys();
            };
            yield return () => Keys(Key.LeftCtrl, Key.F6);
            yield return () => Check(alphaCalls == 1, "Consumer callback executes the saved combination during gameplay");
            yield return () =>
            {
                Check(alphaCalls == 1, "Held combination does not repeat every frame");
                Keys();
            };
            yield return () => Keys(Key.RightCtrl, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 2, "Gameplay accepts right-side modifiers");
                Keys();
            };
            yield return () => Keys(Key.LeftCtrl, Key.LeftShift, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 2, "Extra modifiers do not trigger the consumer callback");
                Keys();
            };
            yield return () => Keys(Key.LeftAlt, Key.F9);
            yield return () =>
            {
                Check(betaCalls == 0, "Consumer predicate can withhold its own feature");
                betaAllowed = true;
                Keys();
            };
            yield return () => Keys(Key.LeftAlt, Key.F9);
            yield return () =>
            {
                Check(betaCalls == 1, "Consumer predicate enables its own binding");
                Keys();
                Time.timeScale = 0;
            };
            yield return () => Keys(Key.LeftCtrl, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 2, "Pause suppresses consumer callbacks");
                Time.timeScale = 1;
                Keys();
                var text = new GameObject("Smoke text entry", typeof(RectTransform), typeof(TMP_InputField));
                EventSystem.current.SetSelectedGameObject(text);
            };
            yield return () => Keys(Key.LeftCtrl, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 2, "Selected text input suppresses consumer callbacks");
                global::UnityEngine.Object.Destroy(EventSystem.current.currentSelectedGameObject);
                EventSystem.current.SetSelectedGameObject(null);
                Keys();
                fault = HotkeyCustomizerApi.Register("smoke.hotkeys.fault", "Fault consumer", new ConfigFile(Path.Combine(directory, "fault.cfg"), true));
                fault.Add("Throw", "Faulty handler", () =>
                {
                    throw new InvalidOperationException("Expected consumer failure");
                }, "Ctrl+F6");
            };
            yield return () => Keys(Key.LeftCtrl, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 3 && errors.Count == 0, "A failing consumer callback does not interrupt other consumers");
                fault.Dispose();
                Keys();
                fault = HotkeyCustomizerApi.Register("smoke.hotkeys.once", "One-shot consumer", new ConfigFile(Path.Combine(directory, "once.cfg"), true));
                fault.Add("Once", "Dispose during callback", () => fault.Dispose(), "Ctrl+F6");
            };
            yield return () => Keys(Key.LeftCtrl, Key.F6);
            yield return () =>
            {
                Check(alphaCalls == 4 && !fault.IsRegistered, "A consumer can unregister inside its callback safely");
                Keys();
            };
            yield return () =>
            {
                beta.Dispose();
                beta.Dispose();
                Check(!betaKey.IsRegistered && Stored(betaConfig, "Action") == "Alt+F9", "Disposal removes callbacks and keeps the saved config");
                Check(!FoundationApi.ModConfig.TryGetSection(BetaId, out _), "Empty owned consumer section is removed");
                Keys(Key.LeftAlt, Key.F9);
            };
            yield return () =>
            {
                Check(betaCalls == 1, "Unregistered consumer no longer receives keys");
                alpha.Dispose();
                Check(alphaSettings.Elements.Count == 1, "Disposal preserves the consumer's unrelated settings");
                beta = HotkeyCustomizerApi.Register(BetaId, "Consumer Beta", betaConfig);
                betaKey = beta.Add("Action", "Restored action");
                Check(betaKey.Binding == "Alt+F9", "Re-registering restores saved bindings");
                Check(errors.Count == 0, "Consumer UI and gameplay produce no Unity errors");
            };
        }

        private void VerifySidebar()
        {
            var content = (RectTransform)AccessTools.Field(typeof(FoundationConfigMenu), "modsSidebarContent").GetValue(menu);
            var scroll = (ScrollRect)AccessTools.Field(typeof(FoundationConfigMenu), "modsSidebarScroll").GetValue(menu);
            Canvas.ForceUpdateCanvases();
            var buttons = content.GetComponentsInChildren<Button>();
            var first = (RectTransform)buttons[0].transform;
            var top = scroll.viewport.InverseTransformPoint(first.TransformPoint(new Vector3(0, first.rect.yMax, 0)));
            float gap = scroll.viewport.rect.yMax - top.y;
            Check(gap >= -1 && gap <= 8, "Mod sidebar starts directly below its heading after repeated registration changes (gap=" + gap + ")");
            Check(buttons.Length == KingdomFoundation.Api.FoundationApi.ModConfig.Sections.Count, "Sidebar layout contains exactly one active button per registered section");
        }
    }
}
