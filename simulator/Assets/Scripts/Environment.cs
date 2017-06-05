using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

public class Environment : MonoBehaviour {

	[SerializeField] private GameObject cam;
	[SerializeField] private GameObject car;
	[SerializeField] private GameObject markers;
    public GameObject cube;

	private List<Vector3> markersPos;
    private List<Vector3> sampledRoad;
	private Vector3 lastProj;
	private Vector3 currProj;
	private Vector3 unitVectorAlongRoad;
	private Vector3 unitVectorAlongRoadOneStepAhead;
	private float nextAngle;
	private float distanceFromRoad = 0.0f;
	private float speedAlongRoad;
	private CarController carController;
	private bool collisionDetected = false;
	private Camera m_cam;
    private Color32[] rendered_screen = null;
    private bool isUpdating = false;

	internal class PairDistanceVector {
		public int number;
		public float dist;

		public PairDistanceVector (int number, float dist) {
			this.number = number;
			this.dist = dist;
		}

        
        public override string ToString() {
            return "Number=" + number + "; dist=" + dist;
        }
	}

	internal class ComparePairDistanceVector : IComparer<PairDistanceVector> {
		public int Compare(PairDistanceVector x, PairDistanceVector y) {
			return (int)(x.dist - y.dist);
		}
	}

	public byte[] GetFrame() {
		byte[] res = Color32ArrayToByteArray(rendered_screen);
		return res;
	}

	public Vector3 GetPosition() {
		return car.transform.position;
	}

	public Vector3 GetForward() {
		return car.transform.forward;
	}

	public void MakeAction(float[] actions) {
		carController.Move(actions[0], actions[1], actions[1], 0f);
		if (actions[2] != 0.0f) {
			car.GetComponent<Rigidbody>().velocity = Vector3.zero;
			car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			SetCarPosition();
		}
	}

	private void SetCarPosition(){
		int id = UnityEngine.Random.Range(0, markersPos.Count);
		car.transform.forward = GetRoadDirectionOnMarker(id);
		car.transform.position = markersPos[id] + car.transform.right * 2.5f;
	}

	private byte[] Color32ArrayToByteArray(Color32[] colors) {
		if (colors == null || colors.Length == 0)
			return null;

		int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
		int length = lengthOfColor32 * colors.Length;
		byte[] bytes = new byte[length];

		GCHandle handle = default(GCHandle);
		try {
			handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			Marshal.Copy(ptr, bytes, 0, length);
		} finally {
			if (handle != default(GCHandle))
				handle.Free();
		}

		return bytes;
	}

	void Start() {
		carController = car.GetComponent<CarController>();
		m_cam = cam.GetComponent<Camera>();
		markersPos = new List<Vector3> ();
		lastProj = GetPosition();
		currProj = lastProj;
		speedAlongRoad = 0.0f;
		foreach (Transform child in markers.transform) {
			markersPos.Add(child.position);
		}
		GenerateFileWithWaypoints();
		SetCarPosition();
        sampledRoad = sampleRoad();
        //printCubeSpline();
    }

	void ComputeDistance() {
		List<PairDistanceVector> distances = new List<PairDistanceVector> ();
		for (int i = 0; i < sampledRoad.Count; ++i) {
			distances.Add(new PairDistanceVector (i, Vector3.Distance(sampledRoad[i], GetPosition())));
		}
		distances.Sort(new ComparePairDistanceVector ());
		// We want AB vector along the road. Therefore A must have a lower indice 
		// in markerPos than B (except when it's the first and last items, therefore the 
		// last item need to be first.
		Vector3 a, b, c;
		if (distances[0].number == 0 && distances[1].number == sampledRoad.Count - 1) {
			a = sampledRoad[distances[1].number];
			b = sampledRoad[distances[0].number];
			c = sampledRoad[distances[0].number + 1];
		} else if (distances[1].number == 0 && distances[0].number == sampledRoad.Count - 1) {
			a = sampledRoad[distances[0].number];
			b = sampledRoad[distances[1].number];
			c = sampledRoad[distances[1].number + 1];
        } else if (distances[0].number > distances[1].number) {
			a = sampledRoad[distances[1].number];
			b = sampledRoad[distances[0].number];
			c = sampledRoad[(distances[0].number + 1) % sampledRoad.Count];
        } else {
			a = sampledRoad[distances[0].number];
			b = sampledRoad[distances[1].number];
			c = sampledRoad[(distances[1].number + 1) % sampledRoad.Count];
        }
		// u is the unit vector associated to AB
		Vector3 u = (b - a).normalized;
		unitVectorAlongRoad = u;
		unitVectorAlongRoadOneStepAhead = (c - b).normalized;
		//v is the vector associated to AC (C is the position of the car)
		Vector3 v = GetPosition() - a;

		// The projected point on the vector AB is (AB.AC) * AB / |AB|² + A.
		// In this case with u = AB/|AB|, proj = (AC.u)*u + a
		//        *C
		//       /|
		//      / |
		//     /  |
		//    /   |
		//  A*----*----*B
		//      proj

		lastProj = currProj;
		currProj = Vector3.Dot(u, v) * u + a;

		Vector3 diffProj = currProj - lastProj;
		speedAlongRoad = Mathf.Sign(Vector3.Dot(u, diffProj)) * diffProj.magnitude;
        // Square distance, Pythagorean theorem in the triangle A-C-proj
        // Negative if left to the road; Positive if right to the road

        distanceFromRoad = Mathf.Sign(Vector3.Cross(u, v.normalized).y) * (GetPosition() - currProj).sqrMagnitude;
	
		// Compute the angle between AB and BC to get the next angle with the road
		// Since it's control points, it's not as precise as it should be (it should be spine interpolated)
		// But we get an approximation of the road shape
		float angle = Mathf.Acos(Vector3.Dot(unitVectorAlongRoad, unitVectorAlongRoadOneStepAhead));
		nextAngle = Mathf.Sign(Vector3.Cross(unitVectorAlongRoad, unitVectorAlongRoadOneStepAhead).y) * angle;
	}

