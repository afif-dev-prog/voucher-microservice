using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class PaginationParams
    {
        // Pagination request parameters

        private const int MaxPageSize = 10000;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    // Pagination metadata
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }

    // Paginated response wrapper
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public PaginationMetadata Pagination { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal? LegacyOpeningBalance { get; set; }
        public long? CutoverDate { get; set; }
    }
}