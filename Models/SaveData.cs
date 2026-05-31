using System;
using System.Collections.Generic;

namespace AntFarmProject.Models
	{ 
     /// <summary>
	/// Повний стан збереження гри.
	/// </summary
	public class SaveData
	{
		public string Version { get; set; } = "2.0";
		public DateTime SaveDate { get; set; }
		public string SaveName { get; set; }
		public int Food { get; set; }
		public int Wood { get; set; }
		public int Stone { get; set; }
		public int Water { get; set; }
		public int ColonyLevel { get; set; }
		public int MaxAnts { get; set; }
		public double NestX { get; set; }
		public double NestY { get; set; }
		public int NestSize { get; set; }
		public int CurrentDay { get; set; }
		public int CurrentHour { get; set; }
		public int CurrentMinute { get; set; }
		public WeatherType Weather { get; set; }
		public bool IsPaused { get; set; }
		public int GameSpeed { get; set; }
		public List<AntSaveData> Ants { get; set; }
		public List<ResourceSaveData> Resources { get; set; }
		public GameStatistics Statistics { get; set; }
	}
}