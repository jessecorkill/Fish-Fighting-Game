using UnityEngine;

public class DeletionBoundryController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Boundry HIT");
        Destroy(other.gameObject);        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
