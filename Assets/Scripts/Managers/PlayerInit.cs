using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set up sync with Save Manager (May not be necessary for this file
        SaveManagerBehaviour.Instance.OnChanged += Refresh; //Hook local Refresh function to OnChange event
        if (SaveManagerBehaviour.Instance.IsLoaded) //Check if save data is ready to be pulled
        {
            Refresh(SaveManagerBehaviour.Instance.Current); //Update the UI with the most up-to-date Save data
        }

        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
        InitPlayer(sd);

    }
    private void InitPlayer(SaveData sd)
    {
        //If player has no tanks
        int tankCount = sd.myTanks.Count;
        if (tankCount == 0)
        {
            //Assign player a tank
            TankObj firstTank = genTank();
            SaveManagerBehaviour.Instance.Mutate(sd => {
                sd.myTanks.Add(firstTank);
            });

        }

        //If player has no fish
        int fishCount = sd.myBettas.Count;
        if(fishCount == 0)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string curSceneName = currentScene.name;
            if(curSceneName != "04_New_Fish")
            {
                //Change scene to New Fish
                SceneManager.LoadScene("04_New_Fish");
            }

        }

    }
    private TankObj genTank()
    {
        TankObj firstTank = new TankObj();
        firstTank.Co2 = false;
        firstTank.Dirtiness = 5f;
        firstTank.FilterTier = 1;
        firstTank.FoodTier = 1;
        firstTank.Sediment = false;
        firstTank.TankID = 0;

        return firstTank;
    }
    private void Refresh(SaveData sd)
    {
        InitPlayer(sd);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
