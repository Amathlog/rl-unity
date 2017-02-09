using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine;


public class sockets : MonoBehaviour
{
    TcpClient clientSocket;
    TcpListener serverSocket;
    int requestCount = 0;

    int[] shape = { 128, 128, 4 };
    int n = 128 * 128 * 4;

    Environment env;

    int t = 0;


    void Start()
    {
        env = GameObject.Find("Env").GetComponent<Environment>();

        Int32 port = 8887;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        serverSocket = new TcpListener(localAddr, port);
        clientSocket = default(TcpClient);
        serverSocket.Start();
        Debug.Log(" >> Server Started");
        clientSocket = serverSocket.AcceptTcpClient();
        Debug.Log(" >> Accept connection from client");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (t > 10)
        {

            try
            {
                int ad = 2;
                int sd = 2;

                requestCount = requestCount + 1;

                byte[] data_in = new byte[ad * 4];

                NetworkStream networkStream = clientSocket.GetStream();

                networkStream.Read(data_in, 0, data_in.Length);

                float[] action = new float[ad];
                Buffer.BlockCopy(data_in, 0, action, 0, data_in.Length);


                //Debug.Log("a = " + action[0] + ' ' + action[1]);

                env.MakeAction(action);

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
                Debug.Log(ex.ToString());
            }

        }

        t++;
    }

    void OnDestroy()
    {
        clientSocket.Close();
        serverSocket.Stop();
        Debug.Log(" >> exit");
    }
}


//(int)clientSocket.ReceiveBufferSize
// string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
// dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

// Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);