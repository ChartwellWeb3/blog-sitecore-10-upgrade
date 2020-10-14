using Sitecore.Data.Items;
using Sitecore.Feature.XBlog.Areas.XBlog.Items.Blog;
using Sitecore.Mvc.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using X.PagedList;

namespace Sitecore.Feature.XBlog.Areas.XBlog.Models.Blog
{
    public class BlogListingModel : RenderingModel
    {
        public BlogListingModel()
        {
            CurrentPage = 1;
            MaximumRows = 5;
            StartRowIndex = 1;
            PageResult = false;
        }
        public string SearchHeading { get; set; }
        public string PagingCatQuery { get; set; }
        public string PagingTagQuery { get; set; }
        public string PagingSearchQuery { get; set; }
        public string BlogName { get; set; }
        public int CurrentPage { get; set; }

        public int MaximumRows { get; set; }

        public int TotalRows { get; set; }

        public Item dataSourceItem { get; set; }
        public int StartRowIndex { get; set; }

        public bool PageResult { get; set; }

        public IPagedList<BlogPost> BlogPosts { get; set; }
    }
}