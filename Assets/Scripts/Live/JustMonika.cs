using UnityEngine;
using System.Collections.Generic;

public class JustMonika : MonoBehaviour
{
    // Game objects to be transformed
    public Transform handleRotationTransform; // Parent GameObject for handle rotation
    public Transform handlePositionTransform; // Handle GameObject for position updates

    // Data streaming components
    public DLCUDP dlcUdpReceiver;
    public MATLABUDP matlabUdpReceiver;

    // Smoothing parameters
    public int smoothingWindowSize = 5;

    // Queue to store recent position values for smoothing
    private Queue<Vector3> handlePositions = new Queue<Vector3>();

    // Bounding box parameters
    private float xMin = 0f;
    private float xMax = 640f;
    private float zMin = -480f; // Adjusting the min and max to reflect the inverted direction
    private float zMax = 0f;

    void Update()
    {
        // Handle rotation from MATLABUDP
        if (handleRotationTransform != null && matlabUdpReceiver != null)
        {
            Quaternion handleRotation = new Quaternion(matlabUdpReceiver.qx, matlabUdpReceiver.qy, matlabUdpReceiver.qz, matlabUdpReceiver.qw);
            handleRotationTransform.rotation = handleRotation;
        }

        // Handle position from DLCUDP
        if (dlcUdpReceiver != null)
        {
            string[] dataSegments = dlcUdpReceiver.lastReceivedUDPPacket.Split(':');

            foreach (string segment in dataSegments)
            {
                string[] data = segment.Split(',');

                if (data.Length == 3)
                {
                    int id;
                    if (int.TryParse(data[0], out id))
                    {
                        if (id == 0)
                        {
                            Vector3 position;
                            if (TryParseVector3(data, 1, out position))
                            {
                                UpdatePositionQueue(handlePositions, position);
                                handlePositionTransform.position = GetClampedSmoothedPosition(handlePositions);
                            }
                        }
                    }
                }
            }
        }
    }

    private bool TryParseVector3(string[] data, int startIndex, out Vector3 result)
    {
        result = Vector3.zero;
        try
        {
            float x = float.Parse(data[startIndex]);
            float z = -float.Parse(data[startIndex + 1]); // Invert the z value
            result = new Vector3(x, 0, z); // Changed y to z
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void UpdatePositionQueue(Queue<Vector3> positions, Vector3 newPosition)
    {
        positions.Enqueue(newPosition);
        if (positions.Count > smoothingWindowSize)
        {
            positions.Dequeue();
        }
    }

    private Vector3 GetSmoothedPosition(Queue<Vector3> positions)
    {
        Vector3 sum = Vector3.zero;
        foreach (var position in positions)
        {
            sum += position;
        }
        return sum / positions.Count;
    }

    private Vector3 GetClampedSmoothedPosition(Queue<Vector3> positions)
    {
        Vector3 smoothedPosition = GetSmoothedPosition(positions);
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, xMin, xMax);
        smoothedPosition.z = Mathf.Clamp(smoothedPosition.z, zMin, zMax); // Clamp the inverted z value
        return smoothedPosition;
    }
}
