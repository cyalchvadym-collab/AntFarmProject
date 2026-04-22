namespace AntFarmProject.Models
{
	/// <summary>
	/// Збережений стан окремої мурахи.
	/// </summary>
	public class AntSaveData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Speed { get; set; }
		public string State { get; set; }
		public double Energy { get; set; }
		public double Health { get; set; }
		public double Age { get; set; }
		public int GatheredFood { get; set; }
		public int GatheredWood { get; set; }
		public int GatheredStone { get; set; }
		public int GatheredWater { get; set; }
	}
}