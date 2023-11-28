using System.Windows;
using System.Windows.Controls;


namespace Hurst.BaseLibWpf.Display
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : UserControl
    {
        public Logo()
        {
            InitializeComponent();

            //this.Height = 50;
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.Opacity = 0.2;
            this.VerticalAlignment = VerticalAlignment.Top;
            //this.Width = 100;
        }
    }
}
