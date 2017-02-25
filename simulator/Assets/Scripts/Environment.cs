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
    private float distanceFromRoad = 0.0f;
    private Vector3 speedAlongRoad;
    private CarController carController;

    internal class PairDistanceVector {
        public Vector3 v;
        public float dist;

        public PairDistanceVector(Vector3 v, float dist) {
            this.v = v;
            this.dist = dist;
        }
    }

    internal class ComparePairDistanceVector : IComparer<PairDistanceVector> {
        public int Compare(PairDistanceVector x, PairDistanceVector y) {
            return (int)(x.dist - y.dist);
        }
    }

    public byte[] GetFrame()
    {
        byte[] res = Color32ArrayToByteArray(ReadScreenImmediate());
        return res;
    }

    public Vector3 GetPosition() {
        return car.transform.position;
    }

    public void MakeAction(float[] actions)
    {
        carController.Move(actions[0], actions[1], actions[1], 0f);
    }

    private byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }

    void Start()
    {
        carController = car.GetComponent<CarController>();
        markersPos = new List<Vector3>();
        lastProj = GetPosition();
        currProj = lastProj;
        speedAlongRoad = Vector3.zero;
        foreach (Transform child in markers.transform) {
            markersPos.Add(child.position);
        }

    }

    void ComputeDistance() {
        List<PairDistanceVector> distances = new List<PairDistanceVector>();
        foreach(Vector3 pos in markersPos) {
            distances.Add(new PairDistanceVector(pos, Vector3.Distance(pos, GetPosition())));
        }
        distances.Sort(new ComparePairDistanceVector());
        Vector3 a = distances[0].v;
        Vector3 b = distances[1].v;
        // u is the unit vector associated to AB
        Vector3 u = (b - a).normalized;
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
        speedAlongRoad = currProj - lastProj;
        // Square distance, Pythagorean theorem in the triangle A-C-proj
        distanceFromRoad = v.sqrMagnitude - (currProj - a).sqrMagnitude;

    }

    Color32[] ReadScreenImmediate() {
        Texture2D tex = cam.GetComponent<CameraCapture>().RenderResult;
        if (tex == null)
            return null;
        return tex.GetPixels32();
    }

    //void GenerateFileWithWaypoints() { 
    //    string json = JsonConvert.SerializeObject(markersPos.ToArray());
    //    string path = Application.dataPath + "/waypoints_" + SceneManager.GetActiveScene().name + ".txt";
    //    System.IO.File.WriteAllText(path, json);
    //}

    public float[] GetState() {
        ComputeDistance();
        float[] res = new float[4];
        res[0] = distanceFromRoad;
        res[1] = speedAlongRoad.x;
        res[2] = speedAlongRoad.y;
        res[3] = speedAlongRoad.z;
        return res;
    }
}
