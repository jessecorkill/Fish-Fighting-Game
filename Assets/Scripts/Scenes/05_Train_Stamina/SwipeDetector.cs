using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDownPos;
    private Vector2 fingerUpPos;
    [SerializeField] private float minDistanceForSwipe = 50f; // in pixels
    [SerializeField] private bool detectSwipeAfterRelease = true;
    [SerializeField] private bool allowKeyboardInput = true;
    [SerializeField] private LaneMover2D laneMover;


    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    fingerDownPos = touch.position;
                    fingerUpPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    if (!detectSwipeAfterRelease)
                    {
                        fingerUpPos = touch.position;
                        DetectSwipe();
                    }
                    break;

                case TouchPhase.Ended:
                    fingerUpPos = touch.position;
                    DetectSwipe();
                    break;
            }
        }
        if (allowKeyboardInput)
        {
            // Desktop testing with arrow keys
            if (Input.GetKeyDown(KeyCode.UpArrow))
                OnSwipeUp();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                OnSwipeDown();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnSwipeLeft();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                OnSwipeRight();
        }
    }

    void DetectSwipe()
    {
        if (SwipeDistanceCheckMet())
        {
            Vector2 diff = fingerUpPos - fingerDownPos;
            Vector2 direction = diff.normalized;

            if (IsVerticalSwipe(diff))
            {
                if (direction.y > 0)
                    OnSwipeUp();
                else
                    OnSwipeDown();
            }
            else
            {
                if (direction.x > 0)
                    OnSwipeRight();
                else
                    OnSwipeLeft();
            }

            fingerDownPos = fingerUpPos;
        }
    }

    bool SwipeDistanceCheckMet()
    {
        return Vector2.Distance(fingerDownPos, fingerUpPos) >= minDistanceForSwipe;
    }

    bool IsVerticalSwipe(Vector2 diff)
    {
        return Mathf.Abs(diff.y) > Mathf.Abs(diff.x);
    }

    void OnSwipeUp(){
        Debug.Log("Swipe Up");
        laneMover?.NudgeLane(1);
    }
    void OnSwipeDown(){
        Debug.Log("Swipe Down");
        laneMover?.NudgeLane(-1);
    }
    void OnSwipeLeft() => Debug.Log("Swipe Left");
    void OnSwipeRight() => Debug.Log("Swipe Right");
}
