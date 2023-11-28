using System.Windows;
using Hurst.LogNut;


namespace VsDevTool.TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LogManager.LogDebug( "About to display the Program-Version" );

            Title = "TestProject,  ProgramVersion " + App.The.ProgramVersionText;
        }
    }
}
