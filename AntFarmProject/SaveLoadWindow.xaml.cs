using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AntFarmProject
{
	public class SaveInfo
	{
		public string Name { get; set; }
		public string Date { get; set; }
		public string Info { get; set; }
		public string FilePath { get; set; }
	}

	public partial class SaveLoadWindow : Window
	{
		private MainWindow mainWindow;
		private bool isSaveMode;

		public SaveLoadWindow(MainWindow main, bool saveMode)
		{
			InitializeComponent();
			mainWindow = main;
			isSaveMode = saveMode;

			ActionBtn.Content = saveMode ? "💾 Зберегти" : "📂 Завантажити";
			Title = saveMode ? "Зберегти гру" : "Завантажити гру";

			LoadSaveList();
		}

		private void LoadSaveList()
		{
			var saves = new List<SaveInfo>();

			// Auto-save
			if (File.Exists("Data/savegame.json"))
			{
				saves.Add(GetSaveInfo("Data/savegame.json", "🔄 Автозбереження"));
			}

			// Slots
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

			// Backups
			var backups = Directory.GetFiles("Data/backups", "*.json")
				.OrderByDescending(f => File.GetCreationTime(f))
				.Take(5);

			foreach (var backup in backups)
			{
				saves.Add(GetSaveInfo(backup, "📋 Резервна копія"));
			}

			SaveList.ItemsSource = saves;
		}

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

		private void ActionBtn_Click(object sender, RoutedEventArgs e)
		{
			if (SaveList.SelectedItem is SaveInfo save)
			{
				if (isSaveMode)
				{
					// Generate new filename for empty slot
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

		private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}