using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.ViewModel;

namespace ChatApp.ViewModel.Commands
{
    internal class StartChatt : ICommand
    {
        private CreateProfileViewModel _parent;
        private CreateProfileViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public StartChatt(CreateProfileViewModel parent) {
            this.Parent = parent;
        
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.startConnection();
        }
    }
}