    IEnumerator UpdateScreenBuffer() {
        isUpdating = true;

        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        rendered_screen = tex.GetPixels32();

        Destroy(tex);

        isUpdating = false;
    }

	//Color32[] ReadScreenImmediate() {
        //RenderTexture currentRT = RenderTexture.active;
        //RenderTexture auxRT = new RenderTexture (Screen.width, Screen.height, 16);
        //RenderTexture.active = auxRT;
        //m_cam.targetTexture = auxRT;
        //m_cam.Render();
        //Texture2D image = new Texture2D(auxRT.width, auxRT.height);
        //image.ReadPixels(new Rect(0, 0, auxRT.width, auxRT.height), 0, 0);
        //image.Apply();
        //RenderTexture.active = currentRT;
        //m_cam.targetTexture = null;
        //return image.GetPixels32();
        /*Texture2D tex = cam.GetComponent<CameraCapture>().RenderResult;
		if (tex == null)
			return null;
		return tex.GetPixels32();*/
    //}

    void FixedUpdate() {
        if (!isUpdating) {
            isUpdating = true;
            StartCoroutine(UpdateScreenBuffer());
        }
        //byte[] aux2 = GetFrame();
        //List<float> aux = GetState();
        //string res = "[";
        //foreach (float f in aux) {
        //    res += f.ToString() + ", ";
        //}
        //res += "]";
        //print(res);
        //if (aux2 == null)
        //    print("Frame is null...");
        //else
        //    print("[" + aux2[0] + ";" + aux2[1] + ";" + aux2[2] + ";" + aux2[3] + "]");
    }

    void GenerateFileWithWaypoints() {
		List<Vector3_base> data = new List<Vector3_base> ();
		foreach (Vector3 v in markersPos) {
			data.Add(new Vector3_base (v));
		}
		string json = JsonConvert.SerializeObject(data.ToArray());
		string path = Application.dataPath + "/waypoints_" + SceneManager.GetActiveScene().name + ".txt";
		System.IO.File.WriteAllText(path, json);
	}

	public List<float> GetState() {
		ComputeDistance();
		List<float> res = new List<float>();
		res.Add(distanceFromRoad);
		res.Add(speedAlongRoad);
		res.AddRange(GetValues(GetPosition()));
		res.AddRange(GetValues(currProj));
		res.Add(Convert.ToSingle(collisionDetected));
		res.AddRange(GetValues(unitVectorAlongRoad));
		res.AddRange(GetValues(GetForward()));
		res.Add(nextAngle);
        
		collisionDetected = false;

		return res;
	}

    //void FixedUpdate() {
    //    ComputeDistance();
    //    print("Next angle =" + nextAngle);
    //}

    private List<float> GetValues(Vector3 v){
		List<float> aux = new List<float>();
		aux.Add(v.x);
		aux.Add(v.y);
		aux.Add(v.z);
		return aux;
	}

	public void DetectedCollision() {
		collisionDetected = true;
	}

	private Vector3 GetRoadDirectionOnMarker(int markerId){
		int next = (markerId + 1) % markersPos.Count;
		int prev = markerId - 1;
		if (prev == -1)
			prev = markersPos.Count - 1;
		return (markersPos[next] - markersPos[prev]) / (2.0f / markersPos.Count);
	}

    private Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        Vector3 m0 = (p2 - p0) / 2.0f;
        Vector3 m1 = (p3 - p1) / 2.0f;
        return (2 * t * t * t - 3 * t * t + 1) * p1 + (t * t * t - 2 * t * t + t) * m0 + (-2 * t * t * t + 3 * t * t) * p2 + (t * t * t - t * t) * m1;
    }

    private void printCubeSpline() {
        for(int i = 0; i < markersPos.Count; i++) {
            Instantiate(cube, markersPos[i] + Vector3.up*0.8f, Quaternion.identity);
            for(float t = 0.25f; t < 1.0f; t += 0.25f) {
                int aux = i - 1;
                if (i - 1 < 0)
                    aux = markersPos.Count - 1;
                Vector3 position = CatmullRom(t, markersPos[aux], markersPos[i], markersPos[(i + 1) % markersPos.Count], markersPos[(i + 2) % markersPos.Count]);
                Instantiate(cube, position + Vector3.up * 0.8f, Quaternion.identity);
            }
        }
    }

    private List<Vector3> sampleRoad() {
        List<Vector3> data = new List<Vector3>();
        for (int i = 0; i < markersPos.Count; i++) {
            // FOR SIMON : You can change the increment there to improve performance
            for (float t = 0.0f; t < 1.0f; t += 0.1f) {
                int aux = i - 1;
                if (i - 1 < 0)
                    aux = markersPos.Count - 1;
                Vector3 position = CatmullRom(t, markersPos[aux], markersPos[i], markersPos[(i + 1) % markersPos.Count], markersPos[(i + 2) % markersPos.Count]);
                data.Add(position);
            }
        }
        return data;
    }
}
