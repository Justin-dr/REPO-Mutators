using Mutators.Mutators.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mutators.Utility
{
    public static class TemporaryItemUtils
    {
        public static void RemoveMarkedItems(string marker)
        {
            StatsManager statsManager = StatsManager.instance;
            foreach (string item in statsManager.item.Where(item => item.Key.Contains($"({marker})")).Select(x => x.Key).ToList())
            {
                statsManager.item.Remove(item);
                statsManager.itemStatBattery.Remove(item);
            }
        }

        public static void DropMarkedItems(string marker)
        {
            Inventory inventory = Inventory.instance;
            foreach (InventorySpot inventorySpot in inventory.inventorySpots)
            {
                ItemEquippable currentItem = inventorySpot.CurrentItem;
                if (currentItem && currentItem.gameObject.GetComponent<TemporaryLevelItemBehaviour>())
                {
                    currentItem.GetComponent<ItemEquippable>().ForceUnequip(inventory.playerAvatar.PlayerVisionTarget.VisionTransform.position, SemiFunc.IsMultiplayer() ? inventory.physGrabber.photonView.ViewID : -1);
                }
            }

            RemoveMarkedItems(marker);
        }
    }
}
