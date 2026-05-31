using System.Windows;
namespace AntFarmProject
{
	/// <summary>
	/// Вікно довідки 
	/// Містить інформацію для користувача про функціонал застосунку AntFarmProject.
	/// </summary>	
	public partial class HelpWindow : Window
	{
		/// <summary>
		/// Ініціалізує новий екземпляр вікна довідки.
		/// </summary>	
		public HelpWindow()
		{
			InitializeComponent();
		}
		/// <summary>
		/// Обробник натискання кнопки закриття.
		/// Закриває вікно довідки.
		/// </summary>
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}