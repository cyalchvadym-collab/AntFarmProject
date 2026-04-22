using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AntFarmProject.Models;
namespace AntFarmProject
{
	/// <summary>
	/// Вікно статистики гри.
	/// Відображає загальні показники прогресу колонії мурах.
	/// </summary>
	public partial class StatisticsWindow : Window
	{
		/// <summary>
		/// Ініціалізує вікно статистики та заповнює UI даними.
		/// </summary>
		public StatisticsWindow(GameStatistics stats, List<Ant> ants)
		{
			InitializeComponent();

			TotalFoodStat.Text = $"🍂 Їжа: {stats.TotalFoodCollected}";
			TotalWoodStat.Text = $"🪵 Деревина: {stats.TotalWoodCollected}";
			TotalStoneStat.Text = $"🪨 Камінь: {stats.TotalStoneCollected}";
			TotalWaterStat.Text = $"💧 Вода: {stats.TotalWaterCollected}";

			AntsBornStat.Text = $"🐣 Народилось: {stats.AntsBorn}";
			AntsDiedStat.Text = $"💀 Померло: {stats.AntsDied}";
			AntsAliveStat.Text = $"❤️ Зараз живі: {ants.Count(a => a.State != AntState.Dead)}";

			NestLevelStat.Text = $"Рівень гнізда: {stats.NestExpansions + 1}";
			ExpansionsStat.Text = $"Розширень: {stats.NestExpansions}";
			DaysStat.Text = $"Днів виживання: {stats.DaysSurvived}";

			PlayTimeStat.Text = $"Загальний час: {stats.PlayTime:hh\\:mm\\:ss}";
		}

		/// <summary>
		/// Обробник кнопки закриття вікна статистики.
		/// Закриває поточне вікно.
		/// </summary>
		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}