using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace threplay
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        List<GameSettingVisibilityItem> gameList;
        public SettingsWindow()
        {
            InitializeComponent();
            string gameVisible = (string)Properties.Settings.Default["gameVisibility"];
            gameList = new List<GameSettingVisibilityItem>();
            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                bool active = gameVisible[i] == 'Y' ? true : false;
                gameList.Add(new GameSettingVisibilityItem() { name = GameData.names[i], index = i });
            }

            modeSettingVisibilityToggleList.ItemsSource = gameList;

            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                if(gameVisible[i] == 'Y')
                {
                    modeSettingVisibilityToggleList.SelectedItems.Add(modeSettingVisibilityToggleList.Items[i]);
                }
            }
        }
        private void FnSettingsApply_Click(object sender, RoutedEventArgs e)
        {
            char[] gameVisible = new char[(int)GameList.thLast];
            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                gameVisible[i] = 'N';
            }
            foreach(GameSettingVisibilityItem item in modeSettingVisibilityToggleList.SelectedItems)
            {
                gameVisible[item.index] = 'Y';
            }
            string visible = new string(gameVisible);
            GameHandler.RegenerateGameList(visible);
            this.Close();
        }

        private void FnSettingsCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class GameSettingVisibilityItem
    {
        public string name { get; set; }
        public int index { get; set; }
    }
}
