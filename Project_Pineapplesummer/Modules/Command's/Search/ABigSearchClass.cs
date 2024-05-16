using System.Collections.Generic;

namespace Project_Pineapplesummer.Modules.Command_s.Search
{
    public class ABigSearchClass
    {
        public class Url
        {
            public string type { get; set; }
            public string template { get; set; }
        }

        public class RequestItem
        {
            public string title { get; set; }
            public string totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string safe { get; set; }
            public string cx { get; set; }
        }

        public class NextPageItem
        {
            public string title { get; set; }
            public string totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string safe { get; set; }
            public string cx { get; set; }
        }

        public class Queries
        {
            public List<RequestItem> request { get; set; }
            public List<NextPageItem> nextPage { get; set; }
        }

        public class FacetsItem
        {
            public string anchor { get; set; }
            public string label { get; set; }
            public string label_with_op { get; set; }
        }

        public class Context
        {
            public string title { get; set; }
            public List<List<FacetsItem>> facets { get; set; }
        }

        public class SearchInformation
        {
            public double searchTime { get; set; }
            public string formattedSearchTime { get; set; }
            public string totalResults { get; set; }
            public string formattedTotalResults { get; set; }
        }

        public class Cse_thumbnailItem
        {
            public string src { get; set; }
            public string width { get; set; }
            public string height { get; set; }
        }

        public class MetatagsItem
        {
            public string viewport { get; set; }
        }

        public class Cse_imageItem
        {
            public string src { get; set; }
        }

        public class Pagemap
        {
            public List<Cse_thumbnailItem> cse_thumbnail { get; set; }
            public List<MetatagsItem> metatags { get; set; }
            public List<Cse_imageItem> cse_image { get; set; }
        }

        public class LabelsItem
        {
            public string name { get; set; }
            public string displayName { get; set; }
            public string label_with_op { get; set; }
        }

        public class ItemsItem
        {
            public string kind { get; set; }
            public string title { get; set; }
            public string htmlTitle { get; set; }
            public string link { get; set; }
            public string displayLink { get; set; }
            public string snippet { get; set; }
            public string htmlSnippet { get; set; }
            public string cacheId { get; set; }
            public string formattedUrl { get; set; }
            public string htmlFormattedUrl { get; set; }
            public Pagemap pagemap { get; set; }
            public List<LabelsItem> labels { get; set; }
        }

        public class Root
        {
            public string kind { get; set; }
            public Url url { get; set; }
            public Queries queries { get; set; }
            public Context context { get; set; }
            public SearchInformation searchInformation { get; set; }
            public List<ItemsItem> items { get; set; }
        }

    }
}