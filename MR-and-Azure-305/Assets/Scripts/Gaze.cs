using UnityEngine;

public class Gaze : MonoBehaviour
{
    /// <summary>
    /// Provides Singleton-like behavior to this class.
    /// </summary>
    public static Gaze instance;

    /// <summary>
    /// The Tag which the Gaze will use to interact with objects. Can also be set in editor.
    /// </summary>
    public string InteractibleTag = "GazeButton";

    /// <summary>
    /// The layer which will be detected by the Gaze ('~0' equals everything).
    /// </summary>
    public LayerMask LayerMask = ~0;

    /// <summary>
    /// The Max Distance the gaze should travel, if it has not hit anything.
    /// </summary>
    public float GazeMaxDistance = 300;

    /// <summary>
    /// The size of the cursor, which will be created.
    /// </summary>
    public Vector3 CursorSize = new Vector3(0.05f, 0.05f, 0.05f);

    /// <summary>
    /// The color of the cursor - can be set in editor.
    /// </summary>
    public Color CursorColour = Color.HSVToRGB(0.0223f, 0.7922f, 1.000f);

    /// <summary>
    /// Provides when the gaze is ready to start working (based upon whether
    /// Azure connects successfully).
    /// </summary>
    internal bool GazeEnabled = false;

    /// <summary>
    /// The currently focused object.
    /// </summary>
    internal GameObject FocusedObject { get; private set; }

    /// <summary>
    /// The object which was last focused on.
    /// </summary>
    internal GameObject _oldFocusedObject { get; private set; }

    /// <summary>
    /// The info taken from the last hit.
    /// </summary>
    internal RaycastHit HitInfo { get; private set; }

    /// <summary>
    /// The cursor object.
    /// </summary>
    internal GameObject Cursor { get; private set; }

    /// <summary>
    /// Provides whether the raycast has hit something.
    /// </summary>
    internal bool Hit { get; private set; }

    /// <summary>
    /// This will store the position which the ray last hit.
    /// </summary>
    internal Vector3 Position { get; private set; }

    /// <summary>
    /// This will store the normal, of the ray from its last hit.
    /// </summary>
    internal Vector3 Normal { get; private set; }

    /// <summary>
    /// The start point of the gaze ray cast.
    /// </summary>
    private Vector3 _gazeOrigin;

    /// <summary>
    /// The direction in which the gaze should be.
    /// </summary>
    private Vector3 _gazeDirection;

    /// <summary>
    /// The method used after initialization of the scene, though before Start().
    /// </summary>
    private void Awake()
    {
        // Set this class to behave similar to singleton
        instance = this;
    }

    /// <summary>
    /// Start method used upon initialization.
    /// </summary>
    private void Start()
    {
        FocusedObject = null;
        Cursor = CreateCursor();
    }

    /// <summary>
    /// Method to create a cursor object.
    /// </summary>
    /// <returns></returns>
    private GameObject CreateCursor()
    {
        GameObject newCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newCursor.SetActive(false);

        // Remove the collider, so it doesn't block raycast.
        Destroy(newCursor.GetComponent<SphereCollider>());
        newCursor.transform.localScale = CursorSize;

        newCursor.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"))
        {
            color = CursorColour
        };

        newCursor.name = "Cursor";

        newCursor.SetActive(true);

        return newCursor;
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        if (GazeEnabled == true)
        {
            _gazeOrigin = Camera.main.transform.position;

            _gazeDirection = Camera.main.transform.forward;

            UpdateRaycast();
        }
    }

    private void UpdateRaycast()
    {
        // Set the old focused gameobject.
        _oldFocusedObject = FocusedObject;

        RaycastHit hitInfo;

        // Initialise Raycasting.
        Hit = Physics.Raycast(_gazeOrigin,
            _gazeDirection,
            out hitInfo,
            GazeMaxDistance, LayerMask);

        HitInfo = hitInfo;

        // Check whether raycast has hit.
        if (Hit == true)
        {
            Position = hitInfo.point;

            Normal = hitInfo.normal;

            // Check whether the hit has a collider.
            if (hitInfo.collider != null)
            {
                // Set the focused object with what the user just looked at.
                FocusedObject = hitInfo.collider.gameObject;
            }
            else
            {
                // Object looked on is not valid, set focused gameobject to null.
                FocusedObject = null;
            }
        }
        else
        {
            // No object looked upon, set focused gameobject to null.
            FocusedObject = null;

            // Provide default position for cursor.
            Position = _gazeOrigin + (_gazeDirection * GazeMaxDistance);

            // Provide a default normal.
            Normal = _gazeDirection;
        }

        // Lerp the cursor to the given position, which helps to stabilize the gaze.
        Cursor.transform.position = Vector3.Lerp(Cursor.transform.position, Position, 0.6f);

        // Check whether the previous focused object is this same 
        //    object. If so, reset the focused object.
        if (FocusedObject != _oldFocusedObject)
        {
            ResetFocusedObject();

            if (FocusedObject != null)
            {
                if (FocusedObject.CompareTag(InteractibleTag.ToString()))
                {
                    // Set the Focused object to green - success!
                    FocusedObject.GetComponent<Renderer>().material.color = Color.green;

                    // Start the Azure Function, to provide the next shape!
                    AzureServices.instance.CallAzureFunctionForNextShape();
                }
            }
        }
    }

    /// <summary>
    /// Reset the old focused object, stop the gaze timer, and send data if it
    /// is greater than one.
    /// </summary>
    private void ResetFocusedObject()
    {
        // Ensure the old focused object is not null.
        if (_oldFocusedObject != null)
        {
            if (_oldFocusedObject.CompareTag(InteractibleTag.ToString()))
            {
                // Set the old focused object to red - its original state.
                _oldFocusedObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}
