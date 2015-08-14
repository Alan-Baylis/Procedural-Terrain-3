using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateNewMesh : MonoBehaviour {

	public Material matLandscape;
	public GameObject landscape;
	private int width = 2;
	private int length = 2;
	private float scale = 0.5f;	
	private int quadWidth = 120;
	private int quadLength = 120;
	private int quadTick = 1;
	private int modTick = 1;
	private int triTick = 1;
	private float perlinValue;
	private float perlinMovement = 5f;
	private float perlinmoveScale = 3f;
	private float heightMult = 3f;
	private float heightMin = 10f;
	private float heightMax = 25f;
	private float perlinmoveScale2 = 10f;
	private float heightMult2 = 12f;

	// Use this for initialization
	void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {

	if (Input.GetKeyDown("space")){

			landscape = new GameObject("Landscape" , typeof(MeshFilter), typeof(MeshRenderer), typeof(ModifyTerrain));
			Mesh mesh = new Mesh();
			int[] triangles = new int[quadLength*quadWidth*6];
			Vector3[] vertices = new Vector3[(width * length)*(quadWidth * quadLength)];
			
			//create 2 dimensional series of quads inside inner loop
			for (int quadY = 0; quadY < quadLength; quadY++){
				for (int quadX = 0; quadX < quadWidth; quadX++){

					//create basic quad of length*width
					for (int y = 0; y < length ; y++){
						for ( int x = 0; x < width ; x++){

							//first pass of perlin noise
							perlinValue = heightMult * Mathf.PerlinNoise((x*scale + quadX*scale)/perlinmoveScale, (y*scale + quadY*scale)/perlinmoveScale);

							if (perlinValue < heightMin){
								perlinValue = heightMin;
							}
							else if (perlinValue > heightMax){
								perlinValue = heightMax;
							}

							vertices[((y*width)+x)+ quadTick-1] = 
								new Vector3(x*scale + quadX*scale, perlinValue, y*scale + quadY*scale);

							//second pass of perlin noise
							vertices[((y*width)+x)+ quadTick-1] += Vector3.up * (heightMult2 * Mathf.PerlinNoise((x*scale + quadX*scale)/perlinmoveScale2, (y*scale + quadY*scale)/perlinmoveScale2));
						
							//add wild, unfettered distortion
							//vertices[((y*width)+x)+ quadTick-1] += Vector3.up * Random.Range (-0.03f,0.03f);;
						}
					}

					//create triangles
					triangles[0+(triTick-1)*6] = 0+quadTick-1; //Debug.Log("Currect triangle index: " + (0+(triTick-1)*6) +", get's vertex " + (0+quadTick-1));
					triangles[1+(triTick-1)*6] = 2+quadTick-1; //Debug.Log("Currect triangle index: " + (1+(triTick-1)*6) +", get's vertex " + (1+quadTick-1));
					triangles[2+(triTick-1)*6] = 3+quadTick-1; //Debug.Log("Currect triangle index: " + (2+(triTick-1)*6) +", get's vertex " + (3+quadTick-1));
					triangles[3+(triTick-1)*6] = 0+quadTick-1; //Debug.Log("Currect triangle index: " + (3+(triTick-1)*6) +", get's vertex " + (0+quadTick-1));
					triangles[4+(triTick-1)*6] = 3+quadTick-1; //Debug.Log("Currect triangle index: " + (4+(triTick-1)*6) +", get's vertex " + (3+quadTick-1));
					triangles[5+(triTick-1)*6] = 1+quadTick-1; //Debug.Log("Currect triangle index: " + (5+(triTick-1)*6) +", get's vertex " + (2+quadTick-1));

					//Debug.Log("Triangle Upper Bound: " + triangles.GetUpperBound(0));
					//Debug.Log("0+ quadTick - 1: " + (0+quadTick-1));
					
					quadTick += length*width;
					triTick ++;
				}
			}

			MeshFilter meshFilter = landscape.GetComponent<MeshFilter>();
			MeshRenderer meshRenderer = landscape.GetComponent<MeshRenderer>();

			mesh.vertices = vertices;
			mesh.triangles = triangles;
			meshFilter.mesh = mesh;

			meshRenderer.material = matLandscape;
			meshFilter.mesh.RecalculateNormals();
			meshFilter.mesh.Optimize ();
			meshFilter.mesh.MarkDynamic ();


			//Debug~
			Debug.Log (triangles.GetUpperBound(0));
			Debug.Log(meshFilter.mesh.vertices.GetUpperBound (0));

	}


		/*if (landscape == false){
			MeshFilter perlFilter = landscape.GetComponent<MeshFilter>();
			Mesh perlMesh = perlFilter.mesh;
			Vector3[] perlVerts = perlMesh.vertices;
			modTick = 1;

			Debug.Log ("upper bound of verts: " + landscape.GetComponent<MeshFilter>().mesh.vertices.GetUpperBound(0));
			for (int quadY = 0; quadY < quadLength; quadY++){
				Debug.Log("quadY: "+ quadY);
				for (int quadX = 0; quadX < quadWidth; quadX++){
					Debug.Log("quadX: "+ quadX);
					//create basic quad of length*width
					for (int y = 0; y < length ; y++){
						Debug.Log("y: "+ y);
						for ( int x = 0; x < width ; x++){
							Debug.Log("x: "+ x);
							//Debug.Log ("perlVet current index:" + (((y*width)+x)+ modTick-1));
							perlVerts[((y*width)+x)+ modTick-1] = Vector3.up * Mathf.PerlinNoise((x*scale + quadX*scale) + perlinMovement, (y*scale + quadY*scale));
							
						}
					}
					
					modTick += length*width;
					perlinMovement += perlinmoveScale;
				}
			}
			//perlMesh.vertices = perlVerts;
			//perlFilter.mesh = perlMesh;
			landscape.GetComponent<MeshFilter>().mesh.vertices = perlVerts;
			landscape.GetComponent<MeshFilter>().mesh.RecalculateNormals();
		}
*/
	}
}
