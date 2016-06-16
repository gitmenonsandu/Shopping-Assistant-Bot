using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TestBot.Controllers
{
    public class LUISToSql
    {
        public String SqlQuery,reply;
        public void initQuery(Rootobject LuisQuery)
        {
            bool flag = true;
            string compare = string.Empty;
            if (LuisQuery.intents.Length > 0)
            {
                switch (LuisQuery.intents[0].intent)
                {
                    case "itemByPrice":
                        SqlQuery = "select itemName,itemPrice,itemDiscount from ItemTable";
                        flag = true;
                        bool costFlag = false;
                        compare = string.Empty;
                        int cost=-3;
                        for (int i = 0; i < LuisQuery.entities.Length; ++i)
                        {
                            if (LuisQuery.entities[i].type.Equals("item::name") || LuisQuery.entities[i].type.Equals("item"))
                            {
                                if (flag)
                                {
                                    if (!(LuisQuery.entities[i].entity.ToLower().Contains("item")))
                                    {
                                        SqlQuery = SqlQuery + " where lower(itemName) like '%" + LuisQuery.entities[i].entity.ToLower() + "%'";
                                        string singular = LuisQuery.entities[i].entity.ToLower();
                                        if (singular.Last() == 's')
                                        {
                                            singular = singular.Remove(singular.Length - 1);
                                            SqlQuery = SqlQuery + " or lower(itemName) like '%" + singular + "%'";
                                        }
                                        flag = false;
                                    }
                                }
                                else
                                    SqlQuery = SqlQuery + " or lower(itemName) like '%" + LuisQuery.entities[i].entity.ToLower() + "%'";
                            }
                            else if (LuisQuery.entities[i].type.Equals("compare::less than"))
                                compare = "<=";
                            else if (LuisQuery.entities[i].type.Equals("compare::greater than"))
                                compare = ">=";
                            else if (LuisQuery.entities[i].type.Equals("compare::equal to"))
                                compare = "=";
                            else if (LuisQuery.entities[i].type.Equals("builtin.number"))
                                costFlag = int.TryParse(LuisQuery.entities[i].entity, out cost);
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
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                    case "None":
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                    default:
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                }
            }
            //Debug.WriteLine(SqlQuery + "3hi");
        }
        public string QueryToData(Rootobject LuisQuery)
        {
            //Debug.WriteLine(SqlQuery+"2hi");
            initQuery(LuisQuery);
            if (SqlQuery.Last()!=';')
                reply = "Sorry. I didnt get that\n";
            else {
                SqlLogin db = new SqlLogin();

                try
                {
                    reply = db.Select(SqlQuery);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    reply = "Sorry. I didnt get that\n";
                }
            }
            return reply;
        }
    }
}
