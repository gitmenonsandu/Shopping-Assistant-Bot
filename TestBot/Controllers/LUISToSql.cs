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
            if (LuisQuery.intents.Length > 0)
            {
                switch (LuisQuery.intents[0].intent)
                {
                    case "ItemNameWithPrice":
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                    case "ItemWithOffer":
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                    case "Offer":
                        SqlQuery = LuisQuery.intents[0].intent;
                        break;
                    case "ItemName":
                        SqlQuery = LuisQuery.intents[0].intent;
                        for (int i = 0; i < LuisQuery.entities.Length; ++i)
                            if (LuisQuery.entities[i].type.Equals("item"))
                            {
                                SqlQuery = "select * from ItemTable;";
                                break;
                            }
                            else if (LuisQuery.entities[i].type.Equals("item::Name"))
                            {
                                if (flag)
                                {
                                    SqlQuery = "select * from ItemTable where lower(itemName) like '%" + LuisQuery.entities[i].entity.ToLower() + "%'";
                                    flag = false;
                                }
                                else
                                    SqlQuery=SqlQuery+" or lower(itemName) like '%"+ LuisQuery.entities[i].entity.ToLower() + "%'";
                            }
                        SqlQuery += ";";

                        break;
                    case "ItemNameWithDiscount":
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
            SqlLogin db = new SqlLogin();
            try
            {
                reply = db.Select(SqlQuery);
                return reply;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
