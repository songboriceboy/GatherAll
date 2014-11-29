using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.ComponentModel;
//using FormDelegate;
using System.Text.RegularExpressions;
using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Web;
using System.Windows.Forms;
using mshtml;
namespace BlogGather
{
    public class BlogGatherCnblogs
    {
        public class DelegatePara
        {

            public string strTitle;
            public string strContent;
        }
        public delegate void GreetingDelegate(DelegatePara dp);
        public event GreetingDelegate delAddBlog;

        private int m_nPageIndex = 0;
        protected List<string> m_lstUrls = new List<string>();
        List<string> lstThisTimesUrls = new List<string>();
        private BloomFilter m_bf = new BloomFilter(10485760);
  


        protected WebDownloader m_wd = new WebDownloader();
       
        protected string m_strTaskName = "";

    


        private Dictionary<string, string> m_dicUrl2Title = new Dictionary<string, string>();

        protected virtual string GetUrlCss()
        {
            string strUrlCss = "div.forFlow";
            return strUrlCss;
        }
    
 

        protected virtual string SaveFinalPageContent(string strMainDateRules,
                                string strMainContentElementRules, string strVisitUrl, string strReturnPage)
        {
            string strRet = "";
            string strMainContent = "";

            HtmlAgilityPack.HtmlDocument htmlDoc = GetHtmlDocument(strReturnPage);
            if (htmlDoc == null)
            {
                return "下载失败！";
            }

            IEnumerable<HtmlNode> NodesMainContent = htmlDoc.DocumentNode.QuerySelectorAll(strMainContentElementRules);

            if (NodesMainContent.Count() > 0)
            {
                strMainContent = NodesMainContent.ToArray()[0].InnerHtml;
                string strTitle = m_dicUrl2Title[strVisitUrl];
                DelegatePara dp = new DelegatePara();
                dp.strTitle = strTitle;
                dp.strContent = strMainContent;
                delAddBlog(dp);
                //Console.WriteLine(strMainContent);
            }
        
            string strCategory = "未分类";
     
            DateTime dtDate = System.DateTime.Now.Date;


            strRet = UpdateLayerNodeToSaveFinalPageContent(strVisitUrl, strMainContent, strCategory, dtDate);
            return strRet;

        }

     
        
   

        protected virtual string GetUrlFilterRule()
        {

            return @"www\.cnblogs\.com/" + m_strTaskName + @"/(p|archive/.*?/.*?/.*?)/.*?\.html$";
        }
        protected virtual void GatherInitFirstUrls()
        {

            m_lstUrls.Clear();
            string strPagePre = "http://www.cnblogs.com/";
            string strPagePost = "/default.html?page={0}&OnlyTitle=1";
            string strPage = strPagePre + m_strTaskName + strPagePost;


            for (int i = 500; i > 0; i--)
            {
                string strTemp = string.Format(strPage, i);
                m_wd.AddUrlQueue(strTemp, GetReferer());

            }
        }
        protected virtual Encoding GetBlogEncoding()
        {
            return Encoding.UTF8;
        }
   
      

        protected virtual string GetMainContentCss()
        {
           
            string strMainContentCss = "div#cnblogs_post_body";
            
            return strMainContentCss;
        }

      
      
      
        public BlogGatherCnblogs()
        {

        }
        public BlogGatherCnblogs(string strTaskID)
        {
            m_strTaskName = strTaskID;
        }
        protected virtual void ShortSleep()
        {
            return;
        }
        protected virtual string ProcessCodeEmbeded(string strPage)
        {
            return strPage;
        }
        protected virtual bool IsFinalPage(string strVisitUrl, string strUrlFilterRule)
        {
            bool bRet = false;

            MatchCollection matchsTemp = Regex.Matches(strVisitUrl.ToString(), strUrlFilterRule, RegexOptions.Singleline);
            if (matchsTemp.Count > 0)
            {
                bRet = true;
            }
            return bRet;
        }
        private string StripJs(string source)
        {
            try
            {
                string result = source;

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                return result;
            }
            catch
            {

                return source;
            }
        }
       
    
      
        protected virtual string GetReferer()
        {
            string strReferer = "";

            return strReferer;
        }

