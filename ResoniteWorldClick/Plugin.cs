using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;

namespace ResoniteWorldClick;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public static ConfigEntry<Key> Key;
    public static ConfigEntry<bool> Toggle;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        
        Key = Config.Bind("General", "Key", Renderite.Shared.Key.Alt);
        Toggle = Config.Bind("General", "Toggle", false);
        HarmonyInstance.PatchAll();
        
        Log.LogInfo($"Plugin {PluginMetadata.GUID} is loaded!");
    }
    
    [HarmonyPatch(typeof(InteractionHandler), "OnCommonUpdate")]
    public static class Inteaction
    {
        private static bool _locked;
        private static bool _lastKeyPressed;

        public static void Postfix(InteractionHandler __instance)
        {
            try
            {
                if (__instance.Slot.ActiveUser != __instance.LocalUser) return;

                bool keyPressed = Userspace.UserspaceWorld.InputInterface.GetKey(Plugin.Key.Value);

                if (Plugin.Toggle.Value)
                {
                    if (keyPressed && !_lastKeyPressed)
                    {
                        _locked = !_locked;
                        if (_locked)
                        {
                            __instance.Input.RegisterCursorUnlock(__instance.LocalUser.Root);
                        }
                        else
                        {
                            __instance.Input.UnregisterCursorUnlock(__instance.LocalUser.Root);
                        }
                    }
                }
                else
                {
                    if (keyPressed && !_locked)
                    {
                        _locked = true;
                        __instance.Input.RegisterCursorUnlock(__instance.LocalUser.Root);
                    }
                    else if (!keyPressed && _locked)
                    {
                        _locked = false;
                        __instance.Input.UnregisterCursorUnlock(__instance.LocalUser.Root);
                    }
                }

                _lastKeyPressed = keyPressed;
            }
            catch
            {
            }
        }
    }
}