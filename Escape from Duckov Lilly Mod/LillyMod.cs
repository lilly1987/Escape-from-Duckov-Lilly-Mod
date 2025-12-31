using Duckov.ItemBuilders;
using Duckov.Modding;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            public int DefaultCapacity { get; set; } = 512;
            public float InventoryCapacity { get; set; } = 4f;
            public int MaxStackCount { get; set; } = 0;
            public float MaxDurability { get; set; } = 2000000000;
            public float WeightMultiplier { get; set; } = 0f;

            public override string ToString()
            {
                return string.Join(", ",
                    $"{nameof(DefaultCapacity)}={DefaultCapacity}",
                    $"{nameof(InventoryCapacity)}={InventoryCapacity}"
                    );

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


        [HarmonyPatch(typeof(PlayerStorage))]
        [HarmonyPatch("DefaultCapacity", MethodType.Getter)]
        public class PlayerStorage_DefaultCapacity_Patch
        {
            static void Postfix(ref int __result)
            {
                // 원래 getter가 반환한 값(__result)에 1024를 더해줌
                __result += config.DefaultCapacity;
            }
        }

        [HarmonyPatch(typeof(ItemStatsSystem.ModifierDescription))]
        [HarmonyPatch("CreateModifier")]
        public class ModifierDescription_CreateModifier_Patch
        {
            static void Postfix(object source, ref Modifier __result, ItemStatsSystem.ModifierDescription __instance)
            {
                // key가 "InventoryCapacity"일 경우 결과를 새 Modifier로 교체
                if (__instance.Key == "InventoryCapacity")
                {
                    //__result = new Modifier(
                    //    __instance.Type,
                    //    __instance.Value * 8f,
                    //    __instance.IsOverrideOrder,
                    //    __instance.Order,
                    //    source
                    //);
                    __result.Value *= config.InventoryCapacity;
                }
            }
        }

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

        [HarmonyPatch(typeof(Item))] // 실제 클래스 이름으로 교체하세요
        public static class Item_Patch
        {
            [HarmonyPatch("MaxStackCount", MethodType.Getter)]
            [HarmonyPostfix]
            static void Stackable_Postfix(ref int __result, Item __instance)
            {
                // 항상 큰 값으로 덮어쓰기 (예: int.MaxValue)
                //__result = int.MaxValue;
                if (__instance.GetBool("IsBullet",false)|| __instance.Tags.Contains("Cash"))
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
            static bool MaxDurability_Prefix(Item __instance, ref float __result)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                __result = config.MaxDurability;
                return false; // 원래 메서드 실행 안함
            }

            [HarmonyPatch("Durability", MethodType.Setter)]
            [HarmonyPrefix]
            static void Durability_Prefix(Item __instance, ref float value)
            {
                // setter에 전달된 value 인수를 강제로 MaxDurability로 바꿈
                value = __instance.MaxDurability;
            }



        }


    }
}
