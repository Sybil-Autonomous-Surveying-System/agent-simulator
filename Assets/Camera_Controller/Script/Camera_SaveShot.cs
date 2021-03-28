using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Camera))]
public class Camera_SaveShot : MonoBehaviour
{
    [Header("Control Properties")]
    [SerializeField] private float maxTilt = 30F;
    private Camera myCamera;
    private bool shotNextFrame;
    private float finalTilt;
    private Camera_inputs input;
    private int width;
    private int height;
    private string pathName = "./Screenshot";
    private void Awake()
    {
        myCamera = GetComponent<Camera>();
        input = GetComponent<Camera_inputs>();
        if (myCamera.tag == "drone_cam") {
            try
            {
                if (!Directory.Exists(pathName))
                {
                    Directory.CreateDirectory(pathName);
                }
            }
            catch (IOException ex)
            {
                Debug.Log(ex.Message);
            }
            if (myCamera.targetTexture == null)
            {
                myCamera.targetTexture = new RenderTexture(256, 256, 24);
            }
            else
            {
                width = myCamera.targetTexture.width;
                height = myCamera.targetTexture.height;
            }
            myCamera.gameObject.SetActive(true);
        }
    }

    string SnapshotName()
    {
        //Application.persistentDataPath afterwards Application.dataPath
        return string.Format("{0}/snap_{1}.png", pathName
            , System.DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss"));
    }
    public void TakeScreenshot()
    {

        myCamera.gameObject.SetActive(true);
        //myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 24);
        //shotNextFrame = true;

    }
    public void HandleTilt()
    {
        if (input.Tilt != 0)
        {
            //Y have to clamp and lerp
            float tilt = input.Tilt * maxTilt;
            // last value is lerpspeed
            finalTilt = Mathf.Lerp(finalTilt, tilt, Time.deltaTime );
            //Euler rot = Quaternion.Euler(0f, finalTilt, 0f);
            //myCamera.transform.Rotate(Vector3.right, finalTilt);

            myCamera.transform.eulerAngles = new Vector3(finalTilt, myCamera.transform.eulerAngles.y, myCamera.transform.eulerAngles.z);
            //mtransform.Rotate(rot);
        }
    }
    public void Update()
    {
        if (input.TakeScreenshot == 1 && myCamera.tag == "drone_cam")
        {

            TakeScreenshot();
            // bootleg stuff?
            //this.OnPostRender();
            //bootleg reset value??
            shotNextFrame = true;
            input.TakeScreenshot = 0;
        };
        HandleTilt();
    }
    public void LateUpdate()
    {
        if (shotNextFrame)
        {
            Texture2D renderResult = new Texture2D(width, height, TextureFormat.ARGB32, false);
            myCamera.Render();
            RenderTexture.active = myCamera.targetTexture;
            renderResult.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(SnapshotName(), byteArray);
            //myCamera.gameObject.SetActive(false);
            Debug.Log("DONE");
            shotNextFrame = false;
            myCamera.gameObject.SetActive(false);
            myCamera.gameObject.SetActive(true);
            //Camera.main.enabled = false;
            //Camera.main.enabled = true;
            //UnityEditor.AssetDatabase.Refresh();
        }
    }
}
