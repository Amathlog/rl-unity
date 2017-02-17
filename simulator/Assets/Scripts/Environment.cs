using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

public class Environment : MonoBehaviour
{

    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject car;

    private CarController carController;
    private Color32[] screenPixels = null;
    private bool reading = false;

    public byte[] GetFrame()
    {
        byte[] res = Color32ArrayToByteArray(ReadScreenImmediate());
        //print("Color32[0] = " + screenPixels[0]);
        //print("byte[0:4] = " + res[0] + ", " + res[1] + ", " + res[2] + ", " + res[3]);
        return res;
    }

    public void MakeAction(float[] actions)
    {
        print("make action " + actions[0] + " " + actions[1]);
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
    }

    Color32[] ReadScreenImmediate() {
        Texture2D tex = cam.GetComponent<CameraCapture>().RenderResult;
        if (tex == null)
            return null;
        return tex.GetPixels32();
    }
}
