using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.Windows.Shapes.Path;
namespace AntFarmProject
{
	// ===== ENUMS =====
	public enum AntState { Idle, Moving, Gathering, Returning, Resting, Dead }
	public enum ResourceType { Food, Wood, Stone, Water }
	public enum WeatherType { Sunny, Rainy, Stormy, Night }

	// ===== MODELS =====
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

	// ===== SAVE DATA =====
	public class SaveData
	{
		public string Version { get; set; } = "2.0";
		public DateTime SaveDate { get; set; }
		public string SaveName { get; set; }

		// Resources
		public int Food { get; set; }
		public int Wood { get; set; }
		public int Stone { get; set; }
		public int Water { get; set; }

		// Colony
		public int ColonyLevel { get; set; }
		public int MaxAnts { get; set; }
		public double NestX { get; set; }
		public double NestY { get; set; }
		public int NestSize { get; set; }

		// Game state
		public int CurrentDay { get; set; }
		public int CurrentHour { get; set; }
		public int CurrentMinute { get; set; }
		public WeatherType Weather { get; set; }
		public bool IsPaused { get; set; }
		public int GameSpeed { get; set; }

		// Lists
		public List<AntSaveData> Ants { get; set; }
		public List<ResourceSaveData> Resources { get; set; }

		// Statistics
		public GameStatistics Statistics { get; set; }
	}

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

