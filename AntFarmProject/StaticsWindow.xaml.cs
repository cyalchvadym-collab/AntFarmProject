using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AntFarmProject
{
	public partial class StatisticsWindow : Window
	{
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

		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}