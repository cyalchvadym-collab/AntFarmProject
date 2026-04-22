using System;
using System.IO;
using System.Windows;

namespace AntFarmProject
{
	/// <summary>
	/// Головний клас застосунку WPF AntFarmProject.
	/// Відповідає за ініціалізацію застосунку під час запуску.
	/// </summary>	
	public partial class App : Application
	{
		/// <summary>
		/// Викликається при запуску застосунку.
		/// Виконує початкову ініціалізацію ресурсів та необхідних директорій.
		/// </summary>	
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			Directory.CreateDirectory("Data");
			Directory.CreateDirectory("Data/backups");
		}
	}
}