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
using System.Device.Location;
using GoogleMaps.LocationServices;

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
        /// 
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

                //getting current location co-ordinates
                GoogleLocationService service = new GoogleLocationService(true);
                MapPoint currentPoint = service.GetLatLongFromAddress("Hyderabad");
                
                GeoCoordinate userLocation = new GeoCoordinate(currentPoint.Latitude, currentPoint.Longitude);
                

                LuisResult LuisResponse = await shoppingService.QueryAsync(message.Text);
                
                reply = lReply.QueryToData(LuisResponse,userLocation);

                // return our reply to the user
                if (reply == null)
                    return message.CreateReplyMessage($"sorry i do not understand");
                
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
                return message.CreateReplyMessage("Pong");
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
                //get user location from api call
                
                
                
                
                return message.CreateReplyMessage("Hi there... How can you help you?");
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
                return message.CreateReplyMessage("Bye.. Have a nice day");
            }
            else if (message.Type == "UserAddedToConversation")
            {
                return message.CreateReplyMessage("Hi "+message.BotUserData);
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
                return message.CreateReplyMessage("Bye "+message.BotUserData);
            }
            else if (message.Type == "EndOfConversation")
            {
                return message.CreateReplyMessage("Bye.. Have a nice day");
            }

            return null;
        }
    }
}