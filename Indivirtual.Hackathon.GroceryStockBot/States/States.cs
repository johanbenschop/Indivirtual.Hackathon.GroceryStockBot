using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Indivirtual.Hackathon.GroceryStockBot.States
{
    public class BaseState : Dictionary<string, object>
    {
        public BaseState(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => Add(x.Key, x.Value));
            }
        }

        protected T GetProperty<T>([CallerMemberName]string propName = null)
        {
            if (TryGetValue(propName, out var value))
            {
                return (T)value;
            }
            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName]string propName = null)
        {
            this[propName] = value;
        }
    }

    public class Reminder : BaseState
    {
        public Reminder() : base(null) { }

        public Reminder(IDictionary<string, object> source = null) : base(source) { }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }

    public class UserState : BaseState
    {
        public UserState() : base(null) { }

        public UserState(IDictionary<string, object> source) : base(source) { }

        public IList<Reminder> Reminders
        {
            get { return GetProperty<IList<Reminder>>(); }
            set { SetProperty(value); }
        }
    }
}
