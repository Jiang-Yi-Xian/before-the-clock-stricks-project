using UnityEngine;

public enum InteractionType { Pick, Observe, Switch }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public InteractionType interactionType;
}
