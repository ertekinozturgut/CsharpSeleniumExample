using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

//That helps for hangout in websites
IWebDriver driver;

//That helps for execute js codes and extend our capabilities for manipulating page
IJavaScriptExecutor jsExecutor;

InitDriver();
var newsList = GetNewsviaJS();
//var newsList = GetNewsviaSeleniumSelectors();

ShowNews(newsList);
GetOrginalLiksOfNews(newsList);
//ShowNews(newsList);
driver.Quit();

void InitDriver()
{

    ChromeOptions chromeOptions = new ChromeOptions();
    //That is not necessary. I set it because of show how to set browser language. 
    //On the other hand if you want to get page's content in spesific language you can use that
    chromeOptions.AddArguments("--lang=en-Us");


    //In some websites you track via cookies and you can continue scraping. For that you can open browser in incognito that.
    //However you can delete cookies with driver.Manage().Cookies.DeleteAllCookies(); command when you need.
    chromeOptions.AddArguments("--incognito");

    //We add code below for don't handle "The HTTP request to the remote WebDriver timed out after 60 seconds" Exception 
    chromeOptions.AddArgument("--no-sandbox");


    //We set options and defined driver and set Timeout
    driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions, TimeSpan.FromMinutes(2));

    //You can maximize browser screen with below code. 
    //That helps you when you don't want see mobile version of website. 
    //Because in some cases your bot logic will be failed in mobile version of the website
    driver.Manage().Window.Maximize();
    //Defined JavaScriptExecutor for using script execution
    jsExecutor = (IJavaScriptExecutor)driver;
}



void ShowNews(List<News> newsList)
{
    foreach (var news in newsList)
    {
        Console.WriteLine(String.Format("{0} : {1}", news.newsTitle, news.newsLink));
        Console.WriteLine("-----------------------");
    }
}

List<News> GetOrginalLiksOfNews(List<News> newsList)
{
    foreach (var news in newsList)
    {
        if (!string.IsNullOrEmpty(news.newsLink))
        {
            try
            {
                //We go google news' directed link
                driver.Navigate().GoToUrl(news.newsLink);
                
                Thread.Sleep(1000);
                //We get current page url
                Console.WriteLine(driver.Url);
                news.newsLink = driver.Url;
            }
            catch (Exception)
            {

            }

        }
    }
    return newsList;
}





List<News> GetNewsviaJS()
{
    List<News> news = new List<News>();
    //You have two option for going to web site
    //Option 1
    driver.Navigate().GoToUrl("https://news.google.com/topstories");

    //Option 2
    //jsExecutor.ExecuteScript("document.location.href='https://news.google.com/topstories'");

    //We wait page load  
    Thread.Sleep(2000);

    //Option 1 Select Elements via JS
    //We run our selecting news script
    //You can inspect page and learn which elements is necessary for you.After that you can focus them with query selectors
    news = JsonConvert.DeserializeObject<List<News>>(jsExecutor.ExecuteScript(@"
    let news=[];
    document.querySelectorAll('article[data-kind=""2""]').forEach(function(item,index){
        var newsTitle = '';
        var newsLink = '';

        newsLink = item.querySelector('a').href;

        if (item.querySelector('h4') != null)
        {
            newsTitle = item.querySelector('h4').innerText;
        }
        if (item.querySelector('h3') != null)
        {
            newsTitle = item.querySelector('h3').innerText;
        }
        news.push({ 'newsTitle':newsTitle,'newsLink':newsLink})
    }); 
        return JSON.stringify(news);") + "");



    return news;
}
//Option 2 Select Elements via FindElement functions
List<News> GetNewsviaSeleniumSelectors()
{
    List<News> news = new List<News>();
    //You have two option for going to web site
    //Option 1
    driver.Navigate().GoToUrl("https://news.google.com/topstories");

    //Option 2
    //jsExecutor.ExecuteScript("document.location.href='https://news.google.com/topstories'");

    //We wait page load  
    Thread.Sleep(2000);


    var articles = driver.FindElements(By.CssSelector("article[data-kind='2']")).ToList();
    foreach (var article in articles)
    {
        try
        {
            news.Add(new News
            {
                newsTitle = article.FindElements(By.CssSelector("h3")).Count > 0 ? article.FindElement(By.CssSelector("h3")).GetAttribute("innerText") : article.FindElement(By.CssSelector("h4")).GetAttribute("innerText"),
                newsLink = article.FindElement(By.CssSelector("a")).GetAttribute("href")
            });
        }
        catch (Exception ex)
        {

        }
    }

    return news;
}
class News
{
    public string newsLink { get; set; }
    public string newsTitle { get; set; }
}




