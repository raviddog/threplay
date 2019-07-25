using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace threplay
{

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
        public static bool SetLive(string path) { return games[currentGame].SetLive(path); }
        public static bool SetBackup(string path) { return games[currentGame].SetBackup(path); }
        public static void LoadLive() { games[currentGame].LoadLive(ref replayLiveView); }
        public static void LoadBackup() { games[currentGame].LoadBackup(ref replayBackupView); }
        public static void LaunchGame() { if (games[currentGame].gameExe != "!") try { System.Diagnostics.Process.Start(games[currentGame].gameExe); } catch { } }
        public static void OpenLiveFolder() { if (games[currentGame].dirLive != "!") try { System.Diagnostics.Process.Start(games[currentGame].dirLive); } catch { } }
        public static void OpenBackupFolder() { if (games[currentGame].dirBackup != "!") try { System.Diagnostics.Process.Start(games[currentGame].dirBackup); } catch { } }
        public static void OpenGameFolder() { if (games[currentGame].gameExe != "!") try { System.Diagnostics.Process.Start(Path.GetDirectoryName(games[currentGame].gameExe)); } catch { } }
        //public static ListViewItem GetGameListEntry(int i) { return games[i].listEntry; }
        public static void UpdateCurrentGame(ref TextBlock title, ref TextBox exe, ref TextBox live, ref TextBox backup, ref TextBlock scoreLiveDate, ref TextBlock scoreBackupDate, ref Button backupEnabled)
        {
            currentGame = gameListView.SelectedIndex;
            title.Text = "Currently selected game: " + GameData.titles[currentGame];
            if (games[currentGame].dirBackup != "!")
            {
                backup.Text = games[currentGame].dirBackup;
                LoadBackup();
            }
            else
            {
                backup.Text = "click to browse for backup folder";
                List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                replayBackupView.ItemsSource = replayListBackup;
            }
            if (games[currentGame].dirLive != "!")
            {
                live.Text = games[currentGame].dirLive;
                LoadLive();
            }
            else
            {
                live.Text = "click to browse for replay folder";
                List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                replayLiveView.ItemsSource = replayListLive;
            }
            if (games[currentGame].gameExe != "!")
            {
                exe.Text = games[currentGame].gameExe;
            }
            else
            {
                exe.Text = "click to browse for game exe";
            }
            if(GameData.scorefileJ[currentGame] != null)
            {
                if(File.Exists(games[currentGame].dirBackup + "\\" + GameData.scorefileJ[currentGame]))
                {
                    try
                    {
                        FileInfo backupScore = new FileInfo(games[currentGame].dirBackup + "\\" + GameData.scorefileJ[currentGame]);
                        scoreBackupDate.Text = backupScore.LastWriteTime.ToShortDateString();
                    } catch
                    {
                        scoreBackupDate.Text = "Unable to open";
                    }
                } else
                {
                    scoreBackupDate.Text = "Never";
                }
                if(File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\" + GameData.scorefileJ[currentGame]))
                {
                    try
                    {
                        FileInfo liveScore = new FileInfo(Directory.GetParent(games[currentGame].dirLive) + "\\" + GameData.scorefileJ[currentGame]);
                        scoreLiveDate.Text = liveScore.LastWriteTime.ToShortDateString();
                    } catch
                    {
                        scoreLiveDate.Text = "Unable to open";
                    }
                } else
                {
                    scoreLiveDate.Text = "None";
                }
                if(Directory.Exists(games[currentGame].dirLive) && Directory.Exists(games[currentGame].dirBackup))
                {
                    backupEnabled.IsEnabled = File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\" + GameData.scorefileJ[currentGame]);
                } else
                {
                    backupEnabled.IsEnabled = false;
                }
            } else
            {
                if (File.Exists(games[currentGame].dirBackup + "\\score" + GameData.setting[currentGame] + ".dat"))
                {
                    try
                    {
                        FileInfo scorefile = new FileInfo(games[currentGame].dirBackup + "\\score" + GameData.setting[currentGame] + ".dat");
                        scoreBackupDate.Text = scorefile.LastWriteTime.ToShortDateString();
                    } catch
                    {
                        scoreBackupDate.Text = "Unable to open";
                    }
                } else
                {
                    scoreBackupDate.Text = "Never";
                }
                if (File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\score" + GameData.setting[currentGame] + ".dat"))
                {
                    try
                    {
                        FileInfo liveScore = new FileInfo(Directory.GetParent(games[currentGame].dirLive) + "\\score" + GameData.setting[currentGame] + ".dat");
                        scoreLiveDate.Text = liveScore.LastWriteTime.ToShortDateString();
                    } catch
                    {
                        scoreLiveDate.Text = "Unable to open";
                    }
                }
                else
                {
                    scoreLiveDate.Text = "None";
                }
                if (Directory.Exists(games[currentGame].dirLive) && Directory.Exists(games[currentGame].dirBackup))
                {
                    backupEnabled.IsEnabled = File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\score" + GameData.setting[currentGame] + ".dat");
                } else
                {
                    backupEnabled.IsEnabled = false;
                }
            }
            
        }

        public static bool BackupScore(ref TextBlock scoreBackupDate)
        {
            //error check this
            if(GameData.scorefileJ[currentGame] != null)
            {
                if (File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\" + GameData.scorefileJ[currentGame]))
                {
                    try
                    {
                        File.Copy(Directory.GetParent(games[currentGame].dirLive) + "\\" + GameData.scorefileJ[currentGame], games[currentGame].dirBackup + "\\" + GameData.scorefileJ[currentGame], true);
                    } catch
                    {
                        return false;
                    }
                    FileInfo scorefile = new FileInfo(games[currentGame].dirBackup + "\\" + GameData.scorefileJ[currentGame]);
                    scoreBackupDate.Text = scorefile.LastWriteTime.ToShortDateString();
                    return true;
                }
                else return false;
            } else
            {
                if (File.Exists(Directory.GetParent(games[currentGame].dirLive) + "\\score" + GameData.setting[currentGame] + ".dat"))
                {
                    try
                    {
                        File.Copy(Directory.GetParent(games[currentGame].dirLive) + "\\score" + GameData.setting[currentGame] + ".dat", games[currentGame].dirBackup + "\\score" + GameData.setting[currentGame] + ".dat", true);
                    } catch
                    {
                        return false;
                    }
                    FileInfo scorefile = new FileInfo(games[currentGame].dirBackup + "\\score" + GameData.setting[currentGame] + ".dat");
                    scoreBackupDate.Text = scorefile.LastWriteTime.ToShortDateString();
                    return true;
                }
                else return false;
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
                        string dest = System.IO.Path.Combine(games[currentGame].dirBackup, item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                proceed = false;
                            } else
                            {
                                System.IO.File.Delete(dest);
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                try
                                {
                                    File.Move(item.FullPath, dest);
                                } catch
                                {
                                    
                                }
                            }
                            else
                            {
                                try
                                {
                                    File.Copy(item.FullPath, dest);
                                } catch
                                {

                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (ReplayEntry item in replayBackupView.SelectedItems)
                    {
                        //ReplayEntry replayItem = item.Content as ReplayEntry;
                        string dest = System.IO.Path.Combine(games[currentGame].dirLive, item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                proceed = false;
                            }
                            else
                            {
                                System.IO.File.Delete(dest);
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                try
                                {
                                    File.Move(item.FullPath, dest);
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    File.Copy(item.FullPath, dest);
                                }
                                catch
                                {

                                }
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
            for (int i = 0; i < (int)GameList.thLast; i++)
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
            if (gameListView != null)
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
            }
            else return false;
        }

        public static string AddReplay(bool backup, string[] files)
        {
            bool ignoreNotif = false, readError = false, fileExists = false;
            foreach(string file in files)
            {
                string extension = file.Substring(file.Length - 4);
                if(extension != ".rpy")
                {
                    //not replay file

                    ignoreNotif = true;
                    //((MainWindow)Application.Current.MainWindow).SetErrorMessage("Non replay files detected and ignored.");
                } else
                {
                    ReplayEntry data = new ReplayEntry();
                    data.FullPath = file;
                    if(GameReplayDecoder.ReadFile(ref data))
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (backup)
                        {
                            if(Directory.Exists(games[data.replay.game].dirBackup))
                            {
                                if(File.Exists(games[data.replay.game].dirBackup + "\\" + fileInfo.Name))
                                {
                                    //overwrite or like generate the next name?
                                    fileExists = true;
                                } else
                                {
                                    try
                                    {
                                        File.Copy(file, games[data.replay.game].dirBackup + "\\" + fileInfo.Name);
                                    }
                                    catch
                                    {
                                        readError = true;
                                    }
                                }
                            }
                        }
                        else
                        { 
                            if(Directory.Exists(games[data.replay.game].dirLive))
                            {
                                if (File.Exists(games[data.replay.game].dirLive + "\\" + fileInfo.Name))
                                {
                                    //overwrite or like generate the next name?
                                    fileExists = true;
                                }
                                else
                                {
                                    try
                                    {
                                        File.Copy(file, games[data.replay.game].dirLive + "\\" + fileInfo.Name);
                                    } catch
                                    {
                                        readError = true;
                                    }
                                }
                            }
                        }
                    } else
                    {
                        readError = true;
                    }
                }
            }
            string message = "Operation complete.";
            if (ignoreNotif && readError)
            {
                message = string.Concat(message, " Some files were not replays or could not be read, and were ignored."); ;
            } else
            {
                if(ignoreNotif) message = string.Concat(message, " Some non-replay files were ignored.");
                if (readError) message = string.Concat(message, " Some files could not be read and were ignored.");
            }
            if (fileExists) MessageBox.Show("A replay with that filename already exists. Please rename it and try again.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return message;
        }

        public static bool SetAllBackup(string path)
        {
            bool success = true;
            path = string.Concat(path, "\\");
            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                if(!Directory.Exists(path + GameData.setting[i]))
                {
                    Directory.CreateDirectory(path + GameData.setting[i]);
                }

                if(!games[i].SetBackup(path + GameData.setting[i]))
                {
                    success = false;
                }
            }

            return success;
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
                if (dirLive == "!")
                {
                    string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            "\\ShanghaiAlice\\" + GameData.setting[GameHandler.currentGame] + "\\replay";
                    if(Directory.Exists(appdatapath))
                    {
                        //catch games that use appdata but the user created a replay folder inside the game folder
                        //consider autodetecting this too? i dont want to duplicate code and the user will probably click the replay field anyway
                        replay = "!";
                    } else if (Directory.Exists(Path.GetDirectoryName(path) + "\\replay"))
                    {
                        //autodetect replay folder
                        dirLive = Path.GetDirectoryName(path) + "\\replay";
                        replay = dirLive;
                        Properties.Settings.Default[GameData.setting[number] + "_l"] = dirLive;
                    }
                    else
                    {
                        replay = "!";
                    }
                }
                else
                {
                    replay = dirLive;
                }
                Properties.Settings.Default.Save();
            }

            public bool SetLive(string path)
            {
                if (path == dirBackup)
                {
                    MessageBox.Show("Replay folder cannot be the same folder as the backup folder", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else
                {
                    dirLive = path;
                    Properties.Settings.Default[GameData.setting[number] + "_l"] = dirLive;
                    Properties.Settings.Default.Save();
                    return true;
                }
            }

            public bool SetBackup(string path)
            {
                if (path == dirLive)
                {
                    MessageBox.Show("Backup folder cannot be the same folder as the replay folder", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else
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
                    try
                    {
                        dirLiveInfo = new DirectoryInfo(dirLive);
                        replaysLive = dirLiveInfo.GetFiles("*.rpy");
                    } catch
                    {
                        ((MainWindow)Application.Current.MainWindow).SetErrorMessage("Unable to open replay folder. Check that it is set correctly.");
                    }

                    List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                    if(replaysLive != null)
                    {
                        foreach (FileInfo curFile in replaysLive)
                        {
                            ReplayEntry replayInfo = new ReplayEntry
                            {
                                Filename = curFile.Name,
                                Filesize = (curFile.Length / 1024.0f).ToString("#,###.#") + "KB",
                                FullPath = curFile.FullName
                            };
                            //GameReplayDecoder.ReadFile(ref replayInfo);
                            //if(replayInfo.replay.game == number)
                            
                            replayListLive.Add(replayInfo);
                            
                        }
                    }

                    list.ItemsSource = replayListLive;
                }
            }

            public void LoadBackup(ref ListView list)
            {
                //this one shouldn't be able to fail
                if (dirBackup != "!")
                {
                    try
                    {
                        dirBackupInfo = new DirectoryInfo(dirBackup);
                        replaysBackup = dirBackupInfo.GetFiles("*.rpy");
                    } catch
                    {
                        ((MainWindow)Application.Current.MainWindow).SetErrorMessage("Unable to open backup folder. Check that it is set correctly.");
                    }

                    List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                    if(replaysBackup != null)
                    {
                        foreach (FileInfo curFile in replaysBackup)
                        {
                            ReplayEntry replayInfo = new ReplayEntry
                            {
                                Filename = curFile.Name,
                                Filesize = (curFile.Length / 1024.0f).ToString("#,###.#") + "KB",
                                FullPath = curFile.FullName
                            };
                            //GameReplayDecoder.ReadFile(ref replayInfo);
                            //if(replayInfo.replay.game == number)
                            
                            replayListBackup.Add(replayInfo);
                            
                        }
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
                System.Windows.Media.Imaging.BitmapImage imageSource = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri((String)("pack://application:,,,/Resources/img_game/img_" + GameData.setting[i] + ".jpg"), UriKind.Absolute));
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
            "Touhou 16.5",
            "Touhou 17"
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
            "Violet Detector",
            "Wily Beast & Weakest Creature"
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
            "th165",
            "th17"
        };

        public static readonly String[] scorefileJ =
        {
            "score.dat",
            "score.dat",
            "score.dat",
            "score.dat",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        };
    }
}
