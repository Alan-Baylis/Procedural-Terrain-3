using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifyTerrain : MonoBehaviour {

	void Update () {
		
		if (Input.GetKeyDown("x")){
			float distRange = 0.05f;
			
			Debug.Log ("x pressed");
			MeshFilter meshFilter = this.GetComponent<MeshFilter>();
			Mesh mesh = meshFilter.mesh;
			
			//Debug.Log (mesh.vertices.GetUpperBound (0));
			
			List<List<int>> allDupeVerts = new List<List<int>>();
			List<int>[] identicalVerts = new List<int>[mesh.vertices.GetUpperBound (0)];
			for ( int i = 0; i < mesh.vertices.GetUpperBound(0); i++){
				identicalVerts[i] = new List<int>();
			}
			
			for (int i = 0; i < mesh.vertices.GetUpperBound(0) ; i++){
				for (int j = 0; j < mesh.vertices.GetUpperBound(0) ; j++){
					
					//if the vertex's position (excluding Y) is identical to that of any other vertex
					if (mesh.vertices[i].x == mesh.vertices[j].x && mesh.vertices[i].z == mesh.vertices[j].z){
						//Debug.Log (i + "=" + j + ": identical match!");
						identicalVerts[i].Add (j);
					}
					
				}

				//Debug.Log (identicalVerts[i][0]);
				allDupeVerts.Add(identicalVerts[i]);
				//identicalVerts.Clear();

			}
			
			//for (int a = 0; a < allDupeVerts.Count; a++){
			//	Debug.Log ("" + allDupeVerts(a.ToString));
			//}
			Vector3[] vertices = meshFilter.mesh.vertices;
			for (int i = 0; i < allDupeVerts.Count -1 ; i++){
				//Debug.Log("List "+ i + " count: " + allDupeVerts[i].Count);
				float randAmount = Random.Range(distRange*-1, distRange);
				if(allDupeVerts[i].Count != 0){
					for (int j = 0; j < allDupeVerts[i].Count; j++){
						//Debug.Log ("All dupes: " + allDupeVerts[i][j]);

						//move vertices : allDupeVerts[i][j] up or down randomly
						vertices[allDupeVerts[i][j]] += Vector3.up * randAmount;
					}
				}

			}
			//Debug.Log("mesh.vertices count: " + mesh.vertices.GetUpperBound(0));
			//Debug.Log("vertices count: " + vertices.GetUpperBound (0));
			//for(int i = 0; i < vertices.GetUpperBound(0); i++){
			//	Debug.Log("mesh.vertices [" + i + "]: " + mesh.vertices[i]);
			//	Debug.Log("vertices [" + i + "]: " + vertices[i]);
			//}

			mesh.vertices = vertices;
			mesh.RecalculateNormals();
			mesh.MarkDynamic();
			meshFilter.mesh = mesh;
			//meshFilter.mesh.RecalculateNormals();
			//meshFilter.mesh.Optimize ();
			//meshFilter.mesh.RecalculateBounds();

			
			
		}
	}
}
