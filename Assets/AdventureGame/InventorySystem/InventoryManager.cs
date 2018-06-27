﻿using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.AdventureGame
{
    //TODO - "picking up" should call AddItem
    //TODO - clicking in the world with an item selected should drop the item

    public class InventoryManager : MonoBehaviour
    {
        public const int INVENTORY_SLOTS = 8;

        public Image[] itemImages = new Image[INVENTORY_SLOTS];
        public InventoryItem[] items = new InventoryItem[INVENTORY_SLOTS];
        public InventoryItem Selected = null;

        private static InventoryManager instance;

		public static InventoryManager Instance
		{
			get
			{
				if (instance == null)
				{
                    InventoryManager existingInventoryManager = FindObjectOfType<InventoryManager>();
                    if(existingInventoryManager != null){
                        instance = existingInventoryManager;
                    }
                    else {
						GameObject manager = new GameObject();
						instance = manager.AddComponent<InventoryManager>();
                    }
				}
				return instance;
			}
		}

        public void UpdateUI(int index)
        {
            InventoryUI.Instance.UpdateSlot(index);
        }

        //TODO should be called by screen click area if an item is selected when a noninteractable part of
        //the world is clicked on
        public void DropItem(Vector3 dropPosition)
        {
            if (Selected == null)
            {
                Debug.Log("Called DropItem with nothing selected!");
                return;
            }

            Selected.transform.position = new Vector3(dropPosition.x - 0.1f, dropPosition.y + 0.1f, dropPosition.z);
            RemoveItem(Selected);
        }

        public bool AddItem(InventoryItem itemToAdd)
        {
            //find the first empty inventory slot
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = itemToAdd;
                    itemImages[i].sprite = itemToAdd.sprite;
                    itemImages[i].enabled = true;
                    UpdateUI(i);
                    return true;
                }
            }
            Debug.Log("Inventory is full!");
            return false;
        }

        public bool RemoveItem(InventoryItem itemToRemove)
        {
            //find the slot containing the item and remove it
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == itemToRemove)
                {
                    items[i] = null;
                    itemImages[i].sprite = null;
                    itemImages[i].enabled = false;
                    UpdateUI(i);
                    return true;
                }
            }
            Debug.Log("That isn't in your inventory!");
            return false;
        }

        private void selectItem(int index)
        {
            if (items[index] == null)
            {
                Debug.Log("Nothing to select here!");
                return;
            }
            Debug.Log("Selected " + items[index].Id);
            this.Selected = items[index];
            AdventureGameOverlayManager.Instance.ChangeCursor(items[index].sprite);
        }

        public void ClearSelected()
        {
            Debug.Log("Selected item cleared");
            this.Selected = null;
            AdventureGameOverlayManager.Instance.ChangeCursor(null);
        }

        public void SlotClicked(int index)
        {
            if (items[index] == null)
            {
                //nothing to do here
                return;
            }

            InventoryItem itemInSlot = items[index];
            //clicking an item on another item
            if (Selected != null)
            {
                if (Selected.Id == itemInSlot.Id)
                {
                    //clicking the item on itself, clear selection
                    ClearSelected();
                }
                //combine the selected item with the item in this slot
                else
                {
                    CombineItems(Selected, itemInSlot);
                }
            }
            //clicking an item when nothing is currently selected
            else
            {
                selectItem(index);
            }
        }

        public void CombineItems(InventoryItem first, InventoryItem second)
        {
            //TODO replace one of the items with the new (combined) item if applicable, clear the other
            Debug.Log("You've combined two items!");
            ClearSelected();
        }
    }
}