using UnityEngine;
using System.Collections.Generic;

//TO DO

//Comment this shit
//add max/min functionality to passes
//add a random seed to noise
//add negative passes and layers
//allow some passes to affect all layers
//allow some layers to affect only specific layers
//implement second color
// allow offset with passes for sparse effect
// allow colors with passes (make additional constructor with colors as arguments


//probably going to have to produce whole mesh here
//send in starting coordinates and chunk size, send back entire colored mesh

public class TerrainPerlin : MonoBehaviour
{
	private List<List<float>> scale = new List<List<float>> ();
	private List<List<float>> height = new List<List<float>> ();
	private List<List<float>> heightMax = new List<List<float>> ();
	private List<List<float>> heightMin = new List<List<float>> ();
	private List<float> offset = new List<float> ();
	private int brownianOctaves = 4;
	private List<Color> colorOne = new List<Color> ();
	private List<Color> globalColor = new List<Color> ();
	public List<float> noiseMax = new List<float> ();
	public List<float> noiseMin = new List<float> ();

	public List<Color> getGlobalColor ()
	{
		return globalColor;
	}

	public TerrainPerlin (float scale, float height, float heightMax, float heightMin, Color colorOne, Color colorTwo)
	{

		this.scale.Add (new List<float> ());
		this.scale [0].Add (scale);
	
		this.height.Add (new List<float> ());
		this.height [0].Add (height);

		this.heightMax.Add (new List<float> ());
		this.heightMax [0].Add (heightMax);

		this.heightMin.Add (new List<float> ());
		this.heightMin [0].Add (heightMin);

		this.colorOne.Add (colorOne);

		this.offset.Add (0);
	}

	public void addPass (int layer, float scale, float height, float heightMax, float heightMin)
	{
		this.scale [layer].Add (scale);
		this.height [layer].Add (height);
		this.heightMax [layer].Add (heightMax);
		this.heightMin [layer].Add (heightMin);
	}

	public void addLayer (float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo)
	{
		this.scale.Add (new List<float> ());
		this.height.Add (new List<float> ());
		this.heightMax.Add (new List<float> ());
		this.heightMin.Add (new List<float> ());

		this.scale [this.scale.Count - 1].Add (scale);
		this.height [this.height.Count - 1].Add (height);
		this.heightMax [this.heightMax.Count - 1].Add (heightMax);
		this.heightMin [this.heightMin.Count - 1].Add (heightMin);

		this.colorOne.Add (colorOne);
		this.noiseMax.Add (-99999);
		this.noiseMin.Add (99999);

		this.offset.Add (offset);
	
	}

	public float getNoise (float x, float z, bool colorizeVertex)
	{
		float[] origHeight = new float[scale.Count + 1];
		Color finalColor = colorOne [0];

		float finalHeight = 0;
		float minusScale = 0;

		for (int layer = 0; layer < scale.Count; layer++) {
			origHeight [layer] = (brownianNoise (x / scale [layer] [0], z / scale [layer] [0]) * height [layer] [0]) - offset [layer];
			if (origHeight [layer] > heightMax [layer] [0]) {
				origHeight [layer] = heightMax [layer] [0];
			}

			for (int pass = 1; pass < scale[layer].Count; pass++) {
				float noise = (brownianNoise (x / scale [layer] [pass], z / scale [layer] [pass]) * height [layer] [pass]);
				origHeight [layer] += noise;
				minusScale += scale [layer] [pass];
			}

			if (origHeight [layer] > heightMin [layer] [0] && origHeight[layer] > finalHeight) {
					finalColor = colorOne [layer];
					finalHeight = origHeight[layer];
			}

		}

		if (colorizeVertex) {
			globalColor.Add (finalColor);
		}

		return finalHeight - minusScale / 10;

	}

	private float brownianNoise (float x, float z)
	{

		float frequency = 1f;
		float amplitude = .55f;
		float gain = .55f;
		float total = 0f;
		float lacunarity = 2f;

		for (int i = 0; i < brownianOctaves; ++i) {
			total += Mathf.PerlinNoise (x * frequency, z * frequency) * amplitude;         
			frequency *= lacunarity;
			amplitude *= gain;
		}

		return total;

	}
}
