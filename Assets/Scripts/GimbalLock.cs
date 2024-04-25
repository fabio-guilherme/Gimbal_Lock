using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public enum Example
{
    None,
    GimbalLockDemo
}

public enum Order
{
    XYZ,
    XZY,
    YXZ,
    YZX,
    ZXY,
    ZYX
}

public class GimbalLock : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private MeshRenderer meshRenderer;

    private Example example = Example.GimbalLockDemo;
    public Order order;

    // Total angles for X and Y axis rotation in degrees
    public float angleX = 0f;
    public float angleY = 0f;
    public float angleZ = 0f;

    // Increment in the angle rotation
    public float incrementX;
    public float incrementY;
    public float incrementZ;

    public bool run = true;
    
    // Start is called before the first frame update
    void Start()
    {
        // Create mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "MyMesh";

        vertices = new Vector3[18];
        normals = new Vector3[18];

        Reset();

        int[] triangles = new int[18];
        //back triangle
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        //front triangle
        triangles[3] = 4;
        triangles[4] = 5;
        triangles[5] = 3;
        //left triangle
        triangles[6] = 6;
        triangles[7] = 8;
        triangles[8] = 7;
        //right triangle
        triangles[9] = 10;
        triangles[10] = 11;
        triangles[11] = 9;
        //bottom triangle 1
        triangles[12] = 13;
        triangles[13] = 14;
        triangles[14] = 12;
        //bottom triangle 2
        triangles[15] = 15;
        triangles[16] = 17;
        triangles[17] = 16;

        //Update mesh
        mesh.triangles = triangles;

        // Add a Mesh Renderer component to the Mesh object
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Create a new material that uses a grey color
        Material material = new Material(Shader.Find("Standard"));
        material.color = Color.grey;

        // Assign the material to the Mesh object
        meshRenderer.material = material;
    }

    void OnGUI()
    {
        // Set font style and size
        GUI.skin.label.fontSize = 20;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(15, 25, 270, 75), $"Angle X: {angleX:0.000f} degrees\n" +
            $"Angle Y: {angleY:0.000f} degrees\n" +
            $"Angle Z: {angleZ:0.000f} degrees");
    }

    // Update is called once per frame
    void Update()
    {
        if (run)
        {
            switch (example)
            {
                case Example.GimbalLockDemo:
                    {
                        // Increase angles continuously
                        angleX += incrementX;
                        angleY += incrementY;
                        angleZ += incrementZ;

                        // Rotate around X, Y and Z axes
                        //RotateXYZ(rotationX * Mathf.Deg2Rad, rotationY * Mathf.Deg2Rad, rotationZ * Mathf.Deg2Rad);
                        Reset();
                        RotateXYZ(angleX * Mathf.Deg2Rad, angleY * Mathf.Deg2Rad, angleZ * Mathf.Deg2Rad);
                        break;
                    }
                default: break;
            }
            Debug.Log($"Current Rotation X: {incrementX} degrees");
            Debug.Log($"Current Rotation Y: {incrementY} degrees");
            Debug.Log($"Current Rotation Z: {incrementZ} degrees\n");
        }
    }

    private void Reset()
    {
        //back triangle vertices
        vertices[0] = new Vector3(-10, 0, -10);
        vertices[1] = new Vector3(10, 0, -10);
        vertices[2] = new Vector3(0, 10, 0);
        //front triangle vertices
        vertices[3] = new Vector3(-10, 0, 10);
        vertices[4] = new Vector3(10, 0, 10);
        vertices[5] = new Vector3(0, 10, 0);
        //left triangle vertices
        vertices[6] = new Vector3(-10, 0, 10);
        vertices[7] = new Vector3(-10, 0, -10);
        vertices[8] = new Vector3(0, 10, 0);
        //right triangle vertices
        vertices[9] = new Vector3(10, 0, 10);
        vertices[10] = new Vector3(10, 0, -10);
        vertices[11] = new Vector3(0, 10, 0);
        //botton triangle 1 vertices
        vertices[12] = new Vector3(-10, 0, -10);
        vertices[13] = new Vector3(10, 0, -10);
        vertices[14] = new Vector3(-10, 0, 10);
        //botton triangle 2 vertices
        vertices[15] = new Vector3(10, 0, 10);
        vertices[16] = new Vector3(10, 0, -10);
        vertices[17] = new Vector3(-10, 0, 10);

        normals[0] = Vector3.back + Vector3.up;
        normals[1] = Vector3.back + Vector3.up;
        normals[2] = Vector3.back + Vector3.up;

        normals[3] = Vector3.forward + Vector3.up;
        normals[4] = Vector3.forward + Vector3.up;
        normals[5] = Vector3.forward + Vector3.up;

        normals[6] = Vector3.left + Vector3.up;
        normals[7] = Vector3.left + Vector3.up;
        normals[8] = Vector3.left + Vector3.up;

        normals[9] = Vector3.right + Vector3.up;
        normals[10] = Vector3.right + Vector3.up;
        normals[11] = Vector3.right + Vector3.up;

        normals[12] = Vector3.down;
        normals[13] = Vector3.down;
        normals[14] = Vector3.down;

        normals[15] = Vector3.down;
        normals[16] = Vector3.down;
        normals[17] = Vector3.down;

        // Update mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
    }

    void RotateXYZ(float x, float y, float z)
    {
        // Keep angle between 0 and 360
        angleX = angleX % 360;
        angleY = angleY % 360;
        angleZ = angleZ % 360;

        // Create individual rotation matrices for X, Y, and Z axes
        Matrix4x4 rxmat = RotXMat(x);
        Matrix4x4 rymat = RotYMat(y);
        Matrix4x4 rzmat = RotZMat(z);

        float lockAngle;    // Angle to check the lock condition

        // Combine the rotations using multiplication (order matters, particularly for gimbal lock!)
        Matrix4x4 combinedRotation;
        switch (order)
        {
            case Order.XYZ:
                {
                    combinedRotation = rxmat * rymat * rzmat;
                    lockAngle = angleY;
                    break;
                }
            case Order.XZY:
                {
                    combinedRotation = rxmat * rzmat * rymat;
                    lockAngle = angleZ;
                    break;
                }
            case Order.YXZ:
                {
                    combinedRotation = rymat * rxmat * rzmat;
                    lockAngle = angleX;
                    break;
                }
            case Order.YZX:
                {
                    combinedRotation = rymat * rzmat * rxmat;
                    lockAngle = angleZ;
                    break;
                }
            case Order.ZXY:
                {
                    combinedRotation = rzmat * rxmat * rymat;
                    lockAngle = angleX;
                    break;
                }
            case Order.ZYX:
                {
                    combinedRotation = rzmat * rymat * rxmat;
                    lockAngle = angleY;
                    break;
                }
            default:
                {
                    combinedRotation = rymat * rxmat * rzmat; // YXZ: Same behaviour as Unity Inspector
                    lockAngle = angleX;
                    break;
                }
        }

        // Make object red to show the gimbal lock
        if (Mathf.Abs(lockAngle) >= 89f && Mathf.Abs(lockAngle) <= 91f)
        {
            meshRenderer.material.color = Color.red; // Change color to red
        }
        else
        {
            meshRenderer.material.color = Color.grey; // Reset color
        }

        // Apply the combined rotation to each vertex and normal
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = combinedRotation.MultiplyPoint(vertices[i]);
            normals[i] = combinedRotation.MultiplyPoint(normals[i]);
        }

        // Update mesh with modified vertices and normals
        mesh.vertices = vertices;
        mesh.normals = normals;
    }

    Matrix4x4 RotXMat(float angle)
    {
        Matrix4x4 rxmat = new Matrix4x4();
        rxmat.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
        rxmat.SetRow(1, new Vector4(0f, Mathf.Cos(angle), -Mathf.Sin(angle), 0f));
        rxmat.SetRow(2, new Vector4(0f, Mathf.Sin(angle), Mathf.Cos(angle), 0f));
        rxmat.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return rxmat;
    }

    Matrix4x4 RotYMat(float angle)
    {
        Matrix4x4 rymat = new Matrix4x4();
        rymat.SetRow(0, new Vector4(Mathf.Cos(angle), 0f, Mathf.Sin(angle), 0f));
        rymat.SetRow(1, new Vector4(0f, 1f, 0f, 0f));
        rymat.SetRow(2, new Vector4(-Mathf.Sin(angle), 0f, Mathf.Cos(angle), 0f));
        rymat.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return rymat;
    }

    Matrix4x4 RotZMat(float angle)
    {
        Matrix4x4 rzmat = new Matrix4x4();
        rzmat.SetRow(0, new Vector4(Mathf.Cos(angle), -Mathf.Sin(angle), 0f, 0f));
        rzmat.SetRow(1, new Vector4(Mathf.Sin(angle), Mathf.Cos(angle), 0f, 0f));
        rzmat.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
        rzmat.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return rzmat;
    }

    // Visualise the normals
    private void OnDrawGizmos()
    {
        if (vertices == null) return;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }
}
