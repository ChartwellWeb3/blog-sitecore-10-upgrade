using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Feature.XBlog.Areas.XBlog.Items.Blog;
using Sitecore.Links.UrlBuilders;
using Sitecore.Mvc.Presentation;
using Sitecore.Resources.Media;
using System;

namespace Sitecore.Feature.XBlog.Areas.XBlog.Models.Blog
{
    public class BlogPostModel : RenderingModel
    {
        public BlogPost BlogPost { get; set; }

        public override void Initialize(Rendering rendering)
        {
            base.Initialize(rendering);
            BlogPost = Context.Item.CreateAs<BlogPost>();
            //if (rendering.Item != null && rendering.Item.IsValidType<BlogPost>())
            //{
            //    BlogPost = rendering.Item.CreateAs<BlogPost>();
            //}

            var blogImageId = BlogPost.BlogImage.Value;
            
            if (!String.IsNullOrEmpty(blogImageId))
            {
                Database database1 = Context.Database;
                Item blogImageItem = database1.GetItem(blogImageId);

                if (blogImageItem != null)
                {
                    //var ImagePath = blogImageItem.Paths.Path.Replace("/sitecore/media library", "/-/media");
                    var ImagePath = MediaManager.GetMediaUrl(blogImageItem, new MediaUrlBuilderOptions { Scale = 1 });
                    BlogPost.BlogImageUrls = HashingUtils.ProtectAssetUrl(ImagePath);
                }
                else
                {

                    BlogPost.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
                }
            }
            else
            {


                BlogPost.BlogImageUrls = "-/media/Images/Fotolia_231660132_Subscription_Monthly_M-991x991";
            }

        }
    }
}