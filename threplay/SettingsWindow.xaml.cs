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
using Ookii.Dialogs.Wpf;

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
            optUpdates.IsChecked = (bool)Properties.Settings.Default["updates"];
        }
        private void FnSettingsApply_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["updates"] = optUpdates.IsChecked;
            Properties.Settings.Default.Save();
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

        private void FnSettingsSetBackup_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if((bool)dialog.ShowDialog(this))
            {
                GameHandler.SetAllBackup(dialog.SelectedPath);
            }
        }
    }

    public class GameSettingVisibilityItem
    {
        public string name { get; set; }
        public int index { get; set; }
    }
}
