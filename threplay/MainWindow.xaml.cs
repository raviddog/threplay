using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
        th125,
        th128,
        th13,
        th14,
        th143,
        th15,
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

            char[] v = ((string)Properties.Settings.Default["gameVisibility"]).ToCharArray();
            int p = 0;
            while (p < (int)GameList.thLast && v[p++] == 'N') { }
            if (p > (int)GameList.thLast)
            {
                GameHandler.gameListView.SelectedIndex = -1;
            } else
            {
                GameHandler.gameListView.SelectedIndex = p - 1;
            }
        }

        private void FnLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.LaunchGame();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Tab)
            {
                e.Handled = true;
            } else if(e.Key == Key.Escape)
            {
                oReplayLiveList.SelectedIndex = -1;
                oReplayBackupList.SelectedIndex = -1;
                e.Handled = true;
            } else if(e.Key == Key.Enter)
            {
                //some weird ass fucking shit about to happen here
                if(odFileNameLive.IsFocused)
                {
                    //user hit enter after renaming
                    //probably want to put like an "are you sure" box or something i guess?
                    //something something dialoghost
                    if(oReplayLiveList.SelectedIndex != -1)
                    {
                        ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                        FileInfo replayFile = new FileInfo(replayEntry.FullPath);
                        string oldFileName = replayFile.FullName;
                        string newFileName = replayFile.DirectoryName + "\\" + odFileNameLive.Text + ".rpy";
                        File.Move(oldFileName, newFileName);
                        GameHandler.LoadLive();
                    }
                } else if(odFileNameBackup.IsFocused)
                {
                    if(oReplayBackupList.SelectedIndex != -1)
                    {
                        ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                        FileInfo replayFile = new FileInfo(replayEntry.FullPath);
                        string oldFileName = replayFile.FullName;
                        string newFileName = replayFile.DirectoryName + "\\" + odFileNameLive.Text + ".rpy";
                        File.Move(oldFileName, newFileName);
                        GameHandler.LoadBackup();
                    }
                }
            }
        }

        private void IDirGame_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if ((bool)dialog.ShowDialog(this))
            {
                iDirGame.Text = dialog.FileName;
                GameHandler.SetExe(dialog.FileName, out string replay);
                if(replay != "!")
                {
                    iDirLive.Text = replay;
                }
                GameHandler.LoadLive();
            }
            CheckMove();
        }

        private void IDirLive_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if((bool)dialog.ShowDialog(this))
            {
                if(GameHandler.SetLive(dialog.SelectedPath))
                {
                    iDirLive.Text = dialog.SelectedPath;
                }
                GameHandler.LoadLive();
            }
            CheckMove();
        }

        private void IDirBackup_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog(this))
            {
                if(GameHandler.SetBackup(dialog.SelectedPath))
                {
                    iDirBackup.Text = dialog.SelectedPath;
                }
                GameHandler.LoadBackup();
            }
            CheckMove();
        }

        private void CheckMove()
        {
            GameHandler.CheckMove(out bool hasGame, out bool hasLive, out bool hasBackup);
            fnLaunchGame.IsEnabled = hasGame;
            fnLaunchFolder.IsEnabled = hasGame;
            fnLaunchLive.IsEnabled = hasLive;
            fnLaunchBackup.IsEnabled = hasBackup;
            fnTransferToBackup.IsEnabled = hasLive && hasBackup;
            fnTransferToLive.IsEnabled = hasLive && hasBackup;
            if (hasLive && hasBackup)
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen;
                oMessage.IsActive = false;
            }
            else
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderSearchOutline;
                if (hasLive && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select a backup folder" };
                    oMessage.IsActive = true;
                }
                else if (!hasLive && hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select a replay folder" };
                    oMessage.IsActive = true;
                }
                else if (!hasLive && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select your current and backup replay folders" };
                    oMessage.IsActive = true;
                }
            }
        }

        private void FnTransferToLive_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.FileMove(false, (bool)fnTypeMove.IsChecked);
            GameHandler.LoadLive();
            GameHandler.LoadBackup();
        }

        private void FnTransferToBackup_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.FileMove(true, (bool)fnTypeMove.IsChecked);
            GameHandler.LoadLive();
            GameHandler.LoadBackup();
        }

        private void ModeGameViewToggle_MouseLeave(object sender, MouseEventArgs e)
        {
            modeGameViewToggle.Width = 65;
        }

        private void ModeGameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirGame, ref iDirLive, ref iDirBackup);
            CheckMove();
        }

        private void ModeGameSelector_MouseEnter(object sender, MouseEventArgs e)
        {
            modeGameViewToggle.Width = 250;
        }

        private void FnLaunchFolder_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.OpenGameFolder();
        }

        private void IViewDir_Expanded(object sender, RoutedEventArgs e)
        {
            odFileGrid.Visibility = Visibility.Collapsed;
        }

        private void IViewDir_Collapsed(object sender, RoutedEventArgs e)
        {
            if(fnMultiEnabled.IsChecked == false)
            {
                odFileGrid.Visibility = Visibility.Visible;
            }
        }

        private void FnMultiEnabled_Checked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Multiple;
            oReplayLiveList.SelectionMode = SelectionMode.Multiple;
            odFileGrid.Visibility = Visibility.Collapsed;
        }

        private void FnMultiEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Single;
            oReplayLiveList.SelectionMode = SelectionMode.Single;
            odFileGrid.Visibility = Visibility.Visible;

            if(oReplayLiveList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                odFileNameLive.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameLive.Focusable = true;
            } else
            {
                odFileNameLive.Text = "(no single replay selected)";
                odFileNameLive.Focusable = false;
            }
            if (oReplayBackupList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                odFileNameBackup.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameBackup.Focusable = true;
            } else
            {
                odFileNameBackup.Text = "(no single replay selected)";
                odFileNameBackup.Focusable = false;
            }
        }

        private void OReplayLiveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fnMultiEnabled.IsChecked == false && oReplayLiveList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                odFileNameLive.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameLive.Focusable = true;
                
                if(GameReplayDecoder.ReadFile(ref replayEntry))
                {
                    odFileDataLive.Text = replayEntry.replay.name;
                    odFileDateLive.Text = replayEntry.replay.date;
                    odFileShotLive.Text = replayEntry.replay.character;
                    odFileDifficultyLive.Text = replayEntry.replay.difficulty;
                    odFileScoreLive.Text = replayEntry.replay.score;
                } else
                {
                    odFileNameLive.Focusable = false;
                    odFileDataLive.Text = null;
                    odFileDateLive.Text = null;
                    odFileShotLive.Text = null;
                    odFileDifficultyLive.Text = null;
                    odFileScoreLive.Text = null;
                }

            } else
            {
                odFileNameLive.Text = "(no single replay selected)";
                odFileNameLive.Focusable = false;
                odFileDataLive.Text = null;
                odFileDateLive.Text = null;
                odFileShotLive.Text = null;
                odFileDifficultyLive.Text = null;
                odFileScoreLive.Text = null;
            }
        }

        private void OReplayBackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fnMultiEnabled.IsChecked == false && oReplayBackupList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                odFileNameBackup.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameBackup.Focusable = true;

                if (GameReplayDecoder.ReadFile(ref replayEntry))
                {
                    odFileDataBackup.Text = replayEntry.replay.name;
                    odFileDateBackup.Text = replayEntry.replay.date;
                    odFileShotBackup.Text = replayEntry.replay.character;
                    odFileDifficultyBackup.Text = replayEntry.replay.difficulty;
                    odFileScoreBackup.Text = replayEntry.replay.score;
                }
                else
                {
                    odFileNameBackup.Focusable = false;
                    odFileDataBackup.Text = null;
                    odFileDateBackup.Text = null;
                    odFileShotBackup.Text = null;
                    odFileDifficultyBackup.Text = null;
                    odFileScoreBackup.Text = null;
                }
            } else
            {
                odFileNameBackup.Text = "(no single replay selected)";
                odFileNameBackup.Focusable = false;
                odFileDataBackup.Text = null;
                odFileDateBackup.Text = null;
                odFileShotBackup.Text = null;
                odFileDifficultyBackup.Text = null;
                odFileScoreBackup.Text = null;
            }
        }

        private void FnSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            
        }

        private void FnLaunchLive_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.OpenLiveFolder();
        }

        private void FnLaunchBackup_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.OpenBackupFolder();
        }
    }

    public static class GameHandler
    {
        public static int currentGame;
        public static ListView gameListView = null;
        public static ListView replayLiveView = null;
        public static ListView replayBackupView = null;
        public static TextBox liveDir = null;
        public static TextBox backupDir = null;

        private static GameObject[] games;

        public static void SetExe(string path, out string replay) { games[currentGame].SetExe(path, out replay); }
        public static bool SetLive(string path) { return games[currentGame].SetLive(path);  }
        public static bool SetBackup(string path) { return games[currentGame].SetBackup(path); }
        public static void LoadLive() { games[currentGame].LoadLive(ref replayLiveView); }
        public static void LoadBackup() { games[currentGame].LoadBackup(ref replayBackupView); }
        public static void LaunchGame() { if(games[currentGame].gameExe != "!") try { System.Diagnostics.Process.Start(games[currentGame].gameExe); } catch { } }
        public static void OpenLiveFolder() { if (games[currentGame].dirLive != "!") try { System.Diagnostics.Process.Start(games[currentGame].dirLive); } catch { } }
        public static void OpenBackupFolder() { if (games[currentGame].dirBackup != "!") try { System.Diagnostics.Process.Start(games[currentGame].dirBackup); } catch { } }
        public static void OpenGameFolder() { if(games[currentGame].gameExe != "!") try { System.Diagnostics.Process.Start(Path.GetDirectoryName(games[currentGame].gameExe)); } catch { } }
        //public static ListViewItem GetGameListEntry(int i) { return games[i].listEntry; }
        public static void UpdateCurrentGame(ref TextBlock title, ref TextBox exe, ref TextBox live, ref TextBox backup)
        {
            currentGame = gameListView.SelectedIndex;
            title.Text = "Currently selected game: " + GameData.titles[currentGame];
            if (games[currentGame].dirBackup != "!")
            {
                backup.Text = games[currentGame].dirBackup;
                LoadBackup();
            } else
            {
                backup.Text = "click to browse for backup folder";
                List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                replayBackupView.ItemsSource = replayListBackup;
            }
            if (games[currentGame].dirLive != "!")
            {
                live.Text = games[currentGame].dirLive;
                LoadLive();
            } else
            {
                live.Text = "click to browse for replay folder";
                List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                replayLiveView.ItemsSource = replayListLive;
            }
            if (games[currentGame].gameExe != "!")
            {
                exe.Text = games[currentGame].gameExe;
            } else
            {
                exe.Text = "click to browse for game exe";
            }
        }

        public static void CheckMove(out bool hasGame, out bool hasLive, out bool hasBackup)
        {
            hasGame = games[currentGame].gameExe != "!" ? true : false;
            hasLive = games[currentGame].dirLive != "!" ? true : false;
            hasBackup = games[currentGame].dirBackup != "!" ? true : false;
        }

        public static void FileMove(bool toBackup, bool deleteOriginal)
        {
            if (games[currentGame].dirBackup != "!" && games[currentGame].dirLive != "!")
            {
                if (toBackup)
                {
                    foreach (ReplayEntry item in replayLiveView.SelectedItems)
                    {
                        //ReplayEntry replayItem = item.Content as ReplayEntry;
                        string source = System.IO.Path.Combine(games[currentGame].dirLive, item.Filename);
                        string dest = System.IO.Path.Combine(games[currentGame].dirBackup, item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if(result == MessageBoxResult.No)
                            {
                                proceed = false;
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                System.IO.File.Move(source, dest);
                            }
                            else
                            {
                                System.IO.File.Copy(source, dest);
                            }
                        }
                    }
                }
                else
                {
                    foreach (ReplayEntry item in replayBackupView.SelectedItems)
                    {
                        //ReplayEntry replayItem = item.Content as ReplayEntry;
                        string source = System.IO.Path.Combine(games[currentGame].dirBackup, item.Filename);
                        string dest = System.IO.Path.Combine(games[currentGame].dirLive, item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                proceed = false;
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                System.IO.File.Move(source, dest);
                            }
                            else
                            {
                                System.IO.File.Copy(source, dest);
                            }
                        }
                    }
                }
            }
        }

        public static void RegenerateGameList(string gameVisible)
        {
            Properties.Settings.Default["gameVisibility"] = gameVisible;
            Properties.Settings.Default.Save();
            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                games[i].listEntry.Visibility = gameVisible[i] == 'Y' ? Visibility.Visible : Visibility.Collapsed;
            }

            char[] v = ((string)Properties.Settings.Default["gameVisibility"]).ToCharArray();
            int p = 0;
            while (p < (int)GameList.thLast && v[p++] == 'N') { }
            if (p > (int)GameList.thLast)
            {
                gameListView.SelectedIndex = -1;
            }
            else
            {
                gameListView.SelectedIndex = p - 1;
            }
        }

        public static bool InitializeGames()
        {
            if(gameListView != null)
            {
                games = new GameObject[(int)GameList.thLast];
                string gameVisible = (string)Properties.Settings.Default["gameVisibility"];
                for (int i = 0; i < (int)GameList.thLast; i++)
                {
                    games[i] = new GameObject(i);
                    games[i].listEntry.Visibility = gameVisible[i] == 'Y' ? Visibility.Visible : Visibility.Collapsed;
                    gameListView.Items.Add(games[i].listEntry);
                }
                return true;
            } else return false;
        }

        private class GameObject
        {
            public ListViewItem listEntry;
            public int number;
            public string dirLive, dirBackup, gameExe;

            private FileInfo[] replaysLive, replaysBackup;
            private DirectoryInfo dirLiveInfo, dirBackupInfo;

            public GameObject(int i)
            {
                number = i;
                CreateListEntry(i, ref listEntry);
                gameExe = (string)Properties.Settings.Default[GameData.setting[number] + "_e"];
                dirLive = (string)Properties.Settings.Default[GameData.setting[number] + "_l"];
                dirBackup = (string)Properties.Settings.Default[GameData.setting[number] + "_b"];
            }

            public void SetExe(string path, out string replay)
            {
                gameExe = path;
                Properties.Settings.Default[GameData.setting[number] + "_e"] = gameExe;
                if(Directory.Exists(Path.GetDirectoryName(path) + "\\replay"))
                {
                    //autodetect replay folder
                    dirLive = Path.GetDirectoryName(path) + "\\replay";
                    replay = dirLive;
                    Properties.Settings.Default[GameData.setting[number] + "_l"] = dirLive;
                } else
                {
                    replay = "!";
                }
                Properties.Settings.Default.Save();
            }

            public bool SetLive(string path)
            {
                if(path == dirBackup)
                {
                    MessageBox.Show("Replay folder cannot be the same folder as the backup folder", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                } else
                {
                    dirLive = path;
                    Properties.Settings.Default[GameData.setting[number] + "_l"] = dirLive;
                    Properties.Settings.Default.Save();
                    return true;
                }
            }

            public bool SetBackup(string path)
            {
                if(path == dirLive)
                {
                    MessageBox.Show("Backup folder cannot be the same folder as the replay folder", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                } else
                {
                    dirBackup = path;
                    Properties.Settings.Default[GameData.setting[number] + "_b"] = dirBackup;
                    Properties.Settings.Default.Save();
                    return true;
                }
            }

            public void LoadLive(ref ListView list)
            {
                if (dirLive != "!")
                {
                    dirLiveInfo = new DirectoryInfo(dirLive);
                    replaysLive = dirLiveInfo.GetFiles("*.rpy");
                    List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                    foreach (FileInfo curFile in replaysLive)
                    {
                        string name = curFile.Name;
                        float size = curFile.Length / 1024.0f;
                        string path = curFile.FullName;
                        replayListLive.Add(new ReplayEntry() { Filename = name, Filesize = size.ToString("#,###.#") + "KB", FullPath = path });
                    }

                    list.ItemsSource = replayListLive;
                }
            }

            public void LoadBackup(ref ListView list)
            {
                //this one shouldn't be able to fail
                if(dirBackup != "!")
                {
                    dirBackupInfo = new DirectoryInfo(dirBackup);
                    replaysBackup = dirBackupInfo.GetFiles("*.rpy");    
                    List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                    foreach (FileInfo curFile in replaysBackup)
                    {
                        string name = curFile.Name;
                        float size = curFile.Length / 1024.0f;
                        string path = curFile.FullName;
                        replayListBackup.Add(new ReplayEntry() { Filename = name, Filesize = (size.ToString("#,###.#") + "KB"), FullPath = path });
                    }
                    
                    list.ItemsSource = replayListBackup;
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

    public static class GameReplayDecoder
    {
        private static FileStream file;

        public static bool ReadFile(ref ReplayEntry replay)
        {
            bool status = false;
            if(replay.replay != null)
            {
                return true;
            } else
            {
                replay.replay = new ReplayEntry.ReplayInfo();
            }

            file = new FileStream(replay.FullPath, FileMode.Open);

            //read first 4 bytes
            int hexIn;
            String hex = string.Empty;

            for(int i = 0; i < 4; i++)
            {
                if((hexIn = file.ReadByte()) != -1)
                {
                    hex = string.Concat(hex, string.Format("{0:X2}", hexIn));
                } else
                {
                    file.Close();
                    return false;
                }
            }

            switch(hex)
            {
                case "54365250":
                    //T6RP
                    break;
                case "54375250":
                    //T7RP
                    break;
                case "54385250":
                    //T8RP
                    status = T8RP(ref replay);
                    break;
                case "54395250":
                    //T9RP
                    break;
                case "74393572":
                    //t95r
                    break;
                case "74313072":
                    //t10r
                    break;
                case "74313172":
                    //t11r
                    break;
                case "74313272":
                    //t12r
                    break;
                case "74313235":
                    //t125
                    break;
                case "31323872":
                    //128r
                    break;
                case "74313372":
                    //t13r
                    //has both td and ddc for some fucking reason
                    break;
                case "74313433":
                    //t143
                    break;
                case "74313572":
                    //t15r
                    break;
                case "74313672":
                    //t16r
                    break;
                case "74313536":
                    //t156
                    //shouldn't this be 165? gg zun
                    break;
                default:
                    break;
            }

            file.Close();
            return status;
        }

        private static UInt32 ReadUInt32()
        {
            uint buf = new uint();
            UInt32 val = new UInt32();

            for(int i = 0; i < 4; i++)
            {
                buf = (uint)file.ReadByte();
                val += buf << (i * 8);
            }

            return val;
        }

        private static string ReadString()
        {
            int[] buf = new int[3];
            string val = string.Empty;
            buf[0] = file.ReadByte();
            buf[1] = file.ReadByte();
            if(buf[0] != 13 && buf[1] != 10)
            {
                buf[2] = file.ReadByte();
                do
                {
                    val = string.Concat(val, Convert.ToChar(buf[0]).ToString());
                    buf[0] = buf[1];
                    buf[1] = buf[2];
                    buf[2] = file.ReadByte();
                }
                while (buf[0] != 13 && buf[1] != 10);
            }

            return val;
        }

        private static bool T8RP(ref ReplayEntry replay)
        {
            file.Seek(12, SeekOrigin.Begin);
            UInt32 offset = ReadUInt32();
            //get offset to user info

            string val = string.Empty;
            int buf;

            //move read position to start of user data
            try { file.Seek(offset, SeekOrigin.Begin); }
            catch { return false; }

            //read user magic to verify correct spot
            for(int i = 0; i < 4; i++)
            {
                buf = file.ReadByte();
                val = string.Concat(val, string.Format("{0:X2}", buf));
            }
            if (val != "55534552") return false;
            val = string.Empty;

            //get and store user data length, because why not
            UInt32 length = ReadUInt32();

            //move to start of player name
            file.Seek(17, SeekOrigin.Current);

            //read player name into replayinfo
            replay.replay.name = ReadString();

            //move to and read date
            file.Seek(10, SeekOrigin.Current);
            replay.replay.date = ReadString();

            //move to and read character
            file.Seek(8, SeekOrigin.Current);
            replay.replay.character = ReadString();

            //move to and read score
            file.Seek(7, SeekOrigin.Current);
            try
            {
                long scoreConv = long.Parse(ReadString());
                replay.replay.score = scoreConv.ToString("N0");
            } catch { }
            

            //move to and read difficulty
            file.Seek(7, SeekOrigin.Current);
            replay.replay.difficulty = ReadString();

            //check if spell practice or game replay
            //actually do this later

            return true;
        }

    }

    public class ReplayEntry
    {
        public string Filename { get; set; }
        public string Filesize { get; set; }
        
        public string FullPath;
        public ReplayInfo replay;

        public class ReplayInfo
        {
            public string name;
            public string date;
            public string character;
            public string difficulty;
            public string score;
        }
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
            "Touhou 12.5",
            "Touhou 12.8",
            "Touhou 13",
            "Touhou 14",
            "Touhou 14.3",
            "Touhou 15",
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
            "Double Spoiler",
            "Great Fairy Wars",
            "Ten Desires",
            "Double Dealing Character",
            "Impossible Spell Card",
            "Legacy of Lunatic Kingdom",
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
            "th125",
            "th128",
            "th13",
            "th14",
            "th143",
            "th15",
            "th16",
            "th165"
        };
    }
}

