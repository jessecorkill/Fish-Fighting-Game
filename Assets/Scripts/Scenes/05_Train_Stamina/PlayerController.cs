using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]  staminaGameController gameController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("HIT");
        gameController.SetFail();

    }

    // Update is called once per frame
    void Update()
    {


    }
}
