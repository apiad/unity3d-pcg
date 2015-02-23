using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.I)) {
            this.transform.Translate(Vector3.up * 10 * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.K)) {
            this.transform.Translate(Vector3.down * 10 * Time.deltaTime);
        }
	}
}
