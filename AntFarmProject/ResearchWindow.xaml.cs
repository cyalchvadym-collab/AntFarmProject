using System.Windows;

using System.Windows;

namespace AntFarmProject
{
	public partial class ResearchWindow : Window
	{
		public ResearchWindow(MainWindow main)
		{
			InitializeComponent();
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}