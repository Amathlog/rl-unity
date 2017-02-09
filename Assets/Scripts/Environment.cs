using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Environment : MonoBehaviour {

    [SerializeField] private GameObject cam;

    public byte[] GetFrame()
    {
        return Color32ArrayToByteArray(ReadScreen());
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

    private void Update()
    {
        //print(GetFrame()[0]);
    }
	
	private Color32[] ReadScreen () {
        Texture2D tex = cam.GetComponent<CameraCapture>().RenderResult;
        if (tex == null)
            return null;
		return tex.GetPixels32();
	}
}
