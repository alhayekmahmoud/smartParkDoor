using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System;
using UnityEngine.UI;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;

using System.IO.Ports;



public class VisionAPItoCAM : MonoBehaviour
{

    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    public GameObject scoreElement;
    public Transform scoreboardContent;


    SerialPort sp = new SerialPort("COM3", 9600);

    private string ApiKey = "d54d403cdf394c6c80812706e8eddd3f";
    static string endpoint = "https://mahmoudalhayek.cognitiveservices.azure.com/";
    //public string ApiKey = "d99f5a16354749a0b2b5c4cf179e57ed";
    // replace with your own key
    // Emotion URL can be /tag or /analyze depending on what you need the vision service to be - View Microsoft Documentation for more details
    // public string emotionURL = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/analyze";
    //public string emotionURL = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/detect";

    public List<AnalyzedObject> imageCategories = new List<AnalyzedObject>();

    public string fileName { get; private set; }
    string responseData;

    //public Text txt;

    // Use this for initialization
    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    void Start()
    {
        fileName = System.IO.Path.Combine(Application.streamingAssetsPath, "lion.jpg");





    }

    public void getData(string fileName)
    {
        ComputerVisionClient client = Authenticate(endpoint, ApiKey);

        ReadFile(client, fileName);
        //StartCoroutine(GetDataFromImages(bytes));

    }

    /*
        * * AUTHENTICATE
        * * Creates a Computer Vision client used by each example.
        * */
    public static ComputerVisionClient Authenticate(string endpoint, string key)
    {
        ComputerVisionClient client =
          new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
          { Endpoint = endpoint };
        return client;
    }
    /*
     * * READ FILE - URL 
     * * Extracts text. 
     * */

    public async Task ReadFile(ComputerVisionClient client, string fileName)
    {


        // Read text from URL
        var textHeaders = await client.ReadInStreamAsync(File.OpenRead(fileName));
        // After the request, get the operation location (operation ID)
        string operationLocation = textHeaders.OperationLocation;
        Thread.Sleep(2000);
        // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
        // We only need the ID and not the full URL
        const int numberOfCharsInOperationId = 36;
        string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

        // Extract the text
        ReadOperationResult results;

        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
        }
        while ((results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted));

        // Display the found text.
        
        string sign = "";
        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        int count=0;
        string[] linesArry = new string[textUrlFileResults[0].Lines.Count];
        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                foreach (char c in line.Text)
                {
                    if (Char.IsLetter(c) || Char.IsNumber(c))
                    {

                        sign += c;

                    }
                }
                //search(sign);
                linesArry[count]=sign;
                count++;
                sign = "";
            }
        }
        
        for (int i = 0; i < linesArry.Length; i++)
        {
            search(linesArry[i]);
        }
        //if (sign == "")
        //{

        //    search(sign);
        //}
    }


    public void search(string autoSignNumber)
    {
        StartCoroutine(searchForUser(autoSignNumber));
    }
    private IEnumerator searchForUser(string autoSignNumber)
    {
        bool ended=false;
        //Get all the users data ordered by kills amount
        var DBTask = DBreference.Child("users").OrderByChild("autoSignNumber").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;


            //Loop through every users ID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();

                string _autSignNumber = childSnapshot.Child("autoSignNumber").Value.ToString();
                if (_autSignNumber != null )
                {
                    if (_autSignNumber == autoSignNumber)
                    {
                        gameObject.GetComponent<CameraController>().chickUserData(true);
                        Debug.Log(_autSignNumber + " ok");
                        Debug.Log(username + " ok");
                        Debug.Log("welcome " + username + " you can pass");

                        ended = true;
                        break;

                        //txt.text = "welcome " + username + " you can pass";
                    }
                }
                
            }

            if (!ended)
            {
                Debug.Log(autoSignNumber + " is not registed sorry you cann't pass");
                gameObject.GetComponent<CameraController>().chickUserData(false);
            }
               
        }
    }
}

//        public void getData(byte[] bytes)
//        {
//            StartCoroutine(GetDataFromImages(bytes));
//        }

//        IEnumerator GetDataFromImages(byte[] bytes)
//        {
//            var headers = new Dictionary<string, string>() {
//                { "Ocp-Apim-Subscription-Key", ApiKey },
//                { "Content-Type", "application/octet-stream" }
//            };

//            WWW www = new WWW(emotionURL, bytes, headers);

//            yield return www;
//            responseData = www.text;
//            ParseJSONData(responseData);
//        }

//        // Parsing Data
//        public void ParseJSONData(string respString)
//        {
//            JSONObject dataArray = new JSONObject(respString);
//            AnalyzedObject _imageObject = ConvertObjectToFoundImageObject(dataArray);
//        }

//        private AnalyzedObject ConvertObjectToFoundImageObject(JSONObject obj)
//        {
//            JSONObject _categories = obj.list[0]; // Get the list of categories
//            return new AnalyzedObject(_categories);
//        }
//}
