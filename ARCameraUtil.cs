using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCameraUtil : MonoBehaviour {

	public static Mesh CameraBackplate(Camera camera, float distance){
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[4];
		Vector2[] uv = new Vector2[]{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};

		for (int i = 0; i < 4; i++) {
			vertices [i] = camera.ViewportPointToRay (new Vector3 (uv[i].x, uv[i].y, 0f)).direction;
			vertices [i] = Quaternion.Inverse (camera.transform.rotation) * (vertices [i] - camera.transform.position);
			vertices [i] = vertices [i] * (1f / vertices [i].z) * distance;
		}

		int[] triangles = new int[6]{0, 3, 1, 0, 2, 3};

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;

		return mesh;
	}
}
