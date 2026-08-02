using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Inventory/Item"
)]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int maximumStackSize = 1;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public int MaximumStackSize => maximumStackSize;
}