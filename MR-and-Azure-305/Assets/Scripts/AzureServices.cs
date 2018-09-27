using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System.IO;
using System.Net;

public class AzureServices : MonoBehaviour
{
    /// <summary>
    /// Provides Singleton-like behavior to this class.
    /// </summary>
    public static AzureServices instance;

    /// <summary>
    /// Reference Target for AzureStatusText Text Mesh object
    /// </summary>
    public TextMesh azureStatusText;

    /// <summary>
    /// Holds the Azure Function endpoint - Insert your Azure Function
    /// Connection String here.
    /// </summary>

    private string azureFunctionEndpoint = "https://mixedreality305.azurewebsites.net/api/HttpTriggerCSharp1?code=bB9OxeJ8iMNyHO70rHSyRTZ/hYh8B2Rj6xk9aO7Vd9CIabap54MEHA==";

    /// <summary>
    /// Holds the Storage Connection String - Insert your Azure Storage
    /// Connection String here.
    /// </summary>
    private string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=mixedrealityazure305;AccountKey=NYpVZGVUGAwxLDrF7sLMEnh0ayZr56lIF6DpzYIZgAAbCnWOPa/NvLOPMOiQO2n2fM5jir9a07yHIm6lWGdbMg==;EndpointSuffix=core.windows.net";

    /// <summary>
    /// Name of the Cloud Share - Hosts directories.
    /// </summary>
    private const string fileShare = "fileshare";

    /// <summary>
    /// Name of a Directory within the Share
    /// </summary>
    private const string storageDirectory = "storagedirectory";

    /// <summary>
    /// The Cloud File
    /// </summary>
    private CloudFile shapeIndexCloudFile;

    /// <summary>
    /// The Linked Storage Account
    /// </summary>
    private CloudStorageAccount storageAccount;

    /// <summary>
    /// The Cloud Client
    /// </summary>
    private CloudFileClient fileClient;

    /// <summary>
    /// The Cloud Share - Hosts Directories
    /// </summary>
    private CloudFileShare share;

    /// <summary>
    /// The Directory in the share that will host the Cloud file
    /// </summary>
    private CloudFileDirectory dir;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        // Disable TLS cert checks only while in Unity Editor (until Unity adds support for TLS)
#if UNITY_EDITOR
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
#endif

        // Set the Status text to loading, whilst attempting connection to Azure.
        azureStatusText.text = "Loading...";

        //Creating the references necessary to log into Azure and check if the Storage Directory is empty
        CreateCloudIdentityAsync();
    }

    /// <summary>
    /// Create the references necessary to log into Azure
    /// </summary>
    private async void CreateCloudIdentityAsync()
    {
        // Retrieve storage account information from connection string
        storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        // Create a file client for interacting with the file service.
        fileClient = storageAccount.CreateCloudFileClient();

        // Create a share for organizing files and directories within the storage account.
        share = fileClient.GetShareReference(fileShare);

        await share.CreateIfNotExistsAsync();

        // Get a reference to the root directory of the share.
        CloudFileDirectory root = share.GetRootDirectoryReference();

        // Create a directory under the root directory
        dir = root.GetDirectoryReference(storageDirectory);

        await dir.CreateIfNotExistsAsync();

        //Check if the there is a stored text file containing the list
        shapeIndexCloudFile = dir.GetFileReference("TextShapeFile");

        if (!await shapeIndexCloudFile.ExistsAsync())
        {
            // File not found, enable gaze for shapes creation
            Gaze.instance.GazeEnabled = true;

            azureStatusText.text = "No Shape\nFile!";
        }
        else
        {
            // The file has been found, disable gaze and get the list from the file
            Gaze.instance.GazeEnabled = false;

            azureStatusText.text = "Shape File\nFound!";

            await ReplicateListFromAzureAsync();
        }
    }

    /// <summary>
    /// Call to the Azure Function App to request a Shape.
    /// </summary>
    public async void CallAzureFunctionForNextShape()
    {
        int azureRandomInt = 0;

        // Call Azure function
        HttpWebRequest webRequest = WebRequest.CreateHttp(azureFunctionEndpoint);

        WebResponse response = await webRequest.GetResponseAsync();

        // Read response as string
        using (Stream stream = response.GetResponseStream())
        {
            StreamReader reader = new StreamReader(stream);

            String responseString = reader.ReadToEnd();

            //parse result as integer
            Int32.TryParse(responseString, out azureRandomInt);
        }

        //add random int from Azure to the ShapeIndexList
        ShapeFactory.instance.shapeHistoryList.Add(azureRandomInt);

        ShapeFactory.instance.CreateShape(azureRandomInt, false);

        //Save to Azure storage
        await UploadListToAzureAsync();
    }

    /// <summary>
    /// Upload the locally stored List to Azure
    /// </summary>
    private async Task UploadListToAzureAsync()
    {
        // Uploading a local file to the directory created above
        string listToString = string.Join(",", ShapeFactory.instance.shapeHistoryList.ToArray());

        await shapeIndexCloudFile.UploadTextAsync(listToString);
    }

    ///<summary>
    /// Get the List stored in Azure and use the data retrieved to replicate 
    /// a Shape creation pattern
    ///</summary>
    private async Task ReplicateListFromAzureAsync()
    {
        string azureTextFileContent = await shapeIndexCloudFile.DownloadTextAsync();

        string[] shapes = azureTextFileContent.Split(new char[] { ',' });

        foreach (string shape in shapes)
        {
            int i;

            Int32.TryParse(shape.ToString(), out i);

            ShapeFactory.instance.shapeHistoryList.Add(i);

            ShapeFactory.instance.CreateShape(i, true);

            await Task.Delay(500);
        }

        Gaze.instance.GazeEnabled = true;

        azureStatusText.text = "Load Complete!";
    }
}
