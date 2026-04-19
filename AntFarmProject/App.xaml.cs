using System;
using System.IO;
using System.Windows;

namespace AntFarmProject
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			Directory.CreateDirectory("Data");
			Directory.CreateDirectory("Data/backups");
		}
	}
}