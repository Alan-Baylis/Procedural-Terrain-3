
using UnityEngine;
using System.Collections.Generic;

//This class allows for deterministic random genteration based on seed values
	public class TerrainRandomizer : MonoBehaviour
	{

	//Pass in a seed value, get a predictable random number between 0 and 1 million
		public static float getRand (int seed)
		{
		Random.seed = seed;

		return Random.Range (0, 1000000);
		}

	//Pass in a seed value, get TWO predictable random numbers between 0 and 1 million,
	//Calls the previous method by passing in the seed parameter for the first number,
	//Then uses that result as a seed for the second number, all deterministic and predictable
		public static float[] getTwoRand (int seed)
		{
		float[] bothRandoms = new float[2];
		bothRandoms [0] = getRand (seed);
		bothRandoms [1] = getRand ((int)bothRandoms [0]);

		return bothRandoms;

		}

	}



