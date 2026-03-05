using UnityEngine;

public static class SpriteScreenUtil
{
    public enum FitMode { Contain, Cover, Stretch }
    public enum Edge { Left, Right, Top, Bottom }
    public enum ScreenCorner {TopLeft, TopRight, BottomLeft, BottomRight};

    /// Fits a SpriteRenderer to the camera view and (optionally) centers it.
    /// Orthographic cameras only.
    public static void FitToScreen(SpriteRenderer sr, Camera cam = null, FitMode mode = FitMode.Stretch, bool center = true)
    {
        if (!sr || !sr.sprite) return;
        cam = cam ? cam : Camera.main;
        if (!cam || !cam.orthographic) return;

        float camH = 2f * cam.orthographicSize;
        float camW = camH * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size; // world units before scaling

        float sX, sY;
        switch (mode)
        {
            case FitMode.Contain: 
                {
                    float s = Mathf.Min(camW / spriteSize.x, camH / spriteSize.y);
                    sX = sY = s;
                    break;
                }
            case FitMode.Cover: 
                {
                    float s = Mathf.Max(camW / spriteSize.x, camH / spriteSize.y);
                    sX = sY = s;
                    break;
                }
            default: // Stretch: fill exactly (aspect may distort)
                {
                    sX = camW / spriteSize.x;
                    sY = camH / spriteSize.y;
                    break;
                }
        }

        // Account for parent scale so final world scale is correct
        Vector3 parentScale = sr.transform.parent ? sr.transform.parent.lossyScale : Vector3.one;
        sr.transform.localScale = new Vector3(
            sX / Mathf.Max(1e-6f, parentScale.x),
            sY / Mathf.Max(1e-6f, parentScale.y),
            1f
        );

        if (center)
        {
            var p = sr.transform.position;
            var cp = cam.transform.position;
            sr.transform.position = new Vector3(cp.x, cp.y, p.z);
        }
    }

    /// Centers the sprite on one axis and places it flush to a chosen edge on the other.
    /// 'offset' is in world units; positive moves inward for Left/Bottom, outward for Right/Top.
    public static void PlaceAtEdgeCentered(SpriteRenderer sr, Edge edge, Camera cam = null, float offset = 0f)
    {
        if (!sr || !sr.sprite) return;
        cam = cam ? cam : Camera.main;
        if (!cam || !cam.orthographic) return;

        float camH = 2f * cam.orthographicSize;
        float camW = camH * cam.aspect;

        float halfW = camW * 0.5f;
        float halfH = camH * 0.5f;
        Vector3 cp = cam.transform.position;

        // Sprite half size AFTER scaling, axis-aligned
        Vector3 ext = sr.bounds.extents;

        Vector3 pos = sr.transform.position; // keep current z
        switch (edge)
        {
            case Edge.Left:
                pos.x = cp.x - halfW + ext.x + offset; // centered vertically
                pos.y = cp.y;
                break;
            case Edge.Right:
                pos.x = cp.x + halfW - ext.x + offset; // use negative offset to inset
                pos.y = cp.y;
                break;
            case Edge.Top:
                pos.y = cp.y + halfH - ext.y + offset; // use negative offset to inset
                pos.x = cp.x;
                break;
            case Edge.Bottom:
                pos.y = cp.y - halfH + ext.y + offset; // centered horizontally
                pos.x = cp.x;
                break;
        }
        sr.transform.position = pos;
    }

    public static void PlaceAtCorner(
        SpriteRenderer spriteRenderer,
        Camera camera,
        float padding,
        ScreenCorner corner)
    {
        if (spriteRenderer == null || camera == null)
            return;

        // Get sprite size in world units
        Vector2 spriteSize = spriteRenderer.bounds.size;

        // Screen bounds in world space
        float camHeight = camera.orthographicSize * 2f;
        float camWidth = camHeight * camera.aspect;

        Vector3 camPos = camera.transform.position;

        float x = camPos.x;
        float y = camPos.y;

        switch (corner)
        {
            case ScreenCorner.TopLeft:
                x -= camWidth / 2f - spriteSize.x / 2f - padding;
                y += camHeight / 2f - spriteSize.y / 2f - padding;
                break;

            case ScreenCorner.TopRight:
                x += camWidth / 2f - spriteSize.x / 2f - padding;
                y += camHeight / 2f - spriteSize.y / 2f - padding;
                break;

            case ScreenCorner.BottomLeft:
                x -= camWidth / 2f - spriteSize.x / 2f - padding;
                y -= camHeight / 2f - spriteSize.y / 2f - padding;
                break;

            case ScreenCorner.BottomRight:
                x += camWidth / 2f - spriteSize.x / 2f - padding;
                y -= camHeight / 2f - spriteSize.y / 2f - padding;
                break;
        }

        spriteRenderer.transform.position = new Vector3(x, y, spriteRenderer.transform.position.z);

    }
    public static void ScaleSpriteToScreenHeight(
        SpriteRenderer spriteRenderer,
        Camera camera,
        float relativeHeight
    )
    {
        if (spriteRenderer == null || camera == null) return;

        // Camera height in world units
        float cameraWorldHeight = camera.orthographicSize * 2f;

        // Sprite height in world units (before scaling)
        float spriteWorldHeight = spriteRenderer.sprite.bounds.size.y;

        // Target height in world units
        float targetHeight = cameraWorldHeight * relativeHeight;

        float scale = targetHeight / spriteWorldHeight;

        spriteRenderer.transform.localScale = new Vector3(
            spriteRenderer.transform.localScale.x,
            scale,
            spriteRenderer.transform.localScale.z
        );
    }
    public static void ScaleSpriteToScreenWidth(
        SpriteRenderer spriteRenderer,
        Camera camera,
        float relativeWidth
    )
    {
        if (spriteRenderer == null || camera == null) return;

        // Camera width in world units
        float cameraWorldHeight = camera.orthographicSize * 2f;
        float cameraWorldWidth = cameraWorldHeight * camera.aspect;

        // Sprite width in world units (before scaling)
        float spriteWorldWidth = spriteRenderer.sprite.bounds.size.x;

        // Target width in world units
        float targetWidth = cameraWorldWidth * relativeWidth;

        float scale = targetWidth / spriteWorldWidth;

        spriteRenderer.transform.localScale = new Vector3(
            scale,
            spriteRenderer.transform.localScale.y,
            spriteRenderer.transform.localScale.z
        );
    }


}
