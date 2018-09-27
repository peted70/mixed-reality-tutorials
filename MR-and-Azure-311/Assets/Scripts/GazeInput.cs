using UnityEngine;

/// <summary>
/// Class responsible for the User's Gaze interactions
/// </summary>
[System.Serializable]
public class GazeInput : MonoBehaviour
{
    [Tooltip("Used to compare whether an object is to be interacted with.")]
    internal string InteractibleTag = "SignInButton";

    /// <summary>
    /// Length of the gaze
    /// </summary>
    internal float GazeMaxDistance = 300;

    /// <summary>
    /// Object currently gazed
    /// </summary>
    internal GameObject FocusedObject { get; private set; }

    internal GameObject oldFocusedObject { get; private set; }

    internal RaycastHit HitInfo { get; private set; }

    /// <summary>
    /// Cursor object visible in the scene
    /// </summary>
    internal GameObject Cursor { get; private set; }

    internal bool Hit { get; private set; }

    internal Vector3 Position { get; private set; }

    internal Vector3 Normal { get; private set; }

    private Vector3 _gazeOrigin;

    private Vector3 _gazeDirection;
    /// <summary>
    /// Start method used upon initialisation.
    /// </summary>
    internal virtual void Start()
    {
        FocusedObject = null;
        Cursor = CreateCursor();
    }

    /// <summary>
    /// Method to create a cursor object.
    /// </summary>
    internal GameObject CreateCursor()
    {
        GameObject newCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newCursor.SetActive(false);
        // Remove the collider, so it doesn't block raycast.
        Destroy(newCursor.GetComponent<SphereCollider>());
        newCursor.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        Material mat = new Material(Shader.Find("Diffuse"));
        newCursor.GetComponent<MeshRenderer>().material = mat;
        mat.color = Color.HSVToRGB(0.0223f, 0.7922f, 1.000f);
        newCursor.SetActive(true);

        return newCursor;
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    internal virtual void Update()
    {
        _gazeOrigin = Camera.main.transform.position;

        _gazeDirection = Camera.main.transform.forward;

        UpdateRaycast();
    }
    /// <summary>
    /// Reset the old focused object, stop the gaze timer, and send data if it
    /// is greater than one.
    /// </summary>
    private void ResetFocusedObject()
    {
        // Ensure the old focused object is not null.
        if (oldFocusedObject != null)
        {
            if (oldFocusedObject.CompareTag(InteractibleTag))
            {
                // Provide the 'Gaze Exited' event.
                oldFocusedObject.SendMessage("OnGazeExited", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void UpdateRaycast()
    {
        // Set the old focused gameobject.
        oldFocusedObject = FocusedObject;
        RaycastHit hitInfo;

        // Initialise Raycasting.
        Hit = Physics.Raycast(_gazeOrigin,
            _gazeDirection,
            out hitInfo,
            GazeMaxDistance);
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

        // Check whether the previous focused object is this same. If so, reset the focused object.
        if (FocusedObject != oldFocusedObject)
        {
            ResetFocusedObject();
            if (FocusedObject != null)
            {
                if (FocusedObject.CompareTag(InteractibleTag))
                {
                    // Provide the 'Gaze Entered' event.
                    FocusedObject.SendMessage("OnGazeEntered",
                        SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
