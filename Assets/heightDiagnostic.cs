
using UnityEngine;
using System.Collections.Generic;

public class heightDiagnostic
{
	private static List<float> maxForLayer = new List<float>();
	private static List<float> minForLayer = new List<float>();
	private static List<List<float>> heightRecord = new List<List<float>>();
	//private static List<float> recordCount = new List<float>();

	public static float getMaxForLayer(int layer)
	{
		return maxForLayer [layer];
	}

	public static void setMaxForLayer(int layer, float value)
	{
		maxForLayer [layer] = value;
	}

	public static float getMinForLayer(int layer)
	{
		return minForLayer [layer];
	}
	
	public static void setMinForLayer(int layer, float value)
	{
		minForLayer [layer] = value;
	}

	public static void addLayer()
	{
		maxForLayer.Add (-99);
		minForLayer.Add (999999999);
		heightRecord.Add (new List<float>());
	}

	public static float getTotalHeightForLayer(int layer)
	{
		return maxForLayer [layer] - minForLayer [layer];
	}

	public static float getAverageHeightForLayer(int layer)
	{
		float total = 0;
		foreach (float element in heightRecord[layer])
		{
			total += element;
		}

		return total / heightRecord[layer].Count;
	}

	public static void recordHeight(int layer, float value)
	{
		heightRecord[layer].Add(value);;
	}
}


