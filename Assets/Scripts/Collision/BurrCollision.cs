using UnityEngine;

public class BurrCollision : MonoBehaviour
{
    public GameObject femur; // Reference to the femur object

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the femur object
        if (collision.gameObject == femur)
        {
            // Shave away the femur (You can implement your shaving logic here)
            ShaveAwayFemur(collision.contacts[0].point);
        }
    }

    private void ShaveAwayFemur(Vector3 collisionPoint)
    {
        // Get the femur's mesh
        Mesh femurMesh = femur.GetComponent<MeshFilter>().mesh;

        // Get the burr's position (assuming burr is a child of Handle)
        Vector3 burrPosition = transform.position;

        // Calculate the index of the closest vertex on the femur mesh
        int closestVertexIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < femurMesh.vertices.Length; i++)
        {
            float distance = Vector3.Distance(femurMesh.vertices[i], burrPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestVertexIndex = i;
            }
        }

        // Modify the vertex position to create a trace imprint
        if (closestVertexIndex != -1)
        {
            Vector3 shavedVertex = femur.transform.TransformPoint(femurMesh.vertices[closestVertexIndex]);
            Vector3 direction = (shavedVertex - collisionPoint).normalized;
            float imprintDepth = 0.1f; // Adjust as needed
            Vector3 newVertexPosition = shavedVertex + direction * imprintDepth;
            femurMesh.vertices[closestVertexIndex] = femur.transform.InverseTransformPoint(newVertexPosition);

            // Update the mesh
            femurMesh.RecalculateNormals();
            femurMesh.RecalculateBounds();
        }
    }
}
