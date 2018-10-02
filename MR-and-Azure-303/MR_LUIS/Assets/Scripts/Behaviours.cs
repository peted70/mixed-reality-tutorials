using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviours : MonoBehaviour
{
    public static Behaviours instance;

    // the following variables are references to possible targets
    public GameObject sphere;
    public GameObject cylinder;
    public GameObject cube;
    internal GameObject gazedTarget;

    void Awake()
    {
        // allows this class instance to behave like a singleton
        instance = this;
    }

    /// <summary>
    /// Changes the color of the target GameObject by providing the name of the object
    /// and the name of the color
    /// </summary>
    public void ChangeTargetColor(string targetName, string colorName)
    {
        GameObject foundTarget = FindTarget(targetName);
        if (foundTarget != null)
        {
            Debug.Log("Changing color " + colorName + " to target: " + foundTarget.name);

            switch (colorName)
            {
                case "blue":
                    foundTarget.GetComponent<Renderer>().material.color = Color.blue;
                    break;

                case "red":
                    foundTarget.GetComponent<Renderer>().material.color = Color.red;
                    break;

                case "yellow":
                    foundTarget.GetComponent<Renderer>().material.color = Color.yellow;
                    break;

                case "green":
                    foundTarget.GetComponent<Renderer>().material.color = Color.green;
                    break;

                case "white":
                    foundTarget.GetComponent<Renderer>().material.color = Color.white;
                    break;

                case "black":
                    foundTarget.GetComponent<Renderer>().material.color = Color.black;
                    break;
            }
        }
    }

    /// <summary>
    /// Reduces the size of the target GameObject by providing its name
    /// </summary>
    public void DownSizeTarget(string targetName)
    {
        GameObject foundTarget = FindTarget(targetName);
        foundTarget.transform.localScale -= new Vector3(0.5F, 0.5F, 0.5F);
    }

    /// <summary>
    /// Increases the size of the target GameObject by providing its name
    /// </summary>
    public void UpSizeTarget(string targetName)
    {
        GameObject foundTarget = FindTarget(targetName);
        foundTarget.transform.localScale += new Vector3(0.5F, 0.5F, 0.5F);
    }

    /// <summary>
    /// Determines which obejct reference is the target GameObject by providing its name
    /// </summary>
    private GameObject FindTarget(string name)
    {
        GameObject targetAsGO = null;

        switch (name)
        {
            case "sphere":
                targetAsGO = sphere;
                break;

            case "cylinder":
                targetAsGO = cylinder;
                break;

            case "cube":
                targetAsGO = cube;
                break;

            case "this": // as an example of target words that the user may use when looking at an object
            case "it":  // as this is the default, these are not actually needed in this example
            case "that":
            default: // if the target name is none of those above, check if the user is looking at something
                if (gazedTarget != null)
                {
                    targetAsGO = gazedTarget;
                }
                break;
        }
        return targetAsGO;
    }
}
