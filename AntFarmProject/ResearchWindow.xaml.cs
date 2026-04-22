using System.Windows;

namespace AntFarmProject
{
	/// <summary>
	/// Вікно досліджень.
	/// Використовується для перегляду та керування дослідницькими можливостями в грі AntFarmProject.
	/// </summary>
	public partial class ResearchWindow : Window
	{
		/// <summary>
		/// Ініціалізує новий екземпляр вікна досліджень.
		/// </summary>	
		public ResearchWindow(MainWindow main)
		{
			InitializeComponent();
		}
		/// <summary>
		/// Обробник кнопки закриття вікна.
		/// Закриває поточне вікно досліджень.
		/// </summary>
		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}