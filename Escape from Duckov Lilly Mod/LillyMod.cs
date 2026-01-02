using Duckov.Buffs;
using Duckov.ItemBuilders;
using Duckov.Modding;
using ECM2.Examples.SlopeSpeedModifier;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Escape_from_Duckov_Lilly_Mod
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Harmony harmony;
        private static bool patchesApplied;
        private static readonly string harmonyName= "com.Lilly.Mod";

        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();
            try
            {
                if (patchesApplied)
                {
                    Debug.Log("이미 적용됨");
                }
                else
                {
                    this.harmony = new Harmony(harmonyName);
                    this.harmony.PatchAll();
                    patchesApplied = true;
                    Debug.Log("적용됨");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("적용 오류:" + ((ex != null) ? ex.ToString() : null));
            }
            try
            {
                GetConfig();
            }
            catch (Exception ex)
            {
                Debug.LogError("Config 오류:" + ((ex != null) ? ex.ToString() : null));
            }
        }

        protected override void OnBeforeDeactivate()
        {
            base.OnBeforeDeactivate();
            try
            {
                if (this.harmony != null)
                {
                    this.harmony.UnpatchAll(harmonyName);
                    this.harmony = null;
                    patchesApplied = false;
                    Debug.Log("해제됨");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("해제 오류:" + ((ex != null) ? ex.ToString() : null));
            }
        }

        public class Config
        {
            public int PlayerStorageDefaultCapacity { get; set; } = 1024;
            public int PetCapcity { get; set; } = 8;
            public float InventoryCapacity { get; set; } = 250f;
            public float WeightMultiplier { get; set; } = 0f;
            public int MaxStackCount { get; set; } = 2000000000;
            public float MaxDurability { get; set; } = 2000000000f;
            public float MaxWeight { get; set; } = 2000000000f;
            public float MaxWater { get; set; } = 500000000f;
            public float MaxStamina { get; set; } = 500000000f;
            public float MaxEnergy { get; set; } = 500000000f;
            public float MaxHealth { get; set; } = 2000000000f;
            public float CharacterRunSpeed { get; set; } = 8f;
            public float CharacterWalkSpeed { get; set; } = 2f;
            public float Buff_totalLifeTime { get; set; } = 64f;
            public float Debuff_totalLifeTime { get; set; } = 64f;

            public override string ToString()
            {
                return string.Join(", ",
                    GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(this)}"));
            }
        }

        static Config config=new Config() ;

        static void GetConfig()
        {
            // 실행 파일 위치 기준으로 config.yml 경로 생성
            string configPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "config.yml"
            );

            if (!File.Exists(configPath))
            {
                Console.WriteLine("config.yml 파일을 찾을 수 없습니다.");
                return;
            }

            // 파일 읽기
            string yamlContent = File.ReadAllText(configPath);

            // YAML 파서 생성
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance) // key 이름 매핑 규칙
                .Build();

            // YAML → 객체 변환
            config = deserializer.Deserialize<Config>(yamlContent);

            // 값 출력
            Console.WriteLine($"config: {config.ToString()}");
        }

        /// <summary>
        /// 팻 가방
        /// </summary>
        [HarmonyPatch(typeof(CharacterMainControl))] // 실제 클래스 이름으로 교체하세요
        public static class CharacterMainControl_Patch
        {

            [HarmonyPatch("PetCapcity", MethodType.Getter)]
            [HarmonyPostfix]
            public static void PetCapcity_Postfix(ref int __result)
            {
                // 항상 0으로 덮어쓰기
                __result = config.PetCapcity;
            }

            [HarmonyPatch("CharacterRunSpeed", MethodType.Getter)]
            [HarmonyPostfix]
            public static void CharacterRunSpeed(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                    // 원래 반환값(__result)을 두 배로 변경
                    __result *= config.CharacterRunSpeed;
            }

            [HarmonyPatch("CharacterWalkSpeed", MethodType.Getter)]
            [HarmonyPostfix]
            public static void CharacterWalkSpeed(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                    // 원래 반환값(__result)을 두 배로 변경
                    __result *= config.CharacterWalkSpeed;
            }

            [HarmonyPatch("MaxEnergy", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool MaxEnergy(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                {
                    __result = config.MaxEnergy;
                    return false; // 원래 메서드 실행 안함
                }
                return true;
            }

            [HarmonyPatch("MaxStamina", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool MaxStamina(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                {
                    __result = config.MaxStamina;
                    return false; // 원래 메서드 실행 안함
                }
                return true;
            }

            [HarmonyPatch("MaxWater", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool MaxWater(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                {
                    __result = config.MaxWater;
                    return false; // 원래 메서드 실행 안함
                }
                return true;
            }

            [HarmonyPatch("MaxWeight", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool MaxWeight(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                {
                    __result = config.MaxWeight;
                    return false; // 원래 메서드 실행 안함
                }
                return true;
            }

            [HarmonyPatch("InventoryCapacity", MethodType.Getter)]
            [HarmonyPrefix]
            public static bool InventoryCapacity(ref float __result, CharacterMainControl __instance)
            {
                if (__instance.IsMainCharacter)
                {
                    __result = config.InventoryCapacity;
                    return false; // 원래 메서드 실행 안함
                }
                return true;
            }

            // AddBuff 메서드 패치
            [HarmonyPatch("AddBuff")]
            [HarmonyPrefix] // 실행 전에 개입
            public static void Prefix(Buff buffPrefab, CharacterMainControl fromWho, int overrideWeaponID, CharacterMainControl __instance)
            {
                // 예: 특정 Buff을 막거나 로그 출력
                if (__instance.IsMainCharacter)
                {
                    //return false; // 원래 메서드 실행을 막음
                }
                //return true; // true면 원래 메서드 계속 실행
            }

        }

        private static readonly HashSet<string> Debuff = new HashSet<string>
        {
            "Buff_BleedS", // Food
        };

        [HarmonyPatch(typeof(Buff))] // 실제 대상 클래스가 Setup을 가진 클래스명으로 교체 필요
        public static class Buff_Patch
        {
            // Setup 메서드 실행 전에 개입
            [HarmonyPatch("Setup")]
            [HarmonyPrefix]
            public static void Setup_Prefix(CharacterBuffManager manager, Buff __instance, ref float ___totalLifeTime)
            {
                //Debug.Log($"[Harmony] Setup called on {__instance} with manager={manager}");
                // 여기서 manager를 조작하거나 조건부로 원래 메서드 실행을 막을 수 있음
                // return false; → 원래 메서드 실행 차단
                if (manager.Master.IsMainCharacter)
                {
                    if (__instance.DisplayNameKey.Contains("Debuff")|| Debuff.Contains(__instance.DisplayNameKey))
                        ___totalLifeTime /= config.Debuff_totalLifeTime;
                    else
                        ___totalLifeTime *= config.Buff_totalLifeTime;
                }
            }

            // Setup 메서드 실행 후 개입
            //[HarmonyPatch("Setup")]
            //[HarmonyPostfix]
            //public static void Postfix(CharacterBuffManager manager, object __instance)
            //{
            //    Debug.Log($"[Harmony] Setup finished on {__instance} with manager={manager}");
            //    // 후처리 로직 추가 가능
            //}
        }


        /// <summary>
        /// 창고
        /// </summary>
        [HarmonyPatch(typeof(PlayerStorage))]
        [HarmonyPatch("DefaultCapacity", MethodType.Getter)]
        public class PlayerStorage_DefaultCapacity_Patch
        {
            public static void Postfix(ref int __result)
            {
                // 원래 getter가 반환한 값(__result)에 1024를 더해줌
                __result += config.PlayerStorageDefaultCapacity;
            }
        }

        /// <summary>
        /// 플레이어?
        /// </summary>
        //[HarmonyPatch(typeof(ItemStatsSystem.ModifierDescription))]
        //[HarmonyPatch("CreateModifier")]
        //public class ModifierDescription_CreateModifier_Patch
        //{
        //    static void Postfix(object source, ref Modifier __result, ItemStatsSystem.ModifierDescription __instance)
        //    {
        //        // key가 "InventoryCapacity"일 경우 결과를 새 Modifier로 교체
        //        if (__instance.Key == "InventoryCapacity")
        //        {
        //            //__result = new Modifier(
        //            //    __instance.Type,
        //            //    __instance.Value * 8f,
        //            //    __instance.IsOverrideOrder,
        //            //    __instance.Order,
        //            //    source
        //            //);
        //            __result.Value *= config.InventoryCapacity;
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(ItemBuilder))]              // 패치할 클래스 지정
        [HarmonyPatch("Instantiate")]             // 대상 메서드 이름
        public class ItemBuilder_Instantiate_Patch
        {
            // 메서드 실행 전에 로그 찍기
            static void Prefix()
            {
                Debug.Log("[Harmony] ItemBuilder.Instantiate() 호출됨 (Prefix)");
            }
        }


        private static readonly HashSet<string> SpecialNamesNot = new HashSet<string>
        {
            "Item_CocoMilk", // Food
        };

        private static readonly HashSet<string> SpecialNames = new HashSet<string>
        {
            "Item_Dynamite",
            "Item_FlamingCore",
            "Item_ColdCore",
            "Item_Chocalate", // Food
            "Item_EnergyStick", // Food
            "Item_Honey", // Food
            "Item_Yogurt", // Food
            "Item_Feather"
        };

        private static readonly List<Regex> SpecialNamePatterns = new List<Regex>
        {
            new Regex(@"^Item_Injection_.*$", RegexOptions.Compiled)
        };


        [HarmonyPatch(typeof(Item))] // 실제 클래스 이름으로 교체하세요
        public static class Item_Patch
        {
            [HarmonyPatch("MaxStackCount", MethodType.Getter)]
            [HarmonyPostfix]
            static void Stackable_Postfix(ref int __result, Item __instance)
            {
                // 항상 큰 값으로 덮어쓰기 (예: int.MaxValue)
                //__result = int.MaxValue;
                if (
                    //! __instance.Tags.Contains("Equipment") &&
                    //! SpecialNamesNot.Contains(__instance.DisplayNameRaw) && 
                    //(__instance.GetBool("IsBullet",false)
                    //|| __instance.Tags.Contains("Cash")
                    //|| __instance.Tags.Contains("Food")
                    //|| SpecialNames.Contains(__instance.DisplayNameRaw)
                    //|| SpecialNamePatterns.Any(r => r.IsMatch(__instance.DisplayNameRaw))
                    //)
                    __instance.GetVariableEntry("Count")!=null
                    )
                    __result = config.MaxStackCount;
            }

            // UnitSelfWeight getter 패치
            [HarmonyPatch("UnitSelfWeight", MethodType.Getter)]
            [HarmonyPostfix]
            static void UnitSelfWeight_Postfix(ref float __result)
            {
                __result *= config.WeightMultiplier;
            }

            // SelfWeight getter 패치
            [HarmonyPatch("SelfWeight", MethodType.Getter)]
            [HarmonyPostfix]
            static void SelfWeight_Postfix(ref float __result)
            {
                __result *= config.WeightMultiplier;
            }

            [HarmonyPatch("MaxDurability", MethodType.Setter)]
            [HarmonyPrefix]
            static void MaxDurability_Setter(Item __instance, ref float value)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                value = config.MaxDurability;
                //return false; // 원래 메서드 실행 안함
            }

            [HarmonyPatch("MaxDurability", MethodType.Getter)]
            [HarmonyPrefix]
            static bool MaxDurability_Getter(Item __instance, ref float __result)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                __result = config.MaxDurability;
                return false; // 원래 메서드 실행 안함
            }

            [HarmonyPatch("Durability", MethodType.Setter)]
            [HarmonyPrefix]
            static void Durability_Setter(Item __instance, ref float value)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                value = __instance.MaxDurability;
            }

            [HarmonyPatch("Durability", MethodType.Getter)]
            [HarmonyPrefix]
            static bool Durability_Getter(Item __instance, ref float __result)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                __result = __instance.MaxDurability;
                return false; // 원래 메서드 실행 안함
            }
        }

        [HarmonyPatch(typeof(Health))]
        public static class HealthPatch
        {
            // MaxHealth의 getter를 패치
            [HarmonyPatch("MaxHealth", MethodType.Getter)]
            [HarmonyPrefix] // getter 실행 후 결과를 수정하거나 확인
            public static bool MaxHealth(ref float __result, Health __instance)
            {
                if (__instance.IsMainCharacterHealth)
                {
                    __result = config.MaxHealth;
                    return false; // 원래 메서드 실행 안함
                }
                return true; // 원래 메서드 실행
            }

            // MaxHealth의 getter를 패치
            [HarmonyPatch("CurrentHealth", MethodType.Getter)]
            [HarmonyPrefix] // getter 실행 후 결과를 수정하거나 확인
            public static bool CurrentHealth(ref float __result, Health __instance)
            {
                if (__instance.IsMainCharacterHealth)
                {
                    __result = config.MaxHealth;
                    return false; // 원래 메서드 실행 안함
                }
                return true; // 원래 메서드 실행
            }

            [HarmonyPatch("CurrentHealth", MethodType.Setter)]
            [HarmonyPrefix]
            public static void CurrentHealth_Setter(Health __instance, ref float value)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                if (__instance.IsMainCharacterHealth)
                    value = __instance.MaxHealth;
            }
        }


    }
}
