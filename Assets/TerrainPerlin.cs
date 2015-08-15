using UnityEngine;
using System.Collections.Generic;

//TO DO

//Comment this shit
//add height per layer diagnostic
//fix color bleed with min height cliffs
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
	//These are all parameters for the terrain layers and passes
	//A layer a fully independent noise heightmap, a pass is a 
	//heightmap which modifies a particular layer 
	private List<List<float>> scale = new List<List<float>> ();
	private List<List<float>> height = new List<List<float>> ();
	private List<List<float>> heightMax = new List<List<float>> ();
	private List<List<float>> heightMin = new List<List<float>> ();
	private List<float> offset = new List<float> ();
	//brownianOctaves is the number of fractal iterations each noise layer is put through
	//height and complexity are heavily independent on this variable
	private int brownianOctaves = 4;
	//colorOne is a list of the colors that coincide with each terrain layer, 
	//it's the first color in a gradient that spans the height of that layer
	private List<Color> colorOne = new List<Color> ();
	//Globalcolor is the finalized list of colors for every vertex that's passed into getNoise(), 
	//it's accessed by CreateNewMeshFidelity to get vertex color data for the terrain mesh
	private List<Color> globalColor = new List<Color> ();
	//Min / Max diagnostic variables for seeing how inputs of scale correspond to actual height-
	//output after going through the brownianNoise() method. Will likely be removed
	public List<float> noiseMax = new List<float> ();
	public List<float> noiseMin = new List<float> ();

	//a means for the mesh generator to get color data for all vertices after generation is complete
	public List<Color> getGlobalColor ()
	{
		return globalColor;
	}

	//Constructor for TerrainPerlin
	//This creates a base layer with heightmap and color parameters
	//Scale - the horizontal scale of the noise pattern
	//Height - the vertical scale of the noise pattern 
	//HeightMax - Maximum height allowed for noise pattern, creates flat tops to structures
	//HeightMin - If the noise pattern isn't this high, it won't appear. Creates cliff formations
	//ColorOne - Lower color of the color gradient exhibited by corresponding layer
	//ColorTwo - Upper color of the color gradient exhibited by corresponding layer
	// *Note: The offset parameter is not passed in with the initialization of the terrain, it defaults to "0"
	public TerrainPerlin (float scale, float height, float heightMax, float heightMin, Color colorOne, Color colorTwo)
	{
		//Adds a new list of parameters for the base layer, any added lists will represent new layers,
		//each float value within the inner list is a pass parameter
		this.scale.Add (new List<float> ());
		this.scale [0].Add (scale);
	
		this.height.Add (new List<float> ());
		this.height [0].Add (height);

		this.heightMax.Add (new List<float> ());
		this.heightMax [0].Add (heightMax);

		this.heightMin.Add (new List<float> ());
		this.heightMin [0].Add (heightMin);

		//The following don't have inner lists because passes don't have these parameters

		//The lower gradient color for the layer
		this.colorOne.Add (colorOne);

		//Offset is how far down the layer is pushed under the previous layer. 
		//Here it defaults to zero because there is no preious layer
		this.offset.Add (0);
	}

	//This adds "pass" parameters that can be used to modify a base layer, 
	//this is done inside the inner loop of getNoise()
	public void addPass (int layer, float scale, float height, float heightMax, float heightMin)
	{
		this.scale [layer].Add (scale);
		this.height [layer].Add (height);
		this.heightMax [layer].Add (heightMax);
		this.heightMin [layer].Add (heightMin);
	}
	//This adds a whole new independent base layer to the terrain with its own heitmap and color parameters
	public void addLayer (float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo)
	{
		//add a new list of floats to iterate through with getNoise()
		this.scale.Add (new List<float> ());
		this.height.Add (new List<float> ());
		this.heightMax.Add (new List<float> ());
		this.heightMin.Add (new List<float> ());

		//adds the passed in values as the first items in the newly added inner loops from above
		this.scale [this.scale.Count - 1].Add (scale);
		this.height [this.height.Count - 1].Add (height);
		this.heightMax [this.heightMax.Count - 1].Add (heightMax);
		this.heightMin [this.heightMin.Count - 1].Add (heightMin);

		//adds the color value passed in for vertex coloring of this layer
		this.colorOne.Add (colorOne);

		//This will be used for diagnostics possibly, I may find another way
		this.noiseMax.Add (-99999);
		this.noiseMin.Add (99999);

		//offset is how far down the layer is pushed under the previous layer
		//there is no inner list because passes currently do not have this funtionality
		this.offset.Add (offset);
	
	}

	//This is where heightmap data is generated for a single vertex
	//The outer loop iterated through each layer, the inner loop iterates
	//through every pass within each layer.
	//x - the x coordinate of the vertex which we need height data for
	//x - the y coordinate of the vertex which we need height data for
	//colorizeVertex - only send color data if this is true, setting this 
	//to false allows for getting height data for a single vertex more than
	//once without sending trash color data into the globalColor list
	public float getNoise (float x, float z, bool colorizeVertex)
	{
		//create an array with a size equalling the number of layers
		//this will store the base heights for the vertex for each layer
		float[] origHeight = new float[scale.Count + 1];

		//We start out with the base layer's color as our final color
		//if the next layer's noise data is higher than the base layer,
		//the color will change to the next layer up.
		//it will continue to bubble up as long as the next layer is higher
		Color finalColor = colorOne [0];

		//initialize a final height which will ultimately be passed to the vertex
		//if a layer's noise data is higher than finalHeight, finalHeight will
		//get that value, otherwise it stays the same, that way the tallest layer
		//at that point of the map gets sent to the vertex
		float finalHeight = 0;

		//will compensate for overheightening with every layer/pass
		float minusScale = 0;

		//The outer loop: iterates through each layer
		for (int layer = 0; layer < scale.Count; layer++) {
			//sets a base height for this layer with the brownianNoise method using the height 
			//and scale parameters of the layer and its zeroeth pass
			origHeight [layer] = (brownianNoise (x / scale [layer] [0], z / scale [layer] [0]) * height [layer] [0]) - offset [layer];

			//doesn't allow layer to be taller than heightMax parameter
			if (origHeight [layer] > heightMax [layer] [0]) {
				origHeight [layer] = heightMax [layer] [0];
			}

			//The inner loop: iterates through each pass within each layer of the outer loop
			//Note: this starts at 1, that's because pass 0 is essentially the outer loop's 
			//initialization of the layer
			for (int pass = 1; pass < scale[layer].Count; pass++) {

				//gets noise for the pass using the passes unique parameters to be added
				//to the layer's base height. This will be repeated for each of the layer's passes
				float noise = (brownianNoise (x / scale [layer] [pass], z / scale [layer] [pass]) * height [layer] [pass]);

				//the noise is added to the height
				origHeight [layer] += noise;

				//minusScale is incremented for each time this is done and will be subtracted from
				//the total height before it's returned
				minusScale += scale [layer] [pass];
			}

			//Now that all the passes have been iterated over, we check to see if the height generated is
			//above the minimum height for the layer and that it's higher than anything generated so far
			//for previous layers. If the latter is false, the current layer at this point on the map 
			//is underneath a previous layer and we default to the previous finalHeight value. If the 
			//height didn't meet the minimum requirement for the layer, finalHeight also won't be affected
			//and this layer's data won't represent the height/color for the vertex.
			if (origHeight [layer] > heightMin [layer] [0] && origHeight[layer] > finalHeight) {

				//The previously mentioned requirements were met, so the height and color both get pushed up.
				//They could bubble up higher if the succeding layers fit the same requirements on the next 
				//iteration of the outer loop(layers)
					finalColor = colorOne [layer];
					finalHeight = origHeight[layer];
			}

		}

		//If colorizeVertex was set to true, add the final color data to the globalColor list
		if (colorizeVertex) {
			globalColor.Add (finalColor);
		}

		//finally, return the ultimate finalHeight with minusScale subtracted
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
