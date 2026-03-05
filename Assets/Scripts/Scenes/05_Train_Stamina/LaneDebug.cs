using UnityEngine;

public class LaneDebug
{

    public static void genLines(GameObject parent, LaneSystem laneSystem, SpriteRenderer trackRenderer, float lineWidth = 0.04f)
    {
        LineRenderer[] lines;
        if (!laneSystem || !trackRenderer) return;

        var b = trackRenderer.bounds;
        float left = b.min.x;
        float right = b.max.x;

        lines = new LineRenderer[laneSystem.laneCount];

        for (int i = 0; i < laneSystem.laneCount; i++)
        {
            var go = new GameObject($"LaneLine_{i}");
            go.transform.SetParent(parent.transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.numCapVertices = 4;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            // Make it render above the track
            lr.sortingLayerID = trackRenderer.sortingLayerID;
            lr.sortingOrder = trackRenderer.sortingOrder + 1;

            // Simple unlit material
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = new Color(1f, 0.5f, 0f, 1f); // orange

            float y = laneSystem.GetLaneY(i);
            lr.SetPosition(0, new Vector3(left,  y, 0f));
            lr.SetPosition(1, new Vector3(right, y, 0f));

            lines[i] = lr;
        }
    }
}
