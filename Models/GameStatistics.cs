using System;

namespace AntFarmProject.Models
{
	/// <summary>
	/// Клас для зберігання загальної статистики гри.
	/// </summary>
	public class GameStatistics
	{
		public int TotalFoodCollected { get; set; }
		public int TotalWoodCollected { get; set; }
		public int TotalStoneCollected { get; set; }
		public int TotalWaterCollected { get; set; }
		public int AntsBorn { get; set; }
		public int AntsDied { get; set; }
		public int NestExpansions { get; set; }
		public TimeSpan PlayTime { get; set; }
		public int DaysSurvived { get; set; } = 1;
	}
}