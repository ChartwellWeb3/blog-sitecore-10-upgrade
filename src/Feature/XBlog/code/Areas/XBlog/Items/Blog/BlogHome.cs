using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Feature.XBlog.Areas.XBlog.ItemMapper;
using Sitecore.Feature.XBlog.Areas.XBlog.ItemMapper.Configuration.Attributes;

namespace Sitecore.Feature.XBlog.Areas.XBlog.Items.Blog
{
    [SitecoreItemTemplate(BlogPostParentTemplateId)]
    public class BlogHome : SitecoreItem
    {
        public const string BlogPostParentTemplateId = "{A68F6495-0EA2-4B54-A96C-4577EDB6EBEE}";
        public const string BlogPostParentTemplate = "Blog Home";

        public const string BlogSettingsFieldId = "{8DA365F3-8504-42D8-9407-99A48D6EC424}";

        public const string BlogNameFieldId = "{522642CF-7B22-4879-B75C-8295A849BE76}";


        [SitecoreItemField(BlogSettingsFieldId)]
        public virtual Item BlogSettings { get; set; }

        [SitecoreItemField(BlogNameFieldId)]
        public virtual string BlogName { get; set; } 
    }
}
