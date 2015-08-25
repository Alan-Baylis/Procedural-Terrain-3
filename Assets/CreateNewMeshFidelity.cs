/* NOTES:
*		To do:
*		***This needs major commenting
*		1. fix triangle orientation
*		2. work on edge cases of smoothing algorithm
*		3. work on "fidelity" argument for LOD
*		4. color assignment / layers
*		5. color by slope
*		6. organize everything into sensible structure -- easy, "black box", usability
*		7. add horizontal vertex distortion
*
*
*		forget smoothing right now, possibly forever. it's expensive and works against the low-poly thing
*
*		set up lods
*		first, start doing fidelity on a per-distance from camera basis
*		this may need to be changed from linear to a curve.
*		
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateNewMeshFidelity : MonoBehaviour
{
	public GameObject landscape;
	public Material matLandscape2;
	public int chunkSize = 25;
	public float fidelity = 0.6f; //must be even


	public GameObject getChunk (float xPos, float zPos, float fidelity)
	{
		landscape = new GameObject ("Landscape", typeof(MeshFilter), typeof(MeshRenderer));
		Mesh mesh = new Mesh ();
		List<Vector3> vertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Color> colors = new List<Color> ();

		float perlinXScale = 25.01f;
		float perlinZScale = 25.01f;
		float perlinHeightMult = 10f;
		float scale = 1 / fidelity;
		int iterations = (int)(chunkSize * fidelity);
	

		//add TerrainHeightGenerator component to this gameobject, then call a method within that passes in parameters
		TerrainHeightGenerator land = gameObject.AddComponent<TerrainHeightGenerator> ();
		land.CreateTerrainHeightGenerator (500f, 20f, 9999, 0, Color.gray , new Color (0.2f, 0.6f, 0.1f, 1));



		//Curve is a 4 point Logistical nonlinear regression model (sigmoid function)
		// y = ((A-D)/(1+((x/C)^B))) + D
		//x = 50*(((100)/(y)))-1)^(1/7.24)
		//Inverse: x = C*(((A-D)/(y)-D))-1)^(1/B

		//a = height of layer (or minimum asymtote)
		//b = hill slope (curvature of graph)
		//c = i think a/2, which would be half of the total height(the inflection point)
		//d = 0, probably always 0 (maximum asymptote)

		//((98.60896+2.385079)/(1+((x/50.93624)^7.239703))) + -2.385079 //Formula for predicting percent coverage from offset
		//((100+0.785)/(1+((x/50)^7))) + -0.785 // more streamlined?\\
		//((100+1.313)/(1+((x/50)^7.25)))-1.313 //maybe more refined

		//(106.583 (100-1 X)^(1/7))/(157+200 X)^(1/7) 	//Inverse. put in a percent, get out the needed offset


		land.addLayer (200, 100, 31, 30, 50f, new Color (.3f, .3f, .3f, 1), new Color (.2f, .2f, .65f, 1));
		land.addLayer (75, 60, 999999999, 0, 44, new Color (.41f, .41f, .41f, 1), new Color (.4F, .2f, .1F, 1));
		//land.addLayer (70, 200, 25, 15, 100, new Color (0, 0, 1, 1), new Color (0, 1, 0, 1));
		//land.addLayer (28, 200, 15, 10, 130, new Color (1, 0, 1, 1), new Color (0, 1, 0, 1));	
		land.addPass (2, 2f, 5f, 999, 0);
		//land.addPass (3, 2, 10f, 0, 0);
		land.addPass (1, 14f, 9, 0, 0);
		//land.addPass (0, 10, 2, 0, 0);
		//land.addPass (3, 2, 15, 999, 0);


		for (int z = 0; z < (int) iterations; z++) {
			for (int x = 0; x < iterations; x++) {

				float leftToRightSlope = Mathf.Abs (land.getNoise (x * scale + scale + xPos, z * scale + scale + zPos, false) - land.getNoise (x * scale + xPos, z * scale + zPos, false));
				float rightToLeftSlope = Mathf.Abs (land.getNoise (x * scale + xPos, z * scale + scale + zPos, false) - land.getNoise (x * scale + scale + xPos, z * scale + zPos, false));

				if (rightToLeftSlope > leftToRightSlope) {

					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos, true), z * scale + zPos));					//0: bottom-left 1
					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos + scale, true), z * scale + zPos + scale));			//1: top-left 1
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos + scale, true), z * scale + zPos + scale));		//2: top-right 1
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos + scale, true), z * scale + zPos + scale));		//3: top-right 2
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos, true), z * scale + zPos));			//4: bottom-right 1
					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos, true), z * scale + zPos));					//0: bottom-left 2

				} else {

					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos, true), z * scale + zPos));					//0: bottom-left 1
					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos + scale, true), z * scale + zPos + scale));			//1: top-left 1
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos, true), z * scale + zPos));			//2: bottom-right 1
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos, true), z * scale + zPos));			//3: bottom-right 2
					vertices.Add (new Vector3 (x * scale + xPos, land.getNoise (x * scale + xPos, z * scale + zPos + scale, true), z * scale + zPos + scale));			//4: top-left 2
					vertices.Add (new Vector3 (x * scale + xPos + scale, land.getNoise (x * scale + xPos + scale, z * scale + zPos + scale, true), z * scale + zPos + scale));	//5: top-right 2

				}
								
				triangles.Add ((z * iterations * 6) + (x * 6) + 0);
				triangles.Add ((z * iterations * 6) + (x * 6) + 1);
				triangles.Add ((z * iterations * 6) + (x * 6) + 2);
				triangles.Add ((z * iterations * 6) + (x * 6) + 3);
				triangles.Add ((z * iterations * 6) + (x * 6) + 4);
				triangles.Add ((z * iterations * 6) + (x * 6) + 5);
			

			}
		}
					

		//apply all the changes made to the mesh
		MeshFilter meshFilter = landscape.GetComponent<MeshFilter> ();
		MeshRenderer meshRenderer = landscape.GetComponent<MeshRenderer> ();
	
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.colors = land.getGlobalColor ().ToArray ();
		meshFilter.mesh = mesh;
		
		meshRenderer.material = matLandscape2;
		meshFilter.mesh.RecalculateNormals ();
		meshFilter.mesh.Optimize (); 		//don't remember what this does
		//meshFilter.mesh.MarkDynamic (); 	//don't remember what this does

		//Log height diagnostics
		//Debug.Log ("MAX: " + heightDiagnostic.getMaxForLayer(1));
		//Debug.Log ("MIN: " + heightDiagnostic.getMinForLayer(1));
		//Debug.Log ("TOTAL: " + heightDiagnostic.getTotalHeightForLayer(1));
		//Debug.Log ("AVG: " + heightDiagnostic.getAverageHeightForLayer(1));
		//Debug.Log ("% DISP: " + heightDiagnostic.getPercentDisplayedForLayer (1));
		
		return landscape;
	}

	int xChunks = 0;
	int zChunks = 0;
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.anyKey) {

			getChunk (xChunks * chunkSize, zChunks * chunkSize, fidelity);
			xChunks++;
			if (xChunks > 10) {
				xChunks = 0;
				zChunks++;
			}
		}
	}
}
