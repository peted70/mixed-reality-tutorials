using System.Collections.Generic;
using UnityEngine;

public class ShapeFactory : MonoBehaviour
{
    /// <summary>
    /// Provide this class Singleton-like behaviour
    /// </summary>
    [HideInInspector]
    public static ShapeFactory instance;

    /// <summary>
    /// Provides an Inspector exposed reference to ShapeSpawnPoint
    /// </summary>
    [SerializeField]
    public Transform spawnPoint;

    /// <summary>
    /// Shape History Index
    /// </summary>
    [HideInInspector]
    public List<int> shapeHistoryList;

    /// <summary>
    /// Shapes Enum for selecting required shape
    /// </summary>
    private enum Shapes { Cube, Sphere, Cylinder }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        shapeHistoryList = new List<int>();
    }

    /// <summary>
    /// Use the Shape Enum to spawn a new Primitive object in the scene
    /// </summary>
    /// <param name="shape">Enumerator Number for Shape</param>
    /// <param name="storageShape">Provides whether this is new or old</param>
    internal void CreateShape(int shape, bool storageSpace)
    {
        Shapes primitive = (Shapes)shape;
        GameObject newObject = null;
        string shapeText = storageSpace == true ? "Storage: " : "New: ";

        AzureServices.instance.azureStatusText.text = string.Format("{0}{1}", shapeText, primitive.ToString());

        switch (primitive)
        {
            case Shapes.Cube:
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;

            case Shapes.Sphere:
                newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;

            case Shapes.Cylinder:
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
        }

        if (newObject != null)
        {
            newObject.transform.position = spawnPoint.position;

            newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            newObject.AddComponent<Rigidbody>().useGravity = true;

            newObject.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
