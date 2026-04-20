using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class PayHistoryService : IPayHistoryService
    {
        private readonly DataContext dataContext;
        private readonly ILogger<PayHistoryService> _logger;
        public PayHistoryService(DataContext dataContext, ILogger<PayHistoryService> logger)
        {
            this.dataContext = dataContext;
            this._logger = logger;
        }
        public async Task<List<PayHistory>> GetPayHistoriesAsync()
        {
            // return await dataContext.payhistory.ToListAsync();
            return await dataContext.payhistory.Where(p => p.seller == "Koperasi Sarawak Skills").Skip((1 - 1) * 10)
                    .Take(10).ToListAsync();
        }

        public async Task<ResponseCustomModel<string>> CreatePayHistoryAsync(PayHistory payHistory)
        {
            ResponseCustomModel<string> rc = new ResponseCustomModel<string>();
            // var transaction = await dataContext.Database.BeginTransactionAsync();
            try
            {
                await dataContext.payhistory.AddAsync(payHistory);

                rc.Success = true;
                rc.Message = "Pay history created successfully.";



                return rc;
            }
            catch (System.Exception)
            {
                throw;
            }


        }

        public async Task<ResponseCustomModel<List<PayHistory>>> GetPAyhistoryBySeller(string seller)
        {
            var rs = new ResponseCustomModel<List<PayHistory>>();

            var sellerExist = await dataContext.seller.FirstOrDefaultAsync(s => s.s_name == seller);
            if (sellerExist == null)
            {
                rs.Success = false;
                rs.Message = "Seller not found!";
            }
            else
            {
                var sellerHistory = await dataContext.payhistory.Where(p => p.seller == seller).ToListAsync();
                rs.Success = true;
                rs.Message = "Seller found!";
                rs.Success = true;
                rs.Count = sellerHistory.Count;
                rs.Data = sellerHistory;
            }



            return rs;
        }


        // public async Task<PagedResult<PayHistory>> GetPagedHistory(PaginationParams paginationParams)
        // {
        //     var result = new PagedResult<PayHistory>();
        //     var query = dataContext.payhistory.AsQueryable();


        // }


        private IQueryable<PayHistory> ApplyFilters(IQueryable<PayHistory> query, TransactionFilterParams filterParams)
        {
            if (filterParams.StartDate.HasValue)
            {
                query = query.Where(p => p.pay_date >= filterParams.StartDate.Value);
            }
            if (filterParams.EndDate.HasValue)
            {
                query = query.Where(p => p.pay_date <= filterParams.EndDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(filterParams.TransactionType))
            {
                switch (filterParams.TransactionType.ToLower())
                {
                    case "credit":
                        query = query.Where(p => p.credit > 0);
                        break;
                    case "debit":
                        query = query.Where(p => p.debit > 0);
                        break;
                }
            }
            if (filterParams.MinAmount.HasValue)
            {
                query = query.Where(p =>
                    (p.credit ?? 0) >= filterParams.MinAmount.Value ||
                    (p.debit ?? 0) >= filterParams.MinAmount.Value);
            }
            return query;
        }

        public async Task<int> GetTransactionCountBySellerAsync(string sellerName)
        {
            return await dataContext.payhistory
                .CountAsync(p => p.seller == sellerName);
        }


        public async Task<decimal> GetTotalSalesBySellerAsync(
        string sellerName,
        int? startDate = null,
        int? endDate = null)
        {
            var query = dataContext.payhistory
                .Where(p => p.seller == sellerName);

            if (startDate.HasValue)
                query = query.Where(p => p.pay_date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.pay_date <= endDate.Value);

            return await query.SumAsync(p => p.debit ?? 0);
        }

        public async Task<PagedResult<PayHistoryDto>> GetTransactionHistoryAsync(
        string sellerName,
        TransactionFilterParams filterParams)
        {
            var result = new PagedResult<PayHistoryDto>();

            try
            {
                // ✅ GOOD: Build query with IQueryable (no execution yet)
                IQueryable<PayHistory> query = dataContext.payhistory
                    .AsNoTracking()  // Read-only, improves performance
                    .Where(p => p.seller == sellerName);  // Still IQueryable - translated to SQL

                // Apply filters (still building SQL query)
                query = ApplyFilters(query, filterParams);

                // Apply sorting (still IQueryable)
                query = query.OrderByDescending(p => p.pay_date);

                // ✅ Execute COUNT query first (only gets count from DB)
                var totalCount = await query.CountAsync();
                // SQL: SELECT COUNT(*) FROM payhistory WHERE seller = 'X'

                var totalPages = (int)Math.Ceiling(totalCount / (double)filterParams.PageSize);

                // ✅ Execute paginated query (only gets required page)
                var transactions = await query
                    .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                    .Take(filterParams.PageSize)
                    .Select(p => new PayHistoryDto
                    {
                        Id = p.h_id,
                        StudentId = p.student_id,
                        Seller = p.seller,
                        Debit = p.debit,
                        Credit = p.credit,
                        Remark = p.remark,
                        PayDate = p.pay_date,
                        UserUpdate = p.user_update,
                        MonthCredit = p.month_credit
                    })
                    .ToListAsync();  // NOW execute - but only get 20 records
                                     // SQL: SELECT * FROM payhistory WHERE seller = 'X' 
                                     //      ORDER BY pay_date DESC LIMIT 20 OFFSET 0

                result.Data = transactions;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = filterParams.PageNumber,
                    TotalPages = totalPages,
                    PageSize = filterParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = filterParams.PageNumber > 1,
                    HasNext = filterParams.PageNumber < totalPages
                };
                result.Success = true;
                result.Message = $"Retrieved {transactions.Count} of {totalCount} transactions";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transaction history: {ex.Message}");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
                return result;
            }
        }

    }

}