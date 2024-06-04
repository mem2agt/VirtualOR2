using UnityEngine;

public class BurrAway : MonoBehaviour
{
    public GameObject handle;
    public GameObject bone;

    private void Start()
    {
        Debug.Log($"Handle is on layer: {LayerMask.LayerToName(handle.layer)}");
        Debug.Log($"Bone is on layer: {LayerMask.LayerToName(bone.layer)}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OnCollisionEnter called.");
        Debug.Log($"Collided with: {collision.gameObject.name}");

        // Check if the collided object is the bone and the collision is caused by the handle
        if (collision.gameObject == bone && collision.other.gameObject == handle)
        {
            Debug.Log("Collision detected between handle and bone.");

            MeshFilter meshFilter = bone.GetComponent<MeshFilter>();
            MeshCollider meshCollider = bone.GetComponent<MeshCollider>();

            if (meshFilter != null && meshCollider != null)
            {
                Debug.Log("MeshFilter and MeshCollider found. Modifying mesh to simulate burring away.");
                Debug.Log($"Contact point: {collision.contacts[0].point}");

                // Modify the mesh to simulate "burring away"
                ModifyMesh(meshFilter.mesh, collision.contacts[0].point);

                // Update the mesh collider to match the modified mesh
                meshCollider.sharedMesh = meshFilter.mesh;
                Debug.Log("Mesh collider updated to match modified mesh.");
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
        Vector3[] vertices = mesh.vertices;
        Debug.Log($"Number of vertices: {vertices.Length}");

        // Convert the contact point to local space of the mesh
        contactPoint = bone.transform.InverseTransformPoint(contactPoint);
        Debug.Log($"Contact point in local space: {contactPoint}");

        float burringRadius = 0.1f; // Define the radius of the burring effect
        bool vertexModified = false;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (Vector3.Distance(vertices[i], contactPoint) < burringRadius)
            {
                // Move vertex inward
                vertices[i] *= 0.95f;
                vertexModified = true;
                Debug.Log($"Vertex {i} moved inward.");
            }
        }

        if (vertexModified)
        {
            // Apply the modified vertices back to the mesh
            mesh.vertices = vertices;

            // Recalculate normals and bounds for the modified mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Debug.Log("Mesh modification complete. Normals and bounds recalculated.");
        }
        else
        {
            Debug.Log("No vertices were close enough to the contact point to be modified.");
        }
    }
}