        protected virtual bool SaveUrlToDB(string strVisitUrl, string strReturnPage, DoWorkEventArgs e)
        {
            try
            {
                string strUrlFilterRule = GetUrlFilterRule();
                HtmlAgilityPack.HtmlDocument htmlDoc = GetHtmlDocument(strReturnPage);
                string strUrlCss = GetUrlCss();
                if (strUrlCss != "")
                {
                    IEnumerable<HtmlNode> NodesUrlContent = htmlDoc.DocumentNode.QuerySelectorAll(strUrlCss);
                    if (NodesUrlContent.Count() > 0)
                    {
                        strReturnPage = NodesUrlContent.ToArray()[0].InnerHtml;//进一步缩小范围
                        htmlDoc = GetHtmlDocument(strReturnPage);
                    }
                }
                string baseUrl = new Uri(strVisitUrl).GetLeftPart(UriPartial.Authority);
                DocumentWithLinks links = htmlDoc.GetLinks();


                foreach (string link in links.Links.Union(links.References))
                {

                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }

                    string decodedLink = link;

                    string normalizedLink = GetNormalizedLink(baseUrl, decodedLink);


                    if (string.IsNullOrEmpty(normalizedLink))
                    {
                        continue;
                    }

                    MatchCollection matchs = Regex.Matches(normalizedLink, strUrlFilterRule, RegexOptions.Singleline);
                    if (matchs.Count > 0)
                    {
                        string strLinkText = "";

                        if (links.m_dicLink2Text.Keys.Contains(normalizedLink))
                            strLinkText = links.m_dicLink2Text[normalizedLink];

                        if (strLinkText == "")
                        {
                            if (links.m_dicLink2Text.Keys.Contains(link))
                                strLinkText = links.m_dicLink2Text[link].TrimEnd().TrimStart();
                        }



                        lstThisTimesUrls.Add(normalizedLink);

                        if (!m_dicUrl2Title.Keys.Contains(normalizedLink))
                        {
                            m_dicUrl2Title.Add(normalizedLink, strLinkText);
                        }
                        m_wd.AddUrlQueue(normalizedLink, GetReferer());




                    }
                }
            }
            catch (Exception ex)
            {

            }
            bool bNoArticle = CheckArticles(lstThisTimesUrls);

            return bNoArticle;

        }


        private bool CheckArticles(List<string> lstThisTimesUrls)
        {
            bool bRet = true;
            foreach (string strTemp in lstThisTimesUrls)
            {
                if (!m_lstUrls.Contains(strTemp))
                {
                    bRet = false;
                    break;
                }
            }
            foreach (string strTemp in lstThisTimesUrls)
            {
                if (!m_lstUrls.Contains(strTemp))
                    m_lstUrls.Add(strTemp);
            }

            return bRet;
        }
    
       

        protected virtual string NormalizeLink(string baseUrl, string link)
        {
            return link.NormalizeUrl(baseUrl);
        }
        protected virtual string GetNormalizedLink(string baseUrl, string decodedLink)
        {
            string normalizedLink = NormalizeLink(baseUrl, decodedLink);
  
            return normalizedLink;
        }

 



        public void GatherBlog(DoWorkEventArgs e)
        {
            //m_bPostBlogID2Srv = GetPostBlogID2Srv();
            //m_strTaskName = GetTaskName();
            m_wd = new WebDownloader();
  
            GatherInitFirstUrls();

            BlogGatherNext(e);
        }

        private void ParseWebPage(string strVisitUrl, string strPageContent, DoWorkEventArgs e)
        {

            string strUrlFilterRule = GetUrlFilterRule();

            if (!IsFinalPage(strVisitUrl, strUrlFilterRule))
            {
                m_nPageIndex++;
              
    
                bool bNoArticle = SaveUrlToDB(strVisitUrl, strPageContent, e);
                if (!bNoArticle)
                {
                    BlogGatherNext(e);
                }
            }
            else
            {

                if (strPageContent != "")
                {
                    string strTitle = SaveFinalPageContent(""
                        , GetMainContentCss(), strVisitUrl, strPageContent);
            

                }
                else
                {
                    string strTitle = GetBlogTitleByUrl(strVisitUrl);
                }
                BlogGatherNext(e);
            }
        }
        protected string UpdateLayerNodeToSaveFinalPageContent(string strVisitUrl, string strFinalContent
            , string strCategory, DateTime dtDate)
        {
            string strRet = "";
           
            return strRet;
        }

        protected string GetBlogTitleByUrl(string strVisitUrl)
        {
            string strRet = "";
  

            return strRet;
        }

        private void BlogGatherNext(DoWorkEventArgs e)
        {
            ShortSleep();
            PageResult pr = m_wd.ProcessQueue(GetBlogEncoding());
            if (pr != null)
            {
                ParseWebPage(pr.strVisitUrl, pr.strPageContent, e);
            }
        }

 
    
        protected HtmlAgilityPack.HtmlDocument GetHtmlDocument(string strPage)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                OptionAddDebuggingAttributes = false,
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                OptionReadEncoding = true
            };
            htmlDoc.LoadHtml(strPage);


            return htmlDoc;
        }


        protected string GetPageDefaultUTF8(string url)                                                       
        {
            WebDownloader wd = new WebDownloader();
            string strPageHtml = wd.GetPageByHttpWebRequest(url, Encoding.UTF8, GetReferer());

            return strPageHtml;
          
        }
    }
}
