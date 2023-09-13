using System;
using System.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Links;
using Foundation.SitecoreExtensions.Extensions;

namespace Foundation.URLRedirection.Pipelines
{
    public class PermanentURLRedirect : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            try
            {
                string language = Context.Language.ToString().ToLower();
                string requestedURL = args.HttpContext.Request.RawUrl;
                int skipValue = requestedURL.Contains("/en/") || requestedURL.Contains("/ar/") ? 2 : 1;
                string itemPath = "/sitecore/content/SiteNode/Home/" + string.Join("", args.HttpContext.Request.Url.Segments.Skip(skipValue));
                Item CMSItem = Context.Database.GetItem(itemPath.Replace("-", " "));
                string CMSHostName = Sitecore.Configuration.Settings.GetSetting("CMSHostName");
                //Check if Item present in content tree or not and
                //Check if URL is not Sitecore CMS URL
                if (CMSItem == null && !args.HttpContext.Request.Url.Host.Equals(CMSHostName))
                {
                    using (var searchContext = ContentSearchManager.GetIndex(Sitecore.Configuration.Settings.GetSetting("SitecoreIndex"))
                                    .CreateSearchContext())
                    {
                        var result = searchContext.GetQueryable<SearchResultItem>()
                            .Where(item => item.Paths.Contains(Context.Site.GetRootItem().ID))
                            .Where(item => item.Language == language)
                            .Where(item => item[Templates._Redirection.Fields.LinkToRedirect_FieldName].Equals(requestedURL))
                            .Select(item => Context.Database.GetItem(item.ItemId)).FirstOrDefault();
                        if (result != null)
                        {
                            var url = LinkManager.GetItemUrl(result);
                            args.HttpContext.Response.RedirectPermanent(url);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}