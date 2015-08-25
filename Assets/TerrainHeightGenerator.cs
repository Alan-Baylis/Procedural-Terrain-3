using UnityEngine;
using System.Collections.Generic;

//TO DO

//change layer borders to prefer height over order of layers
//move color management to its own class
//make circles of terrain at once with a radius input
//fidelity changed over distance (LOD)
//fix color bleed with triangles. Find a way to identify verteces that have crossed layer boundaries
//add max/min functionality to passes
//add negative passes and layers
//allow some passes to affect all layers
//allow some layers to affect only specific layers

//implement second color
//color gradients will have to be assigned with two different methods:
//first method will assign gradient based upon global height value,
//second method will assign gradient based upon exposed height of that layer so that the 
//bottom color will always follow the layer where it is exposed above other layers


//allow offset with passes for sparse effect
//allow colors with passes (make additional constructor with colors as arguments


//probably going to have to change to producing whole mesh here
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
	private const int brownianOctaves = 4;
	//colorOne is a list of the colors that coincide with each terrain layer, 
	//it's the first color in a gradient that spans the height of that layer
	private List<Color> colorOne = new List<Color> ();
	//colorTwo is the second color in a gradient that spans the height of that layer
	private List<Color> colorTwo = new List<Color> ();
	//colorsForTriangle stores 3 colors to be averaged and then applied evenly to every triangle
	private List<Color> colorsForTriangle = new List<Color> ();
	//layerForTriangle stores 3 layers associated with above colors
	private List<int> layersForTriangle = new List<int> ();
	//Globalcolor is the finalized list of colors for every vertex that's passed into getNoise(), 
	//it's accessed by CreateNewMeshFidelity to get vertex color data for the terrain mesh
	private List<Color> globalColor = new List<Color> ();

	//master multiplier for use in noise generation. This helps final noise data match the scale of input data
	private const float masterNoiseMultiplier = 5.25f;//3.875968992248062f;

	//master multiplier which offsets layers to be perfectly zeroed at their minimum height
	private const float masterOffsetMultiplier = 0.3f;//0.0816f;

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
		//The upper gradient color for the layer
		this.colorTwo.Add (colorTwo);
		
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
		
		//adds the color values passed in for vertex coloring of this layer
		this.colorOne.Add (colorOne);
		this.colorTwo.Add (colorTwo);
		
		//This will be used for diagnostics possibly, I may find another way
		heightDiagnostic.addLayer ();
		
		//offset is how far down the layer is pushed under the previous layer
		//there is no inner list because passes currently do not have this funtionality
		this.offset.Add (offset);
		
	}

	//Alternate addLayer method that automatically generates random seed for that layer
	public void addLayer (float scale, float height, float heightMax, float heightMin, float offset, Color colorOne, Color colorTwo)
	{
		addLayer (scale, height, heightMax, heightMin, offset, colorOne, colorTwo, TerrainRandomizer.getMasterSeed ());
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
		addPass (layer, scale, height, heightMax, heightMin, TerrainRandomizer.getMasterSeed ());

	}
	
	//This is where heightmap data is generated for a single vertex
	//The outer loop iterates through each layer, the inner loop iterates
	//through every pass within each layer.
	//x - the x coordinate of the vertex which we need height data for
	//y - the y coordinate of the vertex which we need height data for
	//colorizeVertex - only send color data if this is true, setting this 
	//to false allows for getting height data for a single vertex more than
	//once without sending trash color data into the globalColor list
	public float getNoise (float x, float z, bool colorizeVertex)
	{
		//create an array with a size equalling the number of layers
		//this will store the base heights for the vertex for each layer
		float[] origHeight = new float[scale.Count + 1];

		//a way of keeping up with which layer is ultimately displayed in the mesh
		int displayedLayer = 0;

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
			origHeight [layer] = (brownianNoise ((x + TerrainRandomizer.getTwoRand (seed [layer] [0]) [0]) / scale [layer] [0], (z + TerrainRandomizer.getTwoRand (seed [layer] [0]) [1]) / scale [layer] [0]) * height [layer] [0]) - offset [layer];

			//aligns he lowest point of the layer with zero by multiplying by masterOffsetMultiplier
			origHeight [layer] -= masterOffsetMultiplier * height [layer] [0];

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
				float noise = (brownianNoise ((x + TerrainRandomizer.getTwoRand (seed [layer] [pass]) [0]) / scale [layer] [pass], (z + TerrainRandomizer.getTwoRand (seed [layer] [pass]) [1]) / scale [layer] [pass]) * height [layer] [pass]);

				//the noise is added to the height
				origHeight [layer] += noise - masterOffsetMultiplier * height [layer] [pass];

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
			if (origHeight [layer] > heightMin [layer] [0] && origHeight [layer] > finalHeight) { //supporting only pass 0 now

				//The previously mentioned requirements were met, so the height and color both get pushed up.
				//They could bubble up higher if the succeding layers fit the same requirements on the next 
				//iteration of the outer loop(layers)
				//finalColor = (colorOne [layer] + colorTwo[layer]) / 
				//	(height[layer][0] /origHeight[layer]); //CURRENTLY NOT ACCOUNTING FOR HEIGHT ADDED BY PASSES OVER THE BASE LEVEL
				finalColor = Color.Lerp (colorOne [layer], colorTwo [layer], (float)((height [layer] [0] - offset [layer]) / (origHeight [layer] * 10)));
				//Debug.Log("color ratio" + (height[layer][0] /(origHeight[layer] *10)));

				finalHeight = origHeight [layer];
				displayedLayer = layer;
			}

			//Sends diagnostic info to heightDiagnostic
			heightDiagnostic.recordHeight (layer, origHeight [layer]);
			
		}

		//sends which layer was displayed for this point on the map to the heightDiagnostic class
		heightDiagnostic.recordDisplayedLayer (displayedLayer);

		//If colorizeVertex was set to true, add the final color data to the globalColor list
		if (colorizeVertex) {
			//We add the resulting vertex color to a list that will ultimately store
			//three colors to be averaged and then applied evenly to all three vertices
			//additionally, we add the displayed layer to a second list so we know which
			//layer each color belongs to. This will only take place at borders between layers
			colorsForTriangle.Add (finalColor);
			layersForTriangle.Add (displayedLayer);

			//If all three colors were added to the above list, we can go ahead and average them if necessary
			if (colorsForTriangle.Count == 3) {			
				//if all the colors are equal, this is not a border between layers, 
				//we'll just send in the raw colors
				if (colorsForTriangle [0] == colorsForTriangle [1] 
					&& colorsForTriangle [1] == colorsForTriangle [2]) {
					globalColor.Add (colorsForTriangle [0]);
					globalColor.Add (colorsForTriangle [1]);
					globalColor.Add (colorsForTriangle [2]);
				}

				//this triangle's verteces belong to more than one layer, therefore we know we're at a border
				//now we begin replacing the colors of the lower layers with averages of the highest layer
				else {

					//list of colors that are within the topmost layer displayed in this triangle
					List<Color> colorsInTopLayer = new List<Color> ();

					//temp variable holding highest layer displayed of three vertices, assigned with loop
					int highest = 0;
					foreach (int element in layersForTriangle) {
						if (element > highest) {
							highest = element;
						}
					}

					//add all colors within the highest layer to colorsInTopLayer
					for (int i = 0; i < 3; i++) {
						if (layersForTriangle [i] == highest) {
							colorsInTopLayer.Add (colorsForTriangle [i]);
						}
					}

					//this will be the average of all colors contained in the uppermost layer of the triangle
					Color colorAverage = colorsInTopLayer [0];

					//add up colors in the highest layer
					for (int i = 1; i < colorsInTopLayer.Count; i++) {
						colorAverage += colorsInTopLayer [i];
					}

					//now divide added colors by the number of colors added together
					colorAverage /= colorsInTopLayer.Count;

					//find out which colors arent in the uppermost layer and replace
					//them with the average we just made, then submit color
					for (int i = 0; i < 3; i++) {
						if (layersForTriangle [i] != highest) 
						{
							colorsForTriangle [i] = colorAverage;
						}

						globalColor.Add (colorsForTriangle [i]);

					}
				}

				//now we clear the temporary list of colors for the next batch/triangles
				colorsForTriangle.Clear ();
				layersForTriangle.Clear ();
			}

			//Debug.Log ("count: " +colorsForTriangle.Count);
			 

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
		float amplitude = .15f;
		//lacunarity is the ratio of change of the frequency per iteration/octave 
		float lacunarity = 2f;
		//gain is what is the ratio of change of amplitude pre iteration/octave
		float gain = .6f;
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

		//multiplies the noise output to match the scale of the input
		total *= masterNoiseMultiplier;

		//all iterations complete, return the running total
		return total;

	}
}
