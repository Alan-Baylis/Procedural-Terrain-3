using UnityEngine;
using System.Collections.Generic;

//TO DO

//fidelity changed over distance (LOD)
//fix color bleed with min height cliffs
//add max/min functionality to passes
//add negative passes and layers
//allow some passes to affect all layers
//allow some layers to affect only specific layers
//implement second color
//allow offset with passes for sparse effect
//allow colors with passes (make additional constructor with colors as arguments


//probably going to have to produce whole mesh here
//send in starting coordinates and chunk size, send back entire colored mesh

public class TerrainHeightGenerator : MonoBehaviour
{
	//These are all parameters for the terrain layers and passes
	//A layer a fully independent noise heightmap, a pass is a 
	//heightmap which modifies a particular layer 
	private List<List<float>> scale = new List<List<float>> ();
	private List<List<float>> height = new List<List<float>> ();
	private List<List<float>> heightMax = new List<List<float>> ();
	private List<List<float>> heightMin = new List<List<float>> ();
	private List<List<int>> seed = new List<List<int>> ();
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

	//a means for the mesh generator to get color data for all vertices after generation is complete
	public List<Color> getGlobalColor ()
	{
		return globalColor;
	}

	//Method which gives all parameters to this TerrainHeightGenerator
	//This creates a base layer with heightmap and color parameters
	//Scale - the horizontal scale of the noise pattern
	//Height - the vertical scale of the noise pattern 
	//HeightMax - Maximum height allowed for noise pattern, creates flat tops to structures
	//HeightMin - If the noise pattern isn't this high, it won't appear. Creates cliff formations
	//ColorOne - Lower color of the color gradient exhibited by corresponding layer
	//ColorTwo - Upper color of the color gradient exhibited by corresponding layer
	// *Note: The offset parameter is not passed in with the initialization of the terrain, it defaults to "0"
	public void CreateTerrainHeightGenerator (float scale, float height, float heightMax, float heightMin, Color colorOne, Color colorTwo, int seed)
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

		this.seed.Add (new List<int> ());
		this.seed [0].Add (seed);

		//The following don't have inner lists because passes don't have these parameters

		//The lower gradient color for the layer
		this.colorOne.Add (colorOne);

		//Offset is how far down the layer is pushed under the previous layer. 
		//Here it defaults to zero because there is no preious layer
		this.offset.Add (0);

		//let heightDiagnostic class know there's a new layer
		heightDiagnostic.addLayer ();

	}

	//Alternate creation method excluding seed input parameter, uses master seed instead
	public void CreateTerrainHeightGenerator (float scale, float height, float heightMax, float heightMin, Color colorOne, Color colorTwo)
	{
		CreateTerrainHeightGenerator (scale, height, heightMax, heightMin, colorOne, colorTwo, TerrainRandomizer.getMasterSeed ());
	}


	//This adds a whole new independent base layer to the terrain with its own heitmap and color parameters
	public void addLayer (float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo, int seed)
	{
		//add a new list of floats to iterate through with getNoise()
		this.scale.Add (new List<float> ());
		this.height.Add (new List<float> ());
		this.heightMax.Add (new List<float> ());
		this.heightMin.Add (new List<float> ());
		this.seed.Add (new List<int> ());
		
		//adds the passed in values as the first items in the newly added inner loops from above
		this.scale [this.scale.Count - 1].Add (scale);
		this.height [this.height.Count - 1].Add (height);
		this.heightMax [this.heightMax.Count - 1].Add (heightMax);
		this.heightMin [this.heightMin.Count - 1].Add (heightMin);
		this.seed [this.seed.Count - 1].Add (seed);
		
		//adds the color value passed in for vertex coloring of this layer
		this.colorOne.Add (colorOne);
		
		//This will be used for diagnostics possibly, I may find another way
		heightDiagnostic.addLayer ();
		
		//offset is how far down the layer is pushed under the previous layer
		//there is no inner list because passes currently do not have this funtionality
		this.offset.Add (offset);
		
	}

	//Alternate addLayer method that automatically generates random seed for that layer
	public void addLayer (float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo)
	{
		addLayer (scale, height, heightMax, heightMin, offset, colorOne, colorTwo, TerrainRandomizer.getMasterSeed());
	}


	//This adds "pass" parameters that can be used to modify a base layer, 
	//this is done inside the inner loop of getNoise()
	public void addPass (int layer, float scale, float height, float heightMax, float heightMin, int seed)
	{
		this.scale [layer].Add (scale);
		this.height [layer].Add (height);
		this.heightMax [layer].Add (heightMax);
		this.heightMin [layer].Add (heightMin);
		this.seed [layer].Add (seed);
	}

	//Alternate addLayer method that automatically adds random seed for that layer
	public void addPass (int layer, float scale, float height, float heightMax, float heightMin)
	{
		addPass (layer, scale, height, heightMax, heightMin, TerrainRandomizer.getMasterSeed());

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
			origHeight [layer] = (brownianNoise ((x + TerrainRandomizer.getTwoRand(seed[layer][0])[0]) / scale [layer] [0], (z + TerrainRandomizer.getTwoRand(seed[layer][0])[1]) / scale [layer] [0]) * height [layer] [0]) - offset [layer];

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
				float noise = (brownianNoise ((x + TerrainRandomizer.getTwoRand(seed[layer][pass])[0] )/ scale [layer] [pass], (z + TerrainRandomizer.getTwoRand(seed[layer][pass])[1]) / scale [layer] [pass]) * height [layer] [pass]);

				//the noise is added to the height
				origHeight [layer] += noise;

				//minusScale is incremented for each time this is done and will be subtracted from
				//the total height before it's returned
				minusScale += scale [layer] [pass];
			}

			//Sends diagnostic info to heightDiagnostic
			heightDiagnostic.recordHeight(layer, origHeight[layer]);

			//Now that all the passes have been iterated over, we check to see if the height generated is
			//above the minimum height for the layer and that it's higher than anything generated so far
			//for previous layers. If the latter is false, the current layer at this point on the map 
			//is underneath a previous layer and we default to the previous finalHeight value. If the 
			//height didn't meet the minimum requirement for the layer, finalHeight also won't be affected
			//and this layer's data won't represent the height/color for the vertex.
			if (origHeight [layer] > heightMin [layer] [0] && origHeight[layer] > finalHeight) { //supporting only pass 0 now

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
	
	//This method fractalizes a simple perlin noise pattern when given a simple 2d coordinate
	private float brownianNoise (float x, float z)
	{
		//frequency is the horizontal distance or wavelength of the noise pattern per iteration/octave
		float frequency = 1f;
		//amplitude is the height multiplier per iteration/octave 
		float amplitude = .538921f;
		//lacunarity is the ratio of change of the frequency per iteration/octave 
		float lacunarity = 2f;
		//gain is what is the ratio of change of amplitude pre iteration/octave
		float gain = .55f;
		//the running total for height added up over every octave
		float total = 0f;

		//loop through "octaves", brownianOctaves is a global variable
		for (int i = 0; i < brownianOctaves; ++i) {

			//get the natural noise at a coordinate, multiple that locaton by frequency, then
			//multiply the resulting height by amplitude
			total += Mathf.PerlinNoise (x * frequency, z * frequency) * amplitude; 

			//preparing now for next iteration, multiply frequency by lacunarity factor
			//and amplitude by gain factor
			frequency *= lacunarity;
			amplitude *= gain;
		}

		//all iterations complete, return the running total
		return total;

	}
}
