using System.Windows;
using System.Windows.Controls;

namespace AntFarmProject.UserControls
{
	/// <summary>
	/// Представляє UI-компонент панелі ресурсів у застосунку AntFarm.
	/// Використовується для відображення та взаємодії з ресурсами гри/симуляції.
	/// </summary>	
	public partial class ResourcePanel : UserControl
	{
		/// <summary>
		/// Ініціалізує новий екземпляр <see cref="ResourcePanel"/>.
		/// Викликає ініціалізацію компонентів XAML.
		/// </summary>	
		public ResourcePanel()
		{
			InitializeComponent();
		}
	}
}