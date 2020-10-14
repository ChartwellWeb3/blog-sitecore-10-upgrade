using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Feature.XBlog.Areas.XBlog.General;
using Sitecore.Feature.XBlog.Areas.XBlog.Items.Blog;
using Sitecore.Feature.XBlog.Areas.XBlog.Models.Blog;
using Sitecore.Feature.XBlog.Areas.XBlog.Search;
using Sitecore.Mvc.Presentation;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Sitecore.Feature.XBlog.Areas.XBlog.Controllers
{
    public class BlogController : Controller
    {
        private string _datasource;
        public BlogController()
        {

        }

        [HttpGet]
        public ActionResult Blog(int? page, string category, string tag, string searchterm)
        {
            var model = new BlogListingModel();
            model.Initialize(RenderingContext.Current.Rendering);
            model.dataSourceItem = Context.Database.GetItem(RenderingContext.Current.Rendering.DataSource);
            BlogSettings settingsItem = DataManager.GetBlogSettingsItem(model.dataSourceItem ?? Context.Item);

            string categoryName = model.PagingCatQuery = string.Empty;
            string searchText = model.PagingSearchQuery = string.Empty;
            string tagName = model.PagingTagQuery = string.Empty;

            string queryString = Request.QueryString.ToString();

            if (queryString.Contains("category"))
            {
                char[] splitChars = { '=' };

                string[] splitUrl = queryString.Split(splitChars);
                if (splitUrl.Length > 1)
                {
                    model.PagingCatQuery = category = categoryName = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]);
                }
                model.SearchHeading = WebUtility.HtmlDecode(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(category.ToLower()).Replace("-", " "));
            }
            else if (queryString.Contains("tag"))
            {
                char[] splitChars = { '=' };

                string[] splitUrl = queryString.Split(splitChars);
                if (splitUrl.Length > 1)
                {
                    model.PagingTagQuery = tag = tagName = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]);
                }
                model.SearchHeading = WebUtility.HtmlDecode(settingsItem.SearchFilterTitle) +
                                     WebUtility.HtmlDecode(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(tag.ToLower()).Replace("-", " "));
            }
            else if (queryString.Contains("searchterm"))
            {
                char[] splitChars = { '=' };

                string[] splitUrl = queryString.Split(splitChars);
                if (splitUrl.Length > 1)
                {
                    model.PagingSearchQuery = searchterm = searchText = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]); ;
                }
                model.SearchHeading = WebUtility.HtmlDecode(settingsItem.SearchFilterTitle) +
                                    WebUtility.HtmlDecode(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(searchterm.ToLower()).Replace("-", " "));
            }
            else
            {

            }

            int currentPage = 1;
            int blogPostCount = 0;
            bool rowResult = Int32.TryParse(settingsItem.PageSize, out int maximumRows);
            model.MaximumRows = maximumRows;

            if (!rowResult)
                model.MaximumRows = 5;

            if (!String.IsNullOrEmpty(Request.QueryString[XBSettings.XBPageQS]))
                model.PageResult = Int32.TryParse(Request.QueryString[XBSettings.XBPageQS], out currentPage);
            if (!model.PageResult)
                currentPage = 1;

            int startRowIndex = (currentPage - 1) * maximumRows;
            page = model.CurrentPage = currentPage;

            model.BlogPosts = BlogManager.GetBlogPosts(page, categoryName, tagName, searchText, startRowIndex, maximumRows, ref blogPostCount);


            model.TotalRows = blogPostCount; //BlogManager.GetBlogsCount(Context.Item, categoryName, authorID, tagName, searchText);

            if (!string.IsNullOrEmpty(model.SearchHeading) && (model.SearchHeading.Contains("Results") || model.SearchHeading.Contains("Résultats")))
                model.SearchHeading = model.TotalRows.ToString() + " " + model.SearchHeading;
            else if (queryString.Contains("category"))
                model.SearchHeading = model.SearchHeading;
            else
                model.SearchHeading = string.Empty;

            return View("~/Areas/XBlog/Views/XBlog/BlogListing.cshtml", model);
        }


        [HttpGet]
        public ActionResult FeaturedBlogListing()
        {
            var model = new BlogListingModel();

            model.Initialize(RenderingContext.Current.Rendering);
            model.dataSourceItem = Context.Database.GetItem(RenderingContext.Current.Rendering.DataSource);
            BlogSettings settingsItem = DataManager.GetBlogSettingsItem(model.dataSourceItem ?? Context.Item);

            //Get search results
            int currentPage = 1;
            bool rowResult = Int32.TryParse(settingsItem.PageSize, out int maximumRows);
            model.MaximumRows = maximumRows;

            if (!rowResult)
                model.MaximumRows = 5;

            if (!String.IsNullOrEmpty(Request.QueryString[XBSettings.XBPageQS]))
                model.PageResult = Int32.TryParse(Request.QueryString[XBSettings.XBPageQS], out currentPage);
            if (!model.PageResult)
                currentPage = 1;

            int startRowIndex = (currentPage - 1) * maximumRows;
            model.CurrentPage = currentPage;
            //model.BlogPosts = BlogManager.GetBlogPostsFeatured(Context.Item, categoryID, authorID, tagID, searchText, startRowIndex, maximumRows);
            model.BlogPosts = BlogManager.GetBlogPostsFeatured(maximumRows);

            return View("~/Areas/XBlog/Views/XBlog/BlogFeaturedListing.cshtml", model);
        }

        [HttpGet]
        public ActionResult BlogCategoryMenu()
        {
            var item = Context.Database.GetItem("/sitecore/content/Chartwell/Project/Data/blog/Categories");



            return View("~/Areas/XBlog/Views/XBlog/BlogCategoryMenu.cshtml", item);
        }

        public ActionResult BlogPost()
        {
            var model = new BlogPostModel();
            model.Initialize(RenderingContext.Current.Rendering);

            return View("~/Areas/XBlog/Views/XBlog/BlogPost.cshtml", model);
        }
        public ActionResult RelatedBlog()
        {
            _datasource = GetNavigationItemPath("Related Blog");
            return this.View("~/Areas/XBlog/Views/XBlog/Callouts/RelatedBlog.cshtml");
        }
        private string GetNavigationItemPath(string navigationTypeName)
        {
            string datasource = RenderingContext.Current.Rendering.DataSource;

            if (string.IsNullOrWhiteSpace(datasource))
            {
                return null;
            }

            return datasource;
        }

        public ActionResult OGPostMeta()
        {
            var model = new MetaModel();
            Item dataSourceItem = Context.Database.GetItem(RenderingContext.Current.Rendering.DataSource);
            BlogSettings settingsItem = DataManager.GetBlogSettingsItem(dataSourceItem ?? Context.Item);
            var itemTemplate = TemplateManager.GetTemplate(Context.Item);
            if (itemTemplate.FullName.Contains("Blog Post"))
            {
                BlogPost blogPost = Context.Item.CreateAs<BlogPost>();
                model.title = WebUtility.HtmlDecode(blogPost.Title);

                if (!String.IsNullOrEmpty(blogPost.Summary))
                {
                    model.description = Regex.Replace(blogPost.Summary, "<.*?>", String.Empty);
                }
                else
                {
                    model.description = WebUtility.HtmlDecode(Regex.Replace(Helpers.Helper.SafeSubstring(blogPost.Body, settingsItem.DisplaySummaryLength), "<.*?>", String.Empty));
                }

                if (blogPost.Tags.Any())
                {
                    StringBuilder display = new StringBuilder();
                    int totalTags = blogPost.Tags.Count;
                    int currentCount = 1;
                    foreach (Tag tagItem in blogPost.Tags)
                    {
                        display.Append(String.Format(tagItem.TagName));
                        if (currentCount < totalTags)
                            display.Append(", ");

                        currentCount++;
                    }
                    model.keyword = WebUtility.HtmlDecode(display.ToString());
                }

            }
            else
            {
                if (Context.Language.Name == "en")
                {
                    model.title = "Chartwell Blog | Chartwell Retirement Residences";
                    model.description = "The latest news, tips and information on retiremernt living for seniors and their families";
                    model.keyword = "Chartwell, Chartwell Blog, Chartwell retirement resideneces blog, Advice for Seniors, Advice for Caregivers, Life at Chartwell";
                }
                else
                {
                    model.title = "Blogue Chartwell | Chartwell résidences pour retraités";
                    model.description = "Actualités et l'information pour les aînés et leur famille";
                    model.keyword = "Chartwell, Blogue Chartwell, Chartwell résidences pour retraités, Conseils pour les aînés, Conseils pour les proches aidants, Vivre chez chartwell";
                }

                string queryString = Request.QueryString.ToString();

                if (queryString.Contains("category"))
                {
                    char[] splitChars = { '=' };
                    var searchText = "";
                    string[] splitUrl = queryString.Split(splitChars);
                    if (splitUrl.Length > 1)
                    {
                        searchText = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]); ;
                    }
                    model.keyword = settingsItem.CategoryFilterTitle + searchText;
                }
                if (queryString.Contains("tag"))
                {
                    char[] splitChars = { '=' };
                    var searchText = "";
                    string[] splitUrl = queryString.Split(splitChars);
                    if (splitUrl.Length > 1)
                    {
                        searchText = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]); ;
                    }
                    model.keyword = settingsItem.TagFilterTitle + searchText;
                }
                if (queryString.Contains("searchterm"))
                {
                    char[] splitChars = { '=' };
                    var searchText = "";
                    string[] splitUrl = queryString.Split(splitChars);
                    if (splitUrl.Length > 1)
                    {
                        searchText = WebUtility.UrlDecode(splitUrl[splitUrl.Length - 1]); ;
                    }
                    model.keyword = settingsItem.SearchFilterTitle + searchText;
                }
            }
            return this.View("~/Areas/XBlog/Views/XBlog/Callouts/OGPostMeta.cshtml", model);
        }

        public ActionResult RecentBlog()
        {
            _datasource = GetNavigationItemPath("Recent Blog");
            return this.View("~/Areas/XBlog/Views/XBlog/Callouts/RecentBlog.cshtml");
        }








    }
}