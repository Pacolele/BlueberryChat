using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    public class closeHistory: ICommand
    {
        private bool action = false;
       
        public event EventHandler? CanExecuteChanged;

        private HistoryViewModel? _parent;
        private HistoryViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public closeHistory(HistoryViewModel parent)
        {
            this.Parent = parent;
           
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.CloseHistory();
        }
    }
}
