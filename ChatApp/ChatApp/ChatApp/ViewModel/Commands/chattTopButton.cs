using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Model.Interface;

namespace ChatApp.ViewModel.Commands
{
    public class chattTopButton<TViewModel>  : ICommand where TViewModel : IChattTopBar
    {
        private bool action = false;
        private string Pressed;
        public event EventHandler? CanExecuteChanged;

        private TViewModel Parent
        { get; set;}

        public chattTopButton(TViewModel parent, string pressed)
        {
            this.Parent = parent;
            this.Pressed = pressed;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {   
            Parent.ChattWindowHandler(Pressed);
        }
    }
}
