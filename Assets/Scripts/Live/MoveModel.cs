using UnityEngine;
using System.Collections.Generic;

public class MoveModel : MonoBehaviour
{
    // Game objects to be transformed
    public Transform handleRotationTransform; // Parent GameObject for handle rotation
    public Transform handlePositionTransform; // Handle GameObject for position updates
    public Transform leftHemipelvisTransform;
    public Transform leftFemurTransform;

    // Data streaming components
    public DLCUDP dlcUdpReceiver;
    public MATLABUDP matlabUdpReceiver;

    // Smoothing parameters
    public int smoothingWindowSize = 5;

    // Queues to store recent position values for smoothing
    private Queue<Vector3> handlePositions = new Queue<Vector3>();
    private Queue<Vector3> leftHemipelvisPositions = new Queue<Vector3>();
    private Queue<Vector3> leftFemurPositions = new Queue<Vector3>();

    // Bounding box parameters
    private float xMin = 0f;
    private float xMax = 640f;
    private float yMin = 0f;
    private float yMax = 480f;

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
                        Vector3 position;
                        if (TryParseVector3(data, 1, out position))
                        {
                            switch (id)
                            {
                                case 0:
                                    UpdatePositionQueue(handlePositions, position);
                                    handlePositionTransform.position = GetClampedSmoothedPosition(handlePositions);
                                    break;
                                case 1:
                                    UpdatePositionQueue(leftHemipelvisPositions, position);
                                    leftHemipelvisTransform.position = GetClampedSmoothedPosition(leftHemipelvisPositions);
                                    break;
                                case 2:
                                    UpdatePositionQueue(leftFemurPositions, position);
                                    leftFemurTransform.position = GetClampedSmoothedPosition(leftFemurPositions);
                                    break;
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
            float y = float.Parse(data[startIndex + 1]);
            result = new Vector3(x, y, 0);
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
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, yMin, yMax);
        return smoothedPosition;
    }
}
