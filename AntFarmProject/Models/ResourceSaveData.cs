namespace AntFarmProject.Models
{
	/// <summary>
	/// Дані збереження одного ресурсу у світі.
	/// </summary>
	public class ResourceSaveData
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public int Amount { get; set; }
	}
}