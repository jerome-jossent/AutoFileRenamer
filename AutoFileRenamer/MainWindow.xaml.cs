using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AutoFileRenamer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public string path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged();
            }
        }
        string _path;

        Dictionary<string, FileSystemWatcher> fileWatcher_dico = new Dictionary<string, FileSystemWatcher>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //string[] args = Environment.GetCommandLineArgs();
            //MessageBox.Show(string.Join('\n', args));

            Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            foreach (FileSystemWatcher fw in fileWatcher_dico.Values)
            {
                fw.EnableRaisingEvents = false;
            }
        }

        void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        void TextBox_Drop(object sender, DragEventArgs e)
        {
            string[] folders_or_files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string item in folders_or_files)
            {
                if (File.Exists(item))
                {
                    //is file
                    RenameFile(item);
                }
                else
                {
                    //is folder
                    FileSystemWatcher fw = StartFileWatcher(item);
                    fileWatcher_dico.Add(item, fw);
                    path += item;
                }
            }
        }

        FileSystemWatcher StartFileWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(NewFile);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        void NewFile(object sender, FileSystemEventArgs e)
        {
            RenameFile(e.FullPath);
        }

        void RenameFile(string fullpath)
        {
            FileInfo fileInfo = new FileInfo(fullpath);

            string newName =
                fileInfo.Directory + "\\" +
                fileInfo.CreationTime.ToString("yyyy-MM-dd HH-mm-ss.fff") +
                fileInfo.Extension;

            int occurence = 0;
            bool alreadyExists = false;
            string suffixe = "";
            do
            {
                //vérifier qu'un fichier avec ce nom n'existe pas déjà !
                alreadyExists = File.Exists(newName + suffixe);
                if (alreadyExists)
                    suffixe = "(" + occurence++ + ")";
            } while (alreadyExists);

            //Renommer
            File.Move(fullpath, newName);
        }
    }
}