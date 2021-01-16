using IronWebScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsiderTradingBotScraper
{
    public class InsiderBotScraper
    {
        public async Task<List<Order>> GetOrders()
        {
            var scraper = new OpenInsiderWebScraper();
            await scraper.StartAsync();
            var orders = scraper.GetOrders().OrderBy(x => x.NewOwnPercentage).Reverse().ToList();
            return orders;
        }

        public struct Order
        {
            public DateTime Date { get; set; }
            public string Ticker { get; set; }
            public int Quantity { get; set; }
            public int NewOwnPercentage { get; set; }
        }

        public class OpenInsiderWebScraper : WebScraper
        {
            private readonly List<Order> _orders = new List<Order>();

            public override void Init()
            {
                this.LoggingLevel = WebScraper.LogLevel.All;
                this.Request("http://openinsider.com", Parse);
            }
            public override void Parse(Response response)
            {
                var element = response.Css("#tablewrapper > table:nth-child(10) > tbody")[0];
                var rows = element.ChildNodes;
                for (int i = 1; i < rows.Length; i++)
                {
                    if (i % 2 == 0 && i != 1)
                        continue;
                    var row = rows[i];
                    var cols = row.ChildNodes;

                    var dateString = cols[2].ChildNodes[0].TextContent;
                    var ticker = cols[3].ChildNodes[0].TextContent;
                    var quantity = cols[9].ChildNodes[0].TextContent;
                    var newOwnPercentageString = cols[11].ChildNodes[0].TextContent;

                    string[] dateSegments = dateString.Split('-');
                    int year = int.Parse(dateSegments[0]);
                    int month = int.Parse(dateSegments[1]);
                    int day = int.Parse(dateSegments[2]);
                    DateTime date = new DateTime(year, month, day);

                    int newOwnPercentage = 0;
                    try
                    {
                        newOwnPercentage = int.Parse(newOwnPercentageString.Replace("+", "").Replace("%", ""));
                    }
                    catch (Exception) { }
                    Order order = new Order()
                    {
                        Date = date,
                        Quantity = int.Parse(quantity.Replace("+", "").Replace(",", "")),
                        Ticker = ticker.Trim(),
                        NewOwnPercentage = newOwnPercentage
                    };

                    _orders.Add(order);
                }
            }

            public List<Order> GetOrders()
            {
                return _orders;
            }
        }
    }
}
