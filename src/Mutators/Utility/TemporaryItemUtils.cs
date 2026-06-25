using System.Linq;
using Mutators.Mutators.Behaviours;

namespace Mutators.Utility
{
    /// <summary>
    /// Utility methods for removing certain items from the StatsManager and the player's inventory.'
    /// </summary>
    public static class TemporaryItemUtils
    {
        /// <summary>
        /// Removes all items with the provided marker from the StatsManager.
        /// </summary>
        /// <param name="marker">The marker that will be used to determine which items to remove.</param>
        public static void RemoveMarkedItems(string marker)
        {
            StatsManager statsManager = StatsManager.instance;
            foreach (string item in statsManager.item.Where(item => item.Key.Contains($"({marker})")).Select(x => x.Key).ToList())
            {
                statsManager.item.Remove(item);
                statsManager.itemStatBattery.Remove(item);
            }
        }

        /// <summary>
        /// ForceUnequips all items with the provided marker, then removes them from the StatsManager. 
        /// </summary>
        /// <param name="marker">The marker that will be used to determine which items to drop and remove.</param>
        public static void DropAndRemoveMarkedItems(string marker)
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
