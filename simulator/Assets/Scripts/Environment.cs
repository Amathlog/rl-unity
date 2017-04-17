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

	private List<Vector3> markersPos;
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
		byte[] res = Color32ArrayToByteArray(ReadScreenImmediate());
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
	}

	void ComputeDistance() {
		List<PairDistanceVector> distances = new List<PairDistanceVector> ();
		for (int i = 0; i < markersPos.Count; ++i) {
			distances.Add(new PairDistanceVector (i, Vector3.Distance(markersPos[i], GetPosition())));
		}
		distances.Sort(new ComparePairDistanceVector ());
		// We want AB vector along the road. Therefore A must have a lower indice 
		// in markerPos than B (except when it's the first and last items, therefore the 
		// last item need to be first.
		Vector3 a, b, c;
		if (distances[0].number == 0 && distances[1].number == markersPos.Count - 1) {
			a = markersPos[distances[1].number];
			b = markersPos[distances[0].number];
			c = markersPos[distances[0].number + 1];
		} else if (distances[1].number == 0 && distances[0].number == markersPos.Count - 1) {
			a = markersPos[distances[0].number];
			b = markersPos[distances[1].number];
			c = markersPos[distances[1].number + 1];
		} else if (distances[0].number > distances[1].number) {
			a = markersPos[distances[1].number];
			b = markersPos[distances[0].number];
			c = markersPos[(distances[0].number + 1) % markersPos.Count];
		} else {
			a = markersPos[distances[0].number];
			b = markersPos[distances[1].number];
			c = markersPos[(distances[1].number + 1) % markersPos.Count];
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
		distanceFromRoad = Mathf.Sign(Vector3.Cross(u, v.normalized).y) * (v.sqrMagnitude - (currProj - a).sqrMagnitude);
	
		// Compute the angle between AB and BC to get the next angle with the road
		// Since it's control points, it's not as precise as it should be (it should be spine interpolated)
		// But we get an approximation of the road shape
		float angle = Mathf.Acos(Vector3.Dot(unitVectorAlongRoad, unitVectorAlongRoadOneStepAhead));
		nextAngle = Mathf.Sign(Vector3.Cross(unitVectorAlongRoad, unitVectorAlongRoadOneStepAhead).y) * angle;
	}

	Color32[] ReadScreenImmediate() {
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture auxRT = new RenderTexture (Screen.width, Screen.height, 16);
		RenderTexture.active = auxRT;
		m_cam.targetTexture = auxRT;
		m_cam.Render();
		Texture2D image = new Texture2D(auxRT.width, auxRT.height);
		image.ReadPixels(new Rect(0, 0, auxRT.width, auxRT.height), 0, 0);
		image.Apply();
		RenderTexture.active = currentRT;
		m_cam.targetTexture = null;
		return image.GetPixels32();
		/*Texture2D tex = cam.GetComponent<CameraCapture>().RenderResult;
		if (tex == null)
			return null;
		return tex.GetPixels32();*/
	}

//	void FixedUpdate(){
//		//Color32[] aux = ReadScreenImmediate();
//		List<float> aux = GetState();
//		string res = "[";
//		foreach(float f in aux){
//			res += f.ToString() + ", ";
//		}
//		res += "]";
//		print(res);
//	}

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
}
