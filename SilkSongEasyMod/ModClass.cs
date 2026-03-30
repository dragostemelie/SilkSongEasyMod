using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;

namespace SilkSongEasyMod
{
    [BepInPlugin("com.dragostemelie.easymod", "SilkSongEasyMod", "1.0.0")]
    public class SilkSongEasyMod : BaseUnityPlugin
    {
        // Config entries
        private static ConfigEntry<bool> cfgDoubleGeo;
        private static ConfigEntry<bool> cfgDoubleShards;
        private static ConfigEntry<bool> cfgDoubleSilk;
        private static ConfigEntry<bool> cfgLimitDamage;
        private static ConfigEntry<int> cfgMaxDamageTaken;
        private static ConfigEntry<bool> cfgAlwaysShowCompass;
        private static ConfigEntry<float> cfgDamageMultiplier;
        private static ConfigEntry<bool> cfgRefillSilkOnBench;
        private static ConfigEntry<bool> cfgAutoMagnetPickup;
        private static ConfigEntry<bool> cfgMagnetVisualEffect;

        private void Awake()
        {
            // Config bindings
            cfgDoubleGeo = Config.Bind("Economy", "DoubleGeo", true, "Double Geo gained.");
            cfgDoubleShards = Config.Bind("Economy", "DoubleShards", true, "Double Shards gained.");
            cfgDoubleSilk = Config.Bind("Economy", "DoubleSilk", true, "Double Silk gained.");

            cfgLimitDamage = Config.Bind("Player", "LimitDamage", true, "Limit incoming damage.");
            cfgMaxDamageTaken = Config.Bind("Player", "MaxDamageTaken", 1, "Maximum damage the player can take from one hit.");

            cfgAlwaysShowCompass = Config.Bind("Map", "AlwaysShowCompass", true, "Always show compass on the map.");
            cfgDamageMultiplier = Config.Bind("Combat", "DamageMultiplier",2f,"Multiplier applied to outgoing damage.");
            cfgRefillSilkOnBench = Config.Bind("Player", "RefillSilkOnBench", true, "Refill missing silk when resting on a bench.");
            cfgAutoMagnetPickup = Config.Bind("Pickup", "AutoMagnetPickup", true, "Make rosaries/shards behave as if magnet tool is equipped." );
            cfgMagnetVisualEffect = Config.Bind( "Pickup", "MagnetVisualEffect", true, "Show magnet visual effect on rosaries/shards.");

            var harmony = new Harmony("com.dragostemelie.easymod");
            harmony.PatchAll(typeof(SilkSongEasyMod));

            Logger.LogInfo("Mod initialized");
        }

        [HarmonyPatch(typeof(PlayerData), "AddGeo")]
        [HarmonyPrefix]
        private static void DoubleGeoPrefix(ref int amount)
        {
            if (cfgDoubleGeo.Value)
                amount *= 2;
        }

        [HarmonyPatch(typeof(PlayerData), "AddShards")]
        [HarmonyPrefix]
        private static void DoubleShardsPrefix(ref int amount)
        {
            if (cfgDoubleShards.Value)
                amount *= 2;
        }

        [HarmonyPatch(typeof(PlayerData), "AddSilk")]
        [HarmonyPrefix]
        private static void DoubleSilkPrefix(ref int amount)
        {
            if (cfgDoubleSilk.Value)
                amount *= 2;
        }

        [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
        [HarmonyPrefix]
        private static void LimitDamagePrefix(ref int amount)
        {
            if (!cfgLimitDamage.Value)
                return;

            if (amount > cfgMaxDamageTaken.Value)
                amount = cfgMaxDamageTaken.Value;
        }

        [HarmonyPatch(typeof(GameMap), "PositionCompassAndCorpse")]
        [HarmonyPostfix]
        private static void AlwaysShowCompassPostfix(object __instance)
        {
            if (!cfgAlwaysShowCompass.Value)
                return;

            var compassIconField = __instance.GetType().GetField("compassIcon", BindingFlags.NonPublic | BindingFlags.Instance);
            var compassIcon = compassIconField?.GetValue(__instance) as UnityEngine.GameObject;
            if (compassIcon != null)
            {
                compassIcon.SetActive(true);
            }

            var displayingCompassField = __instance.GetType().GetField("displayingCompass", BindingFlags.NonPublic | BindingFlags.Instance);
            if (displayingCompassField != null)
            {
                displayingCompassField.SetValue(__instance, true);
            }
        }

        [HarmonyPatch(typeof(HealthManager), "TakeDamage")]
        [HarmonyPrefix]
        private static void DoubleDamagePrefix(ref HitInstance hitInstance)
        {
            if (cfgDamageMultiplier.Value <= 0f)
                return;

            hitInstance.DamageDealt = (int)(hitInstance.DamageDealt * cfgDamageMultiplier.Value);
        }


        [HarmonyPatch(typeof(RestBenchHelper), "SetOnBench")]
        [HarmonyPostfix]
        public static void SetOnBenchPostfix(bool onBench)
        {
            if (!cfgRefillSilkOnBench.Value || !onBench)
                return;

            var playerData = GameManager.instance.playerData;
            HeroController controll = HeroController.instance;
            int missingSilk = playerData.CurrentSilkMax - playerData.silk;

            if (missingSilk > 0)
            {
                controll.AddSilk(missingSilk, false);
            }
        }

        [HarmonyPatch(typeof(CurrencyObjectBase), "MagnetToolIsEquipped")]
        [HarmonyPostfix]
        public static void MagnetToolIsEquipped_Postfix(CurrencyObjectBase __instance, ref bool __result)
        {
            if (!cfgAutoMagnetPickup.Value)
                return;

            if (IsRosaryOrShard(__instance))
            {
                __result = true;
            }
        }

        private static bool IsRosaryOrShard(object o)
        {
            if (o == null) return false;
            string name = o.GetType().Name;
            return name.Contains("Rosary") || name.Contains("Shard") || name.Contains("Geo");
        }

        [HarmonyPatch(typeof(CurrencyObjectBase), "OnEnable")]
        [HarmonyPostfix]
        public static void CurrencyObjectBase_OnEnable_Postfix(CurrencyObjectBase __instance)
        {
            if (!cfgMagnetVisualEffect.Value)
                return;

            if (__instance == null) return;
            string typeName = __instance.GetType().Name;
            if (!(typeName.Contains("Rosary") || typeName.Contains("Shard"))) return;

            var magnetEffectField = typeof(CurrencyObjectBase).GetField("magnetEffect", BindingFlags.Instance | BindingFlags.NonPublic);
            var hasMagnetEffectField = typeof(CurrencyObjectBase).GetField("hasMagnetEffect", BindingFlags.Instance | BindingFlags.NonPublic);
            var effect = magnetEffectField?.GetValue(__instance) as GameObject;
            if (effect != null) return;

            if (_magnetTemplate == null)
            {
                foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
                {
                    if (t != null && t.name == "rosary_magnet_effect")
                    {
                        _magnetTemplate = t.gameObject;
                        break;
                    }
                }
                if (_magnetTemplate == null) return;
            }

            GameObject visual = Object.Instantiate(_magnetTemplate, ((Component)__instance).transform);
            visual.name = "rosary_magnet_effect (shard/rosary)";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = _magnetTemplate.transform.localScale;
            visual.SetActive(false);

            magnetEffectField?.SetValue(__instance, visual);
            hasMagnetEffectField?.SetValue(__instance, true);
        }

        private static GameObject _magnetTemplate;

    }
}

