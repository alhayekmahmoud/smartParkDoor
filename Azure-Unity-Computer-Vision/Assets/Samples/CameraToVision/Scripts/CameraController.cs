using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

using System.IO.Ports;

public class CameraController : MonoBehaviour {

    public RawImage image;
    public RectTransform imageParent;
    public AspectRatioFitter imageFitter;


    // Device cameras
    WebCamDevice frontCameraDevice;
    WebCamDevice backCameraDevice;
    WebCamDevice usbCameraDevice;
    WebCamDevice activeCameraDevice;

    WebCamTexture frontCameraTexture;
    WebCamTexture backCameraTexture;
    WebCamTexture usbCameraTexture;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    // set port of your arduino connected to computer
    SerialPort sp = new SerialPort("COM3", 9600);
    bool ReadSignStart = true;
    bool comaraOn = true;


    void Start()
    {
        // Check for device cameras
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No devices cameras found");
            return;
        }

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices.First();
        usbCameraDevice = WebCamTexture.devices.First();

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
        backCameraTexture = new WebCamTexture(backCameraDevice.name);
        usbCameraTexture = new WebCamTexture(usbCameraDevice.name);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;
        usbCameraTexture.filterMode = FilterMode.Trilinear;


        // Set the camera to use by default
        SetActiveCamera(frontCameraTexture);
       // SetActiveCamera(backCameraTexture);

        //open the Serial port
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 1;
        }
        ReadSignStart = true;

    }

    // Set the device camera to use and start it
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
            device.name == cameraToUse.deviceName);

        image.texture = activeCameraTexture;
        image.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();
    }

    // Switch between the device's front and back camera
    public void SwitchCamera()
    {
        SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ?
            backCameraTexture : frontCameraTexture);
    }

    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started
    void Update()
    {
        // Skip making adjustment for incorrect camera data
        if (activeCameraTexture.width < 100)
        {
            Debug.Log("Still waiting another frame for correct info...");
            return;
        }

        // Rotate image to show correct orientation 
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        image.rectTransform.localEulerAngles = rotationVector;

        // Set AspectRatioFitter's ratio
        float videoRatio =
            (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        imageFitter.aspectRatio = videoRatio;

        // Unflip if vertically flipped
        image.uvRect =
            activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        // Mirror front-facing camera's image horizontally to look more natural
        imageParent.localScale =
            activeCameraDevice.isFrontFacing ? fixedScale : defaultScale;
        if (ReadSignStart)
        {
            readSensor();
        }
        

    }
   
    public void readSensor()
    {
        

        if (sp.IsOpen)
        {
           
            try
            {
                if (sp.ReadByte() == 1)
                {
                    if (comaraOn)
                    {
                        ReadSignStart = false;
                        print("Camera1On\n");
                        comaraOn = false;
                        getPhotoData();
                    }
                }
              
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void chickUserData(bool ValedUser)
    {
        if (ValedUser)
        {
            sp.Write("#Camera1On\n");
            activeCameraTexture.Stop();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        //StartCoroutine(pass());
        else{
            //sp.Write("#ResetC1\n");

            activeCameraTexture.Stop();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
            //StartCoroutine(reStart());






    }
    //IEnumerator pass()
    //{
    //   // yield return new WaitForEndOfFrame();
    //    sp.Write("#Camera1On\n");
    //    StartCoroutine(reStart());
        
    //}
    //public IEnumerator reStart()
    //{
    //   // yield return new WaitForEndOfFrame();
    //    sp.Write("Reset");
        
    //    activeCameraTexture.Stop();
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //}

    public void getPhotoData()
    {
        StartCoroutine(TakePhoto());
    }

    public void Exit()
    {
        activeCameraTexture.Stop();
    }


    IEnumerator TakePhoto()
    {
        // NOTE - you almost certainly have to do this here:
        string _currentImagePath;

        yield return new WaitForEndOfFrame();

        // it's a rare case where the Unity doco is pretty clear,
        // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
        // be sure to scroll down to the SECOND long example on that doco page 

        Texture2D photo = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
        photo.SetPixels(activeCameraTexture.GetPixels());
        photo.Apply();

        activeCameraTexture.Pause();

        //Encode to a PNG
        byte[] bytes = photo.EncodeToPNG();

        //Write out the PNG. Of course you have to substitute your_path for something sensible
     
        var fileName = string.Format(@"Image_{0:yyyy-MM-dd_hh-mm-ss-tt}.png", DateTime.Now);
        _currentImagePath = Application.persistentDataPath + "/" + fileName;

        File.WriteAllBytes(System.IO.Path.Combine(_currentImagePath), bytes);
      
        string filePath = System.IO.Path.Combine(_currentImagePath);
      
        gameObject.GetComponent<VisionAPItoCAM>().getData(filePath);
       
    }

    //IEnumerator TakePhoto()
    //{
    //    // NOTE - you almost certainly have to do this here:

    //    yield return new WaitForEndOfFrame();

    //    // it's a rare case where the Unity doco is pretty clear,
    //    // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
    //    // be sure to scroll down to the SECOND long example on that doco page 

    //    Texture2D photo = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
    //    photo.SetPixels(activeCameraTexture.GetPixels());
    //    photo.Apply();

    //    activeCameraTexture.Pause();

    //    //Encode to a PNG
    //    byte[] bytes = photo.EncodeToPNG();
    //    //Write out the PNG. Of course you have to substitute your_path for something sensible
    //    //File.WriteAllBytes(System.IO.Path.Combine(Application.streamingAssetsPath, "photo.png"), bytes);
    //    gameObject.GetComponent<VisionAPItoCAM>().getData(bytes);
    //}
}
