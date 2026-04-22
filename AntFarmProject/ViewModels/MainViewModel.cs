using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AntFarmProject.Models;

namespace AntFarmProject.ViewMod
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged(string name) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		// Події для спілкування з View
		public event EventHandler<Ant>? RequestShowAntDetails;
		public event EventHandler<(string icon, string message)>? RequestShowNotification;
		public event EventHandler<(string message, Color color)>? RequestAddLog;
		public event EventHandler<bool>? RequestThemeChange;

		private Canvas _gameCanvas;
		private Grid _nestGrid;

		public MainViewModel(Canvas gameCanvas, Grid nestGrid)
		{
			_gameCanvas = gameCanvas;
			_nestGrid = nestGrid;
		}

		public void Initialize()
		{
			// Тут буде ініціалізація гри (NewGame, SetupTimers)
		}
	}
}