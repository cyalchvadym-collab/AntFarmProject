using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AntFarmProject.Models;

namespace AntFarmProject.ViewMod
{
	/// <summary>
	/// Головний ViewModel, який керує взаємодією між UI та логікою додатку.
	/// </summary>
	public class MainViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Подія для повідомлення UI про зміну властивостей.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Викликає оновлення властивості в UI.
		/// </summary>
		protected void OnPropertyChanged(string name) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		/// <summary>
		/// Запит на відкриття деталей мурахи.
		/// </summary>
		public event EventHandler<Ant>? RequestShowAntDetails;

		/// <summary>
		/// Запит на показ сповіщення (іконка + повідомлення).
		/// </summary>
		public event EventHandler<(string icon, string message)>? RequestShowNotification;

		/// <summary>
		/// Запит на додавання запису в лог із кольором.
		/// </summary>
		public event EventHandler<(string message, Color color)>? RequestAddLog;

		/// <summary>
		/// Запит на зміну теми інтерфейсу.
		/// </summary>
		public event EventHandler<bool>? RequestThemeChange;

		/// <summary>
		/// Ігрове поле (Canvas).
		/// </summary>
		private Canvas _gameCanvas;

		/// <summary>
		/// Контейнер гнізда (Grid).
		/// </summary>
		private Grid _nestGrid;

		/// <summary>
		/// Конструктор ViewModel з передачею UI-елементів.
		/// </summary>
		public MainViewModel(Canvas gameCanvas, Grid nestGrid)
		{
			_gameCanvas = gameCanvas;
			_nestGrid = nestGrid;
		}

		/// <summary>
		/// Ініціалізація логіки після створення ViewModel.
		/// </summary>
		public void Initialize()
		{

		}
	}
}