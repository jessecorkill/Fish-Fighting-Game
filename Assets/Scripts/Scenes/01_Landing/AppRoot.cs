using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;

public class AppRoot : MonoBehaviour
{
    public BettaObj currentBetta;
    public int CurrentTank {get; private set;} = 0; 
    public static AppRoot Instance;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void SetCurrentTank(int newTank)
    {
        CurrentTank = newTank;    
    }
    private void GetCurrentBetta(int tankID)
    {
        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
        
    }
    public int GetCurrentTank()
    {
        return CurrentTank;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
