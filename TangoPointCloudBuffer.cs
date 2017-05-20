// Attach to a TangoPointCloud prefab. It will automatically check for a new point cloud, duplicate it, and add it to a rolling buffer.
// A few useful functions are copied and modified from TangoPointCloud.cs
// A more efficient version of this script would be written directly into TangoPointCloud.cs.
// The reasoning behind making this a separate script, is so that this should be "plug and play" with future updates to the Tango Unity SDK.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class TangoPointCloudBuffer : MonoBehaviour {

	public int bufferSize = 6;

	private TangoPointCloud tangoPointCloud;

	private Vector3[][] pointsBuffer;
	private double[] timestampBuffer;
	private int[] pointsCountBuffer;
	private int[] offsetBuffer;

	private int writeFrame = 0;

	private double lastPointCloudTimestamp = 0.0;

	// This seems like a bad idea for future proofing devices
	private const int MAX_POINT_COUNT = 61440;

	private Mesh m_mesh;
	private Renderer m_renderer;

	private double renderedMeshTimestamp = 0.0;

	private bool writePointCloud = false;

	// Use this for initialization
	void Start () {
		tangoPointCloud = gameObject.GetComponent<TangoPointCloud> ();

		pointsBuffer = new Vector3[bufferSize][];
		timestampBuffer = new double[bufferSize];
		pointsCountBuffer = new int[bufferSize];
		offsetBuffer = new int[bufferSize];

		for (int i = 0; i < bufferSize; i++) {
			pointsBuffer [i] = new Vector3[MAX_POINT_COUNT];
			offsetBuffer [i] = bufferSize - i - 1;
		}

		m_mesh = GetComponent<MeshFilter>().mesh;
		m_mesh.Clear();

		m_renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (writePointCloud) {
			System.Array.Copy (tangoPointCloud.m_points, pointsBuffer [writeFrame], tangoPointCloud.m_pointsCount);
			timestampBuffer [writeFrame] = tangoPointCloud.m_depthTimestamp;
			pointsCountBuffer [writeFrame] = tangoPointCloud.m_pointsCount;

			writeFrame++;

			if (writeFrame >= bufferSize)
				writeFrame = 0;

			for (int i = 0; i < bufferSize; i++) {
				offsetBuffer [i]++;
				if (offsetBuffer [i] >= bufferSize)
					offsetBuffer [i] = 0;
			}

			lastPointCloudTimestamp = tangoPointCloud.m_depthTimestamp;

			writePointCloud = false;
		}

		if (tangoPointCloud.m_depthTimestamp != lastPointCloudTimestamp) {
			writePointCloud = true;
		}
	}

	
	public Vector3 FindClosestPoint(Camera cam, double colorTimestamp, Vector2 pos, int maxDist)
	{
		int pointsIndex = FindClosestPointCloud(colorTimestamp);

		int bestIndex = -1;

		if (pointsIndex >= 0) {
			float bestDistSqr = 0;

			for (int it = 0; it < pointsCountBuffer [pointsIndex]; ++it) {
				Vector3 screenPos3 = cam.WorldToScreenPoint (pointsBuffer [pointsIndex] [it]);
				Vector2 screenPos = new Vector2 (screenPos3.x, screenPos3.y);

				float distSqr = Vector2.SqrMagnitude (screenPos - pos);
				if (distSqr > maxDist * maxDist) {
					continue;
				}

				if (bestIndex == -1 || distSqr < bestDistSqr) {
					bestIndex = it;
					bestDistSqr = distSqr;
				}
			}
		}

		if (bestIndex >= 0)
			return pointsBuffer [pointsIndex] [bestIndex];
		else
			return Vector3.zero;
	}

	// Finds the closest pointcloud given a timestamp
	public int FindClosestPointCloud(double timestamp) {
		int index = -1;
		float time = 1000f;
		for (int i = 0; i < bufferSize; i++) {
			float timeDifference = Mathf.Abs ((float)(timestampBuffer [i] - timestamp));
			if (timeDifference < time) {
				index = i;
				time = timeDifference;
			}
		}
		return index;
	}

	// This will overwrite any pointcloud mesh set by TangoPointCloud.cs. If UpdateMesh is true, the next pointcloud will overwrite this mesh.
	public void RenderPointCloud(double colorTimestamp){
		int pointsIndex = FindClosestPointCloud(colorTimestamp);

		if (timestampBuffer [pointsIndex] != renderedMeshTimestamp) {
			int[] indices = new int[pointsCountBuffer [pointsIndex]];
			for (int i = 0; i < pointsCountBuffer [pointsIndex]; ++i) {
				indices [i] = i;
			}

			m_mesh.Clear ();
			m_mesh.vertices = pointsBuffer [pointsIndex];
			m_mesh.SetIndices (indices, MeshTopology.Points, 0);

			renderedMeshTimestamp = timestampBuffer [pointsIndex];
		}
	}

	// Checks to see if the color time stamp is within 1/30th of a second of the pointcloud timestamp.
	public bool isSyncFrame(double colorTimestamp) {
		int pointsIndex = FindClosestPointCloud(colorTimestamp);

		float difference = (float)(colorTimestamp - timestampBuffer [pointsIndex]);

		if (Mathf.Abs (difference) < 1f / 60f)
			return true;
		else
			return false;
	}

	// Returns the timestamp of the nearest point cloud
	public double ClosestPointCloudTimestamp(double colorTimestamp) {
		int pointsIndex = FindClosestPointCloud(colorTimestamp);

		return timestampBuffer [pointsIndex];
	}
}
