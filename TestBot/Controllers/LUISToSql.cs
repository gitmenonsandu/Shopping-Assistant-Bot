using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;

namespace TestBot.Controllers
{
    public class LUISToSql
    {
        public LUISToSql()
        {

        }
        public String SqlQuery, reply;
        //converting LUIS response to SQL query
        public void initQuery(LuisResult LuisResponse)
        {
            bool flag = true;
            string compare = string.Empty;
            string colName = string.Empty;
            List<string> type = new List<string>();

            if (LuisResponse.Intents.Count > 0)
            {
                switch (LuisResponse.Intents[0].Intent)
                {
                    case "itemByPrice":
                    case "itemByCategory":
                        SqlQuery = "select itemName,itemPrice,itemDiscount from ItemTable";
                        flag = true;
                        bool costFlag = false;
                        compare = string.Empty;
                        string itemName = string.Empty;
                        string itemType = string.Empty;
                        colName = string.Empty;
                        string whereOr = " where";
                        type.Clear();
                        int cost = -3;

                        LuisResponse.Entities = LuisResponse.Entities.OrderBy(x => x.StartIndex).ToList();


                        for (int i = 0; i < LuisResponse.Entities.Count(); ++i)
                        {
                            if (LuisResponse.Entities[i].Type.Equals("item::name") || LuisResponse.Entities[i].Type.Equals("item") || LuisResponse.Entities[i].Type.Equals("category"))
                            {
                                if (LuisResponse.Entities[i].Type.Equals("item::name"))
                                    colName = "itemName";
                                else if (LuisResponse.Entities[i].Type.Equals("category"))
                                    colName = "itemCategory";
                                if (!(LuisResponse.Entities[i].Entity.ToLower().Contains("item")))
                                {

                                    if (type.Count != 0) 
                                    {
                                        itemType = type.First();
                                        itemType += " ";

                                    }
                                    itemName = itemType + LuisResponse.Entities[i].Entity.ToLower();
                                     
                                    SqlQuery = SqlQuery + whereOr + $" (lower({colName}) like '%" + itemName + "%'";

                                    whereOr = " or";
                                    flag = false;
                                    string singular = LuisResponse.Entities[i].Entity.ToLower();
                                    if (singular.Last() == 's') 
                                    {
                                        singular = singular.Remove(singular.Length - 1);
                                        itemName = itemType + singular;
                                        SqlQuery = SqlQuery + $" or lower({colName}) like '%" + itemName + "%'";
                                    }
                                    if (type.Count != 0)
                                    {
                                        type.Remove(type.First());
                                        while (type.Count != 0)
                                        {
                                            itemName = type.First() + " " + LuisResponse.Entities[i].Entity.ToLower();
                                            SqlQuery = SqlQuery + $" or lower({colName}) like '%" + itemName + "%'";
                                            if (singular != LuisResponse.Entities[i].Entity.ToLower())
                                            {
                                                itemName = type.Last() + " " + singular;
                                                SqlQuery = SqlQuery + $" or lower({colName}) like '%" + itemName + "%'";
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
                        {
                            if (flag)
                                SqlQuery = SqlQuery + " where itemPrice" + compare + cost.ToString();
                            else
                                SqlQuery = SqlQuery + ") and itemPrice" + compare + cost.ToString();
                        }
                        SqlQuery += ";";
                        break;
                    case "getShop":
                        SqlQuery = "SELECT shoptable.shopName,shoptable.shopRating,locationtable.locDesc FROM shoptable JOIN shoploctable ON shoptable.shopID=shoploctable.shopID JOIN locationtable ON shoploctable.locID=locationtable.locID";
                        string shopName = string.Empty;
                        colName = string.Empty;

                        LuisResponse.Entities = LuisResponse.Entities.OrderBy(x => x.StartIndex).ToList();
                        compare = "'";
                        for(int i=0;i<LuisResponse.Entities.Count;++i)
                        {
                            if (LuisResponse.Entities[i].Type.Equals("category") || LuisResponse.Entities[i].Type.Contains("builtin.encyclopedia") || LuisResponse.Entities[i].Type.Equals("item::name"))
                            {
                                if (i < LuisResponse.Entities.Count - 1)
                                    compare = compare + LuisResponse.Entities[i].Entity.ToLower() + "|";
                                else
                                    compare = compare + LuisResponse.Entities[i].Entity.ToLower();
                            }
                        }
                        compare += "'";
                        SqlQuery = SqlQuery + " where lower(shopName) REGEXP " + compare + " or lower(shopCategory) REGEXP " + compare;
                        SqlQuery += ";";
                        break;
                    case "None":
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                    default:
                        SqlQuery = LuisResponse.Intents[0].Intent;
                        break;
                }
                int brackets = SqlQuery.Count(x => x.Equals('(')) - SqlQuery.Count(x => x.Equals(')'));

                for (int b = 0; b < brackets; ++b)
                    SqlQuery = SqlQuery.Insert(SqlQuery.Length - 1, ")");


            }
            //Debug.WriteLine(SqlQuery + " 3hi");
        }
        //executing SqlQuery
        public string QueryToData(LuisResult LuisQuery)
        {
            initQuery(LuisQuery);
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
