using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class PagingResponse<T>
    {
        public int TotalRecords { get; set; }   
        public int CurrentPageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int CountPerRequest { get; set; }
        public T Data { get; set; }
        public string? Message { get; set; }
        public PagingResponse(T data, int totalRecords, int currentPageNumber, int pageSize, int countPerRequest)
        {
            Data = data;
            TotalRecords = totalRecords;
            CurrentPageNumber = currentPageNumber;
            PageSize = pageSize;

            TotalPages = Convert.ToInt32(Math.Ceiling(((double)TotalRecords / (double)pageSize)));
            CountPerRequest = countPerRequest;
            HasNextPage = CurrentPageNumber < TotalPages;
            HasPreviousPage = CurrentPageNumber > 1;
        }
    }
}