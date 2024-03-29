﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using WinForm = System.Windows.Forms;
using System.Text.RegularExpressions;
using Matcher.Properties;
using System.IO.Packaging;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Xml;


namespace Matcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static object o = new object();

        private BindingList<ListRefer> bindListRefer = new BindingList<ListRefer>(); // биндинг для таблицы 1
        private BindingList<MatchesList> bindMatches = new BindingList<MatchesList>(); // биндинг для таблицы 2

        static string fileRefer; // файл образец - flRef
        static string dirRefer; // папка образец - drRef
        static string referenceFile; // файл образец - refer
        static bool oneFile = false; // чекбокс - только один файл - one
        static bool content = false; // чекбокс - по содержжимому - content
        static string dirTarget; // папка для поиска - drTar
        static bool matching = false; // сигнализатор работы программы

        static List<ListRefer> listRefers = new List<ListRefer>(); // список файлов в первой папке
        static List<MatchesList> matchesLists = new List<MatchesList>(); // список файлов во второй папке

        static List<FileInfo> alRef = new List<FileInfo>();  // список файлов в  первой папке
        static List<FileInfo> all = new List<FileInfo>(); // список файлов во второй папке

        static List<ListRefer> referCompare = new List<ListRefer>(); // список совпадающих файлов из первой папки-образца со второй папкой
        static List<MatchesList> dirCompare = new List<MatchesList>(); // список совпадающих файлов второй папки с файлами из первой папки

        static List<Directories> directories = new List<Directories>(); // список совпадающих директорий

        public MainWindow()
        {
            InitializeComponent();
            referenceFile = Settings.Default["refer"].ToString();
            fileRefer = Settings.Default["flRef"].ToString();
            dirRefer = Settings.Default["drRef"].ToString();
            oneFile = (bool)Settings.Default["one"];
            content = (bool)Settings.Default["content"];
            dirTarget = Settings.Default["drTar"].ToString();
            Refer.Text = referenceFile;
            dirFolder.Text = dirTarget;
            if (oneFile)
            {
                chckOneFile.IsChecked = true;
            }
            if (content)
            {
                checkContent.IsChecked = true;
            }
        }

        // клик выбора файла для первой папки
        private void btnRefer_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (dirRefer != null)
                {
                    openFileDialog.InitialDirectory = dirRefer;
                }
                referenceFile = openFileDialog.FileName;
                Settings.Default["refer"] = referenceFile;
                Settings.Default.Save();
                fileRefer = openFileDialog.SafeFileName;
                Settings.Default["flRef"] = fileRefer;
                Settings.Default.Save();
                //Regex reg = new Regex(referenceFile);
                //dirRefer = Regex.Replace(referenceFile, fileRefer, "");
                dirRefer = referenceFile.Substring(0, dirRefer.Length - (fileRefer.Length + 1));
                Settings.Default["drRef"] = dirRefer;
                Settings.Default.Save();
                Refer.Text = referenceFile;
            }
        }

        // клик очистки настроек
        private void delSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default["flRef"] = "";
            Settings.Default["refer"] = "";
            Settings.Default["drRef"] = "";
            Settings.Default["one"] = false;
            Settings.Default["content"] = false;
            Settings.Default["drTar"] = "";
            Settings.Default.Save();
            Refer.Text = "";
            dirFolder.Text = "";
            chckOneFile.IsChecked = false;
            checkContent.IsChecked = false;
            fileRefer = dirRefer = dirTarget = "";
            oneFile = false;
            content = false;
        }

        // чек бокс только один файл
        private void chckOneFile_Click(object sender, RoutedEventArgs e)
        {
            if (chckOneFile.IsChecked == true)
            {
                Settings.Default["one"] = oneFile = true;
                Settings.Default.Save();
            }
            else
            {
                Settings.Default["one"] = oneFile = false;
                Settings.Default.Save();
            }
        }

        // обработка клика кнопки выбора папки 2
        private void btnDir_Click(object sender, RoutedEventArgs e)
        {
            WinForm.FolderBrowserDialog folderBrowser = new WinForm.FolderBrowserDialog();
            if (dirTarget.Length > 0)
            {
                folderBrowser.SelectedPath = dirTarget;
            }

            if (folderBrowser.ShowDialog() == WinForm.DialogResult.OK)
            {
                dirTarget = folderBrowser.SelectedPath;
                Settings.Default["drTar"] = dirTarget;
                Settings.Default.Save();
                dirFolder.Text = dirTarget;
            }
        }

        // обработка клика на кнопку начать поиск
        private void startMatching_Click(object sender, RoutedEventArgs e)
        {
            if (matching)
            {
                matching = false;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    startMatching.IsEnabled = false;
                }));
                Thread.Sleep(10);
                new Thread(() => endOfPrg()).Start();
            }
            else
            {
                if (bindMatches.Count > 0 || alRef.Count > 0)
                {
                    clearLists();
                }
                Thread.Sleep(4);
                matching = true;

                // блокировка элементов на время работы программы
                new Thread(() => blockElems()).Start();
                Thread.Sleep(3);

                // получение файлов из первой папки
                new Thread(() => dirReference()).Start();
                Thread.Sleep(3);

                // получение файлов из второй папки
                new Thread(() => FileSearchFunction(dirTarget)).Start();
                Thread.Sleep(3);

                // сравнение файлов в папках
                new Thread(() => filesCompare()).Start();
                Thread.Sleep(3);

                // заполнение первой таблицы
                new Thread(() => fillTableLeft()).Start();
                Thread.Sleep(3);

                // заполнение второй таблицы
                new Thread(() => fillTableRight()).Start();
                Thread.Sleep(4);

                // запись директорий с количеством повторений
                new Thread(() => dirCount()).Start();

                // вывод количества совпадений в папках
                new Thread(() => countOut()).Start();
                Thread.Sleep(3);

                // завершение, разблокировка элементов и matching = false
                new Thread(() => endOfPrg()).Start();
            }
        }

        // метод для отдельного потока разблокировка элементов и matching = false
        private void endOfPrg()
        {
            lock (o)
            {
                unBlockElems();
                matching = false;
            }
        }

        // вывод количества совпадений в папках
        private void countOut()
        {
            lock (o)
            {
                if (!oneFile && dirCompare.Count != 0 && matching)
                {
                    if (directories.Count != 0)
                    {
                        int max = 0;
                        int num = 0;
                        for (int i = 0; i < directories.Count; i++)
                        {
                            if (max < directories[i].Cnt)
                            {
                                max = directories[i].Cnt;
                                num = i;
                            }
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            infoRef.Content = "Папка " + directories[num].NameDir;
                            infoDir.Content = "имеет " + directories[num].Cnt + " совпадений файлов из " + listRefers.Count;

                        }));


                        for (int i = 0; i < dirCompare.Count; i++)
                        {
                            if (dirCompare[i].Dir.Equals(directories[num].NameDir))
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    dataTable2.SelectedItem = dataTable2.Items[i];
                                    dataTable2.ScrollIntoView(dataTable2.Items[i]);
                                }));
                                break;
                            }
                        }
                    }
                }
            }
        }

        // метод сравнения директорий
        private void dirCount()
        {
            lock (o)
            {
                if (!oneFile && dirCompare.Count != 0 && matching)
                {
                    Directories dirD = new Directories(dirCompare[0].Dir, 1);
                    directories.Add(dirD);
                    for (int i = 1; i < dirCompare.Count; i++)
                    {
                        bool yes = true;
                        string d = dirCompare[i].Dir;
                        dirD = new Directories(d, 1);
                        for (int j = 0; j < directories.Count; j++)
                        {
                            if (directories[j].NameDir.Equals(d))
                            {
                                directories[j].Cnt++;
                                yes = false;
                                break;
                            }
                        }
                        if (yes)
                        {
                            directories.Add(dirD);
                        }
                    }
                }
            }
        }

        // заполнение таблицы 1
        private void fillTableLeft()
        {
            lock (o)
            {
                if (matching)
                {
                    for (int i = 0; i < listRefers.Count; i++)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            bindListRefer.Add(listRefers[i]);
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        dataTabel1.ItemsSource = bindListRefer;
                    }));
                }
            }
        }

        // заполнение таблицы 2
        private void fillTableRight()
        {
            lock (o)
            {
                if (matching)
                {
                    for (int i = 0; i < dirCompare.Count; i++)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            bindMatches.Add(dirCompare[i]);
                        }));
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        dataTable2.ItemsSource = bindMatches;
                    }));
                }
            }
        }

        // метод сравнения файлов
        private void filesCompare()
        {
            lock (o)
            {
                if (matching)
                {
                    for (int i = 0; i < listRefers.Count; i++)
                    {
                        referCompare.Add(listRefers[i]);
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            infoRef.Content = listRefers[i].Name;
                        }));
                        for (int j = 0; j < matchesLists.Count; j++)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                infoDir.Content = matchesLists[j].Name;
                            }));
                            if (listRefers[i].Name.Equals(matchesLists[j].Name))
                            {
                                if (matching)
                                {
                                    dirCompare.Add(matchesLists[j]);
                                    dirCompare[dirCompare.Count - 1].ID = referCompare[referCompare.Count - 1].ID;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (listRefers[i].Size.Equals(matchesLists[j].Size) && content)
                                {
                                    if (matchingBytes(i, j))
                                    {
                                        if (matching)
                                        {
                                            dirCompare.Add(matchesLists[j]);
                                            dirCompare[dirCompare.Count - 1].ID = referCompare[referCompare.Count - 1].ID;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    infoDir.Content = "";
                    infoRef.Content = "";
                }));
            }
        }

        // метод сравнения файлов побайтово
        private bool matchingBytes(int a, int b)
        {
            int filebit1, filebit2;
            bool res = true;
            FileStream fs1, fs2;
            fs1 = new FileStream(alRef[a].FullName, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(all[b].FullName, FileMode.Open, FileAccess.Read);
            int cnt = 0;
            filebit1 = fs1.ReadByte();
            filebit2 = fs2.ReadByte();
            while (cnt < 300000000 && filebit1 != -1)
            {
                cnt++;
                if (filebit1 != filebit2)
                {
                    res = false;
                    break;
                }
                filebit1 = fs1.ReadByte();
                filebit2 = fs2.ReadByte();
            }
            fs1.Close();
            fs2.Close();
            return res;
        }

        // метод разблокировки элементов
        private void unBlockElems()
        {
            lock (o)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    startMatching.Background = new SolidColorBrush(Color.FromRgb(0, 250, 154));
                    startMatching.Content = "Начать поиск";
                    chckOneFile.IsEnabled = true;
                    Refer.IsEnabled = true;
                    dirFolder.IsEnabled = true;
                    checkContent.IsEnabled = true;
                    delSettings.IsEnabled = true;
                    btnRefer.IsEnabled = true;
                    btnDir.IsEnabled = true;
                    startMatching.IsEnabled = true;
                }));
            }
        }

        // метод сбора информации о папке 1
        private void dirReference()
        {
            lock (o)
            {
                if (matching)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (oneFile)
                        {
                            try
                            {
                                FileInfo fi = new FileInfo(referenceFile);
                                alRef.Add(fi);
                                ListRefer lf = new ListRefer(fi.Name, 0, fi.Length, false);
                                listRefers.Add(lf);
                                Refer.Text = referenceFile;
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("error: " + e);
                                Refer.Text = "Что-то здесь не то...";
                                matching = false;
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(dirRefer);
                                int cnt = 0;
                                foreach (FileInfo ff in DI.GetFiles())
                                {
                                    alRef.Add(ff);
                                    ListRefer lr = new ListRefer(ff.Name, cnt, ff.Length, false);
                                    listRefers.Add(lr);
                                    cnt++;
                                }
                                Refer.Text = referenceFile;
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("error: " + e);
                                Refer.Text = "Что-то здесь не то...";
                                matching = false;
                                return;
                            }
                        }
                    });
                }
            }
        }

        // метод блокирования элементов при работе программы
        private void blockElems()
        {
            lock (o)
            {
                this.Dispatcher.Invoke(() =>
                {
                    startMatching.Content = "Остановить поиск";
                    startMatching.Background = new SolidColorBrush(Color.FromRgb(250, 128, 114));
                    chckOneFile.IsEnabled = false;
                    Refer.IsEnabled = false;
                    dirFolder.IsEnabled = false;
                    delSettings.IsEnabled = false;
                    btnRefer.IsEnabled = false;
                    checkContent.IsEnabled = false;
                    btnDir.IsEnabled = false;
                });
            }
        }

        // метод очистки списков
        private void clearLists()
        {
            alRef.Clear();
            all.Clear();
            directories.Clear();
            matchesLists.Clear();
            listRefers.Clear();
            referCompare.Clear();
            dirCompare.Clear();
            bindListRefer.Clear();
            bindMatches.Clear();
            infoDir.Content = "";
            infoRef.Content = "";
        }

        // метод сбора файлов в папке номер 2
        private void FileSearchFunction(string Dir)
        {
            lock (o)
            {
                if (matching)
                {
                    try
                    {
                        int cnt = 0;
                        System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(Dir);
                        System.IO.DirectoryInfo[] SubDir = DI.GetDirectories();
                        for (int i = 0; i < SubDir.Length; ++i)
                        {
                            if (!matching)
                            {
                                break;
                            }
                            this.FileSearchFunction(SubDir[i].FullName);
                        }
                        System.IO.FileInfo[] FI = DI.GetFiles();
                        foreach (FileInfo f in FI)
                        {
                            if (!matching)
                            {

                                break;
                            }
                            all.Add(f);
                            MatchesList mList = new MatchesList(f.Name, f.FullName, f.DirectoryName, cnt, f.Length, false);
                            cnt++;
                            matchesLists.Add(mList);
                        }
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            dirFolder.Text = Dir;
                        }));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("error: " + e);
                        matching = false;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            dirFolder.Text = "Здесь что-то не то...";
                        }));
                    }
                }
            }
        }

        // клик по таблице 2 выбор строки
        private void dataTable2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MatchesList ml = dataTable2.SelectedItem as MatchesList;
            if (ml != null)
            {
                string nameFile = Convert.ToString(ml.FullName);
                int id2 = ml.ID;

                dataTabel1.SelectedItem = dataTabel1.Items[id2];
                dataTabel1.ScrollIntoView(dataTabel1.Items[id2]);
            }
        }

        // клик на чекбокс - по содержимому
        private void checkContent_Click(object sender, RoutedEventArgs e)
        {
            if (checkContent.IsChecked == true)
            {
                Settings.Default["content"] = content = true;
                Settings.Default.Save();
            }
            else
            {
                Settings.Default["content"] = content = false;
                Settings.Default.Save();
            }
        }

        //  двойной клик по правой таблице
        private void dataTable2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MatchesList ml2click = dataTable2.SelectedItem as MatchesList;
            if (ml2click != null)
            {
                string nameFile = Convert.ToString(ml2click.FullName);
                Cmd(nameFile);
            }
        }

        // метод открытия окна браузера с выделенным файлом
        void Cmd(string line)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"/n, /select, {line}"
            });
        }
    }
}
