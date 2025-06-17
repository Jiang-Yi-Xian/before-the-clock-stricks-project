using UnityEngine;

public class ClueSpawner : MonoBehaviour
{
    public ClueData[] cluesToSpawn;
    public GameObject clueCardPrefab;
    public Transform clueCardParent; // ³q±`¬O GridLayoutGroup

    void Start()
    {
        foreach (ClueData clue in cluesToSpawn)
        {
            GameObject card = Instantiate(clueCardPrefab, clueCardParent);
            ClueCard clueCard = card.GetComponent<ClueCard>();
            clueCard.Setup(clue);
        }
    }
}