	public class ResourceSaveData
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public int Amount { get; set; }
	}

	// ===== MAIN WINDOW =====
	public partial class MainWindow : Window
	{
		// Resources
		private int food = 100;
		private int wood = 50;
		private int stone = 25;
		private int water = 30;

		// Colony
		private int colonyLevel = 1;
		private int maxAnts = 10;
		private double nestX = 425;
		private double nestY = 275;
		private int nestSize = 150;

		// Game objects
		private List<Ant> ants = new();
		private List<ResourceNode> resources = new();
		private GameStatistics statistics = new();

		// Time
		private int currentDay = 1;
		private int currentHour = 8;
		private int currentMinute = 0;
		private WeatherType currentWeather = WeatherType.Sunny;

		// Settings
		private bool isPaused = false;
		private int gameSpeed = 1;
		private string currentSaveFile = "savegame.json";

		// Timers
		private DispatcherTimer gameTimer;
		private DispatcherTimer secondTimer;
		private DispatcherTimer autoSaveTimer;
		private Random random = new();
		private DateTime sessionStart;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
			Closing += Window_Closing;
			sessionStart = DateTime.Now;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Check for auto-save
			if (File.Exists("Data/savegame.json"))
			{
				var result = MessageBox.Show("Знайдено автозбереження. Завантажити?",
					"Автозбереження", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
				{
					LoadGame("Data/savegame.json");
					SetupTimers(); 
					return;
				}
			}

			NewGame();
		}

		private void NewGame()
		{
			// Reset all
			food = 100; wood = 50; stone = 25; water = 30;
			colonyLevel = 1; maxAnts = 10;
			currentDay = 1; currentHour = 8; currentMinute = 0;
			statistics = new GameStatistics();

			// Clear canvas
			ants.Clear();
			resources.Clear();
			GameCanvas.Children.Clear();
			GameCanvas.Children.Add(NestGrid);

			// Create initial ants
			for (int i = 0; i < 3; i++)
				SpawnAnt(false);

			// Generate resources
			GenerateResources();

			// Setup timers
			SetupTimers();

			UpdateUI();
			AddLog("🎮 Нова гра розпочата!", Colors.White);
		}

		private void SetupTimers()
		{
			// Main game loop (60 FPS)
			gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
			gameTimer.Tick += GameLoop;
			gameTimer.Start();

			// Second timer (time, weather, auto-save check)
			secondTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
			secondTimer.Tick += SecondTick;
			secondTimer.Start();

			// Auto-save every 2 minutes
			autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
			autoSaveTimer.Tick += (s, e) => AutoSave();
			autoSaveTimer.Start();
		}

		private void SecondTick(object sender, EventArgs e)
		{
			if (isPaused) return;

			// Update time
			currentMinute++;
			if (currentMinute >= 60)
			{
				currentMinute = 0;
				currentHour++;
				if (currentHour >= 24)
				{
					currentHour = 0;
					currentDay++;
					statistics.DaysSurvived = currentDay;
					AddLog($"📅 День {currentDay}!", Colors.Gold);
					ChangeWeather();
				}
			}

			// Resource consumption
			int foodConsumption = (ants.Count / 3) + 1;
			int waterConsumption = (ants.Count / 4) + 1;

			food = Math.Max(0, food - foodConsumption);
			water = Math.Max(0, water - waterConsumption);

			// Check for starving ants
			if (food == 0)
			{
				var starving = ants.Where(a => a.State != AntState.Dead).ToList();
				foreach (var ant in starving.Take(1))
				{
					ant.Health -= 10;
					if (ant.Health <= 0) KillAnt(ant, "голод");
				}
			}

			// Aging
			foreach (var ant in ants.Where(a => a.State != AntState.Dead))
			{
				ant.Age += 0.1;
				if (ant.Age > 100 && random.Next(1000) < 5)
					KillAnt(ant, "старість");
			}

			// Random events
			if (random.Next(100) < 5)
			{
				int bonus = random.Next(10, 30);
				food += bonus;
				AddLog($"🍂 Випадкова знахідка: +{bonus} їжі!", Colors.LightGreen);
			}

			UpdateUI();
		}

		private void ChangeWeather()
		{
			var weathers = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().ToArray();
			currentWeather = weathers[random.Next(weathers.Length)];

			string weatherEmoji = currentWeather switch
			{
				WeatherType.Sunny => "☀️",
				WeatherType.Rainy => "🌧️",
				WeatherType.Stormy => "⛈️",
				WeatherType.Night => "🌙",
				_ => "☀️"
			};

			AddLog($"{weatherEmoji} Погода змінилась на {currentWeather}!", Colors.LightBlue);
		}

		private void GenerateResources()
		{
			int id = 1;

			// Food (more in sunny weather)
			int foodCount = currentWeather == WeatherType.Sunny ? 12 : 8;
			for (int i = 0; i < foodCount; i++)
				CreateResource(id++, ResourceType.Food, "🍂", "#00b894");

			// Wood
			for (int i = 0; i < 6; i++)
				CreateResource(id++, ResourceType.Wood, "🪵", "#e17055");

			// Stone
			for (int i = 0; i < 4; i++)
				CreateResource(id++, ResourceType.Stone, "🪨", "#b2bec3");

			// Water (more in rainy weather)
			int waterCount = currentWeather == WeatherType.Rainy ? 6 : 3;
			for (int i = 0; i < waterCount; i++)
				CreateResource(id++, ResourceType.Water, "💧", "#0984e3");
		}

		private void CreateResource(int id, ResourceType type, string emoji, string color)
		{
			double x, y;
			int attempts = 0;
			do
			{
				x = random.Next(100, 700);
				y = random.Next(100, 500);
				attempts++;
			} while (Distance(x, y, nestX, nestY) < 180 && attempts < 50);

			var node = new ResourceNode
			{
				Id = id,
				Type = type,
				X = x,
				Y = y,
				Amount = random.Next(30, 150),
				MaxAmount = 150
			};

			var border = new Border
			{
				Width = 44,
				Height = 44,
				Background = (SolidColorBrush)new BrushConverter().ConvertFrom(color),
				CornerRadius = new CornerRadius(22),
				BorderBrush = Brushes.White,
				BorderThickness = new Thickness(2),
				Child = new TextBlock
				{
					Text = emoji,
					FontSize = 24,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				},
				Tag = id
			};

			Canvas.SetLeft(border, x - 22);
			Canvas.SetTop(border, y - 22);
			GameCanvas.Children.Add(border);

			node.Visual = border;
			resources.Add(node);
		}

		private double Distance(double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
		}

		private void SpawnAnt(bool countStat = true)
		{
			if (ants.Count(a => a.State != AntState.Dead) >= maxAnts) return;

			int id = ants.Count > 0 ? ants.Max(a => a.Id) + 1 : 1;
			var ant = new Ant(id, nestX + random.Next(-60, 60), nestY + random.Next(-60, 60));

			ant.Visual = CreateAntVisual();
			ant.RotateTransform = new RotateTransform(0);
			ant.Visual.RenderTransform = ant.RotateTransform;
			ant.Visual.RenderTransformOrigin = new Point(0.5, 0.5);
			ant.Visual.MouseLeftButtonDown += (s, e) => ShowAntDetails(ant);

			Canvas.SetLeft(ant.Visual, ant.X - 16);
			Canvas.SetTop(ant.Visual, ant.Y - 16);
			GameCanvas.Children.Add(ant.Visual);

			ants.Add(ant);

			if (countStat)
			{
				statistics.AntsBorn++;
				AddLog($"🐣 {ant.Name} народилася!", Colors.LightYellow);
			}
		}

		private Canvas CreateAntVisual()
		{
			var canvas = new Canvas { Width = 32, Height = 32 };

			// Тінь
			var shadow = new Ellipse
			{
				Width = 22,
				Height = 7,
				Fill = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
			};
			Canvas.SetLeft(shadow, 5);
			Canvas.SetTop(shadow, 24);

			// Ноги - чорні, тонкі
			var legBrush = new SolidColorBrush(Color.FromRgb(20, 15, 12));
			var legs = new[]
			{
				new Line { X1 = 8, Y1 = 20, X2 = 3, Y2 = 26, Stroke = legBrush, StrokeThickness = 1.8 },
				new Line { X1 = 24, Y1 = 20, X2 = 29, Y2 = 26, Stroke = legBrush, StrokeThickness = 1.8 },
				new Line { X1 = 7, Y1 = 16, X2 = 2, Y2 = 16, Stroke = legBrush, StrokeThickness = 1.6 },
				new Line { X1 = 25, Y1 = 16, X2 = 30, Y2 = 16, Stroke = legBrush, StrokeThickness = 1.6 },
				new Line { X1 = 8, Y1 = 12, X2 = 3, Y2 = 6, Stroke = legBrush, StrokeThickness = 1.5 },
				new Line { X1 = 24, Y1 = 12, X2 = 29, Y2 = 6, Stroke = legBrush, StrokeThickness = 1.5 }
			};

			foreach (var leg in legs)
			{
				leg.StrokeStartLineCap = PenLineCap.Round;
				leg.StrokeEndLineCap = PenLineCap.Round;
			}

			// Черевце - велике, чорне з блиском
			var abdomen = new Ellipse
			{
				Width = 15,
				Height = 17,
				Fill = new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Color.FromRgb(55, 48, 42), 0),
						new GradientStop(Color.FromRgb(35, 30, 26), 0.5),
						new GradientStop(Color.FromRgb(20, 17, 14), 1)
					},
					new Point(0, 0),
					new Point(0, 1))
			};
			Canvas.SetLeft(abdomen, 8.5);
			Canvas.SetTop(abdomen, 11);

			// Груди
			var thorax = new Ellipse
			{
				Width = 11,
				Height = 13,
				Fill = new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Color.FromRgb(50, 44, 38), 0),
						new GradientStop(Color.FromRgb(30, 26, 22), 1)
					},
					new Point(0, 0),
					new Point(0, 1))
			};
			Canvas.SetLeft(thorax, 10.5);
			Canvas.SetTop(thorax, 3);

			// Голова
			var head = new Ellipse
			{
				Width = 11,
				Height = 11,
				Fill = new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Color.FromRgb(60, 54, 48), 0),
						new GradientStop(Color.FromRgb(35, 30, 26), 1)
					},
					new Point(0, 0),
					new Point(0, 1))
			};
			Canvas.SetLeft(head, 10.5);
			Canvas.SetTop(head, -6);

			// Вусики
			var antennaBrush = new SolidColorBrush(Color.FromRgb(40, 35, 30));
			var leftAntenna = new Path
			{
				Data = Geometry.Parse("M 12,-4 Q 6,-12 4,-10"),
				Stroke = antennaBrush,
				StrokeThickness = 1.2,
				StrokeStartLineCap = PenLineCap.Round
			};
			var rightAntenna = new Path
			{
				Data = Geometry.Parse("M 20,-4 Q 26,-12 28,-10"),
				Stroke = antennaBrush,
				StrokeThickness = 1.2,
				StrokeStartLineCap = PenLineCap.Round
			};

			// Очі - блискучі
			var eyeBrush = new SolidColorBrush(Color.FromRgb(90, 85, 80));
			var leftEye = new Ellipse { Width = 2.5, Height = 3.5, Fill = eyeBrush };
			Canvas.SetLeft(leftEye, 12);
			Canvas.SetTop(leftEye, -4);
			var rightEye = new Ellipse { Width = 2.5, Height = 3.5, Fill = eyeBrush };
			Canvas.SetLeft(rightEye, 17.5);
			Canvas.SetTop(rightEye, -4);

			// Блиск в очах
			var shine = new SolidColorBrush(Colors.White);
			var leftShine = new Ellipse { Width = 1, Height = 1.5, Fill = shine };
			Canvas.SetLeft(leftShine, 12.5);
			Canvas.SetTop(leftShine, -3.5);
			var rightShine = new Ellipse { Width = 1, Height = 1.5, Fill = shine };
			Canvas.SetLeft(rightShine, 18);
			Canvas.SetTop(rightShine, -3.5);

			// Збірка
			canvas.Children.Add(shadow);
			foreach (var leg in legs) canvas.Children.Add(leg);
			canvas.Children.Add(abdomen);
			canvas.Children.Add(thorax);
			canvas.Children.Add(head);
			canvas.Children.Add(leftAntenna);
			canvas.Children.Add(rightAntenna);
			canvas.Children.Add(leftEye);
			canvas.Children.Add(rightEye);
			canvas.Children.Add(leftShine);
			canvas.Children.Add(rightShine);

			// Tooltip
			canvas.ToolTip = "Клікніть для деталей";

			return canvas;
		}

		private void GameLoop(object sender, EventArgs e)
		{
			if (isPaused) return;

			for (int i = 0; i < gameSpeed; i++)
			{
				foreach (var ant in ants.Where(a => a.State != AntState.Dead))
				{
					ProcessAnt(ant);
					UpdateAntVisual(ant);
				}
			}
		}

		private void ProcessAnt(Ant ant)
		{
			switch (ant.State)
			{
				case AntState.Idle:
					if (random.Next(100) < 3) FindTask(ant);
					break;
				case AntState.Moving:
					MoveAnt(ant);
					break;
				case AntState.Gathering:
					Gather(ant);
					break;
				case AntState.Returning:
					Return(ant);
					break;
				case AntState.Resting:
					Rest(ant);
					break;
			}

			// Energy drain
			if (ant.State != AntState.Idle && ant.State != AntState.Resting)
				ant.Energy -= 0.05;

			if (ant.Energy <= 0 && ant.State != AntState.Resting)
			{
				ant.State = AntState.Returning;
				ant.TargetX = nestX;
				ant.TargetY = nestY;
			}
		}

		private void FindTask(Ant ant)
		{
			var available = resources.Where(r => !r.IsDepleted).ToList();

			// Prioritize by need
			ResourceType? priority = null;
			if (food < 50) priority = ResourceType.Food;
			else if (water < 20) priority = ResourceType.Water;

			if (priority.HasValue && available.Any(r => r.Type == priority.Value) && random.Next(100) < 70)
			{
				var target = available.Where(r => r.Type == priority.Value)
					.OrderBy(r => Distance(ant.X, ant.Y, r.X, r.Y))
					.First();
				SetAntTarget(ant, target);
				return;
			}

			if (available.Any() && random.Next(100) < 80)
			{
				var target = available.OrderBy(r => Distance(ant.X, ant.Y, r.X, r.Y)).First();
				SetAntTarget(ant, target);
			}
			else
			{
				// Explore
				ant.TargetX = random.Next(100, 750);
				ant.TargetY = random.Next(100, 550);
				ant.State = AntState.Moving;
			}
		}

		private void SetAntTarget(Ant ant, ResourceNode target)
		{
			ant.TargetResource = target;
			ant.TargetX = target.X;
			ant.TargetY = target.Y;
			ant.State = AntState.Moving;
		}

		private void MoveAnt(Ant ant)
		{
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double dist = Math.Sqrt(dx * dx + dy * dy);

			// Weather effects
			double speedMultiplier = currentWeather switch
			{
				WeatherType.Rainy => 0.8,
				WeatherType.Stormy => 0.6,
				WeatherType.Night => 0.7,
				_ => 1.0
			};

			if (dist < 8)
			{
				if (ant.TargetResource != null && ant.State == AntState.Moving)
					ant.State = AntState.Gathering;
				else if (ant.State == AntState.Returning && dist < 25)
					ant.State = AntState.Resting;
				else
				{
					ant.State = AntState.Idle;
					ant.TargetResource = null;
				}
				return;
			}

			double moveX = (dx / dist) * ant.Speed * speedMultiplier;
			double moveY = (dy / dist) * ant.Speed * speedMultiplier;
			ant.X += moveX;
			ant.Y += moveY;
		}

		private void Gather(Ant ant)
		{
			if (ant.TargetResource?.IsDepleted != false)
			{
				ant.State = AntState.Idle;
				return;
			}

			int amount = Math.Min(8, ant.TargetResource.Amount);
			ant.TargetResource.Amount -= amount;
			ant.CarryingAmount = amount;
			ant.CarryingType = ant.TargetResource.Type;

			// Update statistics
			switch (ant.CarryingType)
			{
				case ResourceType.Food: ant.GatheredFood += amount; break;
				case ResourceType.Wood: ant.GatheredWood += amount; break;
				case ResourceType.Stone: ant.GatheredStone += amount; break;
				case ResourceType.Water: ant.GatheredWater += amount; break;
			}

			if (ant.TargetResource.IsDepleted)
			{
				GameCanvas.Children.Remove(ant.TargetResource.Visual);
				resources.Remove(ant.TargetResource);

				// Respawn after delay
				var type = ant.CarryingType.Value;
				string[] info = type switch
				{
					ResourceType.Food => new[] { "🍂", "#00b894" },
					ResourceType.Wood => new[] { "🪵", "#e17055" },
					ResourceType.Stone => new[] { "🪨", "#b2bec3" },
					ResourceType.Water => new[] { "💧", "#0984e3" },
					_ => new[] { "❓", "#gray" }
				};

				Dispatcher.BeginInvoke(new Action(() =>
					CreateResource(resources.Max(r => r.Id) + 1, type, info[0], info[1])),
					DispatcherPriority.Background);
			}

			ant.TargetX = nestX;
			ant.TargetY = nestY;
			ant.State = AntState.Returning;
		}

		private void Return(Ant ant)
		{
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double dist = Math.Sqrt(dx * dx + dy * dy);

			if (dist < 25)
			{
				// Deposit
				switch (ant.CarryingType)
				{
					case ResourceType.Food:
						food += ant.CarryingAmount;
						statistics.TotalFoodCollected += ant.CarryingAmount;
						break;
					case ResourceType.Wood:
						wood += ant.CarryingAmount;
						statistics.TotalWoodCollected += ant.CarryingAmount;
						break;
					case ResourceType.Stone:
						stone += ant.CarryingAmount;
						statistics.TotalStoneCollected += ant.CarryingAmount;
						break;
					case ResourceType.Water:
						water += ant.CarryingAmount;
						statistics.TotalWaterCollected += ant.CarryingAmount;
						break;
				}

				ant.CarryingAmount = 0;
				ant.CarryingType = null;
				ant.Energy = Math.Min(100, ant.Energy + 40);
				ant.State = AntState.Resting;
				UpdateUI();
			}
			else
			{
				double speedMult = ant.CarryingAmount > 0 ? 1.2 : 1.0;
				ant.X += (dx / dist) * ant.Speed * speedMult;
				ant.Y += (dy / dist) * ant.Speed * speedMult;
			}
		}

		private void Rest(Ant ant)
		{
			ant.Energy = Math.Min(100, ant.Energy + 2);
			ant.Health = Math.Min(100, ant.Health + 1);

			if (ant.Energy >= 95)
			{
				ant.State = AntState.Idle;
			}
		}

		private void KillAnt(Ant ant, string reason)
		{
			ant.State = AntState.Dead;
			ant.Visual.Opacity = 0.3;
			statistics.AntsDied++;
			AddLog($"💀 {ant.Name} померла ({reason})", Colors.Red);

			// Remove after delay
			DispatcherTimer removeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
			removeTimer.Tick += (s, e) =>
			{
				GameCanvas.Children.Remove(ant.Visual);
				ants.Remove(ant);
				removeTimer.Stop();
			};
			removeTimer.Start();
		}

		private void UpdateAntVisual(Ant ant)
		{
			if (ant.Visual == null) return;

			Canvas.SetLeft(ant.Visual, ant.X - 16);
			Canvas.SetTop(ant.Visual, ant.Y - 16);

			// Rotation
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double angle = Math.Atan2(dy, dx) * 180 / Math.PI + 90;

			double current = ant.RotateTransform.Angle;
			double diff = angle - current;
			while (diff > 180) diff -= 360;
			while (diff < -180) diff += 360;
			ant.RotateTransform.Angle = current + diff * 0.1;

			// Visual feedback for low energy
			if (ant.Energy < 20)
				ant.Visual.Opacity = 0.7;
			else
				ant.Visual.Opacity = 1.0;
		}

		private void UpdateUI()
		{
			// Resources
			FoodText.Text = food.ToString();
			WoodText.Text = wood.ToString();
			StoneText.Text = stone.ToString();
			WaterText.Text = water.ToString();

			// Rates
			int foodRate = ants.Count(a => a.State != AntState.Dead) / 3;
			FoodRateText.Text = $"+{foodRate}/сек";
			WoodRateText.Text = $"+{ants.Count / 4}/сек";
			StoneRateText.Text = $"+{ants.Count / 6}/сек";
			WaterRateText.Text = $"+{ants.Count / 5}/сек";

			// Ants
			int alive = ants.Count(a => a.State != AntState.Dead);
			int working = ants.Count(a => a.State == AntState.Moving || a.State == AntState.Gathering);
			AntsText.Text = $"{alive}/{maxAnts}";
			AntsWorkingText.Text = $"{working} працюють";

			// Level
			LevelText.Text = colonyLevel.ToString();
			int expNeeded = colonyLevel * 100;
			int currentExp = (food / 10) + (wood / 5) + (stone / 3);
			LevelProgress.Value = Math.Min(100, (currentExp * 100) / expNeeded);

			// Time
			DayText.Text = $"День {currentDay}";
			TimeText.Text = $"{currentHour:D2}:{currentMinute:D2}";

			// Statistics
			TotalFoodText.Text = $"🍂 {statistics.TotalFoodCollected}";
			TotalWoodText.Text = $"🪵 {statistics.TotalWoodCollected}";
			TotalStoneText.Text = $"🪨 {statistics.TotalStoneCollected}";
			TotalWaterText.Text = $"💧 {statistics.TotalWaterCollected}";
			AntsBornText.Text = $"🐣 {statistics.AntsBorn} народилось";
			AntsDiedText.Text = $"💀 {statistics.AntsDied} померло";
			NestExpansionsText.Text = $"🏗️ {statistics.NestExpansions} розширень";

			// Buttons
			SpawnAntBtn.IsEnabled = food >= 10 && water >= 5 && alive < maxAnts;
			ExpandNestBtn.IsEnabled = wood >= 50 && stone >= 25;

			// Coords
			if (ants.Any())
			{
				var first = ants.First();
				CoordsText.Text = $"📍 X: {first.X:F0} | Y: {first.Y:F0}";
			}
		}

		private void AddLog(string message, Color color)
		{
			var border = new Border
			{
				Background = new SolidColorBrush(Color.FromRgb(45, 52, 54)),
				CornerRadius = new CornerRadius(6),
				Padding = new Thickness(8),
				Margin = new Thickness(0, 0, 0, 5)
			};

			var text = new TextBlock
			{
				Text = $"[{DateTime.Now:HH:mm}] {message}",
				Foreground = new SolidColorBrush(color),
				FontSize = 11,
				TextWrapping = TextWrapping.Wrap
			};

			border.Child = text;
			EventLog.Children.Insert(0, border);

			// Limit log size
			if (EventLog.Children.Count > 50)
				EventLog.Children.RemoveAt(50);
		}

		private void ShowNotification(string icon, string message)
		{
			NotificationIcon.Text = icon;
			NotificationText.Text = message;
			NotificationPanel.Visibility = Visibility.Visible;

			var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
			timer.Tick += (s, e) =>
			{
				NotificationPanel.Visibility = Visibility.Collapsed;
				timer.Stop();
			};
			timer.Start();
		}

		private void ShowAntDetails(Ant ant)
		{
			var window = new AntDetailsWindow(ant);
			window.ShowDialog();
		}

		// ===== SAVE / LOAD =====
		public void SaveGame(string filename = null)
		{
			string path = filename ?? currentSaveFile;
			if (!path.StartsWith("Data/")) path = "Data/" + path;

			var data = new SaveData
			{
				SaveDate = DateTime.Now,
				SaveName = System.IO.Path.GetFileNameWithoutExtension(path),

				Food = food,
				Wood = wood,
				Stone = stone,
				Water = water,

				ColonyLevel = colonyLevel,
				MaxAnts = maxAnts,
				NestX = nestX,
				NestY = nestY,
				NestSize = nestSize,

				CurrentDay = currentDay,
				CurrentHour = currentHour,
				CurrentMinute = currentMinute,
				Weather = currentWeather,
				IsPaused = isPaused,
				GameSpeed = gameSpeed,

				Ants = ants.Where(a => a.State != AntState.Dead).Select(a => new AntSaveData
				{
					Id = a.Id,
					Name = a.Name,
					X = a.X,
					Y = a.Y,
					Speed = a.Speed,
					State = a.State.ToString(),
					Energy = a.Energy,
					Health = a.Health,
					Age = a.Age,
					GatheredFood = a.GatheredFood,
					GatheredWood = a.GatheredWood,
					GatheredStone = a.GatheredStone,
					GatheredWater = a.GatheredWater
				}).ToList(),

				Resources = resources.Select(r => new ResourceSaveData
				{
					Id = r.Id,
					Type = r.Type.ToString(),
					X = r.X,
					Y = r.Y,
					Amount = r.Amount
				}).ToList(),

				Statistics = statistics
			};

			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(data, options);
			File.WriteAllText(path, json);

			// Backup
			string backupPath = $"Data/backups/{System.IO.Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
			File.WriteAllText(backupPath, json);

			ShowNotification("💾", $"Гру збережено: {data.SaveName}");
			AddLog($"💾 Гру збережено: {path}", Colors.LightGreen);
		}

		public void LoadGame(string filename)
		{
			if (!File.Exists(filename)) return;

			string json = File.ReadAllText(filename);
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var data = JsonSerializer.Deserialize<SaveData>(json, options);

			// Restore state
			food = data.Food;
			wood = data.Wood;
			stone = data.Stone;
			water = data.Water;
			colonyLevel = data.ColonyLevel;
			maxAnts = data.MaxAnts;
			nestX = data.NestX;
			nestY = data.NestY;
			nestSize = data.NestSize;
			currentDay = data.CurrentDay;
			currentHour = data.CurrentHour;
			currentMinute = data.CurrentMinute;
			currentWeather = data.Weather;
			isPaused = data.IsPaused;
			gameSpeed = data.GameSpeed;
			statistics = data.Statistics ?? new GameStatistics();
			currentSaveFile = System.IO.Path.GetFileName(filename);
			// Clear and restore
			ants.Clear();
			resources.Clear();
			GameCanvas.Children.Clear();
			GameCanvas.Children.Add(NestGrid);
			Canvas.SetLeft(NestGrid, nestX - 75);
			Canvas.SetTop(NestGrid, nestY - 75);

			// Restore ants
			foreach (var a in data.Ants)
			{
				var ant = new Ant(a.Id, a.X, a.Y)
				{
					Name = a.Name,
					Speed = a.Speed,
					State = Enum.Parse<AntState>(a.State),
					Energy = a.Energy,
					Health = a.Health,
					Age = a.Age,
					GatheredFood = a.GatheredFood,
					GatheredWood = a.GatheredWood,
					GatheredStone = a.GatheredStone,
					GatheredWater = a.GatheredWater
				};

				ant.Visual = CreateAntVisual();
				ant.RotateTransform = new RotateTransform(0);
				ant.Visual.RenderTransform = ant.RotateTransform;
				ant.Visual.RenderTransformOrigin = new Point(0.5, 0.5);
				ant.Visual.MouseLeftButtonDown += (s, e) => ShowAntDetails(ant);

				Canvas.SetLeft(ant.Visual, ant.X - 16);
				Canvas.SetTop(ant.Visual, ant.Y - 16);
				GameCanvas.Children.Add(ant.Visual);
				ants.Add(ant);
			}

			// Restore resources
			foreach (var r in data.Resources)
			{
				var type = Enum.Parse<ResourceType>(r.Type);
				string[] info = type switch
				{
					ResourceType.Food => new[] { "🍂", "#00b894" },
					ResourceType.Wood => new[] { "🪵", "#e17055" },
					ResourceType.Stone => new[] { "🪨", "#b2bec3" },
					ResourceType.Water => new[] { "💧", "#0984e3" },
					_ => new[] { "❓", "gray" }
				};

				var node = new ResourceNode
				{
					Id = r.Id,
					Type = type,
					X = r.X,
					Y = r.Y,
					Amount = r.Amount,
					MaxAmount = 150
				};

				var border = new Border
				{
					Width = 44,
					Height = 44,
					Background = (SolidColorBrush)new BrushConverter().ConvertFrom(info[1]),
					CornerRadius = new CornerRadius(22),
					BorderBrush = Brushes.White,
					BorderThickness = new Thickness(2),
					Child = new TextBlock
					{
						Text = info[0],
						FontSize = 24,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center
					}
				};

				Canvas.SetLeft(border, r.X - 22);
				Canvas.SetTop(border, r.Y - 22);
				GameCanvas.Children.Add(border);
				node.Visual = border;
				resources.Add(node);
			}

			ShowNotification("📂", "Гру завантажено!");
			AddLog($"📂 Гру завантажено: {filename}", Colors.LightYellow);
			UpdateUI();
		}

		private void AutoSave()
		{
			SaveGame("savegame.json");
			StatusText.Text = $"✅ Автозбереження: {DateTime.Now:HH:mm:ss}";
		}

		// ===== BUTTON HANDLERS =====
		private void SpawnAntBtn_Click(object sender, RoutedEventArgs e)
		{
			if (food >= 10 && water >= 5)
			{
				food -= 10;
				water -= 5;
				SpawnAnt();
				UpdateUI();
			}
		}

		private void ExpandNestBtn_Click(object sender, RoutedEventArgs e)
		{
			if (wood >= 50 && stone >= 25)
			{
				wood -= 50;
				stone -= 25;
				colonyLevel++;
				maxAnts += 5;
				nestSize += 20;
				statistics.NestExpansions++;

				// Visual update
				var nest = NestGrid.Children[0] as Ellipse;
				nest.Width = nestSize;
				nest.Height = nestSize;

				ShowNotification("🏗️", $"Гніздо розширено до рівня {colonyLevel}!");
				AddLog($"🏗️ Гніздо розширено до рівня {colonyLevel}!", Colors.Gold);
				UpdateUI();
			}
		}

		private void GatherFoodBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(20, 50) * ants.Count(a => a.State != AntState.Dead);
			food += amount;
			statistics.TotalFoodCollected += amount;
			ShowNotification("🍂", $"+{amount} їжі!");
			UpdateUI();
		}

		private void GatherWoodBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(10, 30) * ants.Count(a => a.State != AntState.Dead);
			wood += amount;
			statistics.TotalWoodCollected += amount;
			ShowNotification("🪵", $"+{amount} деревини!");
			UpdateUI();
		}

		private void GatherStoneBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(5, 20) * ants.Count(a => a.State != AntState.Dead);
			stone += amount;
			statistics.TotalStoneCollected += amount;
			ShowNotification("🪨", $"+{amount} каменю!");
			UpdateUI();
		}

		private void GatherWaterBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(8, 25) * ants.Count(a => a.State != AntState.Dead);
			water += amount;
			statistics.TotalWaterCollected += amount;
			ShowNotification("💧", $"+{amount} води!");
			UpdateUI();
		}

		private void KillAntBtn_Click(object sender, RoutedEventArgs e)
		{
			var alive = ants.Where(a => a.State != AntState.Dead).ToList();
			if (alive.Any())
			{
				var toKill = alive.Last();
				KillAnt(toKill, "жертва заради колонії");
				UpdateUI();
			}
		}

		private void PauseBtn_Click(object sender, RoutedEventArgs e)
		{
			isPaused = !isPaused;
			PauseBtn.Content = isPaused ? "▶" : "⏸";
			StatusText.Text = isPaused ? "⏸ Гра на паузі" : "✅ Гра запущена";
		}

		private void SpeedBtn_Click(object sender, RoutedEventArgs e)
		{
			gameSpeed = gameSpeed == 1 ? 2 : gameSpeed == 2 ? 4 : 1;
			SpeedBtn.Content = $"▶ x{gameSpeed}";
		}

		private void QuickSaveBtn_Click(object sender, RoutedEventArgs e) => SaveGame();

		// ===== MENU HANDLERS =====
		private void MenuSave_Click(object sender, RoutedEventArgs e)
		{
			var window = new SaveLoadWindow(this, true);
			window.ShowDialog();
		}

		private void MenuLoad_Click(object sender, RoutedEventArgs e)
		{
			var window = new SaveLoadWindow(this, false);
			window.ShowDialog();
		}

		private void MenuNewSave_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Створити новий запис? Поточний прогрес буде втрачено!",
				"Новий запис", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				NewGame();
			}
		}

		private void MenuSettings_Click(object sender, RoutedEventArgs e)
		{
			// Settings window
		}

		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuStatistics_Click(object sender, RoutedEventArgs e)
		{
			var window = new StatisticsWindow(statistics, ants);
			window.ShowDialog();
		}

		private void MenuResearch_Click(object sender, RoutedEventArgs e)
		{
			var window = new ResearchWindow(this);
			window.ShowDialog();
		}

		private void MenuAnts_Click(object sender, RoutedEventArgs e)
		{
			// Show ant management
		}

		private void MenuHelp_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("🐜 Мурашина Ферма v2.0\n\n" +
				"Керування:\n" +
				"- Клік на мураху: деталі\n" +
				"- Кнопки: управління колонією\n" +
				"- Меню: збереження/завантаження\n\n" +
				"Поради:\n" +
				"- Слідкуйте за їжею та водою\n" +
				"- Розширюйте гніздо для більше мурах\n" +
				"- Використовуйте погоду для планування",
				"Довідка", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var result = MessageBox.Show("Зберегти гру перед виходом?",
				"Вихід", MessageBoxButton.YesNoCancel);

			if (result == MessageBoxResult.Yes)
			{
				SaveGame();
			}
			else if (result == MessageBoxResult.Cancel)
			{
				e.Cancel = true;
			}
		}
	}
}