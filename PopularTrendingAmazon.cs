/*PopularTrendingAmazon.cs by Tony Rivas

First C# program
Pulls #1 item from 6 top departments from up to 5 different amazon category pages
By default returns popular and trending items

Usage:
Default, popular and trending:
<prompt>PopularTrendingAmazon.exe
All categories:
<prompt>PopularTrendingAmazon.exe all
Specify categories, popular trending wishedfor:
<prompt>PopularTrendingAmazon.exe p tr w

Options: all - all options, p - popular, t - trending, s - movers and shakers (sales), w - wished for, g - gifted

REFERENCES

How to make a GET request by using Visual C#
https://support.microsoft.com/en-us/kb/307023
WebProxy.GetDefaultProxy Method ()
https://msdn.microsoft.com/en-us/library/system.net.webproxy.getdefaultproxy(v=vs.110).aspx
Tip: Replacement Methods for Obsolete WebProxy.GetDefaultProxy Method
http://www.codeguru.com/csharp/csharp/cs_network/http/article.php/c16479/Tip-Replacement-Methods-for-Obsolete-WebProxyGetDefaultProxy-Method.htm
How to properly exit a C# application?
http://stackoverflow.com/questions/12977924/how-to-properly-exit-a-c-sharp-application
String.Split Method (String[], StringSplitOptions)
https://msdn.microsoft.com/en-us/library/tabh47cf(v=vs.110).aspx
Dictionary<TKey, TValue> Class
https://msdn.microsoft.com/en-us/library/xfhwa508(v=vs.110).aspx
String.Join Method (String, String[])
https://msdn.microsoft.com/en-us/library/57a79xd0(v=vs.110).aspx
Count Dictionary
http://www.dotnetperls.com/count-dictionary
WebUtility Class
https://msdn.microsoft.com/en-us/library/system.net.webutility(v=vs.110).aspx

change log:
2/7/16 - created and tested - tony rivas
TBD - future work, replace checkpoint logic with RegEx to reduce future maintenance
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace PopularTrendingAmazon
{
    public class Program
    {
        static void Main(string[] args)
        {
            string options;
            Dictionary<string, string> sURL = new Dictionary<string, string>();
            if (args.Length > 0)
            {
                options = String.Join("", args);
                bool all = false;
                if (Regex.Match(options, @"(all)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    all = true;
                if (all || Regex.Match(options, @"(p|pop)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    sURL.Add("POPULAR AMAZON ITEMS", "http://www.amazon.com/Best-Sellers/zgbs/");
                if (all || Regex.Match(options, @"(tr|trend)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    sURL.Add("TRENDING AMAZON ITEMS", "http://www.amazon.com/gp/new-releases/");
                if (all || Regex.Match(options, @"(m|sale|move|shake|sell)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    sURL.Add("MOVERS AND SHAKERS AMAZON ITEMS", "http://www.amazon.com/gp/movers-and-shakers/");
                if (all || Regex.Match(options, @"(w|wish|for)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    sURL.Add("MOST WISHED FOR AMAZON ITEMS", "http://www.amazon.com/gp/most-wished-for/");
                if (all || Regex.Match(options, @"(g|gift)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    sURL.Add("MOST GIFTED AMAZON ITEMS", "http://www.amazon.com/gp/most-gifted/");
                if (sURL.Count == 0)
                {
                    Console.WriteLine("Please use an option e.g. PopularTrendingAmazon.exe <option(s)>");
                    Console.WriteLine("no options - popular + trending, all - all options, p - popular, tr - trending, m - movers and shakers (sales), w - wished for, g - gifted");
                    Environment.Exit(1);
                }
            }
            //args.Length == 0, popular and trending
            else
            {
                sURL.Add("POPULAR AMAZON ITEMS", "http://www.amazon.com/Best-Sellers/zgbs/");
                sURL.Add("TRENDING AMAZON ITEMS", "http://www.amazon.com/gp/new-releases/");
            }
            
            // for checking values in sURL
            /*foreach( KeyValuePair<string, string> sURL_kvp in sURL )
            {
                Console.WriteLine("Key = {0}, Value = {1}", sURL_kvp.Key, sURL_kvp.Value);
            }*/
            
            Console.WriteLine("\ncategory\n#, item, manufacturer, department, rating\n");
            
            foreach( KeyValuePair<string, string> sURL_kvp in sURL )
            {
                // "new WebRequest" will not work see webpage ref# 1
                WebRequest wrGETURL = WebRequest.Create(sURL_kvp.Value);
                
                if(wrGETURL == null) {
                    Console.WriteLine("Failed to get http request\n");
                    Environment.Exit(1);
                }
                // if using Windows PC behind a proxy
                IWebProxy iwpxy = WebRequest.GetSystemWebProxy();
                wrGETURL.Proxy = iwpxy;

                Stream objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);
                
                string sLine = "";
                int passCount = 0;
                int itemCount = 0;
                string[] stringSeparators = new string[] {"<span>", "</span>", "<b>", "<h3>", "</h3>", "<", ">"};
                string[] departmentName = new string[] {"generic department"};
                string[] popularItem = new string[] {"generic item"};
                string[] mfgName = new string[] {"generic mfg"};
                string[] rating = new string[] {"generic rating"};
                
                //category name
                Console.WriteLine("\n{0}", sURL_kvp.Key);
                
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    // prevents exception caused when Trim() encounters null
                    if (sLine != null )
                    {
                        // category
                        if (sLine.Trim() == "<div class=\"zg_homeWidget\">")
                        {
                            sLine = objReader.ReadLine();
                            if (sLine != null )
                            {
                                //will print this value after manufacturer
                                departmentName = sLine.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                            }
                        } 
                        // list position
                        if (sLine.Trim() == "<div class=\"zg_item zg_homeWidgetItem\">" && passCount == 0)
                        {
                            ++passCount;
                        }
                        // number 1 item in top department
                        if ( (sLine.Trim() == "1." || sLine.Trim() == "1.<span" ) && passCount == 1)
                        {
                            Console.Write("{0}. ", ++itemCount);
                            ++passCount;
                        }
                        // top item name
                        if (sLine.Trim() == "<span class=\"zg_title\">")
                        {
                            if (passCount == 2)
                            {
                                sLine = objReader.ReadLine();
                                if (sLine != null )
                                {
                                    popularItem = sLine.Trim().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                                    //HtmlDecode() for html entities like &quot;
                                    Console.Write("{0}, ", WebUtility.HtmlDecode(popularItem[1]));
                                    ++passCount;
                                }
                            }
                            // runner up item name, skip
                            else if (passCount == 1)
                            {
                                passCount = 0;
                            }
                        }
                        // manufacturer name and department name
                        if (passCount == 3)
                        {
                            string tempName = "";
                            while (sLine.Trim() != "<div class=\"zg_reviews\">"){
                                sLine = objReader.ReadLine();
                                if (sLine != null )
                                {
                                    tempName += sLine.Trim();
                                }
                            }
                            
                            //e.g. </span><span><b>Blu-ray</b> ~ Harrison Ford</span>
                            //becomes: Blu-ray</b> ~ Harrison Ford
                            //becomes: Blu-ray ~ Harrison Ford
                            //e.g. </span><span>by Exploding Kittens LLC</span>
                            //becomes: by Exploding Kittens LLC
                            mfgName = tempName.Replace("</b>", "").Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                            Console.Write("{0}, {1}, ", WebUtility.HtmlDecode(mfgName[0]), departmentName[0].ToUpper());
                            ++passCount;
                        }
                        // rating (out of 5 stars)
                        if (passCount == 4)
                        {
                            //would only performing these actions on under 10 lines rather than rest of document until next top item
                            //so no big performance issue
                            rating = sLine.Trim().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                            //avoid index out of range exception
                            if (rating.Length > 2)
                            {
                                if (rating[1] == "span class=\"a-icon-alt\"")
                                {
                                    Console.WriteLine("{0}",rating[2]);
                                    passCount = 0;
                                }
                            }
                        }
                    }
                }
                itemCount = 0;
            }
            Console.WriteLine("\npress any key");
            Console.ReadLine();
		}
	}
}