using Microsoft.Win32;
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

namespace CPPacker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
        }
    }


    public class MainWindowViewModel : CPLib.NotifyObject
    {
        private PackProject _Project;
        public PackProject Project
        {
            get { return _Project; }
            set
            {
                if (_Project != value)
                {
                    _Project = value;
                    this.OnPropertyChanged("Project");
                }
            }
        }

        public MainWindowViewModel()
        {
            this.NewProjectCommand = new CPLib.RelayCommand<object>(this.NewProjectCommandExecute, this.CanNewProjectCommandExecute);
            this.OpenProjectCommand = new CPLib.RelayCommand<object>(this.OpenProjectCommandExecute, this.CanOpenProjectCommandExecute);
            this.SaveProjectCommand = new CPLib.RelayCommand<object>(this.SaveProjectCommandExecute, this.CanSaveProjectCommandExecute);
            this.CloseProjectCommand = new CPLib.RelayCommand<object>(this.CloseProjectCommandExecute, this.CanCloseProjectCommandExecute);
        }

        public ICommand NewProjectCommand { get; set; }
        void NewProjectCommandExecute(object parameter)
        {
            if (this.Project != null)
            {
                if (!this.Project.TryCloseProject())
                    return;
            }

            this.Project = new PackProject();
            
        }
        bool CanNewProjectCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand OpenProjectCommand { get; set; }
        void OpenProjectCommandExecute(object parameter)
        {
            if (this.Project != null)
            {
                if (!this.Project.TryCloseProject())
                    return;
            }

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "*.ppro|*.ppro"
            };
            if (ofd.ShowDialog() == true)
            {
                this.Project = PackProject.LoadProject(ofd.FileName);
            }

        }
        bool CanOpenProjectCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand SaveProjectCommand { get; set; }
        void SaveProjectCommandExecute(object parameter)
        {
            this.Project.TrySaveProject();
        }
        bool CanSaveProjectCommandExecute(object parameter)
        {
            return this.Project != null;
        }

        public ICommand CloseProjectCommand { get; set; }
        void CloseProjectCommandExecute(object parameter)
        {
            if (this.Project != null)
            {
                if (this.Project.TryCloseProject())
                {
                    this.Project = null;
                }
            }
        }
        bool CanCloseProjectCommandExecute(object parameter)
        {
            return this.Project != null;
        }


    }
}
