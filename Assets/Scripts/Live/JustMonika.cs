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
    private float zMin = 0f; // Changed to zMin
    private float zMax = 480f; // Changed to zMax

    // Initial Y position
    private float initialY;

    void Start()
    {
        // Store the initial Y position
        initialY = handlePositionTransform.position.y;
        Debug.Log("Initial Y position: " + initialY);
    }

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
                                // Add 50 to x and 100 to z before updating the queue
                                position.x += 200;
                                position.z -= 200;

                                UpdatePositionQueue(handlePositions, position);
                                Vector3 smoothedPosition = GetClampedSmoothedPosition(handlePositions);
                                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, initialY, Mathf.Infinity); // Clamp Y to initialY and positive infinity
                                smoothedPosition.z *= -1; // Invert the Z position
                                handlePositionTransform.position = smoothedPosition;
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
            float z = float.Parse(data[startIndex + 1]); // Changed to parse Z-coordinate
            result = new Vector3(x, handlePositionTransform.position.y, z); // Use current Y position
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
        smoothedPosition.z = Mathf.Clamp(smoothedPosition.z, zMin, zMax); // Change to zMin and zMax
        return smoothedPosition;
    }
}
