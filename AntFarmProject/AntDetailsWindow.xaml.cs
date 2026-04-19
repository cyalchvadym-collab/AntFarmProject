using System.Windows;

namespace AntFarmProject
{
	public partial class AntDetailsWindow : Window
	{
		private Ant ant;

		public AntDetailsWindow(Ant ant)
		{
			InitializeComponent();
			this.ant = ant;
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			AntNameTitle.Text = ant.Name;
			StateText.Text = ant.State.ToString();
			EnergyBar.Value = ant.Energy;
			HealthBar.Value = ant.Health;
			AgeText.Text = $"{ant.Age:F1} днів";

			FoodGatheredText.Text = $"🍂 {ant.GatheredFood}";
			WoodGatheredText.Text = $"🪵 {ant.GatheredWood}";
			StoneGatheredText.Text = $"🪨 {ant.GatheredStone}";
			WaterGatheredText.Text = $"💧 {ant.GatheredWater}";
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}