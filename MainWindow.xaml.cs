using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Timers;
using System.Diagnostics;

namespace StableMarriage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int n = 0;

        int inputFileNumber = 1;

        Stopwatch watch = new Stopwatch();

        public ObservableCollection<Person> Persons = new ObservableCollection<Person>();

        public ObservableCollection<string> StableMatches = new ObservableCollection<string>();

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            openFileDialog1.Multiselect = false;

            // Process input if the user clicked OK.
            if (openFileDialog1.ShowDialog() == true)
            {
                watch = Stopwatch.StartNew();
                Persons.Clear();
                StableMatches.Clear();
                txtFilePath.Text = openFileDialog1.FileName;
                string[] filelines = File.ReadAllLines(txtFilePath.Text);
                if (filelines != null)
                {
                    n = Convert.ToInt32(filelines[0]);
                    for (int i = 1; i < filelines.Count(); i++)
                    {
                        string line = filelines[i];
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] names = line.Split(new[] { ":" }, StringSplitOptions.None);
                            string personName = names[0];
                            Person personObj = new Person();
                            personObj.Name = personName;
                            personObj.IsEngaged = false;
                            Persons.Add(personObj);
                        }
                    }
                    lstPersons.ItemsSource = Persons;
                    lstPersons.DisplayMemberPath = "Name";
                    lstPersons.UpdateLayout();

                    for (int i = 1; i < filelines.Count(); i++)
                    {
                        string line = filelines[i];
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] names = line.Split(new[] { ":" }, StringSplitOptions.None);
                            string personName = names[0];
                            Person personObj = Persons.FirstOrDefault(person => person.Name == personName);
                            string preferences = names[1];
                            string[] individualPreference = preferences.Split(new[] { "," }, StringSplitOptions.None);
                            for (int j = 0; j < individualPreference.Count(); j++)
                            {
                                Person preferredPerson = Persons.FirstOrDefault(person => person.Name == individualPreference[j].ToString());
                                personObj.PreferenceList.Add(preferredPerson);
                            }
                        }
                    }
                }
                CalculateStableMatch(openFileDialog1.SafeFileName);
            }
        }

        private void CalculateStableMatch(string inputfileName)
        {
            StableMatches.Clear();
            ObservableCollection<Person> men = new ObservableCollection<Person>((Persons[Persons.Count-1]).PreferenceList);
            List<Person> availableMen = new List<Person>(men.ToList());
            while (availableMen.Count > 0)
            {
                foreach (Person man in men)
                {
                    if (man.Match == null)
                    {
                        for (int p = 0; p < man.PreferenceList.Count; p++)
                        {
                            Person woman = man.PreferenceList[p];
                            if (woman.Match == null)
                            {
                                man.EngageTo(woman);
                                availableMen.Remove(man);
                                break;
                            }
                            else if (woman.Prefers(man))
                            {
                                availableMen.Add(woman.Match);
                                man.EngageTo(woman);
                                break;
                            }
                        }
                        //if (man.candidateIndex == man.PreferenceList.Count/2)
                        //    MessageBox.Show("last choice");
                    }
                    else
                        availableMen.Remove(man);
                }
            }
            List<string> matches = new List<string>();
            foreach (Person man in men)
            {
                matches.Add(man.Name + " is engaged to " + man.Match.Name);
            }
            matches.Sort();
            StableMatches = new ObservableCollection<string>(matches);
            lstStableMatches.ItemsSource = StableMatches;
            string filename = "output_" + inputfileName + ".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\" + filename))
            {
                for (int i = 0; i < StableMatches.Count; i++)
                    file.WriteLine(StableMatches[i]);
            }
            watch.Stop();
            txtTime.Text = watch.Elapsed.TotalSeconds.ToString() + " seconds";
            watch.Reset();
        }

        public void CreateInput(int n)
        {
            ObservableCollection<Person> menObservableCollection = new ObservableCollection<Person>();
            ObservableCollection<Person> womenObservableCollection = new ObservableCollection<Person>();
            for (int i = 0; i < n; i++)
            {
                Person obj = new Person();
                obj.Name = "m_" + (i+1);
                menObservableCollection.Add(obj);
            }

            for (int i = 0; i < n; i++)
            {
                Person obj = new Person();
                obj.Name = "w_" + (i + 1);
                womenObservableCollection.Add(obj);
            }

            for (int m = 0; m < menObservableCollection.Count; m++ )
            {
                Person person = menObservableCollection[m];
                ObservableCollection<Person> womenRandomObservableCollection = new ObservableCollection<Person>(womenObservableCollection.OrderBy(o => Guid.NewGuid().ToString()).ToList());
                for (int i = 0; i < n; i++)
                {
                    person.PreferenceList.Add(womenRandomObservableCollection[i]);
                }
            }

            for (int w = 0; w < womenObservableCollection.Count; w++)
            {
                Person person = womenObservableCollection[w];
                ObservableCollection<Person> menRandomObservableCollection = new ObservableCollection<Person>(menObservableCollection.OrderBy(o => Guid.NewGuid().ToString()).ToList());
                for (int i = 0; i < n; i++)
                {
                    person.PreferenceList.Add(menRandomObservableCollection[i]);
                }
            }

            string fileName = "input_" + inputFileNumber + ".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\" + fileName))
            {
                file.WriteLine(n);
                file.WriteLine();

                foreach (Person man in menObservableCollection)
                {
                    string line = man.Name + ":"+man.PreferenceList[0].Name;
                    for (int i = 1; i < man.PreferenceList.Count; i++)
                    {
                        line = line + "," + man.PreferenceList[i].Name;
                    }
                    file.WriteLine(line);
                }

                file.WriteLine();

                foreach (Person woman in womenObservableCollection)
                {
                    string line = woman.Name + ":" + woman.PreferenceList[0].Name;
                    for (int i = 1; i < woman.PreferenceList.Count; i++)
                    {
                        line = line + "," + woman.PreferenceList[i].Name;
                    }
                    file.WriteLine(line);
                }
            }
            inputFileNumber++;
        }

        private void btnCreateInputFiles_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter number of men and women", "Preferences", "0", -1, -1);
            if (!string.IsNullOrEmpty(input))
            {
                int n;
                if(Int32.TryParse(input, out n))
                {
                    CreateInput(n);
                }
            }
        }
    }
}
