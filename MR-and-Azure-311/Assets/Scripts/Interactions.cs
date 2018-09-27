using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
public class Interactions : GazeInput
{
    /// <summary>
    /// Allows input recognition with the HoloLens
    /// </summary>
    private GestureRecognizer _gestureRecognizer;

    /// <summary>
    /// Called on initialization, after Awake
    /// </summary>
    internal override void Start()
    {
        base.Start();

        // Register the application to recognize HoloLens user inputs
        _gestureRecognizer = new GestureRecognizer();
        _gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
        _gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        _gestureRecognizer.StartCapturingGestures();

        // Add the Graph script to this object
        gameObject.AddComponent<MeetingsUI>();
        CreateSignInButton();
    }

    /// <summary>
    /// Create the sign in button object in the scene
    /// and sets its properties
    /// </summary>
    void CreateSignInButton()
    {
        GameObject signInButton = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Material mat = new Material(Shader.Find("Diffuse"));
        signInButton.GetComponent<Renderer>().material = mat;
        mat.color = Color.blue;

        signInButton.transform.position = new Vector3(3.5f, 2f, 9f);
        signInButton.tag = "SignInButton";
        signInButton.AddComponent<Graph>();
    }

    /// <summary>
    /// Detects the User Tap Input
    /// </summary>
    private void GestureRecognizer_Tapped(TappedEventArgs obj)
    {
        if (base.FocusedObject != null)
        {
            Debug.Log($"TAP on {base.FocusedObject.name}");
            base.FocusedObject.SendMessage("SignInAsync", SendMessageOptions.RequireReceiver);
        }
    }
}
