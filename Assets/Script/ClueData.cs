using UnityEngine;

[CreateAssetMenu(fileName = "NewClue", menuName = "Clue/ClueData")]
public class ClueData : ScriptableObject
{
    public string id;
    public string name;
    public Sprite icon;
    public string description;
}
