/*using UnityEngine;
using System.Collections.Generic;

//add a random seed later
//add negative passes and layers
//allow some passes to affect all layers
//allow some layers to affect only specific layers
//send back color info
//implement min and max


//probably going to have to produce whole mesh here
//send in starting coordinates and chunk size, send back entire colored mesh

public class TerrainPerlin : MonoBehaviour {
	private List<List<float>> scale = new List<List<float>>();
	private List<List<float>> height= new List<List<float>>();
	private List<List<float>> heightMax= new List<List<float>>();
	private List<List<float>> heightMin= new List<List<float>>();
	private List<float> offset = new List<float>();
	private int brownianOctaves = 4;
	List<Color> colors = new List<Color> ();

public TerrainPerlin(float scale, float height, float heightMax, float heightMin, Color colorOne, Color colorTwo){

	this.scale.Add (new List<float>());
	this.scale[0].Add(scale);
	
	this.height.Add (new List<float>());
	this.height[0].Add(height);

	this.heightMax.Add (new List<float>());
	this.heightMax[0].Add(heightMax);

	this.heightMin.Add (new List<float>());
	this.heightMin[0].Add(heightMin);
	
	this.offset.Add(0);
	}


public void addPass(int layer, float scale, float height, float heightMax, float heightMin){
	this.scale[layer].Add(scale);
	this.height[layer].Add(height);
	this.heightMax[layer].Add(heightMax);
	this.heightMin[layer].Add(heightMin);
	}

	public void addLayer(float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo){
	this.scale.Add(new List<float>());
	this.height.Add(new List<float>());
	this.heightMax.Add(new List<float>());
	this.heightMin.Add(new List<float>());

	this.scale[this.scale.Count-1].Add(scale);
	this.height[this.height.Count-1].Add(height);
	this.heightMax[this.heightMax.Count-1].Add (heightMax);
	this.heightMin[this.heightMin.Count-1].Add (heightMin);

	this.offset.Add(offset);
	
}
public float getNoise(float x, float z){
		float[] origHeight = new float[scale.Count+1];
		origHeight[0] = (brownianNoise(x/scale[0][0], z/scale[0][0])*height[0][0]);

		float finalHeight = -9999999;
		float minusScale = 0;

		for (int layer = 0;  layer < scale.Count ; layer++){
			for (int pass = 0;  pass < scale[layer].Count ; pass++){
				float noise = (brownianNoise(x/scale[layer][pass], z/scale[layer][pass])*height[layer][pass]) - offset[layer];
				if (noise > heightMax[layer][pass]+origHeight[layer]){
					origHeight[layer] = heightMax[layer][pass] - offset[layer];
					if (layer > 0) {
						origHeight[layer] += origHeight[layer-1];
					}
				}
				else if (noise > heightMin[layer][pass]){
					origHeight[layer] += noise;
					if (layer > 0) {
						origHeight[layer] += origHeight[layer-1];
					}
				}

				//origHeight[layer] += Mathf.Min(noise, heightMin[layer][pass]);
				minusScale += scale[layer][pass];
			}

			finalHeight = Mathf.Max(finalHeight , origHeight[layer]);
		}
	
		return finalHeight - minusScale/10;

	}

private float brownianNoise(float x, float z){

		float frequency = 1f;
		float amplitude = .55f;
		float gain = .55f;
		float total = 0f;
		float lacunarity = 2f;

		for (int i = 0; i < brownianOctaves; ++i)
		{
			total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;         
			frequency *= lacunarity;
			amplitude *= gain;
		}

		return total;

	}
}
*/