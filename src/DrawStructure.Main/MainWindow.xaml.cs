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

namespace DrawStructure.Main
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

        #region menu routed commands

        public static readonly RoutedCommand ImportGedcomCommand = new RoutedCommand("ImportGedcom", typeof(MainWindow));
        public static readonly RoutedCommand ExportGedcomCommand = new RoutedCommand("ExportGedcom", typeof(MainWindow));
        public static readonly RoutedCommand WhatIsGedcomCommand = new RoutedCommand("WhatIsGedcom", typeof(MainWindow));
        public static readonly RoutedCommand ExportXpsCommand = new RoutedCommand("ExportXps", typeof(MainWindow));
        public static readonly RoutedCommand ChangeSkinCommand = new RoutedCommand("ChangeSkin", typeof(MainWindow));

        #endregion

        private void ShowPersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {

        }

        private void HidePersonInfo_StoryboardCompleted(object sender, EventArgs e)
        {

        }

        private void ShowFamilyData_StoryboardCompleted(object sender, EventArgs e)
        {

        }

        private void HideFamilyData_StoryboardCompleted(object sender, EventArgs e)
        {

        }

        private void Vertigo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SaveFamilyAs(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveFamily(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void OpenFamily(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void NewFamily(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void PrintFamily(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
