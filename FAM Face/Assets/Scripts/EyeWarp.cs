using UnityEngine;
using System.Collections;

public class EyeWarp : MonoBehaviour {



  public Vector3 mag;
	public Vector3 off;
	public float spd;
  private Vector3 size;

  void Start () {
    size = transform.localScale;
  }
  
	// Update is called once per frame
	void Update () {
	
    transform.localScale = new Vector3(size.x+mag.x*Mathf.Sin(off.x+Time.time*spd),
                                 size.y+mag.y*Mathf.Sin(off.y+Time.time*spd),
                                 size.z+mag.z*Mathf.Sin(off.z+Time.time*spd));
  
  
	}
}
