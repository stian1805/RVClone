using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button button;

    private InventorySlot inventorySlot;
    private System.Action onClickAction;

    public void Setup(
        InventorySlot slot,
        System.Action clickAction
    )
    {
        inventorySlot = slot;
        onClickAction = clickAction;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        Refresh();
    }

    public void Refresh()
    {
        if (inventorySlot == null || inventorySlot.IsEmpty)
        {
            icon.sprite = null;
            icon.enabled = false;
            quantityText.text = "";
            return;
        }

        icon.sprite = inventorySlot.Item.Icon;
        icon.color = Color.white;
        icon.enabled = true;

        quantityText.text =
            inventorySlot.Quantity > 1
                ? inventorySlot.Quantity.ToString()
                : "";
    }

    private void OnClicked()
    {
        onClickAction?.Invoke();
    }
}