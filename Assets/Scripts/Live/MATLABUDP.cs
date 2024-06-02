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

    [Header("Displacement Data")]
    public float x;
    public float y;
    public float z;

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
        string dataStr = Encoding.ASCII.GetString(data).Trim();
        Debug.Log("Data received: " + dataStr);

        string[] dataArray = dataStr.Split(',');

        if (dataArray.Length == 7)
        {
            if (float.TryParse(dataArray[0], out qw) &&
                float.TryParse(dataArray[1], out qx) &&
                float.TryParse(dataArray[2], out qy) &&
                float.TryParse(dataArray[3], out qz) &&
                float.TryParse(dataArray[4], out x) &&
                float.TryParse(dataArray[5], out y) &&
                float.TryParse(dataArray[6], out z))
            {
                // Update the displacement and quaternion data in the Inspector or do whatever you need to do with it
            }
            else
            {
                Debug.LogWarning("Failed to parse data.");
            }
        }
        else
        {
            Debug.LogWarning("Unexpected data format: " + dataStr);
        }

        udpClient.BeginReceive(new System.AsyncCallback(ReceiveData), null);
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
