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

        public static readonly RoutedCommand ExportXpsCommand = new RoutedCommand("ExportXps", typeof(MainWindow));
        public static readonly RoutedCommand ChangeSkinCommand = new RoutedCommand("ChangeSkin", typeof(MainWindow));

        UMLCollection family =GlobalData.Family;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {// Create a new person with the specified inputs
            Shape newShape = new Shape("Test");
            newShape.Comments = new Comments();
            newShape.Avatar = "*";
            newShape.Name = "ShapeName";


            family.Current = newShape;
            family.Add(newShape);

            Shape newShape2 = new Shape("Test2");
            newShape2.Comments = new Comments();
            newShape2.Avatar = "*";
            newShape2.Name = "ShapeName2";

            newShape2.Parents.Add(newShape);

            newShape.Children.Add(newShape2);
            family.Add(newShape2);
          

            family.OnContentChanged();

        }
    }
}
