/* NOTES:
*		To do:
*		1. fix triangle orientation
*		2. work on scaling of smoothing algorithm
*		3. work on "fidelity" argument for LOD
*		4. color assignment / layers
*		5. color by slope
*		6. organize everything into sensible structure -- easy, "black box", usability
*		7. add horizontal vertex distortion
*
*/



/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateNewMeshSmooth2 : MonoBehaviour
{
		public GameObject landscape;
		public Material matLandscape2;
		public float smoothingFactor = 0.85f; //maximum slope to be smoothed. 0.1 is good.
		public int xSize = 100;
		public int zSize = 100;
		
		
	
	
		public GameObject getChunk(int xPos, int zPos, float fidelity)
		{
				landscape = new GameObject ("Landscape", typeof(MeshFilter), typeof(MeshRenderer));
				Mesh mesh = new Mesh ();
				List<Vector3> vertices = new List<Vector3> ();
				List<int> triangles = new List<int> ();
				List<Color> colors = new List<Color> ();

				float perlinXScale = 25.01f;
				float perlinZScale = 25.01f;
				float perlinHeightMult = 10f;
	

				//TerrainPerlin land = new TerrainPerlin (1f, .5f, 999, 0);
				//land.addLayer (20, 50, 0, 0, 30);
				//land.addLayer (200, 300, 99, 0, 150);
				//land.addLayer (45, 50, 99, 0, 30);
				//land.addLayer (60, 50, 99, 0, 30);
				//land.addLayer (3, 20, 99, 0, 10);
				//land.addLayer (3, 10f, 99, 0, 5);
				//land.addPass (2, 0.5f, 1.75f, 99, 0);
				//land.addPass (1, 2.01f, 0.55f, 99, 0);
				//land.addPass (0, 20, 10f, 99, 0);
				//land.addLayer (25, 20, 99, 0, 10);
				land.addPass (2, 1.01f, 1, 99, 0);


				for (int z = 0; z < zSize; z++) {
						for (int x = 0; x < xSize; x++) {

				vertices.Add (new Vector3 (x+xPos, land.getNoise (x+xPos, z+zPos), z+zPos));			//0: bottom-left 1
				vertices.Add (new Vector3 (x+xPos, land.getNoise (x+xPos, z+zPos + 1), z+zPos + 1));		//1: top-left 1
				vertices.Add (new Vector3 (x+xPos + 1, land.getNoise (x+xPos + 1, z+zPos + 1), z+zPos + 1));		//2: top-right 1

				if (z >= zSize-1 || x >= xSize-1){
					vertices.Add (new Vector3 (x+xPos + 1, land.getNoise (x+xPos + 1, z+zPos + 1), z+zPos + 1)); //3: top-right 2
				}		

				vertices.Add (new Vector3 (x+xPos + 1, land.getNoise (x+xPos + 1, z+zPos), z+zPos));		//4: bottom-right 1  now 3!

				if (z == 0 || x == 0){
					vertices.Add (new Vector3 (x+xPos, land.getNoise (x+xPos, z+zPos), z+zPos));			//5: bottom-left 2
				}

					
								triangles.Add ((z * xSize * 6) + (x * 6) + 0);
								triangles.Add ((z * xSize * 6) + (x * 6) + 1);
								triangles.Add ((z * xSize * 6) + (x * 6) + 2);

								if (z != zSize-1 && x != xSize-1){
									triangles.Add ((z+1 * xSize * 6) + (x+1 * 6) + 0); //0 from quad forward and right
								}
								else{
									triangles.Add ((z * xSize * 6) + (x * 6) + 3);
								}

								triangles.Add ((z * xSize * 6) + (x * 6) + 4);

								if (z != 0 && x != 0){
										triangles.Add ((z-1 * xSize * 6) + (x-1 * 6) + 2); //2 from quad back and left
									}
								else{
									triangles.Add ((z * xSize * 6) + (x * 6) + 5);
									
								}

								//for (int i = 0; i<6; i++) {
								//		colors.Add (new Color (.3f + Random.Range (0, .01f), land.getNoise (x+xPos, z+zPos) / 20 + .4f, .73f + Random.Range (0, .01f), 1));
								//}

						}
				}
				
				//this needs to be exported to its own class
				//or maybe figure out how to incorporate this into the generation of the mesh
				

				for (int z = 0; z < zSize-1; z++) {
						for (int x = 0; x < xSize-1; x++) {
								
								
								float rightCreaseHeight = (land.getNoise(x+1, z+1)+land.getNoise(x+1,z)) / 2;
								float rightCreaseLeftHeight = (land.getNoise(x, z)+land.getNoise(x,z+1)) / 2;
								float rightCreaseRightHeight = (land.getNoise(x+2, z+1)+land.getNoise(x+2,z)) / 2;
								float rightAcrossCreaseHeight = (rightCreaseLeftHeight + rightCreaseRightHeight)/2;
												
								bool rightIsSmooth = Mathf.Abs(rightCreaseHeight- rightAcrossCreaseHeight)<smoothingFactor/100;
				
								float topCreaseHeight = (land.getNoise(x, z+1)+land.getNoise(x+1,z+1)) / 2;
								float topCreaseBottomHeight = (land.getNoise(x, z)+land.getNoise(x+1,z)) / 2;
								float topCreaseTopHeight = (land.getNoise(x, z+2)+land.getNoise(x+1,z+2)) / 2;
								float topAcrossCreaseHeight = (topCreaseBottomHeight + topCreaseTopHeight)/2;
				
								bool topIsSmooth = Mathf.Abs(topCreaseHeight- topAcrossCreaseHeight)<smoothingFactor/100;

				/*
								float diagCreaseHeight = (land.getNoise(x, z)+land.getNoise(x+1,z+1)) / 2;
								float diagCreaseTopHeight = land.getNoise(x, z+1);
								float diagCreaseBottomHeight = land.getNoise(x+1, z);
								
								
								bool diagIsSmooth = Mathf.Abs(Mathf.Abs(diagCreaseHeight - diagCreaseTopHeight)
				                	- Mathf.Abs(diagCreaseHeight - diagCreaseBottomHeight)) < smoothingFactor;
*/

							/*	float diagCreaseHeight = (land.getNoise(x, z)+land.getNoise(x+1, z+1)) / 2;
								float acrossCreaseHeight = (land.getNoise(x, z+1)+land.getNoise(x+1, z)) / 2;

								bool diagIsSmooth = Mathf.Abs(diagCreaseHeight-acrossCreaseHeight)<smoothingFactor/100;

								if (topIsSmooth) {

										//replace all vert references in triangles with new reference, skipping verts when drawing triangles
										for (int r = 0; r < triangles.Count; r++) {
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 1)) {
														triangles [r] += (xSize * 6) - 1;
												}
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 2)) {
														triangles [r] += (xSize * 6) + 2;
												}
						
										}
					
								}

								if (rightIsSmooth) {

										for (int r = 0; r < triangles.Count; r++) {
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
														triangles [r] += 6 - 2;
												}
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
														triangles [r] += 6 - 4;
												}
						
										}

								}

								if (diagIsSmooth) {

										for (int r = 0; r < triangles.Count; r++) {
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
														triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 2];
												}
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 5)) {
														triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 0];
												}
						
										}

								}
						}
				}


				for (int i = 0; i<25; i++) {
						Debug.Log (triangles [i]);

				}

				//apply all the changes made to the mesh
				MeshFilter meshFilter = landscape.GetComponent<MeshFilter> ();
				MeshRenderer meshRenderer = landscape.GetComponent<MeshRenderer> ();
	
				mesh.vertices = vertices.ToArray ();
				mesh.triangles = triangles.ToArray ();
				mesh.colors = colors.ToArray ();
				meshFilter.mesh = mesh;
		
				meshRenderer.material = matLandscape2;
				meshFilter.mesh.RecalculateNormals ();
				//meshFilter.mesh.Optimize ();
				//meshFilter.mesh.MarkDynamic ();

		return landscape;
		}

	int xChunks = 0;
	int zChunks = 0;
	
		// Update is called once per frame
		void Update ()
		{
		if (Input.anyKey){
		//for (int zChunks = 0; zChunks < 5; zChunks ++){
			//for (int xChunks = 0; xChunks < 5; xChunks ++){
				getChunk(xChunks*xSize, zChunks*zSize, 20);
			xChunks++;
			if (xChunks>20){
				xChunks=0;
				zChunks++;
			}
			//}
		//}
		}
	}
}
*/