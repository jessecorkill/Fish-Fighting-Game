using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Economy.Model;
using System;
using System.Linq;
using UnityEngine.UIElements;

public class staminaGameController : MonoBehaviour
{
    [SerializeField] GameObject fish;
    [SerializeField] GameObject floater;
    [SerializeField] SpriteRenderer track;
    [SerializeField] LaneSystem laneController;
    [SerializeField] float swipeSensitivity = 0.3f; //Always multiply by Screen.dpi
    [SerializeField] SpriteRenderer obstacleBoundry;
    [SerializeField] GameObject GameOverUI;
    [SerializeField] SpriteRenderer miniMap;
    [SerializeField] SpriteRenderer miniMapMarker;
    private AppRoot appRoot = AppRoot.Instance;

    private float velocity;
    private float time;
    private float target = 240f;
    public string state;
    private int staminaCost = 10;
    public float gainedXP;
    public BettaObj previousBettaValues;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gainedXP = 0; //reset value 
        
        if(appRoot == null)
        {
            Debug.LogError("Failed to link AppRoot");
        }

        //Fit track to screen 
        SpriteScreenUtil.FitToScreen(track, Camera.main, SpriteScreenUtil.FitMode.Cover, center: false);
        SpriteScreenUtil.PlaceAtEdgeCentered(track, SpriteScreenUtil.Edge.Left, Camera.main, offset: 0.0f);
        //Place Obstacle Deletion Boundry
        SpriteScreenUtil.PlaceAtEdgeCentered(obstacleBoundry, SpriteScreenUtil.Edge.Left, Camera.main, -2f);
        //Draw debug lines
        //LaneDebug.genLines(gameObject, laneController, track, 0.04f);
        //Scale Mini Map
        SpriteScreenUtil.ScaleSpriteToScreenHeight(miniMap, Camera.main, .10f);
        SpriteScreenUtil.ScaleSpriteToScreenWidth(miniMap, Camera.main, .50f);
        //Place Mini Map in top-right corner
        SpriteScreenUtil.PlaceAtCorner(miniMap, Camera.main, 0f, SpriteScreenUtil.ScreenCorner.TopRight);
        //Enable Player 
        fish.SetActive(true);

        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
        previousBettaValues = sd.myTanks[appRoot.CurrentTank].fish;
        //Subtract stamina from fish - TODO
        spendStamina(sd, appRoot.CurrentTank);

        //Listen for swipe input
        //Set Game State
        state = "play";        

    }
    private void spendStamina(SaveData sd, int currentTank)
    {
        
        //Make instance of the tank & its fish
        List<TankObj> tankListInstance = sd.myTanks;
        List<BettaObj> bettaListInstance = sd.myBettas;
        Debug.Log("Stamina before: " + tankListInstance[currentTank].fish.Stamina);
        //Manipulate the data
        tankListInstance[currentTank].fish.Stamina = tankListInstance[currentTank].fish.Stamina - staminaCost; //Update the tank's fish
        //Find the betta where the tankID matches the currentTank's ID
        int bettaID = bettaListInstance.FindIndex(betta => betta.TankID == currentTank); 
        bettaListInstance[bettaID].Stamina = bettaListInstance[bettaID].Stamina - staminaCost; //Update the player's fish list
        Debug.Log("Stamina after: " + tankListInstance[currentTank].fish.Stamina);
        //Update SaveData
        SaveManagerBehaviour.Instance.Mutate(sd =>
        {
            sd.myTanks = tankListInstance;
            sd.myBettas = bettaListInstance;
        });


    }
    //Move the Mini Map's Fish function
    private void updateMiniMap(SpriteRenderer marker, SpriteRenderer map)
    {
        float progress = (100f / target) * time;
        progress = Mathf.Clamp(progress, 0f, 100f) / 100f;

        Bounds boundsA = marker.bounds;
        Bounds boundsB = map.bounds;

        // Horizontal limits so A stays fully inside B
        float minX = boundsB.min.x + boundsA.extents.x;
        float maxX = boundsB.max.x - boundsA.extents.x;

        float targetX = Mathf.Lerp(minX, maxX, progress);

        // Vertically centered inside B
        float targetY = boundsB.center.y;

        marker.transform.position = new Vector3(
            targetX,
            targetY,
            marker.transform.position.z
        );
    }
    //Should take the total time the player survived then return the fish's gained experience
    private float calculateXPGain()
    {        
        if (time >= target)//Player won
        {            
            return target * 2; //Give the player 2 xp for every second they survived
        }
        //Player failed
        return time * .5f; //Give the player .5 xp for every second they survived

    }
    private void awardXP(SaveData sd, int currentTank)
    {
        float xp = 0;
        if (time >= target){            
            //Player won
            xp = target * 2; //Give the player 2 xp for every second they survived
        }
        else{                    
            //Player failed
            xp = time * .5f; //Give the player .5 xp for every second they survived
        }
        //Add the XP to the Fish
        //Make instance of the tank & its fish
        List<TankObj> tankListInstance = sd.myTanks;
        List<BettaObj> bettaListInstance = sd.myBettas;
        Debug.Log("Evade xp before: " + tankListInstance[currentTank].fish.EvadeXP);
        //Manipulate the data
        tankListInstance[currentTank].fish.EvadeXP = tankListInstance[currentTank].fish.EvadeXP + xp; //Update the tank's fish
        //Find the betta where the tankID matches the currentTank's ID
        int bettaID = bettaListInstance.FindIndex(betta => betta.TankID == currentTank); 
        bettaListInstance[bettaID].EvadeXP = bettaListInstance[bettaID].EvadeXP + xp; //Update the player's fish list
        Debug.Log("Evade xp after: " + tankListInstance[currentTank].fish.EvadeXP);
        //Update SaveData
        SaveManagerBehaviour.Instance.Mutate(sd =>
        {
            sd.myTanks = tankListInstance;
            sd.myBettas = bettaListInstance;
        });


    }
    public void SetFail()
    {
        state = "fail";
    }
    public string GetState()
    {
        return state;
    }
    
    // Update is called once per frame
    void Update()
    {        

        time += Time.deltaTime;
        //update mini map
        updateMiniMap(miniMapMarker, miniMap);
        if(time >= target && state != "fail" || state != "win" && time >= target)
        {
            var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
            awardXP(sd, appRoot.GetCurrentTank());
            //gainedXP = calculateXPGain();
            state = "win";
            fish.SetActive(false);
            GameOverUI.SetActive(true);
            Debug.Log("Gained XP: " + gainedXP);
        }
        if(state == "fail")
        {
            var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
            //gainedXP = calculateXPGain();
            awardXP(sd, appRoot.GetCurrentTank());
            state = "stop";
            
            //Despawn all obstacles
            //Despawn player
            fish.SetActive(false);
            //Show Fail UI
            GameOverUI.SetActive(true);
        }
        
    }
}
