using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class sockets : MonoBehaviour {

	TcpClient clientSocket;
	TcpListener serverSocket;
	int requestCount = 0;

	// Use this for initialization
	void Start () {
		      // Set the TcpListener on port 13000.
	  Int32 port = 8887;
	  IPAddress localAddr = IPAddress.Parse("127.0.0.1");

		serverSocket = new TcpListener(localAddr, 8888);
	  clientSocket = default(TcpClient);
	  serverSocket.Start();
	  Debug.Log(" >> Server Started");
	  clientSocket = serverSocket.AcceptTcpClient();
	  Debug.Log(" >> Accept connection from client");
	  requestCount = 0;
	}
	
	// Update is called once per frame
	void Update () {
		try
    {
        requestCount = requestCount + 1;
        NetworkStream networkStream = clientSocket.GetStream();
        byte[] bytesFrom = new byte[64*64*3];
        //(int)clientSocket.ReceiveBufferSize
        networkStream.Read(bytesFrom, 0, 1024);
        string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
        // dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
        Debug.Log(" >> Data from client - " + dataFromClient);
        // string serverResponse = "Last Message from client" + dataFromClient;
        // Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
        // networkStream.Write(sendBytes, 0, sendBytes.Length);
        // networkStream.Flush();
        // Debug.Log(" >> " + serverResponse);
    }
    catch (Exception ex)
    {
        Debug.Log(ex.ToString());
    }
	}

	void OnDestroy() {
    clientSocket.Close();
    serverSocket.Stop();
    Debug.Log(" >> exit");
    // Console.ReadLine();
	}
}


