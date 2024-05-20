using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class LivePelvisMove : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 8051;

    private UdpClient client;
    private IPEndPoint remoteEndPoint;

    private void Start()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        client = new UdpClient(serverPort);
        client.Connect(remoteEndPoint);
    }

    private void Update()
    {
        try
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            string positionStr = Encoding.UTF8.GetString(data);
            string[] positionData = positionStr.Split(',');

            if (positionData.Length == 3)
            {
                float x = float.Parse(positionData[0]);
                float y = float.Parse(positionData[1]);
                float z = float.Parse(positionData[2]);

                // Update the position of the game object
                transform.position = new Vector3(x, y, z);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        client.Close();
    }
}
