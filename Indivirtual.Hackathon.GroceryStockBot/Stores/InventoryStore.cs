using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;

namespace Indivirtual.Hackathon.GroceryStockBot.Stores
{
    public class InventoryStoreItem : IStoreItem
    {
        public string eTag { get ; set; }
    }

    public class CartStoreItem : IStoreItem
    {
        public string eTag { get; set; }
    }
}
