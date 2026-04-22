using System.Windows;

namespace AntFarmProject.Models
{
	/// <summary>
	/// Клас, що описує джерело ресурсу на ігровому полі.
	/// </summary>
	public class ResourceNode
	{
		public int Id { get; set; }
		public ResourceType Type { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public int Amount { get; set; }
		public int MaxAmount { get; set; }
		public UIElement Visual { get; set; }
		public bool IsDepleted => Amount <= 0;
	}
}