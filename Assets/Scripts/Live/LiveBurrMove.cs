using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class LiveBurrMove : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 55001;

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
            string orientationStr = Encoding.UTF8.GetString(data);
            string[] orientationData = orientationStr.Split(',');

            if (orientationData.Length == 3)
            {
                float roll = float.Parse(orientationData[0]);
                float pitch = float.Parse(orientationData[1]);
                float yaw = float.Parse(orientationData[2]);

                // Calculate quaternion from Euler angles
                Quaternion newRotation = Quaternion.Euler(roll, pitch, yaw);

                // Apply the rotation to the game object
                transform.rotation = newRotation;
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
