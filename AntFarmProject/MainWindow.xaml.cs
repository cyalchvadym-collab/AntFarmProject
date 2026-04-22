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

	// <summary>
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

	/// <summary>
	/// Клас, що представляє сутність мурахи в грі.
	/// </summary>
	public class Ant
	{
		/// <summary>
		/// Унікальний ідентифікатор мурахи.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Ім'я мурахи (наприклад, "Мураха #1").
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Поточна координата X на ігровому полі.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Поточна координата Y на ігровому полі.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Цільова координата X, до якої рухається мураха.
		/// </summary>
		public double TargetX { get; set; }

		/// <summary>
		/// Цільова координата Y, до якої рухається мураха.
		/// </summary>
		public double TargetY { get; set; }

		/// <summary>
		/// Швидкість пересування мурахи.
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// Поточний стан діяльності мурахи.
		/// </summary>
		public AntState State { get; set; }

		/// <summary>
		/// Вузол ресурсу, до якого прямує мураха.
		/// </summary>
		public ResourceNode TargetResource { get; set; }

		/// <summary>
		/// Кількість ресурсу, яку мураха несе в даний момент.
		/// </summary>
		public int CarryingAmount { get; set; }

		/// <summary>
		/// Тип ресурсу, який мураха несе в даний момент.
		/// </summary>
		public ResourceType? CarryingType { get; set; }

		/// <summary>
		/// Рівень енергії мурахи (максимум 100).
		/// </summary>
		public double Energy { get; set; } = 100;

		/// <summary>
		/// Рівень здоров'я мурахи (максимум 100).
		/// </summary>
		public double Health { get; set; } = 100;

		/// <summary>
		/// Вік мурахи.
		/// </summary>
		public double Age { get; set; } = 0;

		/// <summary>
		/// Загальна кількість їжі, зібрана цією мурахою.
		/// </summary>
		public int GatheredFood { get; set; }

		/// <summary>
		/// Загальна кількість деревини, зібрана цією мурахою.
		/// </summary>
		public int GatheredWood { get; set; }

		/// <summary>
		/// Загальна кількість каменю, зібрана цією мурахою.
		/// </summary>
		public int GatheredStone { get; set; }

		/// <summary>
		/// Загальна кількість води, зібрана цією мурахою.
		/// </summary>
		public int GatheredWater { get; set; }

		/// <summary>
		/// Візуальне представлення мурахи на ігровому полотні.
		/// </summary>
		public Canvas Visual { get; set; }

		/// <summary>
		/// Трансформація обертання для візуального об'єкта мурахи.
		/// </summary>
		public RotateTransform RotateTransform { get; set; }

		/// <summary>
		/// Час народження (створення) мурахи.
		/// </summary>
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

	/// <summary>
	/// Клас, що описує джерело ресурсу на ігровому полі.
	/// </summary>
	public class ResourceNode
	{
		/// <summary>
		/// Унікальний ідентифікатор вузла ресурсу.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Тип ресурсу (їжа, дерево, камінь, вода).
		/// </summary>
		public ResourceType Type { get; set; }

		/// <summary>
		/// Координата X ресурсу.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Координата Y ресурсу.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Поточний залишок ресурсу у вузлі.
		/// </summary>
		public int Amount { get; set; }

		/// <summary>
		/// Максимальна місткість ресурсу.
		/// </summary>
		public int MaxAmount { get; set; }

		/// <summary>
		/// Візуальний елемент ресурсу на інтерфейсі.
		/// </summary>
		public UIElement Visual { get; set; }

		/// <summary>
		/// Вказує, чи вичерпано ресурс повністю.
		/// </summary>
		public bool IsDepleted => Amount <= 0;
	}

	/// <summary>
	/// Клас для зберігання загальної статистики гри.
	/// </summary>
	public class GameStatistics
	{
		/// <summary>
		/// Загальна кількість зібраної їжі за час гри.
		/// </summary>
		public int TotalFoodCollected { get; set; }

		/// <summary>
		/// Загальна кількість зібраної деревини за час гри.
		/// </summary>
		public int TotalWoodCollected { get; set; }

		/// <summary>
		/// Загальна кількість зібраного каменю за час гри.
		/// </summary>
		public int TotalStoneCollected { get; set; }

		/// <summary>
		/// Загальна кількість зібраної води за час гри.
		/// </summary>
		public int TotalWaterCollected { get; set; }

		/// <summary>
		/// Кількість мурах, що народилися.
		/// </summary>
		public int AntsBorn { get; set; }

		/// <summary>
		/// Кількість мурах, що померли.
		/// </summary>
		public int AntsDied { get; set; }

		/// <summary>
		/// Кількість розширень гнізда (колонії).
		/// </summary>
		public int NestExpansions { get; set; }

		/// <summary>
		/// Загальний час, проведений у грі.
		/// </summary>
		public TimeSpan PlayTime { get; set; }

		/// <summary>
		/// Кількість ігрових днів, протягом яких колонія вижила.
		/// </summary>
		public int DaysSurvived { get; set; } = 1;
	}


	/// <summary>
	/// Повний стан збереження гри, включаючи ресурси, дані колонії,
	/// час, сутності (мурахи) та статистику.
	/// </summary>
	public class SaveData
	{
		/// <summary>
		/// Версія формату файлу збереження.
		/// </summary>
		public string Version { get; set; } = "2.0";

		/// <summary>
		/// Дата та час створення збереження.
		/// </summary>
		public DateTime SaveDate { get; set; }

		/// <summary>
		/// Назва збереження, задана користувачем.
		/// </summary>
		public string SaveName { get; set; }

		/// <summary>
		/// Кількість їжі в колонії.
		/// </summary>
		public int Food { get; set; }

		/// <summary>
		/// Кількість деревини в колонії.
		/// </summary>
		public int Wood { get; set; }

		/// <summary>
		/// Кількість каменю в колонії.
		/// </summary>
		public int Stone { get; set; }

		/// <summary>
		/// Кількість води в колонії.
		/// </summary>
		public int Water { get; set; }

		/// <summary>
		/// Рівень розвитку колонії.
		/// </summary>
		public int ColonyLevel { get; set; }

		/// <summary>
		/// Максимальна кількість мурах у колонії.
		/// </summary>
		public int MaxAnts { get; set; }

		/// <summary>
		/// Координата X гнізда.
		/// </summary>
		public double NestX { get; set; }

		/// <summary>
		/// Координата Y гнізда.
		/// </summary>
		public double NestY { get; set; }

		/// <summary>
		/// Розмір гнізда.
		/// </summary>
		public int NestSize { get; set; }

		/// <summary>
		/// Поточний ігровий день.
		/// </summary>
		public int CurrentDay { get; set; }

		/// <summary>
		/// Поточна ігрова година.
		/// </summary>
		public int CurrentHour { get; set; }

		/// <summary>
		/// Поточна ігрова хвилина.
		/// </summary>
		public int CurrentMinute { get; set; }

		/// <summary>
		/// Поточна погода в ігровому світі.
		/// </summary>
		public WeatherType Weather { get; set; }

		/// <summary>
		/// Чи поставлена гра на паузу.
		/// </summary>
		public bool IsPaused { get; set; }

		/// <summary>
		/// Швидкість ігрової симуляції.
		/// </summary>
		public int GameSpeed { get; set; }

		/// <summary>
		/// Список усіх мурах у збереженому стані.
		/// </summary>
		public List<AntSaveData> Ants { get; set; }

		/// <summary>
		/// Список ресурсів у світі.
		/// </summary>
		public List<ResourceSaveData> Resources { get; set; }

		/// <summary>
		/// Загальна статистика гри.
		/// </summary>
		public GameStatistics Statistics { get; set; }
	}

	/// <summary>
	/// Збережений стан окремої мурахи.
	/// </summary>
	public class AntSaveData
	{
		/// <summary>
		/// Унікальний ідентифікатор мурахи.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Ім’я мурахи.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Координата X у світі.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Координата Y у світі.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Швидкість руху мурахи.
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// Поточний стан поведінки (наприклад: відпочинок, збір ресурсів).
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// Рівень енергії мурахи.
		/// </summary>
		public double Energy { get; set; }

		/// <summary>
		/// Рівень здоров’я мурахи.
		/// </summary>
		public double Health { get; set; }

		/// <summary>
		/// Вік мурахи.
		/// </summary>
		public double Age { get; set; }

		/// <summary>
		/// Кількість зібраної їжі.
		/// </summary>
		public int GatheredFood { get; set; }

		/// <summary>
		/// Кількість зібраної деревини.
		/// </summary>
		public int GatheredWood { get; set; }

		/// <summary>
		/// Кількість зібраного каменю.
		/// </summary>
		public int GatheredStone { get; set; }

		/// <summary>
		/// Кількість зібраної води.
		/// </summary>
		public int GatheredWater { get; set; }
	}
	/// <summary>
	/// Дані збереження одного ресурсу у світі.
	/// </summary>
	public class ResourceSaveData
	{
		/// <summary>
		/// Унікальний ідентифікатор ресурсу.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Тип ресурсу (наприклад: їжа, дерево, камінь, вода).
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Координата X розташування ресурсу.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Координата Y розташування ресурсу.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Поточна кількість ресурсу.
		/// </summary>
		public int Amount { get; set; }
	}

	/// <summary>
	/// Головне вікно гри, яке керує всією логікою симуляції мурашиної колонії.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Кількість їжі в колонії.
		/// </summary>
		private int food = 100;

		/// <summary>
		/// Кількість деревини в колонії.
		/// </summary>
		private int wood = 50;

		/// <summary>
		/// Кількість каменю в колонії.
		/// </summary>
		private int stone = 25;

		/// <summary>
		/// Кількість води в колонії.
		/// </summary>
		private int water = 30;

		/// <summary>
		/// Рівень розвитку колонії.
		/// </summary>
		private int colonyLevel = 1;

		/// <summary>
		/// Максимальна кількість мурах у колонії.
		/// </summary>
		private int maxAnts = 10;

		/// <summary>
		/// Координата X гнізда.
		/// </summary>
		private double nestX = 425;

		/// <summary>
		/// Координата Y гнізда.
		/// </summary>
		private double nestY = 275;

		/// <summary>
		/// Розмір гнізда.
		/// </summary>
		private int nestSize = 150;

		/// <summary>
		/// Список усіх активних мурах.
		/// </summary>
		private List<Ant> ants = new();

		/// <summary>
		/// Список ресурсів у світі.
		/// </summary>
		private List<ResourceNode> resources = new();

		/// <summary>
		/// Загальна статистика гри.
		/// </summary>
		private GameStatistics statistics = new();

		/// <summary>
		/// Поточний ігровий день.
		/// </summary>
		private int currentDay = 1;

		/// <summary>
		/// Поточна ігрова година.
		/// </summary>
		private int currentHour = 8;

		/// <summary>
		/// Поточна ігрова хвилина.
		/// </summary>
		private int currentMinute = 0;

		/// <summary>
		/// Поточна погода у світі гри.
		/// </summary>
		private WeatherType currentWeather = WeatherType.Sunny;

		/// <summary>
		/// Чи поставлена гра на паузу.
		/// </summary>
		private bool isPaused = false;

		/// <summary>
		/// Швидкість симуляції гри.
		/// </summary>
		private int gameSpeed = 1;

		/// <summary>
		/// Ім’я поточного файлу збереження.
		/// </summary>
		private string currentSaveFile = "savegame.json";

		/// <summary>
		/// Таймер основного ігрового циклу.
		/// </summary>
		private DispatcherTimer gameTimer;

		/// <summary>
		/// Таймер ігрового часу (секунди/хвилини).
		/// </summary>
		private DispatcherTimer secondTimer;

		/// <summary>
		/// Таймер автоматичного збереження гри.
		/// </summary>
		private DispatcherTimer autoSaveTimer;

		/// <summary>
		/// Генератор випадкових чисел.
		/// </summary>
		private Random random = new();

		/// <summary>
		/// Час початку ігрової сесії.
		/// </summary>
		private DateTime sessionStart;

		/// <summary>
		/// Чи використовується темна тема інтерфейсу.
		/// </summary>
		private bool isDarkTheme = false;

		/// <summary>
		/// Конструктор головного вікна гри.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
			Closing += Window_Closing;
			sessionStart = DateTime.Now;
		}

		
		/// <summary>
		/// Подія завантаження головного вікна гри.
		/// Перевіряє наявність автозбереження та пропонує його завантажити.
		/// Якщо користувач відмовляється — починається нова гра.
		/// </summary>
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
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

		/// <summary>
		/// Ініціалізує нову гру, скидає всі значення та створює початкові об’єкти.
		/// </summary>
		private void NewGame()
		{
			food = 100; wood = 50; stone = 25; water = 30;
			colonyLevel = 1; maxAnts = 10;
			currentDay = 1; currentHour = 8; currentMinute = 0;
			statistics = new GameStatistics();

			ants.Clear();
			resources.Clear();
			GameCanvas.Children.Clear();
			GameCanvas.Children.Add(NestGrid);

			
			for (int i = 0; i < 3; i++)
				SpawnAnt(false);

			
			GenerateResources();

			SetupTimers();

			UpdateUI();
			AddLog("🎮 Нова гра розпочата!", Colors.White);
		}

		/// <summary>
		/// Налаштовує всі ігрові таймери: основний цикл, ігровий час та автозбереження.
		/// </summary>
		private void SetupTimers()
		{
			gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
			gameTimer.Tick += GameLoop;
			gameTimer.Start();

			secondTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
			secondTimer.Tick += SecondTick;
			secondTimer.Start();

			autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
			autoSaveTimer.Tick += (s, e) => AutoSave();
			autoSaveTimer.Start();
		}

		/// <summary>
		/// Виконується кожну секунду і відповідає за ігровий час, споживання ресурсів,
		/// старіння мурах та випадкові події.
		/// </summary>
		private void SecondTick(object sender, EventArgs e)
		{
			if (isPaused) return;

			
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

			
			int foodConsumption = (ants.Count / 3) + 1;
			int waterConsumption = (ants.Count / 4) + 1;

			food = Math.Max(0, food - foodConsumption);
			water = Math.Max(0, water - waterConsumption);

			
			if (food == 0)
			{
				var starving = ants.Where(a => a.State != AntState.Dead).ToList();
				foreach (var ant in starving.Take(1))
				{
					ant.Health -= 10;
					if (ant.Health <= 0) KillAnt(ant, "голод");
				}
			}

			
			foreach (var ant in ants.Where(a => a.State != AntState.Dead))
			{
				ant.Age += 0.1;

				if (ant.Age > 100 && random.Next(1000) < 5)
					KillAnt(ant, "старість");
			}

			
			if (random.Next(100) < 5)
			{
				int bonus = random.Next(10, 30);
				food += bonus;
				AddLog($"🍂 Випадкова знахідка: +{bonus} їжі!", Colors.LightGreen);
			}

			UpdateUI();
		}

		/// <summary>
		/// Змінює поточну погоду на випадкову та додає лог події.
		/// </summary>
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

		/// <summary>
		/// Генерує ресурси на карті відповідно до поточної погоди.
		/// Створює їжу, дерево, камінь і воду.
		/// </summary>
		private void GenerateResources()
		{
			int id = 1;

			
			int foodCount = currentWeather == WeatherType.Sunny ? 12 : 8;
			for (int i = 0; i < foodCount; i++)
				CreateResource(id++, ResourceType.Food, "🍂", "#00b894");

			
			for (int i = 0; i < 6; i++)
				CreateResource(id++, ResourceType.Wood, "🪵", "#e17055");

			
			for (int i = 0; i < 4; i++)
				CreateResource(id++, ResourceType.Stone, "🪨", "#b2bec3");

			
			int waterCount = currentWeather == WeatherType.Rainy ? 6 : 3;
			for (int i = 0; i < waterCount; i++)
				CreateResource(id++, ResourceType.Water, "💧", "#0984e3");
		}

		/// <summary>
		/// Створює один ресурс у світі гри з візуальним відображенням.
		/// Уникає розміщення занадто близько до гнізда.
		/// </summary>
		private void CreateResource(int id, ResourceType type, string emoji, string color)
		{
			double x, y;
			int attempts = 0;

			
			do
			{
				x = random.Next(100, 700);
				y = random.Next(100, 500);
				attempts++;
			}
			while (Distance(x, y, nestX, nestY) < 180 && attempts < 50);

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
		/// <summary>
		/// Обчислює відстань між двома точками у 2D просторі.
		/// </summary>
		private double Distance(double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
		}

		/// <summary>
		/// Створює нову мураху та додає її до гри.
		/// Також ініціалізує її візуальне представлення.
		/// </summary>
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

		/// <summary>
		/// Створює графічне представлення мурахи у вигляді Canvas-об’єкта.
		/// Містить тіло, ноги, очі, вусики та тінь.
		/// </summary>
		private Canvas CreateAntVisual()
		{
			var canvas = new Canvas { Width = 32, Height = 32 };

			
			var shadow = new Ellipse
			{
				Width = 22,
				Height = 7,
				Fill = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0))
			};
			Canvas.SetLeft(shadow, 5);
			Canvas.SetTop(shadow, 24);

			
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

			
			var eyeBrush = new SolidColorBrush(Color.FromRgb(90, 85, 80));
			var leftEye = new Ellipse { Width = 2.5, Height = 3.5, Fill = eyeBrush };
			Canvas.SetLeft(leftEye, 12);
			Canvas.SetTop(leftEye, -4);

			var rightEye = new Ellipse { Width = 2.5, Height = 3.5, Fill = eyeBrush };
			Canvas.SetLeft(rightEye, 17.5);
			Canvas.SetTop(rightEye, -4);

			var shine = new SolidColorBrush(Colors.White);
			var leftShine = new Ellipse { Width = 1, Height = 1.5, Fill = shine };
			Canvas.SetLeft(leftShine, 12.5);
			Canvas.SetTop(leftShine, -3.5);

			var rightShine = new Ellipse { Width = 1, Height = 1.5, Fill = shine };
			Canvas.SetLeft(rightShine, 18);
			Canvas.SetTop(rightShine, -3.5);

			
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

			
			canvas.ToolTip = "Клікніть для деталей";

			return canvas;
		}

		/// <summary>
		/// Основний ігровий цикл.
		/// Обробляє логіку всіх активних мурах залежно від швидкості гри.
		/// </summary>
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

		/// <summary>
		/// Обробляє поведінку однієї мурахи залежно від її стану.
		/// </summary>
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

			
			if (ant.State != AntState.Idle && ant.State != AntState.Resting)
				ant.Energy -= 0.05;

		
			if (ant.Energy <= 0 && ant.State != AntState.Resting)
			{
				ant.State = AntState.Returning;
				ant.TargetX = nestX;
				ant.TargetY = nestY;
			}
		}

		/// <summary>
		/// Призначає мурахі задачу (пошук ресурсу або випадкову ціль).
		/// Враховує пріоритети ресурсів залежно від потреб колонії.
		/// </summary>
		private void FindTask(Ant ant)
		{
			var available = resources.Where(r => !r.IsDepleted).ToList();

			
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
				var target = available
					.OrderBy(r => Distance(ant.X, ant.Y, r.X, r.Y))
					.First();

				SetAntTarget(ant, target);
			}
			else
			{
				
				ant.TargetX = random.Next(100, 750);
				ant.TargetY = random.Next(100, 550);
				ant.State = AntState.Moving;
			}
		}

		/// <summary>
		/// Встановлює цільову точку мурахи на конкретний ресурс.
		/// </summary>
		private void SetAntTarget(Ant ant, ResourceNode target)
		{
			ant.TargetResource = target;
			ant.TargetX = target.X;
			ant.TargetY = target.Y;
			ant.State = AntState.Moving;
		}

		/// <summary>
		/// Рухає мураху до її цілі з урахуванням швидкості та погоди.
		/// Також перевіряє досягнення цілі.
		/// </summary>
		private void MoveAnt(Ant ant)
		{
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double dist = Math.Sqrt(dx * dx + dy * dy);

			
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
		/// <summary>
		/// Процес збору ресурсу мурахою.
		/// Зменшує ресурс у вузлі, переносить його та оновлює статистику.
		/// </summary>
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

		/// <summary>
		/// Повернення мурахи до гнізда та передача зібраного ресурсу.
		/// Також оновлює глобальні ресурси та статистику.
		/// </summary>
		private void Return(Ant ant)
		{
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double dist = Math.Sqrt(dx * dx + dy * dy);

			
			if (dist < 25)
			{
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

		/// <summary>
		/// Відпочинок мурахи: відновлення енергії та здоров’я.
		/// Після відновлення повертається в режим очікування.
		/// </summary>
		private void Rest(Ant ant)
		{
			ant.Energy = Math.Min(100, ant.Energy + 2);
			ant.Health = Math.Min(100, ant.Health + 1);

			if (ant.Energy >= 95)
			{
				ant.State = AntState.Idle;
			}
		}

		/// <summary>
		/// Вбиває мураху та видаляє її з гри через коротку затримку.
		/// Оновлює статистику смертності.
		/// </summary>
		private void KillAnt(Ant ant, string reason)
		{
			ant.State = AntState.Dead;
			ant.Visual.Opacity = 0.3;
			statistics.AntsDied++;

			AddLog($"💀 {ant.Name} померла ({reason})", Colors.Red);

			
			DispatcherTimer removeTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(5)
			};

			removeTimer.Tick += (s, e) =>
			{
				GameCanvas.Children.Remove(ant.Visual);
				ants.Remove(ant);
				removeTimer.Stop();
			};

			removeTimer.Start();
		}

		/// <summary>
		/// Оновлює візуальне положення та стан мурахи на екрані.
		/// Також обертає її у напрямку руху та змінює прозорість залежно від енергії.
		/// </summary>
		private void UpdateAntVisual(Ant ant)
		{
			if (ant.Visual == null) return;

			Canvas.SetLeft(ant.Visual, ant.X - 16);
			Canvas.SetTop(ant.Visual, ant.Y - 16);

		
			double dx = ant.TargetX - ant.X;
			double dy = ant.TargetY - ant.Y;
			double angle = Math.Atan2(dy, dx) * 180 / Math.PI + 90;

			double current = ant.RotateTransform.Angle;
			double diff = angle - current;

			while (diff > 180) diff -= 360;
			while (diff < -180) diff += 360;

			ant.RotateTransform.Angle = current + diff * 0.1;

			
			if (ant.Energy < 20)
				ant.Visual.Opacity = 0.7;
			else
				ant.Visual.Opacity = 1.0;
		}

		/// <summary>
		/// Оновлює весь інтерфейс користувача (UI):
		/// ресурси, статистику, прогрес, час та стан колонії.
		/// </summary>
		private void UpdateUI()
		{
			StoneText.Text = stone.ToString();
			WaterText.Text = water.ToString();

			
			int foodRate = ants.Count(a => a.State != AntState.Dead) / 3;
			FoodRateText.Text = $"+{foodRate}/сек";
			WoodRateText.Text = $"+{ants.Count / 4}/сек";
			StoneRateText.Text = $"+{ants.Count / 6}/сек";
			WaterRateText.Text = $"+{ants.Count / 5}/сек";

			
			int alive = ants.Count(a => a.State != AntState.Dead);
			int working = ants.Count(a => a.State == AntState.Moving || a.State == AntState.Gathering);

			AntsText.Text = $"{alive}/{maxAnts}";
			AntsWorkingText.Text = $"{working} працюють";

			
			LevelText.Text = colonyLevel.ToString();
			int expNeeded = colonyLevel * 100;
			int currentExp = (food / 10) + (wood / 5) + (stone / 3);

			LevelProgress.Value = Math.Min(100, (currentExp * 100) / expNeeded);

			DayText.Text = $"День {currentDay}";
			TimeText.Text = $"{currentHour:D2}:{currentMinute:D2}";

		
			TotalFoodText.Text = $"🍂 {statistics.TotalFoodCollected}";
			TotalWoodText.Text = $"🪵 {statistics.TotalWoodCollected}";
			TotalStoneText.Text = $"🪨 {statistics.TotalStoneCollected}";
			TotalWaterText.Text = $"💧 {statistics.TotalWaterCollected}";
			AntsBornText.Text = $"🐣 {statistics.AntsBorn} народилось";
			AntsDiedText.Text = $"💀 {statistics.AntsDied} померло";
			NestExpansionsText.Text = $"🏗️ {statistics.NestExpansions} розширень";

			
			SpawnAntBtn.IsEnabled = food >= 10 && water >= 5 && alive < maxAnts;
			ExpandNestBtn.IsEnabled = wood >= 50 && stone >= 25;

			
			if (ants.Any())
			{
				var first = ants.First();
				CoordsText.Text = $"📍 X: {first.X:F0} | Y: {first.Y:F0}";
			}
		}

		/// <summary>
		/// Додає повідомлення в ігровий лог подій.
		/// </summary>
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

			
			if (EventLog.Children.Count > 50)
				EventLog.Children.RemoveAt(50);
		}

		/// <summary>
		/// Показує тимчасове сповіщення на екрані.
		/// </summary>
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

		/// <summary>
		/// Відкриває вікно з детальною інформацією про мураху.
		/// </summary>
		private void ShowAntDetails(Ant ant)
		{
			var window = new AntDetailsWindow(ant);
			window.ShowDialog();
		}

		/// <summary>
		/// Зберігає поточний стан гри у JSON файл.
		/// Також створює резервну копію.
		/// </summary>
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

			
			string backupPath = $"Data/backups/{System.IO.Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
			File.WriteAllText(backupPath, json);

			ShowNotification("💾", $"Гру збережено: {data.SaveName}");
			AddLog($"💾 Гру збережено: {path}", Colors.LightGreen);
		}
		/// <summary>
		/// Завантажує стан гри з JSON-файлу.
		/// Відновлює всі ресурси, мурах, статистику та UI.
		/// </summary>
		public void LoadGame(string filename)
		{
			if (!File.Exists(filename)) return;

			string json = File.ReadAllText(filename);
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var data = JsonSerializer.Deserialize<SaveData>(json, options);

			
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
			
			ants.Clear();
			resources.Clear();
			GameCanvas.Children.Clear();
			GameCanvas.Children.Add(NestGrid);
			Canvas.SetLeft(NestGrid, nestX - 75);
			Canvas.SetTop(NestGrid, nestY - 75);

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
		/// <summary>
		/// Автоматично зберігає гру у файл.
		/// Використовується таймером автозбереження.
		/// </summary>
		private void AutoSave()
		{
			SaveGame("savegame.json");
			StatusText.Text = $"✅ Автозбереження: {DateTime.Now:HH:mm:ss}";
		}

		/// <summary>
		/// Створює нову мураху за натисканням кнопки.
		/// </summary>
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
		/// <summary>
		/// Розширює гніздо колонії.
		/// Збільшує максимальну кількість мурах.
		/// </summary>
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

				
				var nest = NestGrid.Children[0] as Ellipse;
				nest.Width = nestSize;
				nest.Height = nestSize;

				ShowNotification("🏗️", $"Гніздо розширено до рівня {colonyLevel}!");
				AddLog($"🏗️ Гніздо розширено до рівня {colonyLevel}!", Colors.Gold);
				UpdateUI();
			}
		}
		/// <summary>
		/// Додає випадкову кількість їжі (чит-кнопка/швидка дія).
		/// </summary>
		private void GatherFoodBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(20, 50) * ants.Count(a => a.State != AntState.Dead);
			food += amount;
			statistics.TotalFoodCollected += amount;
			ShowNotification("🍂", $"+{amount} їжі!");
			UpdateUI();
		}
		/// <summary>
		/// Додає випадкову кількість деревини.
		/// </summary>
		private void GatherWoodBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(10, 30) * ants.Count(a => a.State != AntState.Dead);
			wood += amount;
			statistics.TotalWoodCollected += amount;
			ShowNotification("🪵", $"+{amount} деревини!");
			UpdateUI();
		}
		/// <summary>
		/// Додає випадкову кількість каменю.
		/// </summary>
		private void GatherStoneBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(5, 20) * ants.Count(a => a.State != AntState.Dead);
			stone += amount;
			statistics.TotalStoneCollected += amount;
			ShowNotification("🪨", $"+{amount} каменю!");
			UpdateUI();
		}
		/// <summary>
		/// Додає випадкову кількість води.
		/// </summary>
		private void GatherWaterBtn_Click(object sender, RoutedEventArgs e)
		{
			int amount = random.Next(8, 25) * ants.Count(a => a.State != AntState.Dead);
			water += amount;
			statistics.TotalWaterCollected += amount;
			ShowNotification("💧", $"+{amount} води!");
			UpdateUI();
		}
		/// <summary>
		/// Видаляє одну живу мураху (для тестів або балансування).
		/// </summary>
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
		/// <summary>
		/// Ставить гру на паузу або відновлює її.
		/// </summary>
		private void PauseBtn_Click(object sender, RoutedEventArgs e)
		{
			isPaused = !isPaused;
			PauseBtn.Content = isPaused ? "▶" : "⏸";
			StatusText.Text = isPaused ? "⏸ Гра на паузі" : "✅ Гра запущена";
		}
		/// <summary>
		/// Змінює швидкість гри (x1 → x2 → x4 → x1).
		/// </summary>
		private void SpeedBtn_Click(object sender, RoutedEventArgs e)
		{
			gameSpeed = gameSpeed == 1 ? 2 : gameSpeed == 2 ? 4 : 1;
			SpeedBtn.Content = $"▶ x{gameSpeed}";
		}
		/// <summary>
		/// Швидке збереження гри.
		/// </summary>
		private void QuickSaveBtn_Click(object sender, RoutedEventArgs e) => SaveGame();
		/// <summary>
		/// Відкриває вікно керування збереженнями (збереження).
		/// </summary>
		private void MenuSave_Click(object sender, RoutedEventArgs e)
		{
			var window = new SaveLoadWindow(this, true);
			window.ShowDialog();
		}
		/// <summary>
		/// Відкриває вікно завантаження гри.
		/// </summary>
		private void MenuLoad_Click(object sender, RoutedEventArgs e)
		{
			var window = new SaveLoadWindow(this, false);
			window.ShowDialog();
		}
		/// <summary>
       /// Створює нову гру (перезапуск прогресу).
      /// </summary>
		private void MenuNewSave_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Створити новий запис? Поточний прогрес буде втрачено!",
				"Новий запис", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				NewGame();
			}
		}
		/// <summary>
       /// Обробляє подію натискання на пункт меню "Налаштування".
      /// Відкриває вікно налаштувань програми для користувача.
     /// </summary>
		private void MenuSettings_Click(object sender, RoutedEventArgs e)
		{
			
		}
		/// <summary>
		/// Обробляє подію натискання на пункт меню "Мурахи".
		/// Відкриває вікно з детальною інформацією про мурах.
		/// </summary>
		private void MenuAnts_Click(object sender, RoutedEventArgs e)
		{
			
		}
    
		/// <summary>
		/// Вихід з гри.
		/// </summary>
		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		/// <summary>
		/// Відкриває вікно статистики гри.
		/// </summary>
		private void MenuStatistics_Click(object sender, RoutedEventArgs e)
		{
			var window = new StatisticsWindow(statistics, ants);
			window.ShowDialog();
		}
		/// <summary>
		/// Відкриває вікно досліджень.
		/// </summary>
		private void MenuResearch_Click(object sender, RoutedEventArgs e)
		{
			var window = new ResearchWindow(this);
			window.ShowDialog();
		}

		/// <summary>
		/// Відкриває довідкове вікно.
		/// </summary>
		private void MenuHelp_Click(object sender, RoutedEventArgs e)

		{
			HelpWindow help = new HelpWindow();
			help.ShowDialog();
		}
		/// <summary>
		/// Перемикає світлу/темну тему інтерфейсу.
		/// </summary>
		private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
		{
			isDarkTheme = !isDarkTheme;

			
			Color windowBg, panelBg, textColor, canvasBg, buttonBg, menuBg;

			if (isDarkTheme)
			{
				windowBg = Color.FromRgb(30, 30, 30);
				panelBg = Color.FromRgb(45, 52, 54);
				textColor = Color.FromRgb(223, 230, 233);
				canvasBg = Color.FromRgb(10, 15, 20);
				buttonBg = Color.FromRgb(99, 110, 114);
				menuBg = Color.FromRgb(45, 52, 54);
			}
			else
			{
				
				windowBg = Color.FromRgb(245, 246, 250);
				panelBg = Color.FromRgb(255, 255, 255);
				textColor = Color.FromRgb(45, 52, 54);
				canvasBg = Color.FromRgb(232, 232, 232);
				buttonBg = Color.FromRgb(178, 190, 195);
				menuBg = Color.FromRgb(220, 220, 220);
			}

			
			this.Background = new SolidColorBrush(windowBg);

			var gamePanel = GameCanvas.Parent as Border;
			if (gamePanel != null)
				gamePanel.Background = new SolidColorBrush(canvasBg);

			
			foreach (var border in FindVisualChildren<Border>(this))
			{
				if (border.Name == "NestGrid" || border.Tag != null)
					continue;

				border.Background = new SolidColorBrush(panelBg);
				border.BorderBrush = new SolidColorBrush(isDarkTheme ? Color.FromRgb(99, 110, 114) : Color.FromRgb(178, 190, 195));
			}

			
			foreach (var textBlock in FindVisualChildren<TextBlock>(this))
			{
				
				if (textBlock.Name == "NotificationIcon" || textBlock.Name == "NestLevelText")
					continue;

				textBlock.Foreground = new SolidColorBrush(textColor);
			}

			
			var menu = FindVisualChildren<Menu>(this).FirstOrDefault();
			if (menu != null)
			{
				menu.Background = new SolidColorBrush(menuBg);
				menu.Foreground = new SolidColorBrush(textColor);
			}

			
			foreach (var button in FindVisualChildren<Button>(this))
			{
				if (button.Name == "SpawnAntBtn" || button.Name == "ExpandNestBtn" ||
					button.Name == "KillAntBtn" || button.Name == "ThemeButton")
					continue; 

				button.Background = new SolidColorBrush(buttonBg);
				button.Foreground = new SolidColorBrush(textColor);
			}

			ThemeButton.Content = isDarkTheme ? "☀️" : "🌓";

			AddLog(isDarkTheme ? "🌙 Темна тема увімкнена" : "☀️ Світла тема увімкнена", Colors.Gold);
		}
		/// <summary>
		/// Подія закриття вікна гри.
		/// Запитує у користувача збереження.
		/// </summary>
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

		/// <summary>
		/// Рекурсивно знаходить усі елементи UI заданого типу.
		/// Використовується для зміни теми.
		/// </summary>		
		private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
						yield return (T)child;

					foreach (T childOfChild in FindVisualChildren<T>(child))
						yield return childOfChild;
				}
			}
		}
	}
	}
