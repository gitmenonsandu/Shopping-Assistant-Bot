using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Diagnostics;

namespace TestBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.None);
            }
            else
            {
                await context.PostAsync(string.Format("{0}: You said {1}", this.count++, message.Text));
                context.Wait(MessageReceivedAsync);
            }
        }
        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
    }

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                String reply = String.Empty;
                String SqlQuery = String.Empty;
                if (message.Text.Equals("hi"))
                    return message.CreateReplyMessage("hello");

                Rootobject LuisQuery = await LUISClient.ParseUserInput(message.Text);
                
                if(LuisQuery.intents.Length>0)
                {
                    switch(LuisQuery.intents[0].intent)
                    {
                        case "ItemNameWithPrice":
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                        case "ItemWithOffer":
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                        case "Offer":
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                        case "ItemName":
                            SqlQuery=LuisQuery.intents[0].intent;
                            for (int i = 0; i < LuisQuery.entities.Length; ++i)
                                if (LuisQuery.entities[i].type.Equals("item"))
                                {
                                    SqlQuery = "select * from ItemTable;";
                                    break;
                                }
                                else if (LuisQuery.entities[i].type.Equals("item::Name"))
                                {
                                    SqlQuery = "select * from ItemTable where lower(itemName)='" + LuisQuery.entities[i].entity.ToLower()+"';";
                                    break;
                                }

                            break;
                        case "ItemNameWithDiscount":
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                        case "None":
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                        default:
                            SqlQuery=LuisQuery.intents[0].intent;
                            break;
                    }
                }
                Debug.WriteLine(reply);
                Debug.WriteLine(SqlQuery);
                SqlLogin db = new SqlLogin();
                try {
                    reply = db.Select(SqlQuery);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                // return our reply to the user
                return message.CreateReplyMessage($"Result :\n {reply} \n {SqlQuery}");
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}