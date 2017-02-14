using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.IO;


public class sockets : MonoBehaviour
{
    TcpClient clientSocket;
    TcpListener serverSocket;
    int requestCount = 0;
    Int32 port;
    Environment env;

    // int width;
    // int height;
    int t = 0;
    int skipfirst = 10;

    int ad = 2;
    int sd = 2;


    void Start()
    {
        // Read environment variables
        port = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_PORT"));
        // width = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_WIDTH"));
        // height = Convert.ToInt32(System.Environment.GetEnvironmentVariable("RL_UNITY_HEIGHT"));

        if(port==0)
            port = 8887;
        // if(width==0)
        //     width = 128;
        // if(height==0)
        //     height = 128;

        Debug.Log("Port:" + port);
        // Debug.Log("Width:" + width);
        // Debug.Log("Height:" + height);

        // Screen.SetResolution (width, height, false);  // width, height, windowed

        env = GameObject.Find("Env").GetComponent<Environment>();

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        serverSocket = new TcpListener(localAddr, port);
        clientSocket = default(TcpClient);
        serverSocket.Start();
        Debug.Log("Server Started");

        // wait 1s for client to connect
        for(int i=0; i < 10; i++){
            if(serverSocket.Pending()){
                clientSocket = serverSocket.AcceptTcpClient();
                Debug.Log("Accepting connection");
            }
            System.Threading.Thread.Sleep(100);
        }

        if(clientSocket==null){
            Debug.Log("No connection");
        }

    }

    void Send(){
        try{

            NetworkStream networkStream = clientSocket.GetStream();

            byte[] frame = env.GetFrame();

            //for (int i = 0; i < 10; i++)
            //{

            //    Debug.Log(frame[i]);
            //}

            float[] state = new float[sd];

            state[0] = 0.4f;

            //if (frame.Length != 128 * 128 * 4)
            //{
            //    Debug.Log("fdsfd");
            //}

            //            byte[] data_out = new byte[frame.Length];
            byte[] data_out = new byte[sd * 4 + frame.Length];
            //            byte[] data_out = new byte[sd*4];
            Buffer.BlockCopy(state, 0, data_out, 0, sd * 4);
            //            Buffer.BlockCopy(frame, 0, data_out, 0, data_out.Length);
            Buffer.BlockCopy(frame, 0, data_out, sd * 4, frame.Length);

            networkStream.Write(data_out, 0, data_out.Length);
            networkStream.Flush();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    void Receive(){
        try
        {  
            NetworkStream networkStream = clientSocket.GetStream();

            requestCount = requestCount + 1;

            byte[] data_in = new byte[ad * 4];

            networkStream.Read(data_in, 0, data_in.Length);

            float[] action = new float[ad];
            Buffer.BlockCopy(data_in, 0, action, 0, data_in.Length);


            Debug.Log("a = " + action[0] + ' ' + action[1]);

            env.MakeAction(action);

        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        print("upd");
        if(clientSocket != null){

            if (t == skipfirst){
                print("init send at t=" + t);
                Send(); // initial observation
            }
            else if (t > skipfirst)
            {
                print("interact");
                Receive();
                Send();
            }

            t++;
        }
    }

    void OnDestroy()
    {
        if(clientSocket!=null)
            clientSocket.Close();

        serverSocket.Stop();
        Debug.Log("Exit");
    }
}


//(int)clientSocket.ReceiveBufferSize
// string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
// dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

// Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);