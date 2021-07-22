using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererSetup : MonoBehaviour
{
    public Material lineRendererMaterial;

    private List<GameObject> lineChildren = new List<GameObject>();

    public static float MULTI_LINE_WIDTH = 0.05f;

    public void SetupLine(int size, Color colour, Vector3 startPoint, Vector3 endPoint)
    {
        SetLength(size, startPoint, endPoint);
        SetColor(colour);
    }

    private void SetColor(Color colour)
    {
        foreach (GameObject lineObject in lineChildren)
        {
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

            lineRenderer.startColor = colour;
            lineRenderer.endColor = colour;
        }
    }

    private void SetLength(int size, Vector3 startPoint, Vector3 endPoint)
    {
        if (size != 1) // Line has gaps, time to do math -> Create multiple line renderers with gaps to form line
        {
            // Calc line details
            float lineRealSize = Vector3.Distance(startPoint, endPoint);
            Vector3 lineHeading = (endPoint - startPoint).normalized;

            // Calculate midpoints
            int midpointsNum = size - 1;
            float[] midpoints = new float[midpointsNum];

            float startingPercent = 1.0f / size;
            float currentPercent = startingPercent;

            for (int i = 0; i < midpointsNum; i++)
            {
                midpoints[i] = currentPercent;
                currentPercent += startingPercent;
            }

            // Calculate gap number and dist percentage 
            int gapN = size - 1; // Number of gaps in line
            float gapP = 0.10f - (0.01f * gapN); // Percentage of distance those gaps are
            float halfGapP = gapP / 2;

            // Calculate interquartile numbers -> Formula works out to be: 2n-2 where n is size -> or just 2 * gapN
            int interquartileNum = gapN * 2;
            float[] interquartilePoints = new float[interquartileNum];
            int currentMidpoint = 0;

            for (int i = 0; i < interquartileNum; i += 2)
            {
                interquartilePoints[i] = midpoints[currentMidpoint] - halfGapP;
                interquartilePoints[i + 1] = midpoints[currentMidpoint] + halfGapP;

                currentMidpoint++;
            }

            // Calculate start and end points
            List<Vector3> startPoints = new List<Vector3>();
            List<Vector3> endPoints = new List<Vector3>();

            // First line
            startPoints.Add(startPoint);
            endPoints.Add(startPoint + (lineHeading * (interquartilePoints[0] * lineRealSize)));

            // Final line
            startPoints.Add(startPoint + (lineHeading * (interquartilePoints[interquartileNum - 1] * lineRealSize)));
            endPoints.Add(endPoint);

            if (size > 2) // If size greater than 2, then more than 2 lines means there will be middle lines to accomodate
            {
                int currentIQRPoint = 0;

                for (int i = 0; i < size - 2; i++)
                {
                    startPoints.Add(startPoint + (lineHeading * (interquartilePoints[(2 * currentIQRPoint) + 1] * lineRealSize)));
                    endPoints.Add(startPoint + (lineHeading * (interquartilePoints[(2 * currentIQRPoint) + 2] * lineRealSize)));

                    currentIQRPoint++;
                }
            }

            // Now create the lines
            for (int i = 0; i < startPoints.Count; i++)
            {
                CreateNewLineRenderer(startPoints[i], endPoints[i]);
            }
        }
        else // No gaps, no fancy business -> Set line to start and end point
        {
            CreateNewLineRenderer(startPoint, endPoint);
        }
    }

    private void CreateNewLineRenderer(Vector3 s, Vector3 e)
    {
        GameObject newLineObject = new GameObject("Line Child");
        newLineObject.transform.SetParent(this.transform);

        LineRenderer lineRenderer = newLineObject.gameObject.AddComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[] { s, e });
        lineRenderer.material = lineRendererMaterial;

        lineRenderer.startWidth = MULTI_LINE_WIDTH;
        lineRenderer.endWidth = MULTI_LINE_WIDTH;

        lineChildren.Add(newLineObject);
    }

    public void ClearLine()
    {
        foreach (GameObject g in lineChildren)
        {
            Destroy(g);
        }

        lineChildren.Clear();
    }
}
