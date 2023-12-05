using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

public class scr_scale : MonoBehaviour
{
    public GameObject pointPrefab; // Reference to the point cloud prefab
    public TextMeshProUGUI resultText; // Text to display transformation parameters

    private List<Vector3> pointsP;
    private List<Vector3> pointsQ;

    // RANSAC parameters
    public int numIterations = 1000;
    public float inlierThreshold = 0.1f;


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

    private bool isRegistrationActive = true;
    public void SetRegistrationMethod()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointPrefab = sphere;

        if (isRegistrationActive)
        {
            // Perform registration logic
            LoadPointCloud("File1.txt", out pointsP);
            LoadPointCloud("File2.txt", out pointsQ);

            List<Vector3> transformedPoints = OutputPointList(pointsP, pointsQ);

            // Now you can use the transformedPoints as needed
            // For example, instantiate objects at the transformed positions
            SaveTransformedPoints(transformedPoints);
            isRegistrationActive = false;
            EraseAllObjects();
        }
    }

    public string objectName = "Sphere";
    public void EraseAllObjects()
    {
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == objectName || obj.name == $"{objectName}(Clone)")
            {
                Destroy(obj);
            }
        }
    }

    public List<Vector3> OutputPointList(List<Vector3> pointsP, List<Vector3> pointsQ)
    {
        List<Vector3> transformedPoints = new List<Vector3>();

        ApplyRANSACandAlignment(pointsP, pointsQ, out Matrix4x4 bestRotationMatrix);
        //ExtractTranslationAndRotation(bestRotationMatrix);

        // Apply the obtained transformation to each point in pointsP
        foreach (Vector3 point in pointsP)
        {
            // Apply rigid transformation using matrix multiplication
            Vector3 transformedPoint = bestRotationMatrix.MultiplyPoint3x4(point);
            transformedPoints.Add(transformedPoint);
        }

        SaveTransformedPoints(transformedPoints);
        return transformedPoints;
    }
    
    public void SaveTransformedPoints(List<Vector3> transformedPoints)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter("File4.txt"))
            {
                // Write the number of points
                writer.WriteLine(transformedPoints.Count);

                // Write each point in the format "x y z"
                foreach (Vector3 point in transformedPoints)
                {
                    writer.WriteLine($"{point.x} {point.y} {point.z}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving transformed points: {e.Message}");
        }
    }

    public void ApplyRANSACandAlignment(List<Vector3> pointsP, List<Vector3> pointsQ, out Matrix4x4 bestTransformationMatrix)
    {
        int maxIterations = 1000;
        float inlierThreshold = 0.2f; // Adjust this threshold based on your specific requirements

        bestTransformationMatrix = Matrix4x4.identity;

        int maxInliers = 0;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Randomly select three points
            List<Vector3> randomPointsP = SelectRandomPoints(pointsP, 3);
            List<Vector3> randomPointsQ = SelectRandomPoints(pointsQ, 3);

            // Calculate transformation matrix using the three-point alignment method
            Matrix4x4 transformationMatrix = CalculateTransformationMatrix(randomPointsP, randomPointsQ);

            // Apply the transformation to all points in Q
            List<Vector3> transformedPointQ = ApplyTransformation(pointsP, transformationMatrix);

            // Count inliers
            int inlierCount = CountInliers(pointsQ, transformedPointQ, inlierThreshold);

            // Update the best transformation matrix if the current one has more inliers
            if (inlierCount > maxInliers)
            {

                maxInliers = inlierCount;
                bestTransformationMatrix = transformationMatrix;
            }
        }

        DisplayTransformationParameters(bestTransformationMatrix);
    }

    private List<Vector3> SelectRandomPoints(List<Vector3> points, int count)
    {
        List<Vector3> randomPoints = new List<Vector3>();
        List<int> indices = new List<int>();

        // Generate unique random indices
        while (indices.Count < count)
        {
            int randomIndex = UnityEngine.Random.Range(0, points.Count);
            if (!indices.Contains(randomIndex))
            {
                indices.Add(randomIndex);
                randomPoints.Add(points[randomIndex]);
            }
        }

        return randomPoints;
    }

    private Matrix4x4 CalculateTransformationMatrix(List<Vector3> pointsP, List<Vector3> pointsQ)
    {
        // Ensure that both point sets have at least three points
        if (pointsP.Count < 3 || pointsQ.Count < 3)
        {
            Debug.LogError("Insufficient points for alignment.");
            return Matrix4x4.identity;
        }

        // Calculate scaling factors
        float scaleX = Vector3.Distance(pointsQ[1], pointsQ[0]) / Vector3.Distance(pointsP[1], pointsP[0]);
        float scaleY = Vector3.Distance(pointsQ[2], pointsQ[0]) / Vector3.Distance(pointsP[2], pointsP[0]);
        float scaleZ = Vector3.Distance(pointsQ[2], pointsQ[1]) / Vector3.Distance(pointsP[2], pointsP[1]);

        // Construct the scaling matrix
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(scaleX, scaleY, scaleZ));

        // Calculate the centroids of each point set
        Vector3 centroidP = CalculateCentroid(pointsP);
        Vector3 centroidQ = CalculateCentroid(pointsQ);

        // Calculate the rotation matrix R
        Vector3 directionP = (pointsP[1] - pointsP[0]).normalized;
        Vector3 directionQ = (pointsQ[1] - pointsQ[0]).normalized;
        Quaternion rotation = Quaternion.FromToRotation(directionP, directionQ);

        // Construct the rotation matrix
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotation);

        // Apply scaling to the rotation matrix
        Matrix4x4 scaledRotationMatrix = rotationMatrix * scaleMatrix;

        // Calculate the translation vector T
        Vector3 translation = centroidQ - scaledRotationMatrix.MultiplyPoint3x4(centroidP);

        // Construct the transformation matrix
        Matrix4x4 transformationMatrix = Matrix4x4.TRS(translation, rotation, new Vector3(scaleX, scaleY, scaleZ));

        return transformationMatrix;
    }


    private Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;

        foreach (Vector3 point in points)
        {
            centroid += point;
        }

        centroid /= points.Count;

        return centroid;
    }
    private List<Vector3> ApplyTransformation(List<Vector3> points, Matrix4x4 transformationMatrix)
    {
        List<Vector3> transformedPoints = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            transformedPoints.Add(transformationMatrix.MultiplyPoint(point));
        }

        return transformedPoints;
    }

    private int CountInliers(List<Vector3> pointsP, List<Vector3> pointsQ, float threshold)
    {
        int inlierCount = 0;

        for (int i = 0; i < pointsP.Count; i++)
        {
            float distance = Vector3.Distance(pointsP[i], pointsQ[i]);
            
            if (distance < threshold)
            {
                inlierCount++;
            }
        }

        return inlierCount;
    }


    public void DisplayTransformationParameters(Matrix4x4 bestTransformationMatrix)
    {
        // Extract translation
        Vector3 translation = bestTransformationMatrix.GetColumn(3);

        // Extract rotation
        Quaternion rotation = Quaternion.LookRotation(
            bestTransformationMatrix.GetColumn(2),
            bestTransformationMatrix.GetColumn(1)
        );

        // Extract scaling
        Vector3 scaling = new Vector3(
            bestTransformationMatrix.GetColumn(0).magnitude,
            bestTransformationMatrix.GetColumn(1).magnitude,
            bestTransformationMatrix.GetColumn(2).magnitude
        );

        // Display the transformation parameters
        string result = "Rigid Transformation up to a Global Scale Parameters:\n" +
                        "Translation: " + translation + "\n" +
                        "Rotation: " + rotation + "\n" +
                        "Scaling: " + scaling + "\n";

        // Set the text of the UI component or log the result
        resultText.text = result;
    }


}
