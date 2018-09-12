using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Indivirtual.Hackathon.GroceryStockBot.States
{
    public class ReminderState : BaseState
    {
        public ReminderState() : base(null) { }

        public ReminderState(IDictionary<string, object> source = null) : base(source) { }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }

    public class StockState : BaseState
    {
        public StockState() : base(null) { }

        public StockState(IDictionary<string, object> source = null) : base(source) { }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
