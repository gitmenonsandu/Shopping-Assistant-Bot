using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TestBot.Controllers;

namespace TestBot.Http
{

    //class to connect LUIS model via HTTP requests
    public class LUISClient
    {
        //input is the dialogue entered by the user
        public static async Task<Rootobject> ParseUserInput(string input)
        {
            string strRet = string.Empty;
            //input converted to URL format
            string strEscaped = Uri.EscapeDataString(input);
            using (var client = new HttpClient())
            {
                //input attached to the end of LUIS model URL
                string uri = "https://api.projectoxford.ai/luis/v1/application?id=be32716c-0d3f-4df6-bacf-bf809547d67a&subscription-key=8e313738104945008db930cb54f355a7&q=" + strEscaped;
                HttpResponseMessage msg = await client.GetAsync(uri);
                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject<Rootobject>(jsonResponse);
                    return _Data;
                }

            }
            return null;
        }
    }
    //to store the json response from the LUIS model
    public class Rootobject
    {
        public string query { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }
    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public Action[] actions { get; set; }
    }

    public class Action
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public Parameter[] parameters { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public bool required { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        public string entity { get; set; }
        public string type { get; set; }
        public float score { get; set; }
    }
    //entities of json model
    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }

    

}