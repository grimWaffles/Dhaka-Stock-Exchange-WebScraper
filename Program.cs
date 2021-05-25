using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScrapeTheWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Console.WriteLine("Press 1 for Radhe, Press 2 for DSE: ");
            string number = Console.ReadLine();

            if (Convert.ToInt32(number) == 1)
            {
                ParseHtml2();
            }
            else
            {
                ParseDse();
            }*/
            ParseDse();
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

            client.DefaultRequestHeaders.Accept.Clear();

            var response = client.GetStringAsync(fullUrl);

            return await response;
        }

        //This works fine
        private static void ParseHtml2()
        {
            string url = "https://en.wikipedia.org/wiki/Radhe_(2021_film)";
            var response = CallUrl(url).Result;

            //Get the html doc
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var theTargetDiv = htmlDoc.DocumentNode.Descendants()
                .Where(node => node.GetAttributeValue("class", "").Contains("mw-parser-output"))
                .ToList();

            var availableLists = theTargetDiv[0].Descendants("ul").ToList();

            //since we want the 4th list, which found by countless trial and error
            var theTargetList = availableLists[4].Descendants("li").ToList();

            List<string> wikiLink = new List<string>();

            foreach (var link in theTargetList)
            {
                if (link.FirstChild.Attributes.Count > 0)
                    wikiLink.Add("Actor: " + link.FirstChild.Attributes[1].Value + " ,Role: " + link.InnerText);
            }

            foreach (string s in wikiLink)
            {
                Console.WriteLine(s + '\n');
            }
        }

        private static void ParseDse()
        {
            string url = "https://www.dsebd.org/";
            var response = CallUrl(url).Result;

            HtmlDocument doc = new HtmlDocument(); 
            doc.LoadHtml(response);
            
            string output= "";

            output += GetIndexValues(doc);
            output += GetAllTableValuesShareByPrice(doc);
            output += GetMarketHighlightTableValues(doc);

            Console.WriteLine(output);

            //GetShareByPriceTableValue(doc);
        }

        private static string GetIndexValues(HtmlDocument doc)
        {
            var TargetSection = doc.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("white")).Single();

            var TargetDiv = TargetSection.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("LeftColHome")).ToList();

            var TheOneWeNeed = TargetDiv[0].Descendants().Where(node => node.GetAttributeValue("class", "").Contains("midrow")).ToList();

            int counter = 0;
            string output = "";

            foreach (var div in TheOneWeNeed)
            {
                if (counter < 3)
                {
                    //For the first 3 pattern is Header, val 1,val2,val3,val 4
                    foreach (var descendant in div.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("m_col")).ToList())
                    {
                        output += descendant.InnerText.Trim() + " ";
                    }

                    output += "\n\n";
                }
                else if(counter==3 || counter==5) //means that these are headers
                {
                    foreach(var descendant in div.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("m_col")).ToList())
                    {
                        output += descendant.InnerText + "\t";
                    }
                    output += "\n\n";
                }
                else
                {
                    foreach (var descendant in div.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("m_col")).ToList())
                    {
                        output += descendant.InnerText.Trim() + "\t";
                    }
                    output += "\n\n";
                }

                counter++;
            }
            output += "\n\n";
            return output;
        }
        //Gets all  the tables of the  share-price-by table
        private static string GetAllTableValuesShareByPrice(HtmlDocument doc)
        {
            string output = "";

            output += "Share By Price: "+ '\n' + "------------------------------" + "\n\n";

            var TablesInWebPage = doc.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("share-by-price-table")).ToList();

            int counter = 0;
            string[] section = new string[] { "%Change: ", "Last Trade Price: ", "Debt Board: " };

            foreach (var table in TablesInWebPage)
            {
                var TableHeaders = table.Descendants("th").Where(node => node.GetAttributeValue("class", "").Contains("textAling")).ToList();

                output += "\n\n" + section[counter] + "\n\n\n";

                foreach (var header in TableHeaders)
                {
                    output += header.InnerText.Trim() + "  ";
                }

                output += "\n\n";

                var tableBody = table.Descendants("tbody").Single();
                var tableRows = tableBody.Descendants("tr").ToList();

                foreach (var row in tableRows)
                {
                    //get the td(s) and then  for each td output the value
                    var tds = row.Descendants("td").ToList();

                    foreach (var td in tds)
                    {
                        output += td.InnerText + " ";
                    }
                    output += "\n\n";
                }
                counter++;
            }

            return output;
        }

        private static string GetMarketHighlightTableValues(HtmlDocument doc)
        {
            string output = "";

            output += "Market Highlights: "+'\n'+"------------------------------" + "\n\n";

            var TablesInWebPage = doc.DocumentNode.Descendants().Where(node => node.GetAttributeValue("class", "").Contains("market-highlights-table")).ToList();

            int counter = 0;
            string[] section = new string[] { "By Value: ", "By Volume: ", "By Trade: ","Daily Market: " };

            foreach (var table in TablesInWebPage)
            {
                var TableHeaders = table.Descendants("th").Where(node => node.GetAttributeValue("class", "").Contains("textAling")).ToList();

                output += "\n\n"+section[counter] + "\n\n\n";

                foreach (var header in TableHeaders)
                {
                    output += header.InnerText.Trim() + "  ";
                }

                output += "\n\n";

                var tableBody = table.Descendants("tbody").Single();
                var tableRows = tableBody.Descendants("tr").ToList();

                foreach (var row in tableRows)
                {
                    //get the td(s) and then  for each td output the value
                    var tds = row.Descendants("td").ToList();

                    foreach (var td in tds)
                    {
                        output += td.InnerText + " ";
                    }
                    output += "\n\n";
                }
                counter++;
            }

            return output;
        }

        //Gets only the first tab
        private static void GetShareByPriceTableValue(HtmlDocument doc)
        {
            string output = "";
            
            var TablesInWebPage = doc.DocumentNode.Descendants().Where(node=>node.GetAttributeValue("class","").Contains("share-by-price-table")).ToList();

            var FirstTable = TablesInWebPage[0];

            var TableHeaders = FirstTable.Descendants("th").Where(node => node.GetAttributeValue("class", "").Contains("textAling")).ToList();
            

            foreach (var header in TableHeaders)
            {
                output+= header.InnerText.Trim() + "  ";
            }

            output+= "\n\n";

            var tableBody = FirstTable.Descendants("tbody").Single();
            var tableRows = tableBody.Descendants("tr").ToList();

            foreach(var row in tableRows)
            {
                //get the td(s) and then  for each td output the value
                var tds = row.Descendants("td").ToList();

                foreach(var td in tds)
                {
                    output += td.InnerText + " ";
                }
                output += "\n\n";
            }

            Console.WriteLine(output);
        }

        


    }
}
