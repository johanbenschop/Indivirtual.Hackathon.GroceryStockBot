using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indivirtual.Hackathon.GroceryStockBot.DataContexts;
using Indivirtual.Hackathon.GroceryStockBot.Extensions;
using Indivirtual.Hackathon.GroceryStockBot.States;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Prompts = Microsoft.Bot.Builder.Prompts;

namespace Indivirtual.Hackathon.GroceryStockBot // Indivirtual_Hackathon_GroceryStockBot
{
    public class LuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet dialogs;
        private readonly DataContext _dataContext;

        public LuisBot(DataContext dataContext)
        {
            dialogs = new DialogSet();
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("AddToCart", new WaterfallStep[] { AddToCart });
            dialogs.Add("ProductPrompt", new TextPrompt(ProductPrompt));
            dialogs.Add("RemoveFromCart", new WaterfallStep[] { RemoveFromCart });
            dialogs.Add("CheckInventory", new WaterfallStep[] { CheckInventory });
            dialogs.Add("ShowOverviewBonus", new WaterfallStep[] { ShowOverviewBonus });
            dialogs.Add("WhatIsCheapest", new WaterfallStep[] { WhatIsCheapest });
            dialogs.Add("OrderDate", new WaterfallStep[] { OrderDate });
            dialogs.Add("OrderOverview", new WaterfallStep[] { OrderOverview });
            dialogs.Add("CheckAmountofProduct", new WaterfallStep[] { CheckAmountofProduct });
            _dataContext = dataContext;
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivity("Hi!");
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var userState = turnContext.GetUserState<UserState>();
                if (userState.Reminders == null)
                {
                    userState.Reminders = new List<ReminderState>();
                }

                var state = turnContext.GetConversationState<Dictionary<string, object>>();
                var dialogContext = dialogs.CreateContext(turnContext, state);

                var utterance = turnContext.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        dialogContext.EndAll();
                        await turnContext.SendActivity("Ok... Cancelled");
                    }
                    else
                    {
                        await turnContext.SendActivity("Nothing to cancel.");
                    }
                }

                if (!turnContext.Responded)
                {
                    await dialogContext.Continue();

                    if (!turnContext.Responded)
                    {
                        var luisResult = turnContext.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";

                        await dialogContext.Begin(intent);
                    }
                }
            }
        }

        private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity("Hey! Ik ben Indivirtual's boodschappen manager. Je kan mij vragen om iets op het boodschappen lijstje te zetten of melden wanneer iets op is.");
        }

        private async Task AddToCart(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var luisResult = dialogContext.Context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
            var productName = luisResult.GetEntity<string>("Product");
            var amount = luisResult.GetEntity<int>("Amount");

            if (!string.IsNullOrWhiteSpace(productName))
            {
                var entry = new ShoppingCartEntry { ProductName = productName, Amount = (amount > 0 ? amount : 1) };
                _dataContext.ShoppingCartEntries.Add(entry);
                _dataContext.SaveChanges();
                await dialogContext.Context.SendActivity($"Goede keus! Ik heb het op het boodschappenlijstje gezet voor je.");
            }
            else
            {
                await dialogContext.Prompt("ProductPrompt", "Sorry, wat wil je dat ik op het boodschappenlijstje zet?");
            }
        }

        private async Task ProductPrompt(ITurnContext context, Prompts.TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value))
            {
                var entry = new ShoppingCartEntry { ProductName = result.Value, Amount = 1 };
                _dataContext.ShoppingCartEntries.Add(entry);
                _dataContext.SaveChanges();
                await context.SendActivity($"Ow, dat, zeg dat dan! Staat er nu op.");

                result.Status = Prompts.PromptStatus.Recognized;
            }
            result.Status = Prompts.PromptStatus.NotRecognized;
        }

        private async Task RemoveFromCart(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Waarom wil je het verwijderen? Ga ik niet doen. Niet in mijn voordeel!");
        }

        private async Task ShowOverviewBonus(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ding ding ding! Bonus punten voor jouw dat je het durft te vragen!");
        }

        private async Task WhatIsCheapest(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Jah... dat is niet in mijn voordeel om te verklappen!");
        }

        private async Task OrderDate(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappenlijstje kwijt ben...");
        }

        private async Task OrderOverview(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var entries = _dataContext.ShoppingCartEntries.ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Op het lijstje staan nu:");

            if (entries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    builder.AppendLine($"- {entry.ProductName} x {entry.Amount}");
                }

                await dialogContext.Context.SendActivity(builder.ToString());
            }
            else
            {
                await dialogContext.Context.SendActivity($"Het boodschappenlijstje is nogal kaal! Bestel iets lekkers zou ik zeggen! Het kost jouw namelijk niks dus leef uit :smirk:");
            }
        }

        private async Task CheckAmountofProduct(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var entry = _dataContext.ShoppingCartEntries
                .Where(x => x.ProductName.Equals("", StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            await dialogContext.Context.SendActivity($"Ik heb {10} van {11} op het boodschappenlijstje staan.");
        }

        private async Task CheckInventory(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ik zal even kijken wat we hebben, moment...");

            await _dataContext.Database.EnsureCreatedAsync();

            await dialogContext.Context.SendActivity($"Ik heb even snel in de pantry gekeken en de inventorie bijgewerkt. Snel he?");
        }
    }
}
