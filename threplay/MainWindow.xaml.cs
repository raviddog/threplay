using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ookii.Dialogs.Wpf;

namespace threplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum GameList
    {
        th06,
        th07,
        th08,
        th09,
        th095,
        th10,
        th11,
        th12,
        th123,
        th125,
        th128,
        th13,
        th135,
        th14,
        th143,
        th145,
        th15,
        th155,
        th16,
        th165,

        thLast
    };
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Bluegrams.Application.PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);

            GameHandler.gameListView = modeGameSelector;
            GameHandler.replayLiveView = oReplayLiveList;
            GameHandler.replayBackupView = oReplayBackupList;
            GameHandler.liveDir = iDirLive;
            GameHandler.backupDir = iDirBackup;
            GameHandler.InitializeGames();
            GameHandler.gameListView.SelectedIndex = 0;

            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirLive, ref iDirBackup);
            

            


        }

        private void GameSelector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirLive, ref iDirBackup);

        }

        private void TempGo_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.SetDirectories();
            GameHandler.LoadReplays();
            Properties.Settings.Default.Save();
        }

        private void IDirSource_GotFocus(object sender, RoutedEventArgs e)
        {
            if(iDirLive.Text == "source folder")
            {
                iDirLive.Text = null;
            }
        }

        private void IDirBackup_GotFocus(object sender, RoutedEventArgs e)
        {
            if(iDirBackup.Text == "backup folder")
            {
                iDirBackup.Text = null;
            }
        }

        private void FnBrowseLive_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if((bool)dialog.ShowDialog(this))
            {
                iDirLive.Text = dialog.SelectedPath;
            }
        }

        private void FnBrowseBackup_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog(this))
            {
                iDirBackup.Text = dialog.SelectedPath;
            }
        }
    }

    public static class GameHandler
    {
        public static bool[] gameEnabled = new bool[(int)GameList.thLast];
        public static int currentGame;
        public static ListView gameListView = null;
        public static ListView replayLiveView = null;
        public static ListView replayBackupView = null;
        public static TextBox liveDir = null;
        public static TextBox backupDir = null;

        private static GameObject[] games;

        public static void SetDirectories() { games[currentGame].SetDirectories(liveDir.Text, backupDir.Text); }
        public static void LoadReplays() { games[currentGame].LoadReplays(ref replayLiveView, ref replayBackupView); }

        public static void UpdateCurrentGame(ref TextBlock title, ref TextBox live, ref TextBox backup)
        {
            currentGame = gameListView.SelectedIndex;
            title.Text = "Currently selected game: " + GameData.titles[currentGame];
            if (games[currentGame].dirLive != null)
            {
                live.Text = games[currentGame].dirLive;
            } else
            {
                live.Text = "source folder";
            }
            if (games[currentGame].dirBackup != null)
            {
                backup.Text = games[currentGame].dirBackup;
            } else
            {
                backup.Text = "backup folder";
            }
            if (games[currentGame].dirLive != null && games[currentGame].dirBackup != null)
            {
                LoadReplays();
            } else
            {
                List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                replayLiveView.ItemsSource = replayListLive;
                replayBackupView.ItemsSource = replayListBackup;
            }
        }

        public static void RegenerateGameList()
        {
            int i = 0;
            foreach(ListViewItem listViewItem in gameListView.Items)
            {
                listViewItem.Visibility = gameEnabled[i] ? Visibility.Visible : Visibility.Collapsed;
                i++;
            }
        }

        public static bool InitializeGames()
        {
            if(gameListView != null)
            {
                games = new GameObject[(int)GameList.thLast];
                for (int i = 0; i < (int)GameList.thLast; i++)
                {
                    gameEnabled[i] = true;
                    //temp, implement settings loading code later

                    games[i] = new GameObject(i);
                    games[i].listEntry.Visibility =  gameEnabled[i] ? Visibility.Visible : Visibility.Collapsed;
                    gameListView.Items.Add(games[i].listEntry);
                }
                return true;
            } else return false;
        }

        private class GameObject
        {
            public ListViewItem listEntry;
            public int number;
            public string dirLive, dirBackup;

            private FileInfo[] replaysLive, replaysBackup;
            private DirectoryInfo dirLiveInfo, dirBackupInfo;

            public GameObject(int i)
            {
                number = i;
                CreateListEntry(i, ref listEntry);
                dirLive = (string)Properties.Settings.Default[GameData.setting[number] + "_l"];
                dirBackup = (string)Properties.Settings.Default[GameData.setting[number] + "_b"];
            }

            public void SetDirectories(string live, string backup)
            {
                //be careful with writing to this, make sure to call only when needed
                dirLive = live;
                dirBackup = backup;
                Properties.Settings.Default[GameData.setting[number] + "_l"] = dirLive;
                Properties.Settings.Default[GameData.setting[number] + "_b"] = dirBackup;
            }

            public bool LoadReplays(ref ListView listLive, ref ListView listBackup)
            {
                //DON'T DO THIS EVER IN REAL CODE
                try
                {
                    dirLiveInfo = new DirectoryInfo(dirLive + "/replay");
                    dirBackupInfo = new DirectoryInfo(dirBackup);
                    //check the shit out of any exceptions here
                    replaysLive = dirLiveInfo.GetFiles("*.rpy");
                    replaysBackup = dirBackupInfo.GetFiles("*.rpy");

                    List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                    List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                    foreach(FileInfo curFile in replaysLive)
                    {
                        string name = curFile.Name;
                        float size = curFile.Length / 1024.0f;
                        replayListLive.Add(new ReplayEntry() { Filename = name, Filesize = size.ToString("#,###.#") + "KB" });
                    }

                    foreach(FileInfo curFile in replaysBackup)
                    {
                        string name = curFile.Name;
                        float size = curFile.Length / 1024.0f;
                        replayListBackup.Add(new ReplayEntry() { Filename = name, Filesize = (size.ToString("#,###.#") + "KB") });
                    }

                    listLive.ItemsSource = replayListLive;
                    listBackup.ItemsSource = replayListBackup;

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private void CreateListEntry(int i, ref ListViewItem listItem)
            {
                ListViewItem entry = new ListViewItem();

                StackPanel contentPanel1 = new StackPanel();
                contentPanel1.Orientation = Orientation.Horizontal;

                StackPanel contentPanel2 = new StackPanel();
                contentPanel2.Orientation = Orientation.Vertical;
                contentPanel2.Margin = new Thickness(10, 10, 10, 10);

                Image image = new Image();
                BitmapImage imageSource = new BitmapImage(new Uri((String)("pack://application:,,,/Resources/img_game/img_" + GameData.setting[i] + ".jpg"), UriKind.Absolute));
                image.Source = imageSource;
                image.Width = 50;

                TextBlock entryTitle = new TextBlock();
                TextBlock entryName = new TextBlock();
                entryTitle.FontSize = 18;
                entryName.FontSize = 12;
                entryTitle.Text = GameData.titles[i];
                entryName.Text = GameData.names[i];

                contentPanel2.Children.Add(entryTitle);
                contentPanel2.Children.Add(entryName);

                contentPanel1.Children.Add(image);
                contentPanel1.Children.Add(contentPanel2);

                entry.Content = contentPanel1;
                listItem = entry;
            }
        }

    }
    public class ReplayEntry
    {
        public string Filename { get; set; }
        public string Filesize { get; set; }
    }

    public static class GameData
    {
        /*
         *  In the future, I plan to have a data file that contains all the necessary
         *  data for each game to be displayed and for the replay files to be decoded
         *  if available. I'll also either create an external tool or bake in the capability
         *  to edit this data file to enable easy expansion of this program for new releases
         *  and additional fangames. For now though, baked in game information will suffice.
         *  
         *  I guess having it all in this class actually works nicely since I can just add in
         *  loading functions to this class and populate these string arrays with the same info
         */

        public static readonly String[] titles =
        {
            "Touhou 6",
            "Touhou 7",
            "Touhou 8",
            "Touhou 9",
            "Touhou 9.5",
            "Touhou 10",
            "Touhou 11",
            "Touhou 12",
            "Touhou 12.3",
            "Touhou 12.5",
            "Touhou 12.8",
            "Touhou 13",
            "Touhou 13.5",
            "Touhou 14",
            "Touhou 14.3",
            "Touhou 14.5",
            "Touhou 15",
            "Touhou 15.5",
            "Touhou 16",
            "Touhou 16.5"
        };

        public static readonly String[] names =
        {
            "Embodiment of Scarlet Devil",
            "Perfect Cherry Blossom",
            "Imperishable Night",
            "Phantasmagoria of Flower View",
            "Shoot the Bullet",
            "Mountain of Faith",
            "Subterranean Animism",
            "Unidentified Fantastic Object",
            "Hisoutensoku",
            "Double Spoiler",
            "Great Fairy Wars",
            "Ten Desires",
            "Hopeless Masquerade",
            "Double Dealing Character",
            "Impossible Spell Card",
            "Urban Legend in Limbo",
            "Legacy of Lunatic Kingdom",
            "Antinomy of Common Flowers",
            "Hidden Star in Four Seasons",
            "Violet Detector"
        };

        public static readonly String[] setting =
        {
            "th06",
            "th07",
            "th08",
            "th09",
            "th095",
            "th10",
            "th11",
            "th12",
            "th123",
            "th125",
            "th128",
            "th13",
            "th135",
            "th14",
            "th143",
            "th145",
            "th15",
            "th155",
            "th16",
            "th165"
        };
    }
}

