using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class JoinChat : ICommand
    {
        private bool action = false;
        private CreateProfileViewModel _parent;
        private CreateProfileViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public JoinChat(CreateProfileViewModel parent)
        {
            this.Parent = parent;

        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.joinConnection();
        }
    }
}