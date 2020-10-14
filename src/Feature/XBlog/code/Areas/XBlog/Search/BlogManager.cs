using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.XBlog.Areas.XBlog.General;
using Sitecore.Feature.XBlog.Areas.XBlog.Items.Blog;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using X.PagedList;


namespace Sitecore.Feature.XBlog.Areas.XBlog.Search
{
    public class BlogManager
    {
        public static IPagedList<BlogPost> GetBlogPosts(int? page, string categoryName, string tagName, string searchText, int startRowIndex, int maximumRows, ref int blogPostCount)
        {
            try
            {

                var searchLanguage = Context.Language;
                IPagedList<BlogPost> BlogList = null;
                using (IProviderSearchContext context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())

                {
                    #region predicate
                    Expression<Func<SearchResultItem, bool>> predicate = PredicateBuilder.True<SearchResultItem>();
                    predicate = predicate.And(item => item.TemplateName == "Blog Post");
                    predicate = predicate.And(item => item.Language == searchLanguage.Name);
                    predicate = predicate.And(item => item.Name != "__Standard Values");
                    #endregion
                    if (categoryName != "")
                    {


                        Category categoryItem = CategoryManager.GetCategoryByName(Context.Item, WebUtility.HtmlEncode(categoryName.Replace("-", " ")));
                        if (categoryItem != null)
                        {
                            var CategoryId = categoryItem.InnerItem.ID;
                            var searchQuery = context.GetQueryable<SearchResultItem>().Where(predicate)
                                                        .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])
                                                        .CreateAs<BlogPost>()
                                                        .Where(x => x.Categories.Any(c => c.ItemId.Equals(CategoryId)));

                            blogPostCount = searchQuery.Count();

                            BlogList = searchQuery.ToPagedList(page ?? 1, maximumRows); //.Slice(startRowIndex, maximumRows).ToList();
                        }
                    }
                    else if (tagName != "")
                    {

                        Tag tagItem = TagManager.GetTagByName(Context.Item, WebUtility.HtmlEncode(tagName.Replace("-", " ")));
                        if (tagItem != null)
                        {
                            var TagId = tagItem.InnerItem.ID;

                            var searchQuery = context.GetQueryable<SearchResultItem>().Where(predicate)
                                                     .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])
                                                     .CreateAs<BlogPost>()
                                                     .Where(x => x.Tags.Any(c => c.ItemId.Equals(TagId)));

                            blogPostCount = searchQuery.Count();

                            BlogList = searchQuery.ToPagedList(page ?? 1, maximumRows);  // .Slice(startRowIndex, maximumRows).ToList();
                        }
                    }
                    else if (searchText != "")
                    {
                        var bodyQuery = (from w in context.GetQueryable<SearchResultItem>().Where(predicate)
                                         where w.Content.Contains(searchText) && w.Content.StartsWith(searchText)
                                         select w)
                                        .OrderByDescending(o => o[XBSettings.XBSearchPublishDate])
                                        .CreateAs<BlogPost>()
                                        .ToList();

                        //var searchQuery = context.GetQueryable<SearchResultItem>().Where(predicate)
                        //                         .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])
                        //                         //.CreateAs<BlogPost>().Where(x => x.Title.Contains(searchText) || x.Body.Contains(searchText))
                        //                         .ToList();

                        blogPostCount = bodyQuery.Count();

                        BlogList = bodyQuery.ToPagedList(page ?? 1, maximumRows); //.Slice(startRowIndex, maximumRows).ToList();

                    }
                    else
                    {
                        var searchQuery = context.GetQueryable<SearchResultItem>().Where(predicate)
                                                 .OrderByDescending(t => t[XBSettings.XBSearchPublishDate]);

                        blogPostCount = searchQuery.Count();
                        BlogList = searchQuery.CreateAs<BlogPost>()
                                              .ToPagedList(page ?? 1, maximumRows);
                    }
                    try
                    {
                        foreach (BlogPost lstBlog in BlogList)
                        {
                            var blogImageId = lstBlog.BlogImage.Value;

                            if (!String.IsNullOrEmpty(blogImageId))
                            {
                                Database database1 = Context.Database;
                                Item blogImageItem = database1.GetItem(blogImageId);
                                if (blogImageItem != null)
                                {
                                    var ImagePath = MediaManager.GetMediaUrl(blogImageItem);
                                    string hashedUrl = HashingUtils.ProtectAssetUrl(StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(blogImageItem, new MediaUrlBuilderOptions { MaxWidth = 1280 })));

                                    lstBlog.BlogImageUrls = hashedUrl;
                                }
                                else
                                {

                                    lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                                }
                            }
                            else
                            {


                                lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                            }
                        }
                    }
                    catch
                    {

                    }
                    return BlogList;

                }
            }
            catch (Exception ex)
            {
                Log.Error("XBlog GetBlogResults error", ex, new object());
            }
            return null;
        }

        //public static int GetBlogsCount(Item currentItem, string categoryName, string authorID, string tagName, string searchText)
        //{
        //    try
        //    {

        //        var searchLanguage = Context.Language;
        //        List<BlogPost> BlogList = new List<BlogPost>();
        //        using (IProviderSearchContext context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())

        //        {
        //            Expression<Func<SearchResultItem, bool>> predicate = PredicateBuilder.True<SearchResultItem>();
        //            predicate = predicate.And(item => item.TemplateName == "Blog Post");
        //            predicate = predicate.And(item => item.Language == searchLanguage.Name);

        //            if (categoryName != "")
        //            {

        //                //string CategoryId = "";
        //                Category categoryItem = CategoryManager.GetCategoryByName(Context.Item, WebUtility.HtmlEncode(categoryName.Replace("-", " ")));
        //                if (categoryItem != null)
        //                {
        //                    //CategoryId = categoryItem.InnerItem.ID.ToString().ToLower().Replace("-", "").Replace('{', ' ').Replace('}', ' ').Trim();
        //                    //predicate = predicate.And(item => item["category_sm"].Contains(CategoryId));
        //                    BlogList = context.GetQueryable<SearchResultItem>().Where(predicate)
        //                                                .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])
        //                                                .CreateAs<BlogPost>()
        //                                                .Where(x => x.Categories.Any(c => c.CategoryName.Contains(WebUtility.HtmlEncode(categoryName.Replace("-", " ")))))
        //                                                .ToList();
        //                }

        //            }
        //            else if (tagName != "")
        //            {

        //                string TagId = "";
        //                Tag tagItem = TagManager.GetTagByName(Context.Item, WebUtility.HtmlEncode(tagName.Replace("-", " ")));
        //                if (tagItem != null)
        //                {
        //                    TagId = tagItem.InnerItem.ID.ToString().ToLower().Replace("-", "").Replace('{', ' ').Replace('}', ' ').Trim();
        //                    predicate = predicate.And(item => item["tags_sm"].Contains(TagId));
        //                }
        //            }
        //            else if (searchText != "")
        //            {
        //                predicate = predicate.And(item => item["title_t"].Contains(searchText) ||
        //                                              item["body_t"].Contains(searchText));
        //            }
        //            else
        //            {
        //                var searchQuery = context.GetQueryable<SearchResultItem>().Where(predicate)
        //                                         .OrderByDescending(t => t[XBSettings.XBSearchPublishDate]);
        //                BlogList = searchQuery.CreateAs<BlogPost>().ToList();
        //            }
        //            return BlogList.Count();

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("XBlog GetBlogsCount error", ex, new object());
        //    }
        //    return 0;
        //}
        //public static string GetImageUrl(Item SearchPropertyItem)
        //{
        //    ImageField myImageItem = (ImageField)SearchPropertyItem.Fields["BlogPostImage"];
        //    string mediaItemUrl = myImageItem != null ? MediaManager.GetMediaUrl(myImageItem.MediaItem) : string.Empty;

        //    return mediaItemUrl.Replace("/sitecore/shell", "");
        //}
        public static IPagedList<BlogPost> GetBlogPostsFeatured(int maximumRows)
        {
            try
            {
                var searchLanguage = Context.Language;
                using (IProviderSearchContext context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())
                {
                    Expression<Func<SearchResultItem, bool>> predicate = PredicateBuilder.True<SearchResultItem>();
                    predicate = predicate.And(item => item.TemplateName == "Blog Post");
                    predicate = predicate.And(item => item.Language == searchLanguage.Name);




                    IPagedList<BlogPost> FeaturedBlogList = context.GetQueryable<SearchResultItem>().Where(predicate)

                         .Where(t => t["isFeaturedBlog"] == "1")
                         .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])
                         .CreateAs<BlogPost>().ToPagedList();
                    try
                    {
                        foreach (BlogPost lstBlog in FeaturedBlogList)
                        {
                            var blogImageId = lstBlog.BlogImage.Value;

                            if (!String.IsNullOrEmpty(blogImageId))
                            {
                                Database database1 = Sitecore.Context.Database;
                                Item blogImageItem = database1.GetItem(blogImageId);
                                if (blogImageItem != null)
                                {
                                    var ImagePath = blogImageItem.Paths.Path.Replace("/sitecore/media library", "/-/media");

                                    string hashedUrl = HashingUtils.ProtectAssetUrl(Sitecore.StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(blogImageItem, new MediaUrlBuilderOptions { MaxWidth = 1280 })));

                                    lstBlog.BlogImageUrls = hashedUrl;
                                }
                                else
                                {

                                    lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                                }
                            }
                            else
                            {


                                lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                            }
                        }
                    }
                    catch
                    {

                    }
                    return FeaturedBlogList.Take(maximumRows).ToPagedList();

                }
            }
            catch (Exception ex)
            {
                Log.Error("XBlog GetBlogResults error", ex, new object());
            }
            return null;
        }

        //public static int GetFeaturedBlogsCount(Item currentItem, string categoryID, string authorID, string tagID, string searchText)
        //{
        //    try
        //    {
        //        var searchLanguage = Context.Language;
        //        using (IProviderSearchContext context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())
        //        {
        //            Expression<Func<SearchResultItem, bool>> predicate = PredicateBuilder.True<SearchResultItem>();
        //            predicate = predicate.And(item => item.TemplateName == "Blog Post");
        //            predicate = predicate.And(item => item.Language == searchLanguage.Name);



        //            List<BlogPost> BlogList = context.GetQueryable<SearchResultItem>().Where(predicate)
        //                                        .Where(t => t["isFeaturedBlog"] == "1")
        //                                        .OrderByDescending(t => t[XBSettings.XBSearchPublishDate])

        //                                        .CreateAs<BlogPost>().ToList();

        //            return BlogList.Count();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("XBlog GetBlogsCount error", ex, new object());
        //    }
        //    return 0;
        //}
        //public static string GetBlogCategoryPagination(BlogSettings settingsItem, string categoryName, int currentPage, int totalRows, int maximumRows)
        //{
        //    StringBuilder pagination = new StringBuilder();

        //    double decMaxPages = System.Convert.ToDouble(totalRows) / System.Convert.ToDouble(maximumRows);
        //    int maxPages = System.Convert.ToInt32(Math.Ceiling(decMaxPages));

        //    //categoryName = categoryName.Replace("-", " ");
        //    if (maxPages > 1)
        //    {

        //        if (currentPage > maxPages)
        //        {
        //            // outside our range, make first page
        //            currentPage = 1;
        //        }

        //        if (currentPage != 1)
        //        {
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, "1", settingsItem.FirstPageText, XBSettings.XBCategoryQS, categoryName));
        //        }

        //        if (currentPage - 1 != 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), settingsItem.PreviousPageText, XBSettings.XBCategoryQS, categoryName));
        //    }

        //    if (currentPage - 2 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 2), System.Convert.ToString(currentPage - 2), XBSettings.XBCategoryQS, categoryName));

        //    if (currentPage - 1 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), System.Convert.ToString(currentPage - 1), XBSettings.XBCategoryQS, categoryName));

        //    pagination.Append(String.Format("<span>{0}</span>", System.Convert.ToString(currentPage)));

        //    if (currentPage + 1 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), System.Convert.ToString(currentPage + 1), XBSettings.XBCategoryQS, categoryName));

        //    if (currentPage + 2 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 2), System.Convert.ToString(currentPage + 2), XBSettings.XBCategoryQS, categoryName));

        //    if (currentPage + 1 <= maxPages)
        //    {
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), settingsItem.NextPageText, XBSettings.XBCategoryQS, categoryName));
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(maxPages), settingsItem.LastPageText, XBSettings.XBCategoryQS, categoryName));
        //    }




        //    return pagination.ToString();
        //}
        //public static string GetBlogTagPagination(BlogSettings settingsItem, string tagName, int currentPage, int totalRows, int maximumRows)
        //{
        //    StringBuilder pagination = new StringBuilder();

        //    double decMaxPages = System.Convert.ToDouble(totalRows) / System.Convert.ToDouble(maximumRows);
        //    int maxPages = System.Convert.ToInt32(Math.Ceiling(decMaxPages));

        //    if (maxPages > 1)
        //    {

        //        if (currentPage > maxPages)
        //        {
        //            // outside our range, make first page
        //            currentPage = 1;
        //        }

        //        if (currentPage != 1)
        //        {
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, "1", settingsItem.FirstPageText, XBSettings.XBTagQS, tagName));
        //        }
        //        if (currentPage - 1 != 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), settingsItem.PreviousPageText, XBSettings.XBTagQS, tagName));
        //    }

        //    if (currentPage - 2 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 2), System.Convert.ToString(currentPage - 2), XBSettings.XBTagQS, tagName));

        //    if (currentPage - 1 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), System.Convert.ToString(currentPage - 1), XBSettings.XBTagQS, tagName));

        //    pagination.Append(String.Format("<span>{0}</span>", System.Convert.ToString(currentPage)));

        //    if (currentPage + 1 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), System.Convert.ToString(currentPage + 1), XBSettings.XBTagQS, tagName));

        //    if (currentPage + 2 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 2), System.Convert.ToString(currentPage + 2), XBSettings.XBTagQS, tagName));

        //    if (currentPage + 1 <= maxPages)
        //    {
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), settingsItem.NextPageText, XBSettings.XBTagQS, tagName));
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(maxPages), settingsItem.LastPageText, XBSettings.XBTagQS, tagName));
        //    }




        //    return pagination.ToString();
        //}
        //public static string GetBlogSearchPagination(BlogSettings settingsItem, string searchName, int currentPage, int totalRows, int maximumRows)
        //{
        //    StringBuilder pagination = new StringBuilder();

        //    double decMaxPages = System.Convert.ToDouble(totalRows) / System.Convert.ToDouble(maximumRows);
        //    int maxPages = System.Convert.ToInt32(Math.Ceiling(decMaxPages));

        //    if (maxPages > 1)
        //    {

        //        if (currentPage > maxPages)
        //        {
        //            // outside our range, make first page
        //            currentPage = 1;
        //        }

        //        if (currentPage != 1)
        //        {
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, "1", settingsItem.FirstPageText, XBSettings.XBSearchQS, searchName));
        //        }
        //        if (currentPage - 1 != 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), settingsItem.PreviousPageText, XBSettings.XBSearchQS, searchName));
        //    }

        //    if (currentPage - 2 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 2), System.Convert.ToString(currentPage - 2), XBSettings.XBSearchQS, searchName));

        //    if (currentPage - 1 > 0)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), System.Convert.ToString(currentPage - 1), XBSettings.XBSearchQS, searchName));

        //    pagination.Append(String.Format("<span>{0}</span> ", System.Convert.ToString(currentPage)));

        //    if (currentPage + 1 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), System.Convert.ToString(currentPage + 1), XBSettings.XBSearchQS, searchName));

        //    if (currentPage + 2 <= maxPages)
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 2), System.Convert.ToString(currentPage + 2), XBSettings.XBSearchQS, searchName));

        //    if (currentPage + 1 <= maxPages)
        //    {
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), settingsItem.NextPageText, XBSettings.XBSearchQS, searchName));
        //        pagination.Append(String.Format("<a href=\"?{0}={1}&{3}={4}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(maxPages), settingsItem.LastPageText, XBSettings.XBSearchQS, searchName));
        //    }




        //    return pagination.ToString();
        //}
        //public static string GetBlogPagination(int? page, BlogSettings settingsItem, int currentPage, int totalRows, int maximumRows)
        //{
        //    StringBuilder pagination = new StringBuilder();

        //    double decMaxPages = System.Convert.ToDouble(totalRows) / System.Convert.ToDouble(maximumRows);
        //    int maxPages = System.Convert.ToInt32(Math.Ceiling(decMaxPages));

        //    if (maxPages > 1)
        //    {

        //        if (currentPage > maxPages)
        //        {
        //            // outside our range, make first page
        //            currentPage = 1;
        //        }

        //        if (currentPage != 1)
        //        {
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, "1", settingsItem.FirstPageText));
        //        }
        //        if (currentPage - 1 != 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), settingsItem.PreviousPageText));

        //        if (currentPage - 2 > 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 2), System.Convert.ToString(currentPage - 2)));

        //        if (currentPage - 1 > 0)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage - 1), System.Convert.ToString(currentPage - 1)));

        //        pagination.Append(String.Format("<span>{0}</span>", System.Convert.ToString(currentPage)));

        //        if (currentPage + 1 <= maxPages)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), System.Convert.ToString(currentPage + 1)));

        //        if (currentPage + 2 <= maxPages)
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 2), System.Convert.ToString(currentPage + 2)));

        //        if (currentPage + 1 <= maxPages)
        //        {
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(currentPage + 1), settingsItem.NextPageText));
        //            pagination.Append(String.Format("<a href=\"?{0}={1}\">{2}</a> ", XBSettings.XBPageQS, System.Convert.ToString(maxPages), settingsItem.LastPageText));
        //        }
        //    }



        //    return pagination.ToString();
        //}
        public static IList<BlogPost> GetBlogRelatedValues(BlogPost currentItem, string maxDisplay)
        {
            Dictionary<string, int> blogItems = new Dictionary<string, int>();
            IList<BlogPost> blogList = new List<BlogPost>();

            try
            {

                BlogSettings settingsItem = XBlog.General.DataManager.GetBlogSettingsItem(currentItem.InnerItem);
                var searchLanguage = Context.Language;


                using (IProviderSearchContext context = ContentSearchManager.GetIndex("sitecore_web_index").CreateSearchContext())

                {


                    Expression<Func<SearchResultItem, bool>> predicate = PredicateBuilder.True<SearchResultItem>();
                    predicate = predicate.And(item => item.TemplateName == "Blog Post");
                    predicate = predicate.And(item => item.Language == searchLanguage.Name);
                    IEnumerable<BlogPost> resultList = context.GetQueryable<SearchResultItem>().Where(predicate)
                                                        .Where(t => t.ItemId != currentItem.ItemId)
                                                      .OrderByDescending(t => t[XBSettings.XBSearchPublishDate]).CreateAs<BlogPost>();
                    var Tagvalue1 = currentItem.Tags[currentItem.Tags.Count - 1].TagName;

                    var Tagvalue2 = "";
                    var Tagvalue3 = "";
                    if (currentItem.Tags.Count > 2)
                    {
                        Tagvalue2 = currentItem.Tags[currentItem.Tags.Count - 2].TagName;

                        Tagvalue3 = currentItem.Tags[currentItem.Tags.Count - 3].TagName;

                    }
                    var TagList = (from filterBlog in resultList
                                   where
                                        filterBlog.Tags.Any(c => c.TagName.Contains(Tagvalue1))
                                         || filterBlog.Tags.Any(c1 => c1.TagName.Contains(Tagvalue2))
                                         || filterBlog.Tags.Any(c2 => c2.TagName.Contains(Tagvalue3)
                                         )
                                   select filterBlog).Take(Int32.Parse(maxDisplay)).ToList();

                    try
                    {
                        foreach (BlogPost lstBlog in TagList)
                        {
                            var blogImageId = lstBlog.BlogImage.Value;

                            if (!String.IsNullOrEmpty(blogImageId))
                            {
                                Database database1 = Sitecore.Context.Database;
                                Item blogImageItem = database1.GetItem(blogImageId);
                                if (blogImageItem != null)
                                {
                                    var ImagePath = blogImageItem.Paths.Path.Replace("/sitecore/media library", "/-/media");

                                    lstBlog.BlogImageUrls = ImagePath;
                                }
                                else
                                {

                                    lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                                }
                            }
                            else
                            {


                                lstBlog.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                            }
                        }
                    }
                    catch
                    {

                    }

                    return TagList;

                }


            }
            catch (Exception ex)
            {
                Log.Error("XBlog GetAuthorCount error", ex, new object());
            }
            return blogList;
        }
        public static IEnumerable<BlogPost> SetBlogDisplayLimit(string maxDisplay, IEnumerable<BlogPost> blogs)
        {
            int number;
            bool result = Int32.TryParse(maxDisplay, out number);
            var blogsList = blogs.ToList();
            if (!result)
            {
                number = 99;
            }

            if (blogsList.Count < number)
            {
                number = blogsList.Count;
            }

            return blogsList.Take(number);
        }
    }
}
