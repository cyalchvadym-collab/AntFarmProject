using System.Windows;
using AntFarmProject.Models;
namespace AntFarmProject
{
	/// <summary>
	/// Вікно деталей мурахи.
	/// Відображає основну інформацію про конкретну мураху,
	/// включаючи її стан, характеристики та зібрані ресурси.
	/// </summary>
	public partial class AntDetailsWindow : Window
	{
		private Ant ant;

		/// <summary>
		/// Створює нове вікно деталей мурахи та ініціалізує відображення даних.
		/// </summary>
		public AntDetailsWindow(Ant ant)
		{
			InitializeComponent();
			this.ant = ant;
			UpdateDisplay();
		}

		/// <summary>
		/// Оновлює UI елементи відповідно до поточного стану мурахи.
		/// Відображає ім'я, стан, енергію, здоров'я, вік та зібрані ресурси.
		/// </summary>
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

		/// <summary>
		/// Обробник кнопки закриття вікна.
		/// Закриває поточне вікно деталей мурахи.
		/// </summary>
		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}