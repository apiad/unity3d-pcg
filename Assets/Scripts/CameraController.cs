using UnityEngine;
using System.Collections;
using System.IO;

public class CameraController : MonoBehaviour {

	private Vector3 mousePosition;
    private TerrainGenerator world;

    private bool autopilot;
    private Vector3 destination;
    private Vector3 velocity;

    private GameObject target;
    private bool wireframe;
    private bool showGui;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
        world = GameObject.Find("World").GetComponent<TerrainGenerator>();

        //this.transform.position = new Vector3(Random.value - 0.5f, 0.1f, Random.value - 0.5f) * world.Width / 2;
        //this.transform.LookAt(Vector3.up * 50f);

        target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        var pos = this.transform.position;
        var terrainPos = world != null ? world.transform.InverseTransformPoint(pos) : pos;
        float terrainHeight = world != null ? Mathf.Max(world.SampleHeight(terrainPos.x, terrainPos.z), 0) : 0;
        
        float cameraHeight = this.transform.position.y;
        float height = cameraHeight - terrainHeight;
		float speed = Time.deltaTime * (height + 1);

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) {
            direction += this.transform.TransformDirection(Vector3.forward);
            autopilot = false;
		}
		else if (Input.GetKey(KeyCode.S)) {
            direction += this.transform.TransformDirection(Vector3.back);
            autopilot = false;
        }

		if (Input.GetKey(KeyCode.A)) {
            direction += this.transform.TransformDirection(Vector3.left);
            autopilot = false;
        }
		else if (Input.GetKey(KeyCode.D)) {
            direction += this.transform.TransformDirection(Vector3.right);
            autopilot = false;
        }

		if (Input.GetKey(KeyCode.Space)) {
            direction += Vector3.up;
            autopilot = false;

            //this.rigidbody.useGravity = false;
        }
        else if (Input.GetKey(KeyCode.LeftControl)) {
            direction += Vector3.down;
            autopilot = false;

            //this.rigidbody.useGravity = false;
        }
        else {
            //this.rigidbody.useGravity = true;
        }

        if (autopilot) {
            if (Vector3.Distance(target.transform.position, destination) < speed) {
                //destination = Random.insideUnitSphere * 100f + velocity.normalized * 100f + target.transform.position;

                //if (destination.magnitude > world.Width / 2f) {
                    destination = Random.insideUnitSphere * world.Width / 2f;
                //}

                var terrainDestination = world.transform.InverseTransformPoint(destination);
                float destinationHeight = Mathf.Max(0f, world.SampleHeight(terrainDestination.x, terrainDestination.z));

                if (destination.y > 100f) {
                    destination.y = 100f;
                }

                if (destination.y < destinationHeight + 0f) {
                    destination.y = destinationHeight + 0f;
                }
            }

            Vector3 start = target.transform.position;
            target.transform.Translate((destination - target.transform.position).normalized * speed, Space.World);
            SnapToTerrain(target, 0f);
            Vector3 end = target.transform.position;
            Vector3 displ = (end - start).normalized;

            this.transform.position = Vector3.SmoothDamp(pos, target.transform.TransformPoint(-displ * 10), ref velocity, 2f);
            SnapToTerrain(this.gameObject, 20f);
            this.transform.rotation = Quaternion.LookRotation(velocity);
        }
        else {
            velocity = Vector3.Lerp(velocity, direction * speed, 0.1f);
            this.transform.Translate(velocity, Space.World);

            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            float rx = 1f;
            float ry = 1f;

            this.transform.Rotate(-dy * ry, dx * rx, 0);
            this.transform.LookAt(this.transform.TransformPoint(Vector3.forward));

            SnapToTerrain(this.gameObject, 1f);
        }

        //if (Input.GetMouseButtonDown(0)) {
        //    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    sphere.transform.localScale *= 0.1f; 
        //}

        if (Input.GetKeyUp(KeyCode.P)) {
            TurnAutoPilot();
        }

        //if (this.transform.position.y < 0) {
        //    RenderSettings.fog = true;
        //}
        //else {
        //    RenderSettings.fog = false;
        //}

        if (Input.GetKeyUp(KeyCode.G)) {
            showGui = !showGui;
            Screen.lockCursor = !showGui;
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            wireframe = !wireframe;
        }

        if (Input.GetKeyUp(KeyCode.C)) {
            if (!Directory.Exists("Screenshots")) {
                Directory.CreateDirectory("Screenshots");
            }
            
            string path = string.Format("Screenshots/Screenshot-{0}.png", System.DateTime.Now.ToFileTimeUtc());
            Application.CaptureScreenshot(path);
            Debug.Log("Saving screenshot: " + path);
        }

        if (Input.GetKeyUp(KeyCode.R)) {
            recording = !recording;

            if (recording) {
                Time.captureFramerate = 25;
                recordingFolder = string.Format("Recording-{0}", System.DateTime.Now.ToFileTimeUtc());
                Directory.CreateDirectory(recordingFolder);
                Debug.Log("Recording started at: " + recordingFolder);
            }
            else {
                Debug.Log("Recording stopped");
                Time.captureFramerate = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.B)) {
            Vector3 position = this.transform.position;
            Vector3 rotation = this.transform.rotation.eulerAngles;

            PlayerPrefs.SetFloat("PosX", position.x);
            PlayerPrefs.SetFloat("PosY", position.y);
            PlayerPrefs.SetFloat("PosZ", position.z);

            PlayerPrefs.SetFloat("RotX", rotation.x);
            PlayerPrefs.SetFloat("RotY", rotation.y);
            PlayerPrefs.SetFloat("RotZ", rotation.z);

            PlayerPrefs.Save();
        }
        else if (Input.GetKey(KeyCode.N)) {
            float px = PlayerPrefs.GetFloat("PosX", transform.position.x);
            float py = PlayerPrefs.GetFloat("PosY", transform.position.y);
            float pz = PlayerPrefs.GetFloat("PosZ", transform.position.z);

            float rx = PlayerPrefs.GetFloat("RotX", transform.rotation.eulerAngles.x);
            float ry = PlayerPrefs.GetFloat("RotY", transform.rotation.eulerAngles.y);
            float rz = PlayerPrefs.GetFloat("RotZ", transform.rotation.eulerAngles.z);

            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(px, py, pz), speed);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(rx, ry, rz), Time.deltaTime * 45);
        }

        if (recording) {
            Application.CaptureScreenshot(string.Format("{0}/{1:D04}.png", recordingFolder, Time.frameCount));
        }

        Debug.DrawRay(this.transform.position, this.transform.TransformDirection(Vector3.forward * 100f));
	}

    void OnPreRender() {
        GL.wireframe = wireframe;
    }

    private void TurnAutoPilot() {
        autopilot = !autopilot;
        destination = this.transform.position + this.transform.TransformDirection(Vector3.forward * 10f);
        target.transform.position = destination;
    }

    private void SnapToTerrain(GameObject obj, float height) {
        var pos = obj.transform.position;
        var terrainPos = world != null ? world.transform.InverseTransformPoint(pos) : pos;
        //float terrainHeight = world.SampleHeight(terrainPos.x, terrainPos.z);
        float terrainHeight = world != null ? Mathf.Max(world.SampleHeight(terrainPos.x, terrainPos.z), 0f) : 0;

        if (pos.y < terrainHeight + height) {
            obj.transform.position = new Vector3(pos.x, terrainHeight + height, pos.z);
        }
        else if (pos.y > 10000) {
            obj.transform.position = new Vector3(pos.x, 10000, pos.z);
        }
    }

    void OnGUI() {
        if (!showGui)
            return;

        GL.wireframe = false;

        GUI.Box(new Rect(20, 20, 200, 25), string.Format("Open Nodes: {0}", Generator.OpenNodes));
        GUI.Box(new Rect(20, 50, 200, 25), string.Format("Steps per Second: {0:0.0}", Generator.StepsPerSecond));
        GUI.Box(new Rect(20, 80, 200, 25), string.Format("Wireframe: {0}", GL.wireframe));

        if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 30), "Restart")) {
            world.Restart();
        }
        if (GUI.Button(new Rect(Screen.width - 120, 55, 100, 30), "Quit")) {
            Application.Quit();
        }
    }

    private string recordingFolder;
    private bool recording;
}
