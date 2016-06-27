using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Diagnostics;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using TestBot.Controllers;
namespace TestBot
{
    [Serializable]

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
                if (message.Text.Equals("hi"))
                    return message.CreateReplyMessage("hello");

                LUISToSql lReply = new LUISToSql();
                LuisModelAttribute shoppingModel = new LuisModelAttribute("be32716c-0d3f-4df6-bacf-bf809547d67a", "8e313738104945008db930cb54f355a7");
                LuisService shoppingService = new LuisService(shoppingModel);
                LuisResult LuisResponse = await shoppingService.QueryAsync(message.Text);
                reply = lReply.QueryToData(LuisResponse);

                // return our reply to the user
                return message.CreateReplyMessage($"Result :\n {reply}");
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