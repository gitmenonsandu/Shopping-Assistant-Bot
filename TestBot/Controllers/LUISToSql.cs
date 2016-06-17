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
            string itemName = string.Empty;
            string whereOr = string.Empty;
            string itemType = string.Empty;
            List<string> type = new List<string>();
            
            if (LuisResponse.Intents.Count > 0)
            {
                switch (LuisResponse.Intents[0].Intent)
                {
                    case "itemByPrice":
                        SqlQuery = "select itemName,itemPrice,itemDiscount from ItemTable";
                        flag = true;
                        bool costFlag = false;
                        compare = string.Empty;
                        itemName = string.Empty;
                        itemType = string.Empty;
                        whereOr = " where";
                        type.Clear();
                        int cost = -3;
                        //Array.Sort(LuisResponse.entities, delegate (Rootobject x, Rootobject y) { return x}

                        
                        LuisResponse.Entities= LuisResponse.Entities.OrderBy(x => x.StartIndex).ToList();
                        //Debug.WriteLine(LuisResponse.Entities);

                        for (int i = 0; i < LuisResponse.Entities.Count(); ++i)
                        {
                            Debug.WriteLine(LuisResponse.Entities[i].Entity+LuisResponse.Entities[i].StartIndex);
                            if (LuisResponse.Entities[i].Type.Equals("item::name") || LuisResponse.Entities[i].Type.Equals("item"))
                            {
                                    if (!(LuisResponse.Entities[i].Entity.ToLower().Contains("item")))
                                    {
                                    
                                        if (type.Count != 0)
                                        {
                                            itemType = type.First();
                                            itemType += " ";

                                        }
                                            itemName = itemType + LuisResponse.Entities[i].Entity.ToLower();
                                            SqlQuery = SqlQuery + whereOr + " lower(itemName) like '%" + itemName + "%'";
                                            whereOr = " or";
                                            string singular = LuisResponse.Entities[i].Entity.ToLower();
                                            if (singular.Last() == 's')
                                            {
                                                singular = singular.Remove(singular.Length - 1);
                                                itemName = itemType + singular;
                                                SqlQuery = SqlQuery + " or lower(itemName) like '%" + itemName + "%'";
                                            }
                                        if (type.Count != 0)
                                        {
                                            type.Remove(type.First());
                                            while (type.Count != 0)
                                            {
                                                itemName = type.First() + " " + LuisResponse.Entities[i].Entity.ToLower();
                                                SqlQuery = SqlQuery + " or lower(itemName) like '%" + itemName + "%'";
                                                if (singular != LuisResponse.Entities[i].Entity.ToLower())
                                                {
                                                    itemName = type.Last() + " " + singular;
                                                    SqlQuery = SqlQuery + " or lower(itemName) like '%" + itemName + "%'";
                                                }
                                                type.Remove(type.First());
                                            }
                                        }
                                        
                                    }
                                    
                            }
                            else if (LuisResponse.Entities[i].Type.Equals("compare::less than"))
                                compare = "<=";
                            else if (LuisResponse.Entities[i].Type.Equals("compare::greater than"))
                                compare = ">=";
                            else if (LuisResponse.Entities[i].Type.Equals("compare::equal to"))
                                compare = "=";
                            else if (LuisResponse.Entities[i].Type.Equals("builtin.number"))
                                costFlag = int.TryParse(LuisResponse.Entities[i].Entity, out cost);
                            else if (LuisResponse.Entities[i].Type.Equals("type"))
                                type.Add(LuisResponse.Entities[i].Entity.ToLower());
                        }
                        if (costFlag && compare.Length != 0)
                            SqlQuery = SqlQuery + whereOr+" itemPrice" + compare + cost.ToString();
                        
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
