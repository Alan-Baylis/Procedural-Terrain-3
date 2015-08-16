using UnityEngine;
using System.Collections.Generic;

//This class takes care of diagnostic information dealing with heightmap generation
public class heightDiagnostic
{
	//maxForLayer holds the highest current "altitude" for each layer 
	private static List<float> maxForLayer = new List<float>();
	//minForLayer holds the lowest current "altitude" for each layer 
	private static List<float> minForLayer = new List<float>();
	//heightRecord hold a record of every single height value at every point in each layer
	private static List<List<float>> heightRecord = new List<List<float>>();

	//method for recording the height value of a layer when calculated, 
	//if this isn't done, no diagnostics can be made
	public static void recordHeight(int layer, float value)
	{
		heightRecord[layer].Add(value);
		
		if (value > maxForLayer[layer])
		{
			maxForLayer[layer] = value;
		}
		
		if (value < minForLayer[layer])
		{
			minForLayer[layer] = value;
		}
		
	}

	//this allows records to be made for a new layer, automatically sets min and max to extremes
	public static void addLayer()
	{
		maxForLayer.Add (-999999999);
		minForLayer.Add (999999999);
		heightRecord.Add (new List<float>());
	}

	//method for retrieving max for a particular layer
	public static float getMaxForLayer(int layer)
	{
		return maxForLayer [layer];
	}

	//method for retrieving min for a particular layer
	public static float getMinForLayer(int layer)
	{
		return minForLayer [layer];
	}

	//method for retriving the total heightspan from lowest-to-highest of a particular layer
	public static float getTotalHeightForLayer(int layer)
	{
		return maxForLayer [layer] - minForLayer [layer];
	}
	
	//method for getting the overall average height of all points of a layer
	public static float getAverageHeightForLayer(int layer)
	{
		float total = 0;
		foreach (float element in heightRecord[layer])
		{
			total += element;
		}
		
		return total / heightRecord[layer].Count;
	}
	
}


