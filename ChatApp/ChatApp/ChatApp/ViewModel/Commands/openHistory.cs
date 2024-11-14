using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.ViewModel;
using ChatApp.Model;

namespace ChatApp.ViewModel.Commands
{
    internal class openHistory : ICommand
    {
        private ChattViewModel _cvm;
        private HistoryViewModel _hvm;
        private Action<object> _execute;

        public openHistory(ChattViewModel cvm, HistoryViewModel hvm)
        {
            _cvm = cvm;
            _hvm = hvm;

        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _cvm.OpenHistoryWindow(_hvm);
            Console.WriteLine($"Button clicked for ChatHistory");
           
        }   
    }
}
