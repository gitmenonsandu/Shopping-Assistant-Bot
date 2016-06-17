using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
namespace TestBot.Controllers
{
    public class LUISToSql
    {
        public LUISToSql()
        {

        }
        public String SqlQuery,reply;
        //converting LUIS response to SQL query
        public void initQuery(LuisResult LuisResponse)
        {
            bool flag = true;
            string compare = string.Empty;
            
            if (LuisResponse.Intents.Count > 0)
            {
                IntentRecommendation bestResponse = LuisResponse.Intents.ElementAt(0);
                switch (LuisResponse.Intents[0].Intent)
                {
                    case "itemByPrice":
                        SqlQuery = "select itemName,itemPrice,itemDiscount from ItemTable";
                        flag = true;
                        bool costFlag = false;
                        compare = string.Empty;
                        int cost = -3;
                        Debug.WriteLine(LuisResponse.Entities.Count() + " 1hi");
                        //Array.Sort(LuisResponse.entities, delegate (Rootobject x, Rootobject y) { return x}
                        for (int i = 0; i < LuisResponse.Entities.Count(); ++i)
                        {
                            
                            if (LuisResponse.Entities[i]. Type.Equals("item::name") || LuisResponse.Entities[i].Type.Equals("item"))
                            {
                                if (flag)
                                {
                                    if (!(LuisResponse.Entities[i].Entity.ToLower().Contains("item")))
                                    {
                                        SqlQuery = SqlQuery + " where lower(itemName) like '%" + LuisResponse.Entities[i].Entity.ToLower() + "%'";
                                        string singular = LuisResponse.Entities[i].Entity.ToLower();
                                        if (singular.Last() == 's')
                                        {
                                            singular = singular.Remove(singular.Length - 1);
                                            SqlQuery = SqlQuery + " or lower(itemName) like '%" + singular + "%'";
                                        }
                                        flag = false;
                                    }
                                }
                                else
                                    SqlQuery = SqlQuery + " or lower(itemName) like '%" + LuisResponse.Entities[i].Entity.ToLower() + "%'";
                            }
                            else if (LuisResponse.Entities[i].Type.Equals("compare::less than"))
                                compare = "<=";
                            else if (LuisResponse.Entities[i].Type.Equals("compare::greater than"))
                                compare = ">=";
                            else if (LuisResponse.Entities[i].Type.Equals("compare::equal to"))
                                compare = "=";
                            else if (LuisResponse.Entities[i].Type.Equals("builtin.number"))
                                costFlag = int.TryParse(LuisResponse.Entities[i].Entity, out cost);
                        }
                        if (costFlag && compare.Length != 0)
                        {
                            if (flag)
                                SqlQuery = SqlQuery + " where itemPrice" + compare + cost.ToString();
                            else
                                SqlQuery = SqlQuery + " and itemPrice" + compare + cost.ToString();
                        }
                        SqlQuery += ";";
                        break;
                    case "itemByCategory":
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                    case "None":
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                    default:
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                }
            }
           // Debug.WriteLine(SqlQuery + " 3hi");
        }
        //executing SqlQuery
        public string QueryToData(LuisResult LuisQuery)
        {
            initQuery(LuisQuery);

            if (SqlQuery.Last() != ';')
                reply = "Sorry. I didnt get that\n";
            else {
                SqlLogin db = new SqlLogin();

                try
                {
                    reply = db.Select(SqlQuery);
                }
                catch (Exception e)
                {
                    reply = "Sorry. I didnt get that\n";
                }
            }
            return reply;
        }
    }
}
