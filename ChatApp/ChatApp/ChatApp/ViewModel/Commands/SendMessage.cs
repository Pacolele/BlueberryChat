using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class SendMessages : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private ChattViewModel _parent;
        private ChattViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public SendMessages(ChattViewModel parent) {
            this.Parent = parent;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.handleSendMessage();
        }
    }
}
