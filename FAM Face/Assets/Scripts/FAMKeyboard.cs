using UnityEngine; 
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using TechTweaking.Bluetooth;
using Boomlagoon.JSON;
using AUP;

public class FAMKeyboard : MonoBehaviour {

  //face and speech control variables
  private float eyeBlend = 0f;
  public float blendSpeed = 1f;
  private int previousExpression = 0;
  private Color faceColor = new Color(0.66f,1.0f,0.5f);
  private Color currentFaceColor = new Color(0.66f,1.0f,0.5f);

  public GameObject face;
  public SkinnedMeshRenderer leftEye;
  public SkinnedMeshRenderer rightEye;
  private TextToSpeechPlugin textToSpeechPlugin;

  //JSON Variables
  private string jsonStr = "";
  public string speech = "";
  public int expression = 0;
  public int volume = 100;

  //bluetooth receiver variables
  private const string UUID = "0acc9c7c-48e1-41d2-acaa-610d1a7b085e";
  private  BluetoothDevice device;
  public Text dataToSend;

  //Initialization
  private void Awake(){
    BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
    BluetoothAdapter.OnDevicePicked += HandleOnDevicePicked;
    BluetoothAdapter.OnClientRequest += HandleOnClientRequest;
    BluetoothAdapter.startServer (UUID, 1000); // Init server

    textToSpeechPlugin = TextToSpeechPlugin.GetInstance();
    textToSpeechPlugin.SetDebug(0);
    textToSpeechPlugin.Initialize();
  }

	// Update is called once per frame
	void Update () {

    //read keyboard input
    foreach (char c in Input.inputString) {
      jsonStr += c;
      if (c == '}') { //if end of message, parse JSON
        Debug.Log(jsonStr);
        setFaceData(jsonStr); 
        jsonStr = "";
      }
    }

    //update FAM Face
    setExpression();
    setFaceColor();
    
	}
    
  //Android Bluetooth Direct Connection
  void HandleOnClientRequest (BluetoothDevice device) {
    this.device = device;
    this.device.setEndByte (10);
    this.device.ReadingCoroutine = ManageConnection;
    this.device.connect();
  }
  void HandleOnDevicePicked (BluetoothDevice device) {
    this.device = device;
    this.device.UUID = UUID;
    device.setEndByte (10);
    device.ReadingCoroutine = ManageConnection;
  }
  void OnDestroy () {
    BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
    BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
    BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
  }
  //Manage Reading Coroutine
  IEnumerator  ManageConnection (BluetoothDevice device) {
    while (device.IsReading) {
      if (device.IsDataAvailable) {
        byte [] msg = device.read ();
        if (msg != null && msg.Length > 0) {
          string content = System.Text.ASCIIEncoding.ASCII.GetString (msg);
          setFaceData(content); 
        }
      }
      yield return null;
    }
  }

  //set face data
  public void setFaceData(string json) {
    previousExpression = expression;
    JSONObject jsonObject = JSONObject.Parse(json);
    if (jsonObject != null) { 
      try {
        volume = (int)(jsonObject.GetValue("volume").Number);
        expression = (int)(jsonObject.GetValue("expression").Number);
        speech = jsonObject.GetValue("speech").ToString();
        faceColor = new Color( (float)(jsonObject.GetValue("red").Number / 255f), 
                               (float)(jsonObject.GetValue("green").Number / 255f),
                               (float)(jsonObject.GetValue("blue").Number / 255f) );
        //textToSpeechPlugin.SetPitch(pitch); //(0-2)
        //textToSpeechPlugin.SetSpeechRate(speechRate); //(0-2)
        textToSpeechPlugin.IncreaseMusicVolumeByValue(volume); //(0-15)
        textToSpeechPlugin.SpeakOut(speech,"id"); 
        eyeBlend = 0f;
      } catch (Exception e) {
        Debug.LogException(e, this);
      }
    }
  }

  //controls face color
  public void setFaceColor() {
    currentFaceColor = Color.Lerp(currentFaceColor, faceColor, Time.deltaTime * blendSpeed);
    face.GetComponent<Renderer>().material.SetColor("_Color",currentFaceColor);
  }
  
  //controls face expression
  public void setExpression() {
    //increase blend shape frame
    eyeBlend = Mathf.Lerp(eyeBlend,100f,Time.deltaTime * blendSpeed);
    //remove all blendshapes
    for (int i = 0; i < 5; i++) {
      leftEye.SetBlendShapeWeight(i,0);
      rightEye.SetBlendShapeWeight(i,0);
    }
    if(previousExpression > 0 && expression != previousExpression) {
      int sub = (previousExpression == 3) ? 0 : 1;
      leftEye.SetBlendShapeWeight(previousExpression-sub,100f-eyeBlend);
      rightEye.SetBlendShapeWeight(previousExpression-1,100f-eyeBlend);
    }
    //set expression
    switch(expression) {
      case 0: //Neutral;
        break;
      case 1: //Surprised; 
        leftEye.SetBlendShapeWeight(0,eyeBlend);
        rightEye.SetBlendShapeWeight(0,eyeBlend);
        break;
      case 2: //Happy; 
        leftEye.SetBlendShapeWeight(1,eyeBlend);
        rightEye.SetBlendShapeWeight(1,eyeBlend);
        break;
      case 3: //Sad; 
        leftEye.SetBlendShapeWeight(3,eyeBlend);
        rightEye.SetBlendShapeWeight(2,eyeBlend);
        break;
      case 4: //Concerned; 
        leftEye.SetBlendShapeWeight(4,eyeBlend);
        rightEye.SetBlendShapeWeight(4,eyeBlend);
        break;
      default: //Neutral
        break;
    } 
  }
    
}
