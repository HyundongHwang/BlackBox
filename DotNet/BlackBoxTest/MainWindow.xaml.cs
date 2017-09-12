using BlackBoxLib;
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

namespace BlackBoxTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.BtnHello.Click += BtnHello_Click;
            this.BtnWorld.Click += BtnWorld_Click;
            this.BtnSession.Click += BtnSession_Click;
            this.BtnCaptureScreen.Click += BtnCaptureScreen_Click;
        }

        private void BtnHello_Click(object sender, RoutedEventArgs e)
        {
            BlackBox.d("hello");
        }

        private void BtnWorld_Click(object sender, RoutedEventArgs e)
        {
            BlackBox.i("world");
        }

        private void BtnSession_Click(object sender, RoutedEventArgs e)
        {
            BlackBox.session("h2d2002@naver.com");
        }

        private void BtnCaptureScreen_Click(object sender, RoutedEventArgs e)
        {
            BlackBox.CaptureScreen();
        }
    }
}
