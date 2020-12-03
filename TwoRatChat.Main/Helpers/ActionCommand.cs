using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TwoRatChat.Model {
    public class ActionCommand : ICommand {
        readonly Action _action;
        readonly object _owner;

        public ActionCommand( object owner, Action action ) {
            this._action = action;
            this._owner = owner;
        }

        public bool CanExecute( object parameter ) {
            return true; // TODO: Подумать про потоки загрузки
        }

        public event EventHandler CanExecuteChanged;

        public void Execute( object parameter ) {
            if (_action != null)
                _action();
        }
    }

    public class ParametrizedActionCommand : ICommand {
        readonly Action<object> _action;
        readonly object _owner;

        public ParametrizedActionCommand( object owner, Action<object> action ) {
            this._action = action;
            this._owner = owner;
        }

        public bool CanExecute( object parameter ) {
            return true; // TODO: Подумать про потоки загрузки
        }

        public event EventHandler CanExecuteChanged;

        public void Execute( object parameter ) {
            if (_action != null)
                _action(parameter);
        }
    }
}
