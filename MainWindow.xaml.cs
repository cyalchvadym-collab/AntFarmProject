using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace AntFarmProject
{
    /// <summary>
    /// Перерахування станів, у яких може перебувати мураха.
    /// </summary>
    public enum AntState { Idle, Moving, Gathering, Returning, Resting, Dead }

    /// <summary>
    /// Типи ресурсів, які доступні для збору в грі.
    /// </summary>
    public enum ResourceType { Food, Wood, Stone, Water }

    /// <summary>
    /// Погодні умови, які впливають на ігровий процес та поведінку мурах.
    /// </summary>
    public enum WeatherType { Sunny, Rainy, Stormy, Night }

    /// <summary>
    /// Представляє окрему мураху, її фізичні характеристики, стан та візуальне відображення.
    /// </summary>
    public class Ant
    {
        public int Id;
        public string Name;
        public double X, Y, TargetX, TargetY, Speed, BaseSpeed, Energy = 100, Health = 100, MaxHealth = 100, Age;
        public int GatheredFood, GatheredWood, GatheredStone, GatheredWater, CarryingAmount;
        public AntState State;
        public ResourceType? CarryingType;
        public ResourceNode TargetResource;
        public Canvas Visual;
        public RotateTransform RotateTransform;
        public DateTime BornTime;

        /// <summary>
        /// Ініціалізує новий екземпляр класу із 
        /// початковими координатами та випадковою базовою швидкістю.
        /// </summary>
        public Ant(int id, double x, double y)
        {
            Id = id; Name = $"Мураха #{id}"; X = x; Y = y; TargetX = x; TargetY = y;
            BaseSpeed = Speed = 1.5 + new Random().NextDouble() * 2;
            State = AntState.Idle; BornTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Представляє вузол (джерело) певного ресурсу на мапі.
    /// </summary>
    public class ResourceNode
    {
        public int Id;
        public ResourceType Type;
        public double X, Y;
        public int Amount, MaxAmount;
        public UIElement Visual;

        /// <summary>
        /// Повертає значення, яке вказує, чи є джерело ресурсу повністю вичерпаним.
        /// </summary>
        public bool IsDepleted => Amount <= 0;
    }

    /// <summary>
    /// Представляє статичну перешкоду, яку мурахи мають огинати.
    /// </summary>
    public class Obstacle
    {
        public int Id;
        public double X, Y, Width, Height;
        public UIElement Visual;

        /// <summary>
        /// Перевіряє, чи знаходяться задані координати всередині меж цієї перешкоди.
        /// </summary>
        public bool Contains(double x, double y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    /// <summary>
    /// Описує наукове дослідження (апгрейд), яке покращує характеристики колонії.
    /// </summary>
    public class Research
    {
        public string Name, Description;
        public int CostFood, CostWood, CostStone;

        /// <summary>
        /// Дія, яка застосовує ефект від дослідження до головного вікна гри.
        /// </summary>
        public Action<MainWindow> Apply;
    }
    /// <summary>
    /// Зберігає глобальну статистику поточної ігрової сесії для аналізу досягнень гравця.
    /// </summary>
    public class GameStatistics
    {
        public int TotalFoodCollected, TotalWoodCollected, TotalStoneCollected, TotalWaterCollected, AntsBorn, AntsDied, NestExpansions, DaysSurvived = 1;
        public TimeSpan PlayTime;
    }
    /// <summary>
    /// Контейнер для збереження та завантаження повного стану гри (серіалізація у JSON).
    /// </summary>
    public class SaveData
    {
        public string Version = "3.0";
        public DateTime SaveDate;
        public string SaveName;
        public int Food, Wood, Stone, Water, ColonyLevel, MaxAnts, NestSize, CurrentDay, CurrentHour, CurrentMinute, GameSpeed;
        public double NestX, NestY;
        public WeatherType Weather;
        public bool IsPaused;
        public AntSaveData[] Ants;
        public ResourceSaveData[] Resources;
        public ObstacleSaveData[] Obstacles;
        public string[] UnlockedResearch;
        public GameStatistics Statistics;
    }

    /// <summary>
    /// Полегшена структура даних мурахи для збереження у файл.
    /// </summary>
    public class AntSaveData
    {
        public int Id; public string Name;
        public double X, Y, Speed, Energy, Health, Age;
        public string State;
        public int GatheredFood, GatheredWood, GatheredStone, GatheredWater;
    }

    /// <summary>
    /// Полегшена структура даних вузла ресурсу для збереження у файл.
    /// </summary>
    public class ResourceSaveData
    {
        public int Id; public string Type;
        public double X, Y;
        public int Amount;
    }

    /// <summary>
    /// Структура даних перешкоди для збереження у файл.
    /// </summary>
    public class ObstacleSaveData
    {
        public int Id;
        public double X, Y, Width, Height;
    }

    /// <summary>
    /// Початкові налаштування складності гри, що вибираються перед стартом нової сесії.
    /// </summary>
    public class StartSettings
    {
        public int InitialAnts = 3, InitialFood = 100, InitialWood = 50, InitialStone = 25, InitialWater = 30, ObstacleCount = 5;
    }

    public partial class MainWindow : Window
    {
        int food, wood, stone, water, colonyLevel = 1, maxAnts = 10, nestSize = 150, currentDay = 1, currentHour = 8, currentMinute, gameSpeed = 1;
        double nestX = 425, nestY = 275;
        Ant[] ants = new Ant[0];
        ResourceNode[] resources = new ResourceNode[0];
        Obstacle[] obstacles = new Obstacle[0];
        Research[] researches = new Research[0];
        string[] unlockedResearch = new string[0];
        GameStatistics statistics = new GameStatistics();
        WeatherType currentWeather = WeatherType.Sunny;
        bool isPaused, isDarkTheme = true, isPlacingObstacle;
        double obstacleWidth = 60, obstacleHeight = 60;
        string currentSaveFile = "savegame.json";

       
        DispatcherTimer gameTimer, secondTimer, autoSaveTimer;
        Random random = new Random();
        StartSettings startSettings = new StartSettings();

       
        readonly Brush BgDark = new SolidColorBrush(Color.FromRgb(15, 20, 25));
        readonly Brush BgPanel = new SolidColorBrush(Color.FromRgb(26, 31, 46));
        readonly Brush BgCard = new SolidColorBrush(Color.FromRgb(37, 43, 61));
        readonly Brush BorderColor = new SolidColorBrush(Color.FromRgb(58, 65, 85));
        readonly Brush TextMain = new SolidColorBrush(Color.FromRgb(224, 230, 237));
        readonly Brush TextSub = new SolidColorBrush(Color.FromRgb(139, 149, 165));
        readonly Brush Accent = new SolidColorBrush(Color.FromRgb(0, 212, 170));
        readonly Brush Danger = new SolidColorBrush(Color.FromRgb(255, 71, 87));
        readonly Brush Warning = new SolidColorBrush(Color.FromRgb(255, 165, 2));

        /// <summary>
        /// Головний конструктор вікна гри.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                try
                {
                    if (File.Exists("Data/savegame.json") && MessageBox.Show("Знайдено автозбереження. Завантажити?", "Автозбереження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    { LoadGame("Data/savegame.json"); SetupTimers(); }
                    else ShowStartDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка запуску: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ShowStartDialog();
                }
            };
            Closing += (s, e) =>
            {
                var r = MessageBox.Show("Зберегти гру перед виходом?", "Вихід", MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.Yes) SaveGame();
                else if (r == MessageBoxResult.Cancel) e.Cancel = true;
            };
            InitResearches();
        }

        /// <summary>
        /// Ініціалізує доступне дерево досліджень та прописує лямбда-вирази для ефектів кожного апгрейду.
        /// </summary>
        void InitResearches()
        {
            researches = new Research[]
            {
                new Research { Name = "Швидкість +20%", Description = "Швидкість мурах", CostFood = 50, CostWood = 30, CostStone = 10, Apply = w => { foreach (var a in w.ants) a.BaseSpeed *= 1.2; } },
                new Research { Name = "Міцність +50%", Description = "Здоров'я мурах", CostFood = 30, CostWood = 50, CostStone = 20, Apply = w => { foreach (var a in w.ants) a.MaxHealth = 150; } },
                new Research { Name = "Автозбір", Description = "Швидше збирають", CostFood = 100, CostWood = 50, CostStone = 30 },
                new Research { Name = "Нічне бачення", Description = "Не сповільнюються вночі", CostFood = 40, CostWood = 20, CostStone = 40 },
                new Research { Name = "Подвійна місткість", Description = "Удвічі більше ресурсів", CostFood = 80, CostWood = 60, CostStone = 20 },
                new Research { Name = "Швидке відновлення", Description = "Енергія +50%", CostFood = 60, CostWood = 40, CostStone = 10 }
            };
        }

        /// <summary>
        /// Оновлює ресурси інтерфейсу відповідно до обраної теми програми (світла або темна) та перемальовує фон.
        /// </summary>
        void ApplyTheme()
        {
            var dict = this.Resources;
            if (isDarkTheme)
            {
                dict["BgDark"] = new SolidColorBrush(Color.FromRgb(15, 20, 25));
                dict["BgPanel"] = new SolidColorBrush(Color.FromRgb(26, 31, 46));
                dict["BgCard"] = new SolidColorBrush(Color.FromRgb(37, 43, 61));
                dict["BorderColor"] = new SolidColorBrush(Color.FromRgb(58, 65, 85));
                dict["TextMain"] = new SolidColorBrush(Color.FromRgb(224, 230, 237));
                dict["TextSub"] = new SolidColorBrush(Color.FromRgb(139, 149, 165));
                dict["Accent"] = new SolidColorBrush(Color.FromRgb(0, 212, 170));
                dict["Danger"] = new SolidColorBrush(Color.FromRgb(255, 71, 87));
                dict["Warning"] = new SolidColorBrush(Color.FromRgb(255, 165, 2));
                dict["Info"] = new SolidColorBrush(Color.FromRgb(55, 66, 250));
            }
            else
            {
                dict["BgDark"] = new SolidColorBrush(Color.FromRgb(245, 246, 250));
                dict["BgPanel"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                dict["BgCard"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                dict["BorderColor"] = new SolidColorBrush(Color.FromRgb(220, 221, 225));
                dict["TextMain"] = new SolidColorBrush(Color.FromRgb(47, 54, 64));
                dict["TextSub"] = new SolidColorBrush(Color.FromRgb(113, 128, 147));
                dict["Accent"] = new SolidColorBrush(Color.FromRgb(0, 184, 148));
                dict["Danger"] = new SolidColorBrush(Color.FromRgb(232, 65, 24));
                dict["Warning"] = new SolidColorBrush(Color.FromRgb(225, 177, 44));
                dict["Info"] = new SolidColorBrush(Color.FromRgb(39, 60, 117));
            }
            this.Background = (Brush)dict["BgDark"];
        }

        /// <summary>
        /// Відображає модальне діалогове вікно вибору стартових параметрів для створення нової гри.
        /// </summary>
        void ShowStartDialog()
        {
            var d = new Window { Title = "Нова гра", Width = 380, Height = 480, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, ResizeMode = ResizeMode.NoResize, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(24) };
            var s = new StartSettings();

            p.Children.Add(new TextBlock { Text = "Нова гра", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 20), HorizontalAlignment = HorizontalAlignment.Center });

            void F(string l, int v, Action<int> set)
            {
                p.Children.Add(new TextBlock { Text = l, Margin = new Thickness(0, 10, 0, 4), FontWeight = FontWeights.SemiBold, Foreground = TextSub });
                var sl = new Slider { Minimum = 0, Maximum = 50, Value = v, TickFrequency = 1, IsSnapToTickEnabled = true, Background = BgCard, Foreground = Accent };
                var t = new TextBlock { Text = v.ToString(), HorizontalAlignment = HorizontalAlignment.Right, Foreground = TextMain, FontWeight = FontWeights.Bold, FontSize = 16 };
                sl.ValueChanged += (_, e) => { int val = (int)e.NewValue; set(val); t.Text = val.ToString(); };
                p.Children.Add(sl); p.Children.Add(t);
            }
            F("Мурахи:", s.InitialAnts, v => s.InitialAnts = v);
            F("Їжа:", s.InitialFood, v => s.InitialFood = v);
            F("Деревина:", s.InitialWood, v => s.InitialWood = v);
            F("Камінь:", s.InitialStone, v => s.InitialStone = v);
            F("Вода:", s.InitialWater, v => s.InitialWater = v);
            F("Перешкоди:", s.ObstacleCount, v => s.ObstacleCount = v);

            var b = new Button { Content = "ПОЧАТИ ГРУ", Margin = new Thickness(0, 20, 0, 0), Padding = new Thickness(20, 12, 20, 12), Background = Accent, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 14 };
            b.Click += (_, __) => { startSettings = s; d.DialogResult = true; d.Close(); };
            p.Children.Add(b);
            d.Content = new ScrollViewer { Content = p, Background = BgDark };
            if (d.ShowDialog() == true) NewGame();
        }

        /// <summary>
        /// Скидає поточний стан симуляції та генерує новий світ на основі обраних початкових налаштувань.
        /// </summary>
        void NewGame()
        {
            food = startSettings.InitialFood;
            wood = startSettings.InitialWood;
            stone = startSettings.InitialStone;
            water = startSettings.InitialWater;
            colonyLevel = 1; maxAnts = 10; currentDay = 1; currentHour = 8; currentMinute = 0;
            statistics = new GameStatistics(); unlockedResearch = new string[0]; InitResearches();
            ants = new Ant[0]; resources = new ResourceNode[0]; obstacles = new Obstacle[0];
            GameCanvas.Children.Clear(); GameCanvas.Children.Add(NestGrid);

            for (int i = 0; i < startSettings.ObstacleCount; i++) CreateObstacle(random.Next(50, 700), random.Next(50, 500), random.Next(30, 100), random.Next(30, 100));
            for (int i = 0; i < startSettings.InitialAnts; i++) SpawnAnt(false);
            GenerateResources();
            SetupTimers();
            UpdateUI();
            AddLog("Нова гра розпочата!", Colors.White);
        }

        /// <summary>
        /// Конфігурує та запускає ігрові таймери: кадрову логіку (60 FPS), секундний розрахунок часу та циклічне автозбереження.
        /// </summary>
        void SetupTimers()
        {
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            gameTimer.Tick += GameLoop; gameTimer.Start();
            secondTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            secondTimer.Tick += SecondTick; secondTimer.Start();
            autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            autoSaveTimer.Tick += (_, __) => { SaveGame("savegame.json"); StatusText.Text = $"Автозбереження: {DateTime.Now:HH:mm:ss}"; };
            autoSaveTimer.Start();
        }

        /// <summary>
        /// Обробляє щосекундні події: витрати їжі/води, старіння або смерть мурах від голоду, випадкові знахідки та зміну доби.
        /// </summary>
        void SecondTick(object _, EventArgs __)
        {
            if (isPaused) return;
            if (++currentMinute >= 60) { currentMinute = 0; if (++currentHour >= 24) { currentHour = 0; currentDay++; statistics.DaysSurvived = currentDay; ChangeWeather(); } }
            int fc = Math.Max(1, AliveAnts() / 3), wc = Math.Max(1, AliveAnts() / 4);
            food = Math.Max(0, food - fc); water = Math.Max(0, water - wc);
            if (food == 0) foreach (var a in ants.Where(a => a.State != AntState.Dead).Take(1)) { a.Health -= 10; if (a.Health <= 0) KillAnt(a, "голод"); }
            foreach (var a in ants.Where(a => a.State != AntState.Dead)) { a.Age += 0.1; if (a.Age > 100 && random.Next(1000) < 5) KillAnt(a, "старість"); }
            if (startSettings.InitialFood > 0 && random.Next(100) < 5) { int b = random.Next(10, 30); food += b; AddLog($"Випадкова знахідка: +{b} їжі!", Colors.LightGreen); }
            UpdateUI();
        }

        /// <summary>
        /// Повертає загальну кількість живих мурах у колонії.
        /// </summary>
        int AliveAnts() => ants.Count(a => a.State != AntState.Dead);

        /// <summary>
        /// Випадковим чином змінює поточну погоду в симуляції та оновлює стилі елементів інтерфейсу погоди.
        /// </summary>
        void ChangeWeather()
        {
            var w = Enum.GetValues(typeof(WeatherType));
            currentWeather = (WeatherType)w.GetValue(random.Next(w.Length));
            string e = currentWeather == WeatherType.Sunny ? "Сонячно" : currentWeather == WeatherType.Rainy ? "Дощ" : currentWeather == WeatherType.Stormy ? "Шторм" : "Ніч";
            WeatherText.Text = $"{e} {currentWeather}";
            WeatherText.Foreground = currentWeather == WeatherType.Sunny ? new SolidColorBrush(Colors.Gold) : currentWeather == WeatherType.Night ? new SolidColorBrush(Colors.Silver) : new SolidColorBrush(Colors.LightBlue);
            AddLog($"Погода: {currentWeather}!", Colors.LightBlue);
        }

        /// <summary>
        /// Розраховує кількість та генерує на мапі початкові вузли ресурсів (їжа, дерево, камінь, вода) залежно від погоди.
        /// </summary>
        void GenerateResources()
        {
            if (startSettings.InitialFood == 0) return;
            int id = 1;
            int fc = Math.Min(20, startSettings.InitialFood / 10 + (currentWeather == WeatherType.Sunny ? 2 : 0));
            int wc = Math.Min(15, startSettings.InitialWood / 10);
            int sc = Math.Min(10, startSettings.InitialStone / 10);
            int wtc = Math.Min(12, startSettings.InitialWater / 10 + (currentWeather == WeatherType.Rainy ? 2 : 0));
            for (int i = 0; i < fc; i++) CreateResource(id++, ResourceType.Food, "\uD83C\uDF42", "#00d4aa", random.Next(20, 50));
            for (int i = 0; i < wc; i++) CreateResource(id++, ResourceType.Wood, "\uD83E\uDEB5", "#ff7675", random.Next(20, 50));
            for (int i = 0; i < sc; i++) CreateResource(id++, ResourceType.Stone, "\uD83E\uDEA8", "#b2bec3", random.Next(20, 50));
            for (int i = 0; i < wtc; i++) CreateResource(id++, ResourceType.Water, "\uD83D\uDCA7", "#74b9ff", random.Next(20, 50));
        }

        /// <summary>
        /// Створює візуальний круглий елемент ресурсу з емодзі та додає його на Canvas за випадковими координатами, уникаючи накладання на мурашник чи перешкоди.
        /// </summary>
        void CreateResource(int id, ResourceType type, string emoji, string color, int amount = -1)
        {
            double x, y; int t = 0;
            do { x = random.Next(100, 700); y = random.Next(100, 500); t++; }
            while ((Distance(x, y, nestX, nestY) < 180 || obstacles.Any(o => o.Contains(x, y))) && t < 50);
            int amt = amount > 0 ? amount : random.Next(30, 150);
            var n = new ResourceNode { Id = id, Type = type, X = x, Y = y, Amount = amt, MaxAmount = amt };
            var b = new Border { Width = 40, Height = 40, Background = (SolidColorBrush)new BrushConverter().ConvertFrom(color), CornerRadius = new CornerRadius(20), BorderBrush = Brushes.White, BorderThickness = new Thickness(2), Child = new TextBlock { Text = emoji, FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
            Canvas.SetLeft(b, x - 20); Canvas.SetTop(b, y - 20);
            GameCanvas.Children.Add(b); n.Visual = b; resources = resources.Concat(new[] { n }).ToArray();
        }

        /// <summary>
        /// Видаляє вузол ресурсу з мапи за допомогою плавної WPF анімації згасання та зменшення масштабу.
        /// </summary>
        void RemoveResource(ResourceNode n)
        {
            if (n == null || n.Visual == null) return;
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            anim.Completed += (_, __) => { if (n.Visual != null && GameCanvas.Children.Contains(n.Visual)) GameCanvas.Children.Remove(n.Visual); };
            n.Visual.BeginAnimation(OpacityProperty, anim);
            var sc = new ScaleTransform(1, 1);
            n.Visual.RenderTransform = sc; n.Visual.RenderTransformOrigin = new Point(0.5, 0.5);
            var sa = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            sc.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
            sc.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
            resources = resources.Where(r => r != n).ToArray();
        }

        /// <summary>
        /// Створює прямокутну статичну перешкоду на мапі та підписує її на подію видалення по правому кліку миші.
        /// </summary>
        void CreateObstacle(double x, double y, double w, double h)
        {
            if (Distance(x + w / 2, y + h / 2, nestX, nestY) < 200) return;
            int id = obstacles.Length > 0 ? obstacles.Max(o => o.Id) + 1 : 1;
            var r = new Rectangle { Width = w, Height = h, Fill = new SolidColorBrush(Color.FromRgb(80, 85, 100)), Stroke = new SolidColorBrush(Color.FromRgb(120, 125, 140)), StrokeThickness = 2, Cursor = Cursors.Hand, RadiusX = 8, RadiusY = 8 };
            r.MouseRightButtonDown += (_, __) => { GameCanvas.Children.Remove(r); obstacles = obstacles.Where(o => o.Id != id).ToArray(); AddLog("Перешкоду видалено", Colors.Orange); };
            Canvas.SetLeft(r, x); Canvas.SetTop(r, y);
            GameCanvas.Children.Add(r); obstacles = obstacles.Concat(new[] { new Obstacle { Id = id, X = x, Y = y, Width = w, Height = h, Visual = r } }).ToArray();
        }

        /// <summary>
        /// Розраховує евклідову відстань між двома точками на двовимірній площині.
        /// </summary>
        double Distance(double x1, double y1, double x2, double y2) => Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        /// <summary>
        /// Створює та спавнить нову мураху в грі, якщо не досягнуто ліміту макс. кількості.
        /// </summary>
        void SpawnAnt(bool stat = true)
        {
            if (AliveAnts() >= maxAnts) return;
            int id = ants.Length > 0 ? ants.Max(a => a.Id) + 1 : 1;
            var a = new Ant(id, nestX + random.Next(-60, 60), nestY + random.Next(-60, 60));
            a.Visual = CreateAntVisual();
            a.RotateTransform = new RotateTransform(0);
            a.Visual.RenderTransform = a.RotateTransform; a.Visual.RenderTransformOrigin = new Point(0.5, 0.5);
            a.Visual.MouseLeftButtonDown += (_, __) => ShowAntDialog(a);
            Canvas.SetLeft(a.Visual, a.X - 16); Canvas.SetTop(a.Visual, a.Y - 16);
            GameCanvas.Children.Add(a.Visual); ants = ants.Concat(new[] { a }).ToArray();
            if (stat) { statistics.AntsBorn++; AddLog($"{a.Name} народилася!", Colors.LightYellow); }
        }

        /// <summary>
        /// Створює візуальне відображення (рендеринг) мурахи за допомогою WPF елементів Canvas, Ellipse та Line.
        /// </summary>
        /// <returns>Об'єкт Canvas, що містить усі деталі тіла та лапок мурахи.</returns>
        Canvas CreateAntVisual()
        {
            var c = new Canvas { Width = 32, Height = 32 };
            var lb = new SolidColorBrush(Color.FromRgb(180, 170, 160));
            var legs = new[] {
            new Line { X1 = 8, Y1 = 20, X2 = 3, Y2 = 26, Stroke = lb, StrokeThickness = 1.8 },
            new Line { X1 = 24, Y1 = 20, X2 = 29, Y2 = 26, Stroke = lb, StrokeThickness = 1.8 },
            new Line { X1 = 7, Y1 = 16, X2 = 2, Y2 = 16, Stroke = lb, StrokeThickness = 1.6 },
            new Line { X1 = 25, Y1 = 16, X2 = 30, Y2 = 16, Stroke = lb, StrokeThickness = 1.6 },
            new Line { X1 = 8, Y1 = 12, X2 = 3, Y2 = 6, Stroke = lb, StrokeThickness = 1.5 },
            new Line { X1 = 24, Y1 = 12, X2 = 29, Y2 = 6, Stroke = lb, StrokeThickness = 1.5 }
        };
            foreach (var l in legs) { l.StrokeStartLineCap = PenLineCap.Round; l.StrokeEndLineCap = PenLineCap.Round; }
            var ab = new Ellipse { Width = 15, Height = 17, Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromRgb(80, 70, 60), 0), new GradientStop(Color.FromRgb(50, 44, 38), 0.5), new GradientStop(Color.FromRgb(30, 26, 22), 1) }, new Point(0, 0), new Point(0, 1)) };
            Canvas.SetLeft(ab, 8.5); Canvas.SetTop(ab, 11);
            var th = new Ellipse { Width = 11, Height = 13, Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromRgb(70, 62, 54), 0), new GradientStop(Color.FromRgb(40, 35, 30), 1) }, new Point(0, 0), new Point(0, 1)) };
            Canvas.SetLeft(th, 10.5); Canvas.SetTop(th, 3);
            var hd = new Ellipse { Width = 11, Height = 11, Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromRgb(90, 82, 74), 0), new GradientStop(Color.FromRgb(50, 44, 38), 1) }, new Point(0, 0), new Point(0, 1)) };
            Canvas.SetLeft(hd, 10.5); Canvas.SetTop(hd, -6);
            var eb = new SolidColorBrush(Color.FromRgb(60, 55, 50));
            var la = new System.Windows.Shapes.Path { Data = Geometry.Parse("M 12,-4 Q 6,-12 4,-10"), Stroke = eb, StrokeThickness = 1.2, StrokeStartLineCap = PenLineCap.Round };
            var ra = new System.Windows.Shapes.Path { Data = Geometry.Parse("M 20,-4 Q 26,-12 28,-10"), Stroke = eb, StrokeThickness = 1.2, StrokeStartLineCap = PenLineCap.Round };
            c.Children.Add(new Ellipse { Width = 22, Height = 7, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)) });
            foreach (var l in legs) c.Children.Add(l);
            c.Children.Add(ab); c.Children.Add(th); c.Children.Add(hd); c.Children.Add(la); c.Children.Add(ra);
            return c;
        }

        /// <summary>
        /// Основний ігровий цикл, що викликається таймером. Оновлює стан усіх живих мурах відповідно до швидкості гри.
        /// </summary>
        void GameLoop(object _, EventArgs __)
        {
            if (isPaused) return;
            int it = Math.Min(gameSpeed, 100);
            for (int i = 0; i < it; i++)
                foreach (var a in ants.Where(a => a.State != AntState.Dead))
                { ProcessAnt(a); UpdateAntVisual(a); }
        }

        /// <summary>
        /// Обробляє поточну поведінку мурахи (машину станів) та витрати її енергії.
        /// </summary>
       
        void ProcessAnt(Ant a)
        {
            switch (a.State)
            {
                case AntState.Idle: if (random.Next(100) < 3) FindTask(a); break;
                case AntState.Moving: MoveAnt(a); break;
                case AntState.Gathering: Gather(a); break;
                case AntState.Returning: Return(a); break;
                case AntState.Resting: Rest(a); break;
            }
            if (a.State != AntState.Idle && a.State != AntState.Resting) a.Energy -= 0.05;
            if (a.Energy <= 0 && a.State != AntState.Resting) { a.State = AntState.Returning; a.TargetX = nestX; a.TargetY = nestY; }
        }

        /// <summary>
        /// Визначає наступну задачу для мурахи на основі поточних потреб колонії в ресурсах або відправляє її на випадкову розвідку.
        /// </summary>
        void FindTask(Ant a)
        {
            var av = resources.Where(r => !r.IsDepleted).ToArray();
            ResourceType? p = food < 50 ? ResourceType.Food : water < 20 ? ResourceType.Water : null;
            if (p.HasValue && av.Any(r => r.Type == p.Value) && random.Next(100) < 70)
            { SetTarget(a, av.Where(r => r.Type == p.Value).OrderBy(r => Distance(a.X, a.Y, r.X, r.Y)).First()); return; }
            if (av.Any() && random.Next(100) < 80) SetTarget(a, av.OrderBy(r => Distance(a.X, a.Y, r.X, r.Y)).First());
            else { double tx, ty; int t = 0; do { tx = random.Next(100, 750); ty = random.Next(100, 550); t++; } while (obstacles.Any(o => o.Contains(tx, ty)) && t < 20); a.TargetX = tx; a.TargetY = ty; a.State = AntState.Moving; }
        }

        /// <summary>
        /// Встановлює обраний ресурсний вузол як ціль для мурахи та переводить її у стан руху.
        /// </summary>
        void SetTarget(Ant a, ResourceNode r) { a.TargetResource = r; a.TargetX = r.X; a.TargetY = r.Y; a.State = AntState.Moving; }

        /// <summary>
        /// Переміщує мураху до її поточної цілі з урахуванням погоди, досліджених технологій та оминанням перешкод.
        /// </summary>
        void MoveAnt(Ant a)
        {
            double dx = a.TargetX - a.X, dy = a.TargetY - a.Y, d = Math.Sqrt(dx * dx + dy * dy);
            double sm = currentWeather == WeatherType.Rainy ? 0.8 : currentWeather == WeatherType.Stormy ? 0.6 : currentWeather == WeatherType.Night && !unlockedResearch.Contains("Нічне бачення") ? 0.7 : 1.0;
            if (d < 8) { if (a.TargetResource != null && a.State == AntState.Moving) a.State = AntState.Gathering; else if (a.State == AntState.Returning && d < 25) a.State = AntState.Resting; else { a.State = AntState.Idle; a.TargetResource = null; } return; }
            double nx = a.X + dx / d * a.Speed * sm, ny = a.Y + dy / d * a.Speed * sm;
            if (!obstacles.Any(o => o.Contains(nx, ny))) { a.X = nx; a.Y = ny; }
            else if (!obstacles.Any(o => o.Contains(nx, a.Y))) a.X = nx;
            else if (!obstacles.Any(o => o.Contains(a.X, ny))) a.Y = ny;
            else { a.State = AntState.Idle; a.TargetResource = null; }
        }

        /// <summary>
        /// Виконує збір ресурсу мурахою з вузла, враховує бонуси досліджень та оновлює карту, якщо ресурс вичерпано.
        /// </summary>
        void Gather(Ant a)
        {
            if (a.TargetResource == null || a.TargetResource.IsDepleted) { a.State = AntState.Idle; return; }
            int amt = Math.Min(unlockedResearch.Contains("Подвійна місткість") ? 16 : 8, a.TargetResource.Amount);
            a.TargetResource.Amount -= amt; a.CarryingAmount = amt; a.CarryingType = a.TargetResource.Type;
            switch (a.CarryingType) { case ResourceType.Food: a.GatheredFood += amt; break; case ResourceType.Wood: a.GatheredWood += amt; break; case ResourceType.Stone: a.GatheredStone += amt; break; case ResourceType.Water: a.GatheredWater += amt; break; }
            if (a.TargetResource.IsDepleted)
            {
                var dr = a.TargetResource; var t = a.CarryingType.Value;
                RemoveResource(dr);
                string[] info = t switch { ResourceType.Food => new[] { "\uD83C\uDF42", "#00d4aa" }, ResourceType.Wood => new[] { "\uD83E\uDEB5", "#ff7675" }, ResourceType.Stone => new[] { "\uD83E\uDEA8", "#b2bec3" }, ResourceType.Water => new[] { "\uD83D\uDCA7", "#74b9ff" }, _ => new[] { "❓", "gray" } };
                Dispatcher.BeginInvoke(new Action(() => { if (resources.Length < 50) CreateResource(resources.Length > 0 ? resources.Max(r => r.Id) + 1 : 1, t, info[0], info[1]); }), DispatcherPriority.Background);
            }
            a.TargetX = nestX; a.TargetY = nestY; a.State = AntState.Returning;
        }

        /// <summary>
        /// Керує поверненням мурахи до гнізда для розвантаження зібраних ресурсів.
        /// </summary>
        /// <param name="a">Мураха, яка повертається до мурашника.</param>
        void Return(Ant a)
        {
            double dx = a.TargetX - a.X, dy = a.TargetY - a.Y, d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 25)
            {
                switch (a.CarryingType) { case ResourceType.Food: food += a.CarryingAmount; statistics.TotalFoodCollected += a.CarryingAmount; break; case ResourceType.Wood: wood += a.CarryingAmount; statistics.TotalWoodCollected += a.CarryingAmount; break; case ResourceType.Stone: stone += a.CarryingAmount; statistics.TotalStoneCollected += a.CarryingAmount; break; case ResourceType.Water: water += a.CarryingAmount; statistics.TotalWaterCollected += a.CarryingAmount; break; }
                a.CarryingAmount = 0; a.CarryingType = null; a.Energy = Math.Min(100, a.Energy + 40); a.State = AntState.Resting; UpdateUI();
            }
            else
            {
                double nx = a.X + dx / d * a.Speed * (a.CarryingAmount > 0 ? 1.2 : 1.0), ny = a.Y + dy / d * a.Speed * (a.CarryingAmount > 0 ? 1.2 : 1.0);
                if (!obstacles.Any(o => o.Contains(nx, ny))) { a.X = nx; a.Y = ny; }
                else if (!obstacles.Any(o => o.Contains(nx, a.Y))) a.X = nx;
                else if (!obstacles.Any(o => o.Contains(a.X, ny))) a.Y = ny;
            }
        }

        /// <summary>
        /// Відновлює здоров'я та енергію мурахи під час відпочинку в мурашнику.
        /// </summary>
        void Rest(Ant a) { a.Energy = Math.Min(100, a.Energy + (unlockedResearch.Contains("Швидке відновлення") ? 3 : 2)); 
        a.Health = Math.Min(a.MaxHealth, a.Health + 1); if (a.Energy >= 95) a.State = AntState.Idle; }
        /// <summary>
        /// Переводить мураху у стан смерті, знижує її прозорість на екрані та чергує видалення з Canvas через 5 секунд.
        /// </summary>
        void KillAnt(Ant a, string r)
        {
            a.State = AntState.Dead; a.Visual.Opacity = 0.3; statistics.AntsDied++;
            AddLog($"{a.Name} померла ({r})", Colors.Red);
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            t.Tick += (_, __) => { GameCanvas.Children.Remove(a.Visual); ants = ants.Where(x => x != a).ToArray(); t.Stop(); };
            t.Start();
        }

        /// <summary>
        /// Оновлює координати Canvas та плавно повертає кут нахилу текстури мурахи у напрямку її руху.
        /// </summary>
        /// <param name="a">Мураха, чий візуальний стан оновлюється.</param>
        void UpdateAntVisual(Ant a)
        {
            if (a.Visual == null) return;
            Canvas.SetLeft(a.Visual, a.X - 16); Canvas.SetTop(a.Visual, a.Y - 16);
            double dx = a.TargetX - a.X, dy = a.TargetY - a.Y, ang = Math.Atan2(dy, dx) * 180 / Math.PI + 90;
            double c = a.RotateTransform.Angle, df = ang - c;
            while (df > 180) df -= 360; while (df < -180) df += 360;
            a.RotateTransform.Angle = c + df * 0.1;
            a.Visual.Opacity = a.Energy < 20 ? 0.7 : 1.0;
        }

        /// <summary>
        /// Оновлює текстові поля інтерфейсу користувача (UI), прогрес-бари та стан кнопок дій.
        /// </summary>
        void UpdateUI()
        {
            FoodText.Text = food.ToString(); WoodText.Text = wood.ToString(); StoneText.Text = stone.ToString(); WaterText.Text = water.ToString();
            int alive = AliveAnts(), working = ants.Count(a => a.State == AntState.Moving || a.State == AntState.Gathering);
            FoodRateText.Text = $"+{alive / 3}/с"; WoodRateText.Text = $"+{alive / 4}/с"; StoneRateText.Text = $"+{alive / 6}/с"; WaterRateText.Text = $"+{alive / 5}/с";
            AntsText.Text = $"{alive}/{maxAnts}"; AntsWorkingText.Text = $"{working} працюють"; LevelText.Text = colonyLevel.ToString();
            LevelProgress.Value = Math.Min(100, ((food / 10 + wood / 5 + stone / 3) * 100) / (colonyLevel * 100));
            DayText.Text = $"День {currentDay}"; TimeText.Text = $"{currentHour:D2}:{currentMinute:D2}";
            TotalFoodText.Text = $"\uD83C\uDF42 {statistics.TotalFoodCollected}"; TotalWoodText.Text = $"\uD83E\uDEB5 {statistics.TotalWoodCollected}"; TotalStoneText.Text = $"\uD83E\uDEA8 {statistics.TotalStoneCollected}"; TotalWaterText.Text = $"\uD83D\uDCA7 {statistics.TotalWaterCollected}";
            AntsBornText.Text = $"\uD83E\uDD23 {statistics.AntsBorn}"; AntsDiedText.Text = $"\uD83D\uDC80 {statistics.AntsDied}"; NestExpansionsText.Text = $"\uD83C\uDFD7\uFE0F {statistics.NestExpansions}"; DaysSurvivedText.Text = $"\uD83D\uDCC5 {statistics.DaysSurvived}";
            SpawnAntBtn.IsEnabled = food >= 10 && water >= 5 && alive < maxAnts; ExpandNestBtn.IsEnabled = wood >= 50 && stone >= 25;
            if (ants.Any()) { var f = ants.First(); CoordsText.Text = $"X: {f.X:F0} | Y: {f.Y:F0}"; }
        }

        /// <summary>
        /// Додає новий рядок у текстовий лог подій гри з часовою міткою та заданим кольором тексту.
        /// </summary>
        void AddLog(string m, Color c)
        {
            var b = new Border { Background = BgCard, CornerRadius = new CornerRadius(10), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 6), BorderBrush = BorderColor, BorderThickness = new Thickness(1) };
            b.Child = new TextBlock { Text = $"[{DateTime.Now:HH:mm}] {m}", Foreground = new SolidColorBrush(c), FontSize = 12, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold };
            EventLog.Children.Insert(0, b);
            if (EventLog.Children.Count > 50) EventLog.Children.RemoveAt(50);
        }

        /// <summary>
        /// Показує спливаюче сповіщення у грі на 3 секунди, після чого приховує його.
        /// </summary>
        void ShowNotify(string i, string m)
        {
            NotificationIcon.Text = i;
            NotificationText.Text = m;
            NotificationPanel.Visibility = Visibility.Visible;
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            t.Tick += (_, __) => { NotificationPanel.Visibility = Visibility.Collapsed; t.Stop(); };
            t.Start();
        }

        /// <summary>
        /// Відкриває діалогове вікно з детальною інформацією про стан, характеристики та здобутки конкретної мурахи.
        /// </summary>
        void ShowAntDialog(Ant a)
        {
            var w = new Window { Title = a.Name, Width = 320, Height = 380, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(20) };
            p.Children.Add(new TextBlock { Text = $"{a.Name}", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 16) });
            p.Children.Add(new TextBlock { Text = $"Стан: {a.State}\nЕнергія: {a.Energy:F1}%\nЗдоров'я: {a.Health:F1}%\nВік: {a.Age:F1}\nШвидкість: {a.Speed:F2}\n\nЗібрано:\nЇжа {a.GatheredFood}  Деревина {a.GatheredWood}\nКамінь {a.GatheredStone}  Вода {a.GatheredWater}", FontSize = 13, TextWrapping = TextWrapping.Wrap, Foreground = TextMain, LineHeight = 22 });
            w.Content = new ScrollViewer { Content = p, Background = BgDark }; w.ShowDialog();
        }

        /// <summary>
        /// Створює та відображає кастомне діалогове вікно зі списком елементів у ScrollViewer.
        /// </summary>
        void ShowListDialog(string title, IEnumerable<string> items)
        {
            var w = new Window { Title = title, Width = 380, Height = 500, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(20) };
            p.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 16), HorizontalAlignment = HorizontalAlignment.Center });
            foreach (var item in items) p.Children.Add(new Border { Background = BgCard, CornerRadius = new CornerRadius(10), Padding = new Thickness(14), Margin = new Thickness(0, 0, 0, 8), BorderBrush = BorderColor, BorderThickness = new Thickness(1), Child = new TextBlock { Text = item, TextWrapping = TextWrapping.Wrap, Foreground = TextMain } });
            w.Content = new ScrollViewer { Content = p, Background = BgDark }; w.ShowDialog();
        }

        /// <summary>
        /// Обробник кліку: Витрачає ресурси та створює нову мураху вручну.
        /// </summary>
        void SpawnAntBtn_Click(object _, RoutedEventArgs __) { if (food >= 10 && water >= 5) { food -= 10; water -= 5; SpawnAnt(); UpdateUI(); } }

        /// <summary>
        /// Обробник кліку: Розширює мурашник, підвищує ліміт мурах та оновлює радіус гнізда на екрані.
        /// </summary>
        void ExpandNestBtn_Click(object _, RoutedEventArgs __) { if (wood >= 50 && stone >= 25) { wood -= 50; stone -= 25; colonyLevel++; maxAnts += 5; nestSize += 20; statistics.NestExpansions++; (NestGrid.Children[0] as Ellipse).Width = nestSize; (NestGrid.Children[0] as Ellipse).Height = nestSize; ShowNotify("\uD83C\uDFD7\uFE0F", $"Рівень {colonyLevel}!"); AddLog($"Гніздо рівень {colonyLevel}!", Colors.Gold); UpdateUI(); } }

        /// <summary>
        /// Обробники кліків: Ручний збір відповідних ресурсів гравцем залежно від кількості живих мурах.
        /// </summary>
        void GatherFoodBtn_Click(object _, RoutedEventArgs __) { if (startSettings.InitialFood == 0) { ShowNotify("\u26A0", "Ресурси відсутні!"); return; } int a = random.Next(20, 50) * AliveAnts(); food += a; statistics.TotalFoodCollected += a; ShowNotify("\uD83C\uDF42 Їжа", $"+{a} їжі!"); UpdateUI(); }
        void GatherWoodBtn_Click(object _, RoutedEventArgs __) { if (startSettings.InitialFood == 0) { ShowNotify("\u26A0", "Ресурси відсутні!"); return; } int a = random.Next(10, 30) * AliveAnts(); wood += a; statistics.TotalWoodCollected += a; ShowNotify("\uD83E\uDEB5 Деревина", $"+{a} деревини!"); UpdateUI(); }
        void GatherStoneBtn_Click(object _, RoutedEventArgs __) { if (startSettings.InitialFood == 0) { ShowNotify("\u26A0", "Ресурси відсутні!"); return; } int a = random.Next(5, 20) * AliveAnts(); stone += a; statistics.TotalStoneCollected += a; ShowNotify("\uD83E\uDEA8 Камінь", $"+{a} каменю!"); UpdateUI(); }
        void GatherWaterBtn_Click(object _, RoutedEventArgs __) { if (startSettings.InitialFood == 0) { ShowNotify("\u26A0", "Ресурси відсутні!"); return; } int a = random.Next(8, 25) * AliveAnts(); water += a; statistics.TotalWaterCollected += a; ShowNotify("\uD83D\uDCA7 Вода", $"+{a} води!"); UpdateUI(); }

        /// <summary>
        /// Обробник кліку: Примусово вбиває останню живу мураху зі списку (жертвопринесення).
        /// </summary>
        void KillAntBtn_Click(object _, RoutedEventArgs __) { var alive = ants.Where(a => a.State != AntState.Dead).ToArray(); if (alive.Any()) { KillAnt(alive.Last(), "жертва"); UpdateUI(); } }

        /// <summary>
        /// Обробник кліку: Ставить гру на паузу або знімає з неї, змінюючи текст статусу.
        /// </summary>
        void PauseBtn_Click(object _, RoutedEventArgs __) { isPaused = !isPaused; PauseBtn.Content = isPaused ? "\u25B6" : "\u23F8"; StatusText.Text = isPaused ? "Пауза" : "Гра активна"; StatusText.Foreground = isPaused ? Danger : Accent; }

        /// <summary>
        /// Обробник кліку: Циклічно змінює швидкість гри між x1, x10, x100 та x1000.
        /// </summary>
        void SpeedBtn_Click(object _, RoutedEventArgs __) { gameSpeed = gameSpeed == 1 ? 10 : gameSpeed == 10 ? 100 : gameSpeed == 100 ? 1000 : 1; SpeedBtn.Content = $"x{gameSpeed}"; AddLog($"Швидкість x{gameSpeed}", Colors.Gold); }

        /// <summary>
        /// Обробники меню: Збереження, завантаження, вихід та виклик вікон загальної статистики колонії.
        /// </summary>
        void QuickSaveBtn_Click(object _, RoutedEventArgs __) => SaveGame();
        void MenuSave_Click(object _, RoutedEventArgs __) => ShowSaveDialog(true);
        void MenuLoad_Click(object _, RoutedEventArgs __) => ShowSaveDialog(false);
        void MenuNewSave_Click(object _, RoutedEventArgs __) { if (MessageBox.Show("Новий запис? Прогрес втрачено!", "Новий запис", MessageBoxButton.YesNo) == MessageBoxResult.Yes) ShowStartDialog(); }
        void MenuSettings_Click(object _, RoutedEventArgs __) { }
        void MenuExit_Click(object _, RoutedEventArgs __) => Close();
        void MenuStatistics_Click(object _, RoutedEventArgs __) => ShowListDialog("Статистика", new[] { $"\uD83C\uDF42 {statistics.TotalFoodCollected}", $"\uD83E\uDEB5 {statistics.TotalWoodCollected}", $"\uD83E\uDEA8 {statistics.TotalStoneCollected}", $"\uD83D\uDCA7 {statistics.TotalWaterCollected}", $"Народилось: {statistics.AntsBorn}", $"Померло: {statistics.AntsDied}", $"Розширень: {statistics.NestExpansions}", $"Днів: {statistics.DaysSurvived}" });
        void MenuAnts_Click(object _, RoutedEventArgs __) => ShowListDialog($"Мурахи ({AliveAnts()} живих)", ants.Where(a => a.State != AntState.Dead).Select(a => $"{a.Name} | {a.State} | Енергія: {a.Energy:F0}% | Їжа{a.GatheredFood} Деревина{a.GatheredWood}"));

        /// <summary>
        /// Відкриває інтерактивне вікно дерева технологій (досліджень), дозволяючи купувати поліпшення за ресурси.
        /// </summary>
        void MenuResearch_Click(object _, RoutedEventArgs __)
        {
            var w = new Window { Title = "Дослідження", Width = 420, Height = 520, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(20) };
            p.Children.Add(new TextBlock { Text = "ДОСЛІДЖЕННЯ", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 16), HorizontalAlignment = HorizontalAlignment.Center });
            foreach (var r in researches)
            {
                bool u = unlockedResearch.Contains(r.Name);
                var b = new Border { Background = u ? new SolidColorBrush(Color.FromRgb(0, 120, 90)) : BgCard, CornerRadius = new CornerRadius(12), Padding = new Thickness(16), Margin = new Thickness(0, 0, 0, 10), BorderBrush = u ? Accent : BorderColor, BorderThickness = new Thickness(2) };
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = (u ? "\u2705 " : "\uD83D\uDD12 ") + r.Name, FontWeight = FontWeights.Bold, Foreground = Brushes.White, FontSize = 14 });
                sp.Children.Add(new TextBlock { Text = r.Description, Foreground = TextSub, FontSize = 11, Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap });
                sp.Children.Add(new TextBlock { Text = $"Вартість \uD83C\uDF42{r.CostFood} \uD83E\uDEB5{r.CostWood} \uD83E\uDEA8{r.CostStone}", Foreground = Warning, FontSize = 11, Margin = new Thickness(0, 6, 0, 0) });
                if (!u)
                {
                    var btn = new Button { Content = "Дослідити", Margin = new Thickness(0, 8, 0, 0), Padding = new Thickness(16, 8, 16, 8), Background = Accent, Foreground = Brushes.White, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
                    bool can = food >= r.CostFood && wood >= r.CostWood && stone >= r.CostStone;
                    btn.IsEnabled = can; if (!can) btn.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                    var research = r;
                    btn.Click += (_, __) => { if (food >= research.CostFood && wood >= research.CostWood && stone >= research.CostStone) { food -= research.CostFood; wood -= research.CostWood; stone -= research.CostStone; unlockedResearch = unlockedResearch.Concat(new[] { research.Name }).ToArray(); research.Apply?.Invoke(this); ShowNotify("\uD83D\uDD2C", $"Досліджено: {research.Name}!"); AddLog($"{research.Name}!", Colors.Gold); UpdateUI(); w.Close(); MenuResearch_Click(null, null); } };
                    sp.Children.Add(btn);
                }
                else sp.Children.Add(new TextBlock { Text = "Готово", Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 150)), FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Right });
                b.Child = sp; p.Children.Add(b);
            }
            w.Content = new ScrollViewer { Content = p, Background = BgDark }; w.ShowDialog();
        }
        /// <summary>
        /// Відкриває модальне вікно керування перешкодами, де можна налаштувати розміри, 
        /// увімкнути режим малювання або повністю очистити ігрове поле.
        /// </summary>
        void MenuObstacles_Click(object _, RoutedEventArgs __)
        {
            var w = new Window { Title = "Перешкоди", Width = 320, Height = 340, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(20) };
            p.Children.Add(new TextBlock { Text = "ПЕРЕШКОДИ", FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 12), HorizontalAlignment = HorizontalAlignment.Center });
            p.Children.Add(new TextBlock { Text = $"Перешкод: {obstacles.Length}\n\nКлік на полі — додати\nПКМ на перешкоді — видалити", Foreground = TextSub, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 16), LineHeight = 20 });

            /// <summary>
            /// Локальна функція для швидкого створення повзунка (Slider) із числовим індикатором значення.
            /// </summary>
            void S(string l, double v, Action<double> set)
            {
                p.Children.Add(new TextBlock { Text = l, Margin = new Thickness(0, 8, 0, 3), Foreground = TextSub, FontWeight = FontWeights.SemiBold });
                var sl = new Slider { Minimum = 20, Maximum = 150, Value = v, TickFrequency = 10, IsSnapToTickEnabled = true, Background = BgCard, Foreground = Accent };
                var t = new TextBlock { Text = v.ToString("F0"), HorizontalAlignment = HorizontalAlignment.Right, Foreground = TextMain, FontWeight = FontWeights.Bold };
                sl.ValueChanged += (_, e) => { set(e.NewValue); t.Text = e.NewValue.ToString("F0"); };
                p.Children.Add(sl); p.Children.Add(t);
            }
            S("Ширина:", obstacleWidth, v => obstacleWidth = v); S("Висота:", obstacleHeight, v => obstacleHeight = v);
            var tb = new Button { Content = isPlacingObstacle ? "Скасувати" : "Додати", Margin = new Thickness(0, 16, 0, 0), Padding = new Thickness(16, 10, 16, 10), Background = isPlacingObstacle ? Danger : Accent, Foreground = Brushes.White, FontWeight = FontWeights.Bold };
            tb.Click += (_, __) => { isPlacingObstacle = !isPlacingObstacle; w.Close(); AddLog(isPlacingObstacle ? "Режим додавання" : "Режим вимкнено", isPlacingObstacle ? Colors.Yellow : Colors.LightGray); };
            p.Children.Add(tb);
            var cb = new Button { Content = "Видалити всі", Margin = new Thickness(0, 10, 0, 0), Padding = new Thickness(12, 8, 12, 8), Background = Danger, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold };
            cb.Click += (_, __) => { if (MessageBox.Show("Видалити ВСІ?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { foreach (var o in obstacles) GameCanvas.Children.Remove(o.Visual); obstacles = new Obstacle[0]; AddLog("Всі видалено", Colors.Orange); w.Close(); } };
            p.Children.Add(cb); w.Content = p; w.ShowDialog();
        }

        /// <summary>
        /// Відкриває діалогове вікно "Довідка" з описом гарячих клавіш, призначення ресурсів та механік гри.
        /// </summary>
        void MenuHelp_Click(object _, RoutedEventArgs __)
        {
            var w = new Window { Title = "Довідка", Width = 480, Height = 520, WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = BgDark };
            w.Content = new ScrollViewer { Content = new TextBlock { Text = "Керування:\nЛКМ на мурасі — деталі\nПКМ на перешкоді — видалити\nКлік на полі (режим перешкод) — додати\n\nРесурси:\n\uD83C\uDF42 Їжа — для мурах\n\uD83E\uDEB5 Деревина/\uD83E\uDEA8 Камінь — для гнізда\n\uD83D\uDCA7 Вода — для мурах\n\nМурахи автоматично збирають ресурси\n\nШвидкість: x1, x10, x100, x1000", TextWrapping = TextWrapping.Wrap, FontSize = 14, Margin = new Thickness(24), Foreground = TextMain, LineHeight = 26 }, Background = BgDark }; w.ShowDialog();
        }

        /// <summary>
        /// Перемикає візуальне оформлення гри між світлою та темною темами та оновлює іконку на кнопці.
        /// </summary>
        void ThemeToggleButton_Click(object _, RoutedEventArgs __)
        {
            isDarkTheme = !isDarkTheme;
            ThemeBtn.Content = isDarkTheme ? "\uD83C\uDF19" : "\u2600\uFE0F";
            ApplyTheme();
            AddLog(isDarkTheme ? "Темна тема" : "Світла тема", Colors.Gold);
        }

        /// <summary>
        /// Обробник натискання миші на ігровому полі. Створює нову перешкоду, якщо увімкнено відповідний режим 
        /// і точка кліку знаходиться на безпечній відстані від мурашника.
        /// </summary>
        void GameCanvas_PlaceObstacle(object _, MouseButtonEventArgs e)
        {
            if (!isPlacingObstacle) return;
            var pos = e.GetPosition(GameCanvas);
            if (Distance(pos.X, pos.Y, nestX, nestY) < 150) { AddLog("Занадто близько!", Colors.Red); return; }
            CreateObstacle(pos.X - obstacleWidth / 2, pos.Y - obstacleHeight / 2, obstacleWidth, obstacleHeight);
            AddLog("Перешкоду додано", Colors.Yellow);
        }

        /// <summary>
        /// Серіалізує поточний стан гри (ресурси, мурах, перешкоди, прогрес) у формат JSON 
        /// та зберігає файл у папку Data разом зі створенням резервної копії (backup).
        /// </summary>
        public void SaveGame(string f = null)
        {
            string p = f ?? currentSaveFile;
            if (!p.StartsWith("Data/")) p = "Data/" + p;
            var d = new SaveData
            {
                SaveDate = DateTime.Now,
                SaveName = System.IO.Path.GetFileNameWithoutExtension(p),
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
                Ants = ants.Where(a => a.State != AntState.Dead).Select(a => new AntSaveData { Id = a.Id, Name = a.Name, X = a.X, Y = a.Y, Speed = a.Speed, State = a.State.ToString(), Energy = a.Energy, Health = a.Health, Age = a.Age, GatheredFood = a.GatheredFood, GatheredWood = a.GatheredWood, GatheredStone = a.GatheredStone, GatheredWater = a.GatheredWater }).ToArray(),
                Resources = resources.Select(r => new ResourceSaveData { Id = r.Id, Type = r.Type.ToString(), X = r.X, Y = r.Y, Amount = r.Amount }).ToArray(),
                Obstacles = obstacles.Select(o => new ObstacleSaveData { Id = o.Id, X = o.X, Y = o.Y, Width = o.Width, Height = o.Height }).ToArray(),
                UnlockedResearch = unlockedResearch,
                Statistics = statistics
            };
            var o = new JsonSerializerOptions { WriteIndented = true };
            string j = JsonSerializer.Serialize(d, o);
            File.WriteAllText(p, j);
            Directory.CreateDirectory("Data/backups");
            File.WriteAllText($"Data/backups/{System.IO.Path.GetFileNameWithoutExtension(p)}_{DateTime.Now:yyyyMMdd_HHmmss}.json", j);
            ShowNotify("\uD83D\uDCBE", $"Збережено: {d.SaveName}"); AddLog($"{p}", Colors.LightGreen);
        }
        /// <summary>
        /// Завантажує стан гри з JSON-файлу збереження. Відновлює ресурси, мурах, перешкоди,
        /// статистику та всі параметри симуляції з вказаного файлу.
        /// </summary>
        public void LoadGame(string f)
        {
            if (!File.Exists(f)) return;
            var d = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(f), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            food = d.Food; wood = d.Wood; stone = d.Stone; water = d.Water;
            colonyLevel = d.ColonyLevel; maxAnts = d.MaxAnts;
            nestX = d.NestX; nestY = d.NestY; nestSize = d.NestSize;
            currentDay = d.CurrentDay; currentHour = d.CurrentHour; currentMinute = d.CurrentMinute;
            currentWeather = d.Weather; isPaused = d.IsPaused; gameSpeed = d.GameSpeed;

            statistics = d.Statistics ?? new GameStatistics();
            currentSaveFile = System.IO.Path.GetFileName(f);
            unlockedResearch = d.UnlockedResearch ?? new string[0];

            foreach (var r in researches)
                if (unlockedResearch.Contains(r.Name))
                    r.Apply?.Invoke(this);

         
            ants = new Ant[0]; resources = new ResourceNode[0]; obstacles = new Obstacle[0];
            GameCanvas.Children.Clear();
            GameCanvas.Children.Add(NestGrid);
            Canvas.SetLeft(NestGrid, nestX - 75);
            Canvas.SetTop(NestGrid, nestY - 75);

            foreach (var a in d.Ants)
                RestoreAnt(a);

            
            foreach (var r in d.Resources)
                RestoreResource(r);

          
            foreach (var o in d.Obstacles ?? new ObstacleSaveData[0])
                RestoreObstacle(o);

            ShowNotify("📂", "Завантажено!");
            AddLog($"{f}", Colors.LightYellow);
            UpdateUI();
        }

        /// <summary>
        /// Відновлює окрему мураху з даних збереження, створює її візуальне відображення
        /// та додає на ігрове поле.
        /// </summary>
        void RestoreAnt(AntSaveData a)
        {
            var ant = new Ant(a.Id, a.X, a.Y)
            {
                Name = a.Name,
                Speed = a.Speed,
                State = (AntState)Enum.Parse(typeof(AntState), a.State),
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
            ant.Visual.MouseLeftButtonDown += (_, __) => ShowAntDialog(ant);

            Canvas.SetLeft(ant.Visual, ant.X - 16);
            Canvas.SetTop(ant.Visual, ant.Y - 16);
            GameCanvas.Children.Add(ant.Visual);

            ants = ants.Concat(new[] { ant }).ToArray();
        }
        /// <summary>
        /// Відновлює вузол ресурсу з даних збереження, створює його візуальний елемент
        /// (коло з емодзі) та розміщує на Canvas.
        /// </summary>
        void RestoreResource(ResourceSaveData r)
        {
            var t = (ResourceType)Enum.Parse(typeof(ResourceType), r.Type);
            var (emoji, color) = GetResourceInfo(t);

            var n = new ResourceNode
            {
                Id = r.Id,
                Type = t,
                X = r.X,
                Y = r.Y,
                Amount = r.Amount,
                MaxAmount = 150
            };

            var b = new Border
            {
                Width = 40,
                Height = 40,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom(color),
                CornerRadius = new CornerRadius(20),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Child = new TextBlock
                {
                    Text = emoji,
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            Canvas.SetLeft(b, r.X - 20);
            Canvas.SetTop(b, r.Y - 20);
            GameCanvas.Children.Add(b);

            n.Visual = b;
            resources = resources.Concat(new[] { n }).ToArray();
        }

        (string emoji, string color) GetResourceInfo(ResourceType t) => t switch
        {
            ResourceType.Food => ("🍂", "#00d4aa"),
            ResourceType.Wood => ("🪵", "#ff7675"),
            ResourceType.Stone => ("🪨", "#b2bec3"),
            ResourceType.Water => ("💧", "#74b9ff"),
            _ => ("❓", "gray")
        };
        /// <summary>
        /// Відновлює статичну перешкоду з даних збереження, створює прямокутний візуальний
        /// елемент та додає обробник видалення по правому кліку миші.
        /// </summary>
        void RestoreObstacle(ObstacleSaveData o)
        {
            var r = new Rectangle
            {
                Width = o.Width,
                Height = o.Height,
                Fill = new SolidColorBrush(Color.FromRgb(80, 85, 100)),
                Stroke = new SolidColorBrush(Color.FromRgb(120, 125, 140)),
                StrokeThickness = 2,
                Cursor = Cursors.Hand,
                RadiusX = 8,
                RadiusY = 8
            };

            var id = o.Id;
            r.MouseRightButtonDown += (_, __) =>
            {
                GameCanvas.Children.Remove(r);
                obstacles = obstacles.Where(x => x.Id != id).ToArray();
            };

            Canvas.SetLeft(r, o.X);
            Canvas.SetTop(r, o.Y);
            GameCanvas.Children.Add(r);

            obstacles = obstacles.Concat(new[]
            {
        new Obstacle
        {
            Id = o.Id,
            X = o.X,
            Y = o.Y,
            Width = o.Width,
            Height = o.Height,
            Visual = r
        }
    }).ToArray();
        }

        /// <summary>
        /// Показує діалогове вікно для вибору файлу збереження. Залежно від прапорця 
        /// дозволяє або ввести назву нового сейву, або завантажити наявний зі списку.
        /// </summary>

        void ShowSaveDialog(bool save)
        {
            var w = new Window { Title = save ? "Зберегти" : "Завантажити", Width = 380, Height = 480, 
            WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = BgDark };
            var p = new StackPanel { Margin = new Thickness(20) };
            p.Children.Add(new TextBlock { Text = w.Title, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Accent, Margin = new Thickness(0, 0, 0, 16) });
            var lb = new ListBox { Height = 260, Margin = new Thickness(0, 0, 0, 10), Background = BgCard, Foreground = TextMain, BorderBrush = BorderColor };
            if (Directory.Exists("Data")) foreach (var f in Directory.GetFiles("Data", "*.json").
            Select(System.IO.Path.GetFileNameWithoutExtension)) lb.Items.Add(f);
            p.Children.Add(lb);
            if (save)
            {
                var tb = new TextBox { Text = "savegame", Margin = new Thickness(0, 0, 0, 10), 
                Background = BgCard, Foreground = TextMain, BorderBrush = BorderColor, Padding = new Thickness(8) };
                p.Children.Insert(1, new TextBlock { Text = "Назва:", Margin = new Thickness(0, 5, 0, 3), Foreground = TextSub }); p.Children.Insert(2, tb);
                var sb = new Button { Content = "Зберегти", Padding = new Thickness(16, 8, 16, 8), 
                Background = Accent, Foreground = Brushes.White, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
                sb.Click += (_, __) => { SaveGame(tb.Text + ".json"); w.Close(); };
                p.Children.Add(sb);
            }
            else
            {
                var ldb = new Button { Content = "Завантажити", Padding = new Thickness(16, 8, 16, 8), 
                Background = Accent, Foreground = Brushes.White, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right };
                ldb.Click += (_, __) => { if (lb.SelectedItem != null) { LoadGame($"Data/{lb.SelectedItem}.json"); w.Close(); } };
                p.Children.Add(ldb);
            }
            w.Content = p; w.ShowDialog();
        }
    }
}