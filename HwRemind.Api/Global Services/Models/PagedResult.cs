namespace HwRemind.Api.Gloabl_Services.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Uri FirstPage { get; set; }
        public Uri LastPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public Uri NextPage { get; set; }
        public Uri PreviousPage { get; set; }

        public PagedResult(IEnumerable<T> pagedData, PageFilter filter, 
            int totalRecords, IUriService uriService, string route)
        {
            Data = pagedData;

            double totalPages =  ( (double) totalRecords / (double) filter.PageSize);
            int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

            NextPage =
            filter.PageNumber >= 1 && filter.PageNumber < roundedTotalPages
            ? uriService.GetPageUri(new PageFilter(filter.PageNumber + 1, filter.PageSize), route)
            : null;

            PreviousPage =
            filter.PageNumber - 1 >= 1 && filter.PageNumber <= roundedTotalPages
            ? uriService.GetPageUri(new PageFilter(filter.PageNumber - 1, filter.PageSize), route)
            : null;

            FirstPage = uriService.GetPageUri(new PageFilter(1, filter.PageSize), route);
            LastPage = uriService.GetPageUri(new PageFilter(roundedTotalPages, filter.PageSize), route);
            TotalPages = roundedTotalPages;
            TotalRecords = totalRecords;
        }
    }
}
