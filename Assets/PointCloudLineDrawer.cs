using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class PointCloudLineDrawer : MonoBehaviour
{
    public GameObject pointPrefab; // Reference to the point prefab
    public Material lineMaterial; // Material for the lines
    public string filePath;

    private List<Vector3> pointsP;
    private List<Vector3> pointsQ;
    private List<Vector3> pointsR;

    public void onClick()
    {
        LoadPointCloud("File1.txt", out pointsP);
        LoadPointCloud("File2.txt", out pointsQ);
        LoadPointCloud(filePath, out pointsR);

        // Initialize the point clouds in the scene
        InstantiatePointCloud(pointsP, Color.red);
        InstantiatePointCloud(pointsQ, Color.blue);
        InstantiatePointCloud(pointsR, Color.green);

        DrawLines(pointsP, pointsQ,  Color.white);
        DrawLines(pointsP, pointsR,  Color.magenta);

        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == "Spehere")
            {
                Destroy(obj);
            }
        }
    }

    void LoadPointCloud(string filePath, out List<Vector3> points)
    {
        points = new List<Vector3>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Check if there is at least one line
            if (lines.Length > 0)
            {
                // Read the number of points from the first line
                if (int.TryParse(lines[0], out int numPoints))
                {
                    // Loop through the lines starting from the second line (index 1)
                    for (int i = 1; i < lines.Length && i <= numPoints; i++)
                    {
                        string[] values = lines[i].Split(' ');

                        if (values.Length >= 3)
                        {
                            float x = float.Parse(values[0]);
                            float y = float.Parse(values[1]);
                            float z = float.Parse(values[2]);
                            points.Add(new Vector3(x, y, z));
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error parsing the number of points from the first line.");
                }
            }
            else
            {
                Debug.LogError("File is empty.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading file {filePath}: {e.Message}");
        }
    }

    void InstantiatePointCloud(List<Vector3> points, Color color)
    {
        foreach (Vector3 point in points)
        {
            GameObject pointObj = Instantiate(pointPrefab, point, Quaternion.identity);
            pointObj.GetComponent<Renderer>().material.color = color;
        }
    }

    void DrawLines(List<Vector3> pointsA, List<Vector3> pointsB, Color color)
    {
        for (int i = 0; i < Mathf.Min(pointsA.Count, pointsB.Count); i++)
        {
            // Draw line between corresponding points
            GameObject line = new GameObject("Line" + i);
            line.AddComponent<LineRenderer>();
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.SetPosition(0, pointsA[i]);
            lineRenderer.SetPosition(1, pointsB[i]);
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material
            lineRenderer.material.color = color;
        }
    }
}
