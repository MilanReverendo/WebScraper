using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;
using CsvHelper;
using System.IO;
using System.Globalization;
class Program
{
    static void Main()
    {
        // Path to the ChromeDriver executable
        string driver_path = @"C:\Users\milan\Documents\School\2APPAI2\DevOps & Security\Case Study\chromedriver-win64\chromedriver.exe";

        // Initialize ChromeDriver
        IWebDriver driver = new ChromeDriver(driver_path);

        // User chooses a functionality
        Console.WriteLine("What do you want to do? ");
        Console.WriteLine("|1 = Search on YouTube | 2 = Search ICTjobs.be | 3 = Search on Wikipedia");

        // Read user's choice
        int option = Convert.ToInt16(Console.ReadLine());

        switch (option)
        {
            case 1: // YouTube search
                driver.Navigate().GoToUrl("https://www.youtube.com");

                // Prompt user for search term
                Console.Write("What do you want to search on YouTube? ");
                string search = Console.ReadLine();

                // Find the search box and submit the search
                IWebElement searchBox = driver.FindElement(By.Name("search_query"));
                searchBox.SendKeys(search);
                searchBox.Submit();

                System.Threading.Thread.Sleep(5000);  // Wait for results to load

                // Get information for the top 5 videos
                List<(string, string, string, string)> videoInfoList = new List<(string, string, string, string)>();

                // Locate the elements for the top 5 videos
                IReadOnlyCollection<IWebElement> videoElements = driver.FindElements(By.CssSelector("div#contents ytd-video-renderer"));

                // Iterate through the top 5 videos
                for (int i = 0; i < Math.Min(videoElements.Count, 5); i++)
                {
                    IWebElement video = videoElements.ElementAt(i);

                    // Extract information from each video
                    string title = video.FindElement(By.CssSelector("#video-title")).Text;
                    string link = video.FindElement(By.CssSelector("#video-title")).GetAttribute("href");
                    string channel = video.FindElement(By.CssSelector("div#channel-info ytd-channel-name a")).Text;
                    string views = video.FindElement(By.CssSelector("div#metadata-line span.style-scope.ytd-video-meta-block")).Text;

                    // Add the information to the list
                    videoInfoList.Add((title, link, channel, views));
                }

                // Display the information for the top 5 videos
                Console.WriteLine("Top 5 Videos:");

                foreach (var videoInfo in videoInfoList)
                {
                    Console.WriteLine($"Title: {videoInfo.Item1}");
                    Console.WriteLine($"Link: {videoInfo.Item2}");
                    Console.WriteLine($"Channel: {videoInfo.Item3}");
                    Console.WriteLine($"Views: {videoInfo.Item4}");
                    Console.WriteLine();
                }

                // Save videoInfoList to JSON
                SaveToJson(videoInfoList, "YouTubeSearchResults.json");

                // Save videoInfoList to CSV
                SaveToCsv(videoInfoList, "YouTubeSearchResults.csv");
                break;

            case 2: // ICTJobs.be search
                driver.Navigate().GoToUrl("https://www.ictjob.be/nl/");

                // Prompt user for search term
                Console.Write("What do you want to search on ICTjobs? ");
                string search2 = Console.ReadLine();

                // Find the search box and submit the search
                IWebElement searchBox2 = driver.FindElement(By.Name("keywords"));
                searchBox2.SendKeys(search2);
                searchBox2.Submit();

                // Get information for the first 5 jobs
                List<(string, string, string, string, string)> jobInfoList = new List<(string, string, string, string, string)>();

                // Locate the <ul> element containing job listings (modify as per the website's structure)
                IWebElement jobListContainer = driver.FindElement(By.CssSelector("ul.search-result-list"));

                // Locate the <li> elements within the <ul> (modify as per the website's structure)
                IReadOnlyCollection<IWebElement> jobElements = jobListContainer.FindElements(By.CssSelector("li.search-item"));

                // Iterate through the first 5 jobs
                for (int i = 0; i < Math.Min(jobElements.Count, 5); i++)
                {
                    // Find the job elements again to ensure they are up-to-date
                    IReadOnlyCollection<IWebElement> updatedJobElements = driver.FindElements(By.CssSelector("li.search-item"));

                    IWebElement job = updatedJobElements.ElementAt(i);

                    // Wait for the job title element to be present and visible
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    IWebElement jobTitleElement = wait.Until((d) =>
                    {
                        var element = job.FindElement(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li[1]/span[2]/a/h2"));
                        return element.Displayed ? element : null;
                    });

                    IWebElement companyElement = job.FindElement(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li[1]/span[2]/span[1]"));
                    IWebElement locationElement = job.FindElement(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li[1]/span[2]/span[2]/span[2]/span/span"));
                    IWebElement keywordsElement = job.FindElement(By.XPath("//*[@id=\"search-result-body\"]/div/ul/li[1]/span[2]/span[3]"));

                    // Extract information from each job
                    string jobTitle = jobTitleElement.Text;
                    string jobLink = jobTitleElement.GetAttribute("href");
                    string companyName = companyElement.Text;
                    string location = locationElement.Text;
                    string keywords = keywordsElement.Text;

                    // Add the information to the list
                    jobInfoList.Add((jobTitle, jobLink, companyName, location, keywords));
                }

                // Display the information for the top 5 jobs
                Console.WriteLine("Top 5 IT Jobs on ICTJobs.be:");

                foreach (var jobInfo in jobInfoList)
                {
                    Console.WriteLine($"Title: {jobInfo.Item1}");
                    Console.WriteLine($"Link: {jobInfo.Item2}");
                    Console.WriteLine($"Company: {jobInfo.Item3}");
                    Console.WriteLine($"Location: {jobInfo.Item4}");
                    Console.WriteLine($"Keywords: {jobInfo.Item5}");
                    Console.WriteLine();
                }
                // Save jobInfoList to JSON
                SaveToJson(jobInfoList, "ICTJobsSearchResults.json");

                // Save jobInfoList to CSV
                SaveToCsv(jobInfoList, "ICTJobsSearchResults.csv");

                break;

            case 3: // Amazon search
                Console.Write("Enter a product name: ");
                string productName = Console.ReadLine();

                // Assuming 'driver' is already created earlier in your project
                var url = $"https://www.amazon.com/s?k={productName}";
                driver.Navigate().GoToUrl(url);

                var items = driver.FindElements(By.CssSelector("div[data-component-type='s-search-result']"));

                if (items.Count > 0)
                {
                    Console.WriteLine($"Top 5 search results for '{productName}':\n");

                    for (int i = 0; i < Math.Min(items.Count, 5); i++)
                    {
                        var item = items[i];

                        var titleNode = item.FindElement(By.CssSelector("h2.a-size-mini.a-spacing-none.a-color-base.s-line-clamp-2 span.a-size-medium.a-color-base.a-text-normal"));
                        var priceNode = item.FindElement(By.CssSelector("span.a-price-whole"));
                        var ratingNode = item.FindElement(By.CssSelector("span.a-icon-alt"));
                        var reviewsNode = item.FindElement(By.CssSelector("span.a-size-base"));

                        if (titleNode != null)
                        {
                            var title = titleNode.Text.Trim();
                            var price = priceNode?.Text.Trim() ?? "N/A";
                            var rating = ratingNode?.Text.Trim() ?? "N/A";
                            var reviews = reviewsNode?.Text.Trim() ?? "N/A";

                            Console.WriteLine($"{i + 1}. {title}");
                            Console.WriteLine($"   Price: ${price}");
                            Console.WriteLine($"   Rating: {rating}");
                            Console.WriteLine($"   Reviews: {reviews}\n");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"No search results found for '{productName}'.");
                }
                // Save items to JSON
                SaveToJson(items, "AmazonSearchResults.json");

                // Save items to CSV
                SaveToCsv(items, "AmazonSearchResults.csv");
                break;

                
        }

        // Helper method to save data to JSON file
        static void SaveToJson<T>(IEnumerable<T> data, string fileName)
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(fileName, jsonData);
            Console.WriteLine($"Data saved to {fileName}");
        }

        // Helper method to save data to CSV file
        static void SaveToCsv<T>(IEnumerable<T> data, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
                Console.WriteLine($"Data saved to {fileName}");
            }
        }

        // Close the WebDriver instance
        driver.Quit();
    }
}
