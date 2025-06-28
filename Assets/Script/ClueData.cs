using UnityEngine;

[CreateAssetMenu(fileName = "NewClue", menuName = "Clue/ClueData")]
public class ClueData : ScriptableObject
{
    public string id;
    public new string name;
    public Sprite icon;
    public string description;
}
