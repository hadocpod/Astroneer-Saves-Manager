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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace WpfExportSaveAstro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isPath = false;
        string path = string.Empty;
        string picked = string.Empty;
        string importPicked = string.Empty;
        List<string> list = new List<string>();
        string importName = string.Empty;
        string filetoDelete = string.Empty;
        string rename = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            LoadSaves();

        }
        public void LoadSaves()
        {
            var selectedFile = string.Empty;
            var fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Astro\Saved\SaveGames";
            //MessageBox.Show(fileName, "Name");
            DirectoryInfo dir = new DirectoryInfo(fileName);
            FileInfo last = null;
            int count = 0;
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name != "PersistentLocalPlayerData.savecfg" && file.Name != "steam_autocloud.vdf")
                {
                    listBox1.Items.Add(file.Name);
                    list.Add(file.Name);
                    if (count == 0)
                        last = file;
                    else
                    {
                        if (last.LastWriteTime < file.LastWriteTime)
                            last = file;
                    }
                    count++;
                }
            }
            count = 0;
            foreach (string file in list)
            {
                if (file == last.Name)
                {
                    var item = listBox1.Items.GetItemAt(count);
                    listBox1.Items.RemoveAt(count);
                    listBox1.Items.Insert(count, item + " <- LAST SAVE");
                }
                count++;
            }
            list.Clear();
            if (!listBox1.HasItems)
            {
                listBox1.Items.Add("No saves found in your Astroneer saves folder :/ ...");
            }
        }
        public void ReloadSaves()
        {
            listBox1.Items.Clear();
            btnButton.IsEnabled = false;
            LoadSaves();
        }
        public string GetDatefromSaveFile(string filename)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            bool flag = false;
            foreach (char letter in filename)
            {
                if (letter == '$')
                {
                    flag = true;
                    sb.Append(letter);
                }
                else if (flag)
                {
                    sb.Append(letter);
                }
            }
            return sb.ToString();
        }
        private void btnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isPath == true)
                {
                    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string name = "NEWHOST";//"SAVE_H$"+ DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss")+".savegame";
                    Regex re = new Regex(@"\A[a-zA-Z0-9_-]{1,}\Z");
                    if (txtRename.Text == String.Empty)
                    {
                        using (var archive = ZipFile.Open(desktop + @"\AstroSave.zip", ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(path, name + GetDatefromSaveFile(picked)); //[MAIN NAME]$2022.07.04-18.57.56.savegame
                            MessageBox.Show("Saved in Desktop as 'AstroSave.zip' and named as 'NEWHOST' : " + desktop + @"\AstroSave.zip");
                            filetoDelete = desktop + @"\AstroSave.zip";
                        }
                    }
                    else if (txtRename.Text != String.Empty && re.IsMatch(txtRename.Text))
                    {
                        name = txtRename.Text;
                        {
                            using (var archive = ZipFile.Open(desktop + @"\AstroSave.zip", ZipArchiveMode.Create))
                            {
                                archive.CreateEntryFromFile(path, name + GetDatefromSaveFile(picked)); //[MAIN NAME]$2022.07.04-18.57.56.savegame
                                MessageBox.Show(String.Format("Saved in Desktop as 'AstroSave.zip' and named as '{0}' : " + desktop + @"\AstroSave.zip", name));
                                filetoDelete = desktop + @"\AstroSave.zip";
                                txtRename.Text = String.Empty;
                            }
                        }
                    }
                    else if (txtRename.Text != String.Empty && !re.IsMatch(txtRename.Text))
                    {
                        MessageBox.Show("Name specified can only have letters, numbers, hyphens '-' and underscores '_'", "Name invalid Format");
                        txtRename.Focus();
                        txtRename.SelectAll();
                    }
                }
                else if (listBox1.SelectedIndex == -1)
                {
                    MessageBox.Show("No save file selected", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Dumb Dumb");
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                btnButton.IsEnabled = true;
                isPath = true;
                var item = listBox1.SelectedItem.ToString();
                if (!item.EndsWith('e'))
                {
                    item = item.Substring(0, item.Length - 13);
                }
                picked = item;
                path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Astro\Saved\SaveGames" + @"\" + picked;
            }
        }
        private void btnFindLoc_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            //ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Astro\Saved\SaveGames";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ofd.Filter = "Select Type|*.*|SAVEGAME files (*.savegame)|*.savegame|ZIP files (*.zip)|*.zip|All Files (*.*)|*.*";
            ofd.Title = "Look for your save!";
            ofd.ShowDialog();
            var filePicked = ofd.FileName;
            importName = ofd.SafeFileName;
            txtImport.Text = filePicked;
        }

        private void txtImport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtImport.SelectAll();
        }
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (txtImport.Text == string.Empty)
            {
                MessageBox.Show("Must specify save file path.", "Bruh...", MessageBoxButton.OK, MessageBoxImage.Error);
                txtImport.Focus();
            }
            else
            {
                if (System.IO.Path.GetExtension(importPicked) == ".zip")
                {
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(importPicked))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                entry.ExtractToFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Astro\Saved\SaveGames\" + entry.Name);
                            }
                        }
                        System.IO.File.Delete(importPicked);
                        MessageBox.Show("Save file imported succesfully and named as 'NEWHOST'.", "POG success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (System.IO.Path.GetExtension(importPicked) == ".savegame")
                {
                    try
                    {
                        File.Move(txtImport.Text, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Astro\Saved\SaveGames\" + importName, true);
                        System.IO.File.Delete(importPicked);
                        MessageBox.Show("Save file imported succesfully and named as 'NEWHOST'.", "POG success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        txtImport.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Must select a .zip or .savegame file", "Bruh...");
                    txtImport.Focus();
                }
                txtImport.Text = string.Empty;
            }
        }

        private void txtImport_TextChanged(object sender, TextChangedEventArgs e)
        {
            importPicked = txtImport.Text;
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadSaves();
        }

        private void txtImport_DragEnter(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileList != null && fileList.Any())
                txtImport.Text = fileList.First();
            else
                MessageBox.Show("no string");
        }

        private void txtImport_Drop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileList != null && fileList.Any())
                txtImport.Text = fileList.First();
            else
                MessageBox.Show("no string");
        }
        private void txtImport_DragOver(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (fileList != null && fileList.Any())
                txtImport.Text = fileList.First();
            else
                MessageBox.Show("no string");
        }
    }
}