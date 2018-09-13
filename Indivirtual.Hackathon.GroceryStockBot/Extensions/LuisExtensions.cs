using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace Indivirtual.Hackathon.GroceryStockBot.Extensions
{
    public static class LuisExtensions
    {
        // Get entities from LUIS result
        public static T GetEntity<T>(this RecognizerResult luisResult, string entityKey)
        {
            var data = luisResult.Entities as IDictionary<string, JToken>;
            if (data.TryGetValue(entityKey, out JToken value))
            {
                return value.First.Value<T>();
            }
            return default(T);
        }
    }
}
