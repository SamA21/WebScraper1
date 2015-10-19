using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Mvc;

namespace WebScraper1.Controllers
{
    public class ScraperController : Controller
    {
        // GET: Scrapper
        public ActionResult Index()
        {
            ViewBag.ReturnedPositions = GetData();
            return View();
        }

        private string GetData()
        {
            WebClient wbclient = new WebClient();
            string url = ConfigurationManager.AppSettings["defaultURL"].Replace("%26", "&");
            string webPageResults = wbclient.DownloadString(url);

            string htmlText = String.Empty;
            if (webPageResults.IndexOf("www.stlgroup.co.uk") > 0)//only bothers passing results if it finds stlgroup in the list
            {
                string result = String.Empty;
                Boolean gotJustResults = false;
                try
                {
                    result = GetJustSearchResults(webPageResults);
                    gotJustResults = true;
                }
                catch (Exception ex)
                {
                    htmlText = $"Error passing url or data from google (due to changes their end), {ex.Message}";
                    gotJustResults = false;
                }

                if(gotJustResults)
                { 
                    try
                    {                    
                        List<int> resultFoundAt = GetPositions(result);                   

                        //checks if found url if so turn list of results into a readable string
                        if (resultFoundAt.Count > 0)
                        {
                            int added = 0;
                            foreach (int item in resultFoundAt)
                            {
                                if (added == 0)
                                    htmlText = item.ToString();
                                else
                                    htmlText = $"{htmlText}, {item}";
                                added++;
                            }
                        }
                        else
                        {
                            htmlText = "0"; // here stl group could have been mentioned in an ad section but not mentioned in the normal search results
                        }                                       
                    }
                    catch (Exception ex)
                    {
                        htmlText = $"Error passing result data from google (due to changes their end), {ex.Message}"; 
                    }
                }
            }
            else
            {
                htmlText = $"No mentions of STL group URL using the following search term: {url}"; // here there is no mentions of stl group in either search or ads 
            }
            return htmlText;
        }


        private string GetJustSearchResults(string allResults)
        {
            // get the list of just the search results (not ads/ sponsered links).
            string startSplit = "id=\"ires\"";
            string endSplit = "<br></div></li></ol>";
            int resultsLocation = allResults.IndexOf(startSplit);
            int startSubstringLength = startSplit.Length;
            int endSubstringLength = endSplit.Length;
            int endLocation = allResults.IndexOf(endSplit);
            int length = endLocation - resultsLocation + endSubstringLength - startSubstringLength - 1;
            string result = allResults.Substring(resultsLocation + startSubstringLength + 1, length);
            return result;
        }

        private List<int> GetPositions(string results)
        {
            List<int> resultFoundAt = new List<int>();
            int count = 1;
            int currentIndex = 0;
            while (count <= 99) //loops through all 100 results checking if the url part has stlgroup in it, if so adds the current count to the list of results
            {
                int tempCurrentIndex = results.IndexOf("<li class=\"g\">", currentIndex);
                int nextResultIndex = results.IndexOf("<li class=\"g\">", tempCurrentIndex + 1);
                int templength = nextResultIndex - tempCurrentIndex;
                string tempSubstring = results.Substring(tempCurrentIndex, templength);
                if (tempSubstring.IndexOf("www.stlgroup.co.uk") >= 0)
                {
                    resultFoundAt.Add(count);
                }
                count++;
                currentIndex = tempCurrentIndex + 1;
            }
            return resultFoundAt;
        }
    }
}