using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using TechTweaking.Bluetooth;

public class FAMBluetooth : MonoBehaviour {

	private  BluetoothDevice device;
	public Text statusText;

  private float connectTimer = 4.00f;
  private float delay = 1.00f;
  private float timer = 0f;
  private uint count = 0;
  private byte[] bytes;


	void Awake () {
		BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
		device = new BluetoothDevice();

    //device.MacAddress = "XX:XX:XX:XX:XX:XX";
    device.Name = "HC-06";
		device.setEndByte (10);
	}
	


	public void connect() {
		statusText.text = "Status : ...";
		device.connect();
	}

	public void disconnect() {
		device.close();
	}
    
  void Update() {

    if(timer < 0f) {
      if(device.IsConnected) {
        if(count % 2 == 1) {
          //bytes = System.Text.Encoding.UTF8.GetBytes("a\n");
          bytes = System.Text.Encoding.UTF8.GetBytes("{\"pan\":180,\"tilt\":0,\"volume\":100,\"expression\":0,\"speech\":\"Hello World.\"}\n");
          device.send(bytes);
        } else {
          //bytes = System.Text.Encoding.UTF8.GetBytes("b\n");
          bytes = System.Text.Encoding.UTF8.GetBytes("{\"pan\":0,\"tilt\":180,\"volume\":100,\"expression\":0,\"speech\":\"Hello World.\"}\n");
          device.send(bytes);
        } 
        timer = delay;
        count ++;
      } else {
        device.connect();
        timer = connectTimer;
      }
    } else {
      timer -= Time.deltaTime;
    }


  }


}
