using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class MATLABUDP : MonoBehaviour
{
    public int port; // Port to listen for incoming UDP packets

    private UdpClient udpClient;

    [Header("Quaternion Data")]
    public float qw;
    public float qx;
    public float qy;
    public float qz;

    void Start()
    {
        udpClient = new UdpClient(port);
        Debug.Log("UDP listener started on port " + port);
        udpClient.BeginReceive(new System.AsyncCallback(ReceiveData), null);
    }

    private void ReceiveData(System.IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] data = udpClient.EndReceive(ar, ref ip);
        string orientationStr = Encoding.ASCII.GetString(data).Trim();
        Debug.Log("Data received: " + orientationStr);

        string[] orientationData = orientationStr.Split(',');

        if (orientationData.Length == 4)
        {
            if (float.TryParse(orientationData[0], out qw) &&
                float.TryParse(orientationData[1], out qx) &&
                float.TryParse(orientationData[2], out qy) &&
                float.TryParse(orientationData[3], out qz))
            {
                // Update the quaternion data in the Inspector or do whatever you need to do with it
            }
            else
            {
                Debug.LogWarning("Failed to parse quaternion data.");
            }
        }
        else
        {
            Debug.LogWarning("Unexpected data format: " + orientationStr);
        }

        udpClient.BeginReceive(new System.AsyncCallback(ReceiveData), null);
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
