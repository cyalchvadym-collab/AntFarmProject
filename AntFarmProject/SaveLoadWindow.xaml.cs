using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AntFarmProject.Models;
namespace AntFarmProject
{
	/// <summary>
	/// Інформація про збереження гри.
	/// Використовується для відображення списку сейвів у SaveLoadWindow.
	/// </summary>
	public class SaveInfo
	{
		/// <summary>Назва збереження.</summary>
		public string Name { get; set; }

		/// <summary>Дата створення або останнього збереження.</summary>
		public string Date { get; set; }

		/// <summary>Короткий опис стану гри у цьому збереженні.</summary>
		public string Info { get; set; }

		/// <summary>Шлях до файлу збереження.</summary>
		public string FilePath { get; set; }
	}

	/// <summary>
	/// Вікно збереження та завантаження гри.
	/// Дозволяє користувачу керувати слотами збережень та резервними копіями.
	/// </summary>
	public partial class SaveLoadWindow : Window
	{
		private MainWindow mainWindow;
		private bool isSaveMode;

		/// <summary>
		/// Ініціалізує вікно Save/Load режиму.
		/// </summary>
		public SaveLoadWindow(MainWindow main, bool saveMode)
		{
			InitializeComponent();
			mainWindow = main;
			isSaveMode = saveMode;

			ActionBtn.Content = saveMode ? "💾 Зберегти" : "📂 Завантажити";
			Title = saveMode ? "Зберегти гру" : "Завантажити гру";

			LoadSaveList();
		}

		/// <summary>
		/// Завантажує список доступних збережень (слоти, автосейв, бекапи).
		/// </summary>
		private void LoadSaveList()
		{
			var saves = new List<SaveInfo>();

			if (File.Exists("Data/savegame.json"))
			{
				saves.Add(GetSaveInfo("Data/savegame.json", "🔄 Автозбереження"));
			}

			
			for (int i = 1; i <= 3; i++)
			{
				string path = $"Data/savegame_{i}.json";
				if (File.Exists(path))
				{
					saves.Add(GetSaveInfo(path, $"💾 Слот {i}"));
				}
				else
				{
					saves.Add(new SaveInfo
					{
						Name = $"💾 Слот {i} (порожній)",
						Date = "Немає даних",
						Info = "Натисніть для створення",
						FilePath = path
					});
				}
			}

			var backups = Directory.GetFiles("Data/backups", "*.json")
				.OrderByDescending(f => File.GetCreationTime(f))
				.Take(5);

			foreach (var backup in backups)
			{
				saves.Add(GetSaveInfo(backup, "📋 Резервна копія"));
			}

			SaveList.ItemsSource = saves;
		}

		/// <summary>
		/// Створює об'єкт SaveInfo на основі файлу збереження.
		/// </summary>
		private SaveInfo GetSaveInfo(string path, string defaultName)
		{
			try
			{
				var json = File.ReadAllText(path);
				var data = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

				return new SaveInfo
				{
					Name = data.SaveName ?? defaultName,
					Date = $"📅 {data.SaveDate:dd.MM.yyyy HH:mm}",
					Info = $"🐜 {data.Ants?.Count ?? 0} мурах | 🏰 Рівень {data.ColonyLevel} | День {data.CurrentDay}",
					FilePath = path
				};
			}
			catch
			{
				return new SaveInfo
				{
					Name = defaultName,
					Date = "Помилка читання",
					Info = "Файл пошкоджено",
					FilePath = path
				};
			}
		}

		/// <summary>
		/// Обробник кнопки виконання дії (збереження або завантаження).
		/// </summary>
		private void ActionBtn_Click(object sender, RoutedEventArgs e)
		{
			if (SaveList.SelectedItem is SaveInfo save)
			{
				if (isSaveMode)
				{
					string filename = save.FilePath;
					if (save.Name.Contains("порожній"))
					{
						filename = $"savegame_{new Random().Next(1, 1000)}.json";
					}

					mainWindow.SaveGame(filename);
				}
				else
				{
					if (!save.Name.Contains("порожній"))
					{
						mainWindow.LoadGame(save.FilePath);
					}
				}
				Close();
			}
		}

		/// <summary>
		/// Видаляє вибране збереження після підтвердження користувача.
		/// </summary>
		private void DeleteSave_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.Tag is string path)
			{
				if (File.Exists(path) && MessageBox.Show("Видалити це збереження?",
					"Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					File.Delete(path);
					LoadSaveList();
				}
			}
		}

		/// <summary>
		/// Закриває вікно без виконання дій.
		/// </summary>
		private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}