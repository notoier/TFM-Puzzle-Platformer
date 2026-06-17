using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("X")]
    public float lookAheadDistance;
    public float smoothTimeX = 0.2f;

    [Header("X")]
    public float lookUpDistance;
    public float smoothTimeY = 0.2f;
    [Header("Y")]
    public float verticalThreshold = 3f;

    private float currentOffsetX;
    private float velocityX;

    private float currentOffsetY;
    private float baseY;
    private float velocityY;

    private bool isLookingUp = false;

    private void Start()
    {
        baseY = player.position.y;


    }

    void Update()
    {
        Vector3 basePosition = player.position;

        //LOOKAHEAD (Moviemiento en X)
        float inputX = GetInputX();

        float targetOffsetX = inputX * lookAheadDistance;
        currentOffsetX = Mathf.SmoothDamp(currentOffsetX, targetOffsetX, ref velocityX, smoothTimeX);

        basePosition.x += currentOffsetX;

        //LOOKUP/DOWN (Moviemiento en y)

        float inputY = GetInputY();
        if (isLookingUp)
        { 
            float targetOffsetY = inputY * lookUpDistance;
            currentOffsetY = Mathf.SmoothDamp(currentOffsetY, targetOffsetY, ref velocityY, smoothTimeY);

            basePosition.y += currentOffsetY;
        }
        

        //DEADZONE JUMP (Movimietno en y)

        float deadZoneY = player.position.y - baseY;

        if (deadZoneY > verticalThreshold)
        {
            baseY = player.position.y - verticalThreshold;
        }
        else if (deadZoneY < -verticalThreshold)
        {
            baseY = player.position.y + verticalThreshold;
        }

        if (!isLookingUp)
        {
            basePosition.y = baseY;
        }
            
        transform.position = basePosition;

        Debug.DrawLine(new Vector3(player.position.x - 5, baseY, 0), new Vector3(player.position.x + 5, baseY, 0), Color.red, 0.1f);
        Debug.DrawRay(new Vector3(player.position.x, baseY, 0), Vector3.up * 0.5f, Color.green);
    }

    public float GetInputX()
    {
        
        float value = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            value -= 1;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            value += 1; ;
        }

        return value;
    }

    public float GetInputY()
    {
        float value = 0f;

        if (Keyboard.current.sKey.isPressed)
        {
            isLookingUp = true;
            value -= 1;
            Debug.Log("S");
        }
        else if (Keyboard.current.wKey.isPressed)
        {
            isLookingUp = true;
            value += 1; ;
            Debug.Log("w");
        }
        else
        {
            isLookingUp = false;
        }
        return value;
    }
}
