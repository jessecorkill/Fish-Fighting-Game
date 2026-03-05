using UnityEngine;

public class PlaceRandomSprite : MonoBehaviour
{
    public SpriteRenderer areaSprite;  // The sprite that defines the area
    public GameObject spriteToPlace;   // The prefab you want to place
    public Transform container;

    private void Start()
    {
        InvokeRepeating(nameof(PlaceSprite), 0f, 5f);
    }
    public void PlaceSprite()
    {
        // Get bounds of the area sprite
        Bounds bounds = areaSprite.bounds;

        // Pick random X and Y within bounds
        float randX = Random.Range(bounds.min.x, bounds.max.x);
        float randY = Random.Range(bounds.min.y, bounds.max.y);

        // New position
        Vector3 randomPos = new Vector3(randX, randY, 0f);

        // Place/instantiate sprite
        GameObject newAlgae = Instantiate(spriteToPlace, randomPos, Quaternion.identity);
        newAlgae.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
