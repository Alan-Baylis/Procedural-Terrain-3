/* NOTES:
*		To do:
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
		public float smoothingFactor = 0.85f; //maximum slope to be smoothed. 0.1 is good.
		public int chunkSize = 25;
		//public int zSize = 100;
		public float fidelity = 100;


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
				float scale = 1/fidelity;
				int iterations = (int)(chunkSize*fidelity);
	

				TerrainPerlin land = new TerrainPerlin (2f, 0f, 9999, 0, new Color(0f, 1f, 1f, 0.5f), new Color(0, 1, 0, 1));
				
				land.addLayer (20, 100, 15, 0, 60, new Color(0, 1, 0, 1), new Color(0, 1, 0, 1));
				land.addLayer (75, 50, 999, 0, 25, new Color(1, 0, 0, 1), new Color(0, 1, 0, 1));
				land.addLayer (70, 200, 25, 15, 100, new Color(0, 0, 1, 1), new Color(0, 1, 0, 1));
				land.addLayer (28, 200, 15, 10, 130, new Color(1, 0, 1, 1), new Color(0, 1, 0, 1));	
				//land.addPass (2, 2f, 5f, 999, 0);
				//land.addPass (3, 2, 10f, 0, 0);
				//land.addPass (1, 7f, 8, 0, 0);
				//land.addPass (0, 10, 2, 0, 0);
				land.addPass (3, 2, 15, 999, 0);


				for (int z = 0; z < (int) iterations; z++) {
						for (int x = 0; x < iterations; x++) {

				float leftToRightSlope = Mathf.Abs (land.getNoise (x * scale + scale + xPos, z * scale + scale + zPos, false) - land.getNoise (x* scale + xPos, z* scale + zPos, false));
				float rightToLeftSlope = Mathf.Abs (land.getNoise (x* scale + xPos, z * scale + scale + zPos, false) - land.getNoise (x* scale + scale + xPos, z* scale + zPos, false));

								if (rightToLeftSlope > leftToRightSlope) {

					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x* scale + xPos, z* scale + zPos , true), z* scale + zPos));					//0: bottom-left 1
					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x * scale+ xPos, z * scale+ zPos + scale, true), z * scale+ zPos + scale));			//1: top-left 1
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x * scale+ xPos + scale, z * scale+ zPos + scale, true), z * scale+ zPos + scale));		//2: top-right 1
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x * scale+ xPos + scale, z * scale+ zPos + scale, true), z * scale+ zPos + scale));		//3: top-right 2
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x * scale+ xPos + scale, z * scale+ zPos, true), z * scale+ zPos));			//4: bottom-right 1
					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x * scale+ xPos, z * scale+ zPos, true), z * scale+ zPos));					//0: bottom-left 2

								} else {

					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x * scale+ xPos, z * scale+ zPos, true), z * scale+ zPos));					//0: bottom-left 1
					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x * scale+ xPos, z * scale+ zPos + scale, true), z * scale+ zPos + scale));			//1: top-left 1
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x * scale+ xPos + scale, z * scale+ zPos, true), z * scale+ zPos));			//2: bottom-right 1
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x * scale+ xPos + scale, z * scale+ zPos, true), z * scale+ zPos));			//3: bottom-right 2
					vertices.Add (new Vector3 (x * scale+ xPos, land.getNoise (x * scale+ xPos, z * scale+ zPos + scale, true), z * scale+ zPos + scale));			//4: top-left 2
					vertices.Add (new Vector3 (x * scale+ xPos + scale, land.getNoise (x  * scale + xPos + scale, z * scale+ zPos + scale, true), z * scale+ zPos + scale));	//5: top-right 2

								}

								//Debug.Log (land.getNoise (x, z));
				
								triangles.Add ((z * iterations * 6) + (x * 6) + 0);
				triangles.Add ((z * iterations * 6) + (x * 6) + 1);
				triangles.Add ((z * iterations * 6) + (x * 6) + 2);
				triangles.Add ((z * iterations * 6) + (x * 6) + 3);
				triangles.Add ((z * iterations * 6) + (x * 6) + 4);
				triangles.Add ((z * iterations * 6) + (x * 6) + 5);

								//for (int i = 0; i<6; i++) {
								//colors.Add (new Color ( (land.getNoise (x * scale + xPos, z * scale + zPos)+46) / 50 + .3f, .63f + Random.Range (0, .01f), .53f + Random.Range (0, .01f), 1));
								//}
			

						}
				}
				
		/*
				//this needs to be exported to its own class
				//or maybe figure out how to incorporate this into the generation of the mesh
				

				for (int z = 0; z < zSize-1; z++) {
						for (int x = 0; x < xSize-1; x++) {

								float leftToRightSlope = Mathf.Abs (land.getNoise (x + 1 + xPos, z + 1 + zPos) - land.getNoise (x + xPos, z + zPos));
								float rightToLeftSlope = Mathf.Abs (land.getNoise (x + xPos, z + 1 + zPos) - land.getNoise (x + 1 + xPos, z + zPos));

								float nextZLeftToRightSlope = Mathf.Abs (land.getNoise (x + 1 + xPos, z + 2 + zPos) - land.getNoise (x + xPos, z + zPos + 1));
								float nextZRightToLeftSlope = Mathf.Abs (land.getNoise (x + xPos, z + 2 + zPos) - land.getNoise (x + 1 + xPos, z + zPos + 1));

								Debug.Log ("left-to-right slope: " + leftToRightSlope + ",  right-to-left slope: " + rightToLeftSlope);

								
					
								float rightCreaseHeight = (land.getNoise (x + 1, z + 1) + land.getNoise (x + 1, z)) / 2;
								float rightCreaseLeftHeight = (land.getNoise (x, z) + land.getNoise (x, z + 1)) / 2;
								float rightCreaseRightHeight = (land.getNoise (x + 2, z + 1) + land.getNoise (x + 2, z)) / 2;
								float rightAcrossCreaseHeight = (rightCreaseLeftHeight + rightCreaseRightHeight) / 2;
					
								bool rightIsSmooth = Mathf.Abs ((rightCreaseHeight - rightCreaseLeftHeight) - (rightCreaseRightHeight - rightCreaseHeight)) < smoothingFactor / 100;
					
								float topCreaseHeight = (land.getNoise (x, z + 1) + land.getNoise (x + 1, z + 1)) / 2;
								float topCreaseBottomHeight = (land.getNoise (x, z) + land.getNoise (x + 1, z)) / 2;
								float topCreaseTopHeight = (land.getNoise (x, z + 2) + land.getNoise (x + 1, z + 2)) / 2;
								//float topAcrossCreaseHeight = (topCreaseBottomHeight + topCreaseTopHeight)/2;
					
								bool topIsSmooth = Mathf.Abs ((topCreaseTopHeight - topCreaseHeight) - (topCreaseHeight - topCreaseBottomHeight)) < smoothingFactor / 100;

										//older smoothing method
										/*
										float diagCreaseHeight = (land.getNoise(x, z)+land.getNoise(x+1,z+1)) / 2;
										float diagCreaseTopHeight = land.getNoise(x, z+1);
										float diagCreaseBottomHeight = land.getNoise(x+1, z);
										
										
										bool diagIsSmooth = Mathf.Abs(Mathf.Abs(diagCreaseHeight - diagCreaseTopHeight)
						                	- Mathf.Abs(diagCreaseHeight - diagCreaseBottomHeight)) < smoothingFactor;

					
								float diagCreaseHeight = (land.getNoise (x, z) + land.getNoise (x + 1, z + 1)) / 2;
								float acrossCreaseHeight = (land.getNoise (x, z + 1) + land.getNoise (x + 1, z)) / 2;
					
					
								bool diagIsSmooth = Mathf.Abs (diagCreaseHeight - acrossCreaseHeight) < smoothingFactor / 100;
					
								//comment all this stuff so I know the orientations of the quads i'm dealing with
								
								if (topIsSmooth) {
										if(rightToLeftSlope > leftToRightSlope){
												if(nextZRightToLeftSlope > nextZRightToLeftSlope){
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
												else{
														//replace all vert references in triangles with new reference, skipping verts when drawing triangles
														for (int r = 0; r < triangles.Count; r++) {
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 1)) {
																triangles [r] += (xSize * 6) - 1;
															}
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 2)) {
																triangles [r] += (xSize * 6) + 4;
															}
															
														}
												}
										}

										else{
												if(nextZRightToLeftSlope > nextZRightToLeftSlope){
														//replace all vert references in triangles with new reference, skipping verts when drawing triangles
														for (int r = 0; r < triangles.Count; r++) {
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
																triangles [r] += (xSize * 6) - 4;
															}
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 5)) {
																triangles [r] += (xSize * 6) - 1;
															}

														}
												}
												else{
														//replace all vert references in triangles with new reference, skipping verts when drawing triangles
														for (int r = 0; r < triangles.Count; r++) {
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
																triangles [r] += (xSize * 6) - 4;
															}
															if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 5)) {
																triangles [r] += (xSize * 6) - 2;
															}
															
														}
												}
										}
										
										
								}
					

								if (rightIsSmooth) {
										if(rightToLeftSlope > leftToRightSlope){
						
												for (int r = 0; r < triangles.Count; r++) {
														if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
																triangles [r] += 6 - 2;
														}
														if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
																triangles [r] += 6 - 4;
														}
									
												}
										}
										else{
												for (int r = 0; r < triangles.Count; r++) {
													if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
														//triangles [r] += 6 + 2;
													}
													if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
														//triangles [r] += 6 - 1;
													}
													
												}
										}
								}
					
								if (diagIsSmooth && rightToLeftSlope > leftToRightSlope) {
						
										for (int r = 0; r < triangles.Count; r++) {
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
														triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 2];
												}
												if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 5)) {
														triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 0];
												}
							
										}
						
								}

								if (diagIsSmooth && rightToLeftSlope < leftToRightSlope) {
									
									for (int r = 0; r < triangles.Count; r++) {
										if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 4)) {
											//triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 1];
										}
										if (triangles [r].Equals ((z * xSize * 6) + (x * 6) + 3)) {
											//triangles [r] = triangles [(z * xSize * 6) + (x * 6) + 2];
										}
										
									}
									
								}
								

						}
				} */


				//for (int i = 0; i<25; i++) {
				//		Debug.Log (triangles [i]);

				//}

				//apply all the changes made to the mesh
				MeshFilter meshFilter = landscape.GetComponent<MeshFilter> ();
				MeshRenderer meshRenderer = landscape.GetComponent<MeshRenderer> ();
	
				mesh.vertices = vertices.ToArray ();
				mesh.triangles = triangles.ToArray ();
				mesh.colors = land.getGlobalColor().ToArray ();
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
				if (Input.anyKey) {
						//for (int zChunks = 0; zChunks < 5; zChunks ++){
						//for (int xChunks = 0; xChunks < 5; xChunks ++){
						getChunk (xChunks * chunkSize, zChunks * chunkSize, fidelity);
						xChunks++;
						if (xChunks > 10) {
								xChunks = 0;
								zChunks++;
						}

			//TerrainRandomizer rando = new TerrainRandomizer();
			float[] randomNumbers = TerrainRandomizer.getTwoRand(90);
			Debug.Log(   randomNumbers[0] + ", " + randomNumbers[1] );

				}
		}
}
