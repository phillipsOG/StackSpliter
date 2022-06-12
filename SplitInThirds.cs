using ProjectM;
using HarmonyLib;
using Unity.Entities;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace StackSpliter
{
    [HarmonyPatch]
    public class SplitInThirds 
    {
        [HarmonyPatch(typeof(InventoryUtilitiesServer), "SplitItemStacks")]
        [HarmonyPostfix]
        private static bool Prefix(EntityManager entityManager, NativeHashMap<PrefabGUID, ItemData> itemDataMap, Entity inventoryOwner, int slotIndex, ref bool __result)
        {
            bool success = InventoryUtilities.TryGetInventoryEntity(entityManager, inventoryOwner, out Entity entity);

            Plugin.Logger.LogMessage("is " + success);

            InventoryUtilities.TryGetInventoryEntity(entityManager, inventoryOwner, out Entity inventory);
            DynamicBuffer<InventoryBuffer> invBuffer = entityManager.GetBufferFromEntity<InventoryBuffer>()[inventory];
            PrefabGUID itemType = invBuffer[slotIndex].ItemType;

            //int itemStacks = invBuffer[slotIndex].Stacks;
            //int remainingStacks = itemStacks / 3;
            //amt = 5 / 2 = 2.5
            
            double itemStacks = invBuffer[slotIndex].Stacks / 2;
            double newItemStacks = Math.Floor(itemStacks); //Floor(2.5) = 2
            double ceilStack = Math.Ceiling(itemStacks); //Ceil(2.5) = 2

            Plugin.Logger.LogMessage("floor amt: " + newItemStacks);
            Plugin.Logger.LogMessage("ceil amt: " + ceilStack);
            //DebugComponentTypes(entityManager.GetComponentTypes(entity));
            unsafe
            {
                var bytes = stackalloc byte[Marshal.SizeOf<FakeNull>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr<FakeNull>(new()
                {
                    value = 7,
                    has_value = true
                }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);
                var hack = new Il2CppSystem.Nullable<int>(boxedBytePtr);

                if (success == false)
                {
                    InventoryUtilitiesServer.TryRemoveItemAtIndex(entityManager, inventoryOwner, itemType, (int) itemStacks, slotIndex, default);
                    //InventoryUtilitiesServer.TryAddItem(entityManager, itemDataMap,
                    //inventoryOwner, itemType, itemStacks, out remainingStacks, out Entity e, default, hack, true, false, false);
                }
                return true;
            }
        }
         struct FakeNull
        {
            public int value;
            public bool has_value;
        }
        public static void DebugComponentTypes(NativeArray<ComponentType> nativeArray,
           [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            for (int i = 0; i < nativeArray.Length; i++)
            {
                Plugin.Logger.LogInfo($"[{caller}:{lineNumber}]: {nativeArray[i].ToString()} ({nativeArray[i].GetType()})");
            }
        }
    }
}

