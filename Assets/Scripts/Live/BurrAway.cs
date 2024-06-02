using UnityEngine;

public class BurrAway : MonoBehaviour
{
    // The handle object that includes the burr
    public GameObject handle;

    // The bone object to be modified
    public GameObject bone;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the bone and the collision is caused by the handle
        if (collision.gameObject == bone && this.gameObject == handle)
        {
            Debug.Log("Collision detected between handle and bone.");

            // Get the mesh filter and collider of the bone
            MeshFilter meshFilter = collision.gameObject.GetComponent<MeshFilter>();
            MeshCollider meshCollider = collision.gameObject.GetComponent<MeshCollider>();

            if (meshFilter != null && meshCollider != null)
            {
                Debug.Log("MeshFilter and MeshCollider found. Modifying mesh to simulate burring away.");

                // Modify the mesh to simulate "burring away"
                ModifyMesh(meshFilter.mesh, collision.contacts[0].point);

                // Update the mesh collider to match the modified mesh
                meshCollider.sharedMesh = meshFilter.mesh;
            }
            else
            {
                Debug.LogWarning("MeshFilter or MeshCollider not found on the bone object.");
            }
        }
        else
        {
            Debug.Log("Collision detected but not between the specified handle and bone.");
        }
    }

    private void ModifyMesh(Mesh mesh, Vector3 contactPoint)
    {
        // Get the vertices and triangles of the mesh
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Convert the contact point to local space of the mesh
        contactPoint = transform.InverseTransformPoint(contactPoint);

        // Example modification: move vertices inward near the contact point to simulate "burring away"
        float burringRadius = 0.1f; // Define the radius of the burring effect
        for (int i = 0; i < vertices.Length; i++)
        {
            if (Vector3.Distance(vertices[i], contactPoint) < burringRadius)
            {
                // Move vertex inward
                vertices[i] *= 0.95f;
                Debug.Log($"Vertex {i} moved inward.");
            }
        }

        // Apply the modified vertices back to the mesh
        mesh.vertices = vertices;

        // Recalculate normals and bounds for the modified mesh
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Debug.Log("Mesh modification complete. Normals and bounds recalculated.");
    }
}
