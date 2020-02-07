using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BasicInjector
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow _instance;
        private Injection inj = new Injection();
        string dllloc;

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            GetRunningApps();
        }

        void GetRunningApps()
        {
            foreach (Process p in Process.GetProcesses("."))
            {
                try
                {
                    if (p.MainWindowTitle.Length > 0)
                    {
                        ComboboxItem combobox = new ComboboxItem();
                        combobox.Text = p.MainWindowTitle;
                        combobox.Value = p.Id;
                        procListBox.Items.Add(combobox);
                    }
                }
                catch { }
            }
        }

        private void Inject_Click(object sender, RoutedEventArgs e)
        {
            if (procListBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("No Selected Process", "ERROR");
                return;
            }
            int id = Convert.ToInt32((procListBox.SelectedItem as ComboboxItem).Value);
            if (!inj.InjectionPrep(dllloc, id))
            {
                System.Windows.MessageBox.Show("Couldnt inject the DLL!", "ERROR");
                return;
            }
        }

        private void OpenDllDirec_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.Filter = "dll|*.DLL";
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                dllloc = openFileDlg.FileName;
                AppendTextBox("Loading DLL: " + dllloc);
            }
        }

        public void AppendTextBox(string value)
        {
            logbox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate ()
            {
                logbox.Text += value + Environment.NewLine;
            }));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (procListBox.Items.Count != 0)
            {
                AppendTextBox("Changed Process to: " + (procListBox.SelectedValue as ComboboxItem).Text + "\nProcess ID: " + (procListBox.SelectedValue as ComboboxItem).Value);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            procListBox.Items.Clear();
            GetRunningApps();
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
