using System.Linq;
using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    [Header("Source of lane height")]
    [SerializeField] SpriteRenderer trackRenderer; // The road/track sprite
    [SerializeField] float sidePaddingWorld = 0.2f; // keep player off the very edge

    [Header("Lanes")]
    [Range(2,10)] public int laneCount = 3;

    float[] laneYs; //An array of floats representing the positions of each lane

    void Awake()
    {
        Recalculate();
    }

    ///<summary>
    ///
    /// </summary>
    public void Recalculate()
    {
        var b = trackRenderer.bounds;
        float bottom = b.min.y + sidePaddingWorld; // Bottom most edge of the sprite - padding
        float top = b.max.y - sidePaddingWorld; // Top most edge of the sprite - padding 

        laneYs = new float[laneCount]; //An array of floats representing the positions of each lane
        if (laneCount == 1) { laneYs[0] = (bottom + top) * 0.5f; return; }

        float step = (top - bottom) / (laneCount - 1);
        for (int i = 0; i < laneCount; i++)
            laneYs[i] = bottom + step * i;

        Debug.Log("Lanes: " + string.Join(", ", laneYs));
    }
    
    /// <summary>
    /// Get the Y coordinate position of a lane.
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns>Y coordinate (as a float)</returns>
    public float GetLaneY(int laneIndex)
    {
        if (laneYs.Count() > 0)
        {
            laneIndex = Mathf.Clamp(laneIndex, 0, laneCount - 1);
            return laneYs[laneIndex];
        }
        else
        {
            Debug.LogError("GetLaneY ran without a laneY array ready");
            return 0;
        }

    }
    public int GetTotalLanes()
    {
        return laneCount;
    }
}
