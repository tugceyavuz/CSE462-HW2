using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PointCloudLoader : MonoBehaviour
{
    public GameObject pointPrefab; // Reference to a prefab for the point cloud point
    public string filePath = "File1.txt";

    void Start()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointPrefab = sphere;
        LoadPointCloud();
    }

    void LoadPointCloud()
    {
        List<Vector3> pointPositions = new List<Vector3>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip the first line (num_pts)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(' ');

                if (values.Length >= 3)
                {
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);
                    float z = float.Parse(values[2]);
                    pointPositions.Add(new Vector3(x, y, z));
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading file {filePath}: {e.Message}");
        }

        // Instantiate points in the scene
        InstantiatePoints(pointPositions);
    }

    void InstantiatePoints(List<Vector3> positions)
    {
        foreach (Vector3 position in positions)
        {
            Instantiate(pointPrefab, position, Quaternion.identity);
        }
    }
}
