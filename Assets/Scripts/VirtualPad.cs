using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEditor.Experimental.GraphView;


public class VirtualPad : MonoBehaviour
{
    public GameObject character;
    public float speed = 0.5f;
    public Camera camara;
    public float cameraSpeed = 1f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction cameraAction;
    private Vector2 moveInput;
    private Vector2 cameraInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        Debug.Log("PlayerInput: " + playerInput);

        moveAction = playerInput.actions["Move"];
        Debug.Log("Move Action: " + moveAction);

        cameraAction = playerInput.actions["Camera"];
        Debug.Log("Camera Action " + cameraAction);

    }

    // Update is called once per frame
    void Update()
    {
        var currentTouches = Touch.activeTouches;

        foreach(var touch in currentTouches)
        {
           if (touch.startScreenPosition.x < Screen.width / 2f)
           continue; 
           Vector2 direction = calculateDirection(touch.startScreenPosition, touch.screenPosition);
           MoveCamera(direction.x, direction.y);
        }

        MoveCharacter();

    }


    public void MoveCharacter()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        //Debug.Log("Move Input: " + moveInput);


        if (moveInput.sqrMagnitude > 0.01f)
        {
            moveInput = moveInput.normalized;
        }

        else
        {
            moveInput = Vector2.zero;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        character.transform.Translate(movement * speed * Time.deltaTime);
        //print(character.transform.position);
    }

    public void MoveCamera(float x, float y)
    {
        cameraInput = cameraAction.ReadValue<Vector2>();

        
        if( x > 0)
        {
            camara.transform.Rotate(speed * Time.deltaTime, 0f, 0f);
        }

        if( x < 0)
        {
            camara.transform.Rotate(-speed * Time.deltaTime, 0f, 0f);
        }

        if( x > 0)
        {
            camara.transform.Rotate(0f, speed * Time.deltaTime, 0f);
        }

        if( x < 0)
        {
            camara.transform.Rotate(0f, -speed * Time.deltaTime, 0f);
        }

        

        
    }

    public Vector2 calculateDirection(Vector2 StartPos, Vector2 currentPos)
    {
        Vector2 direction = currentPos - StartPos;
        direction = direction.normalized;
        print(direction);
        return direction;
    }
}
