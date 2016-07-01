using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using GoogleMaps.LocationServices;
using System.Device.Location;

namespace TestBot.Controllers
{
    public class LUISToSql
    {
        public LUISToSql()
        {

        }
        public String SqlQuery, reply;
        //converting LUIS response to SQL query
        public void initQuery(LuisResult LuisResponse,GeoCoordinate userLocation)
        {
            string compare = string.Empty;
            string colName = string.Empty;
            List<string> type = new List<string>();

            

            if (LuisResponse.Intents.Count > 0 && LuisResponse.Entities.Count > 0)
            {
                switch (LuisResponse.Intents[0].Intent)
                {
                    case "itemByPrice":
                    case "itemByCategory":
                        SqlQuery = "SELECT itemtable.itemName,itemtable.itemPrice,itemtable.itemDiscount, shoptable.shopName,shoptable.shopRating,locationtable.locDesc FROM itemtable JOIN shopitemtable ON itemtable.itemID=shopitemtable.itemID JOIN shoptable ON shoptable.shopID=shopitemtable.shopID JOIN shoploctable ON shoptable.shopID=shoploctable.shopID JOIN locationtable ON shoploctable.locID=locationtable.locID";
                        LuisResponse.Entities = LuisResponse.Entities.OrderBy(x => x.StartIndex).ToList();

                        string regex = "'";
                        string shopRegex = "'";
                        string itemName = string.Empty;
                        bool costFlag = false;
                        compare = string.Empty;
                        type.Clear();
                        int cost = 0;
                        for (int i = 0; i < LuisResponse.Entities.Count; ++i)
                        {
                            if (LuisResponse.Entities[i].Type.Equals("type"))
                                type.Add(LuisResponse.Entities[i].Entity.ToLower());
                            else if (LuisResponse.Entities[i].Type.Equals("item::name") || LuisResponse.Entities[i].Type.Equals("item") || LuisResponse.Entities[i].Type.Equals("category") || LuisResponse.Entities[i].Type.Contains("builtin.encyclopedia"))
                            {
                                if (LuisResponse.Entities[i].Type.Equals("category") || LuisResponse.Entities[i].Type.Contains("builtin.encyclopedia"))
                                {
                                    shopRegex += LuisResponse.Entities[i].Entity;
                                    shopRegex += "|";
                                }
                                itemName = LuisResponse.Entities[i].Entity;
                                if (itemName.Last() == 's')
                                    itemName = itemName.Remove(itemName.Length - 1);
                                if (itemName.Contains("item"))
                                    itemName = string.Empty;
                                if (type.Count == 0)
                                {
                                    regex = regex + itemName;
                                    regex += "|";
                                }
                                while (type.Count != 0)
                                {
                                    regex = regex + type.First() + " " + itemName;
                                    type.Remove(type.First());
                                    regex += "|";
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
                        }
                        if (regex.Length != 1)
                        {
                            regex = regex.Remove(regex.Length - 1);
                            regex += "'";
                            SqlQuery = SqlQuery + " where (lower(itemTable.itemName) REGEXP " + regex + " or lower(itemTable.itemCategory) REGEXP " + regex + ")";
                            if (shopRegex.Length != 1)
                            {
                                shopRegex = shopRegex.Remove(shopRegex.Length - 1);
                                shopRegex += "'";
                                SqlQuery = SqlQuery + " and (lower(shopTable.shopName) REGEXP " + shopRegex + " or lower(shopTable.shopCategory) REGEXP " + shopRegex + ")";

                            }
                            if (costFlag && compare.Length != 0)
                                SqlQuery = SqlQuery + " and itemPrice " + compare + cost.ToString();
                        }
                        else if (costFlag && compare.Length != 0)
                            SqlQuery = SqlQuery + " where itemPrice " + compare + cost.ToString();

                        SqlQuery += ";" ;

                        break;
                    case "getShop":
                        SqlQuery = "SELECT shoptable.shopName,shoptable.shopRating,locationtable.locDesc FROM shoptable JOIN shoploctable ON shoptable.shopID=shoploctable.shopID JOIN locationtable ON shoploctable.locID=locationtable.locID";
                        string shopName = string.Empty;
                        colName = string.Empty;

                        LuisResponse.Entities = LuisResponse.Entities.OrderBy(x => x.StartIndex).ToList();
                        regex = "'";
                        for (int i = 0; i < LuisResponse.Entities.Count; ++i)
                        {
                            if (LuisResponse.Entities[i].Type.Equals("category") || LuisResponse.Entities[i].Type.Contains("builtin.encyclopedia") || LuisResponse.Entities[i].Type.Equals("item::name"))
                            {
                                regex = regex + LuisResponse.Entities[i].Entity.ToLower();
                                regex += "|";
                            }
                        }
                        if (regex.Length != 1)
                        {
                            regex = regex.Remove(regex.Length - 1);
                            regex += "'";
                            SqlQuery = SqlQuery + " where lower(shopName) REGEXP " + regex + " or lower(shopCategory) REGEXP " + regex;
                        }
                        SqlQuery += ";";
                        break;
                    case "None":
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                    default:
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                }

            }
            else
                SqlQuery = "no";
            //Debug.WriteLine(SqlQuery + " 3hi");
        }
        //executing SqlQuery
        public string QueryToData(LuisResult LuisQuery, GeoCoordinate userLocation)
        {
            initQuery(LuisQuery,userLocation);
            Debug.WriteLine(SqlQuery);
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
