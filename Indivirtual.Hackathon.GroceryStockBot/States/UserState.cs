using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Indivirtual.Hackathon.GroceryStockBot.States
{
    public class UserState : BaseState
    {
        public UserState() : base(null) { }

        public UserState(IDictionary<string, object> source) : base(source) { }

        public IList<ReminderState> Reminders
        {
            get { return GetProperty<IList<ReminderState>>(); }
            set { SetProperty(value); }
        }
    }
}
