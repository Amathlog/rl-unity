using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.IO;

using System;

public class Socket {
    private static Socket instance;

    private TcpClient clientSocket;
    private TcpListener serverSocket;
    private int requestCount = 0;
    private Int32 port;
    private Environment env;

    private int width;
    private int height;
    private int frame_width;
    private int frame_height;
    private int t = 0;
    private int skipfirst = 10;
    private bool send_frame;

    public int GetT() {
        return t;
    }

    public void ResetT() {
        t = 0;
    }

    public void IncrT() {
        t++;
    }

    public int GetSkipFirst() {
        return skipfirst;
    }

    public bool IsTcpClientOk() {
        return clientSocket != null;
    }

    private int sd;
    private int ad = 3;

    private bool graphicsMode = true;

    private Socket() {
        // Read environment variables
        port = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_PORT"));
        width = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_WIDTH"));
        height = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_HEIGHT"));
        send_frame = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("RL_UNITY_FRAME"));
        frame_width = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_FRAME_WIDTH"));
        frame_height = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_FRAME_HEIGHT"));

        // make framerate constant
        // https://docs.unity3d.com/ScriptReference/Time-captureFramerate.html
        Time.captureFramerate = 20;

        // Check if it's graphics mode
        string commandLineOptions = System.Environment.CommandLine;

        if (commandLineOptions.Contains("-nographics")) {
            graphicsMode = false;
        }

        if (port == 0)
            port = 8887;
        // if(width==0)
        //     width = 128;
        // if(height==0)
        //     height = 128;

        Debug.Log("Port:" + port);
        Debug.Log("Width:" + width);
        Debug.Log("Height:" + height);

        Screen.SetResolution(width, height, false);  // width, height, windowed
        AudioListener.pause = true;
        AudioListener.volume = 0;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        serverSocket = new TcpListener(localAddr, port);
        clientSocket = default(TcpClient);
        serverSocket.Start();
        Debug.Log("Server Started");

        // wait 10s for client to connect
        for (int i = 0; i < 100; i++) {
            if (serverSocket.Pending()) {
                clientSocket = serverSocket.AcceptTcpClient();
                Debug.Log("Accepting connection");
                break;
            }
            System.Threading.Thread.Sleep(100);
        }

        if (clientSocket == null) {
            Debug.Log("No connection");
        }

        // attempt to reduce memory footprint
        Resources.UnloadUnusedAssets();
    }

    public static Socket Instance {
        get {
            if (instance == null) {
                instance = new Socket();
            }
            return instance;
        }
    }

    public void SetupEnv(Environment env) {
        this.env = env;
        this.env.frame_update = send_frame;
        this.env.frameSize = new Vector2(frame_width, frame_height);
    }

    public void Send() {
        try {
            NetworkStream networkStream = clientSocket.GetStream();
            byte[] frame = new byte[0];
            if (send_frame) {
                frame = env.GetFrame();
                if (frame == null) {
                    frame = new byte[0];
                }
            }

            // Send distance to the road and vector3 speedAlongTheRoad
            List<float> state = env.GetState();
            sd = state.Count;

            // Copy the data to send.
            byte[] data_out = new byte[sd * sizeof(float) + frame.Length];

            Buffer.BlockCopy(state.ToArray(), 0, data_out, 0, sd * sizeof(float));
            Buffer.BlockCopy(frame, 0, data_out, sd * sizeof(float), frame.Length);

            networkStream.Write(data_out, 0, data_out.Length);
            networkStream.Flush();
        } catch (Exception ex) {
            Debug.LogError(ex.ToString());
        }
    }

    public void Receive() {
        try {
            NetworkStream networkStream = clientSocket.GetStream();

            requestCount = requestCount + 1;

            byte[] data_in = new byte[ad * sizeof(float)];

            networkStream.Read(data_in, 0, data_in.Length);

            float[] action = new float[ad];
            Buffer.BlockCopy(data_in, 0, action, 0, data_in.Length);

            env.MakeAction(action);

        } catch (Exception ex) {
            Debug.Log(ex.ToString());
        }
    }

    public void Close() {
        if (clientSocket != null)
            clientSocket.Close();

        serverSocket.Stop();
        Debug.Log("Exit");
    }
}
