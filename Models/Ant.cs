using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntFarmProject.Models
{
	/// <summary>
	/// Клас, що представляє сутність мурахи в грі.
	/// </summary>
	public class Ant
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double TargetX { get; set; }
		public double TargetY { get; set; }
		public double Speed { get; set; }
		public AntState State { get; set; }
		public ResourceNode TargetResource { get; set; }
		public int CarryingAmount { get; set; }
		public ResourceType? CarryingType { get; set; }
		public double Energy { get; set; } = 100;
		public double Health { get; set; } = 100;
		public double Age { get; set; } = 0;
		public int GatheredFood { get; set; }
		public int GatheredWood { get; set; }
		public int GatheredStone { get; set; }
		public int GatheredWater { get; set; }
		public Canvas Visual { get; set; }
		public RotateTransform RotateTransform { get; set; }
		public DateTime BornTime { get; set; }

		public Ant(int id, double x, double y)
		{
			Id = id;
			Name = $"Мураха #{id}";
			X = x;
			Y = y;
			TargetX = x;
			TargetY = y;
			Speed = 1.5 + new Random().NextDouble() * 2;
			State = AntState.Idle;
			BornTime = DateTime.Now;
		}
	}
}