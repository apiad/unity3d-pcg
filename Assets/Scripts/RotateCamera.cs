using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

    Generator tree;

	// Use this for initialization
	void Start () {
        tree = GameObject.Find("Tree").GetComponent<Generator>();
	}
	
	// Update is called once per frame
	void Update () {
        float distance = this.transform.position.magnitude;

        if (Input.GetKey(KeyCode.W)) {
            transform.Translate(Vector3.forward * Time.deltaTime * distance);
        }
        else if (Input.GetKey(KeyCode.S)) {
            transform.Translate(Vector3.back * Time.deltaTime * distance);
        }

        distance = this.transform.position.magnitude;
        
        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(Vector3.left * Time.deltaTime * distance);
        } 
        else if (Input.GetKey(KeyCode.D)) {
            transform.Translate(Vector3.right * Time.deltaTime * distance);
        }
        
        if (Input.GetKey(KeyCode.Space)) {
            transform.Translate(Vector3.up * Time.deltaTime * distance);
        }
        else if (Input.GetKey(KeyCode.LeftControl)) {
            transform.Translate(Vector3.down * Time.deltaTime * distance);
        }

        transform.position = Vector3.ClampMagnitude(transform.position, distance);
        transform.LookAt(Vector3.up * 3f);
	}

    void OnGUI() {
        GL.wireframe = false;

        GUI.Box(new Rect(20, 20, 200, 25), string.Format("Open Nodes: {0}", Generator.OpenNodes));
        GUI.Box(new Rect(20, 50, 200, 25), string.Format("Steps per Second: {0:0.0}", Generator.StepsPerSecond));
        GUI.Box(new Rect(20, 80, 200, 25), string.Format("Wireframe: {0}", GL.wireframe));

        if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 30), "Restart")) {
            tree.Restart();
        }
        if (GUI.Button(new Rect(Screen.width - 120, 55, 100, 30), "Quit")) {
            Application.Quit();
        }
    }
}
