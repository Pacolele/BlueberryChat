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
using System.Windows.Shapes;
using ChatApp.Model;
using ChatApp.ViewModel;

namespace ChatApp.View
{
    /// Interaction logic for CreateProfile.xaml
    public partial class CreateProfile : Window
    {
        public CreateProfile()
        {
            InitializeComponent();
            CreateProfileViewModel viewModel = new CreateProfileViewModel(new NetworkManager());
            DataContext = viewModel;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
