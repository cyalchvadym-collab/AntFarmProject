using System;

namespace AntFarmProject.Models
{
	/// <summary>
	/// Перелік можливих станів, у яких може перебувати мураха.
	/// </summary>
	public enum AntState { Idle, Moving, Gathering, Returning, Resting, Dead }

	/// <summary>
	/// Типи ресурсів, доступних для збирання на ігровому полі.
	/// </summary>
	public enum ResourceType { Food, Wood, Stone, Water }

	/// <summary>
	/// Типи погодних умов, які впливають на ігровий процес.
	/// </summary>
	public enum WeatherType { Sunny, Rainy, Stormy, Night }
}