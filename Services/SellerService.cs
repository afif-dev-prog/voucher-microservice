using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class SellerService : ISellerService
    {
        private readonly DataContext dataContext;
        public readonly ResponseCustomModel<string> rc;
        public readonly ResponseCustomModel<List<Seller>> rcL;
        public readonly ResponseCustomModel<List<PayHistory>> rcP;
        private readonly IPayHistoryService payHistoryService;
        private readonly IPasswordService passwordService;
        private readonly ILogger<SellerService> logger;

        public SellerService(DataContext dataContext, ResponseCustomModel<string> rc, ResponseCustomModel<List<Seller>> rcL, IPayHistoryService payHistoryService, ResponseCustomModel<List<PayHistory>> rcP, ILogger<SellerService> logger, IPasswordService passwordService)
        {
            this.dataContext = dataContext;
            this.rc = rc;
            this.rcL = rcL;
            this.rcP = rcP;
            this.payHistoryService = payHistoryService;
            this.logger = logger;
            this.passwordService = passwordService;
        }

        public async Task<List<Seller>> GetSellerList()
        {
            var seller = await dataContext.seller.ToListAsync();
            // rcL.Count = seller.Count;
            // rcL.Success = true;
            // rcL.Data = seller;
            return seller;
        }

        // public async Task<ResponseCustomModel<List<PayHistory>>> TransactionHistory(string sellerName)
        // {
        //     var transactionHistory = await dataContext.payhistory.OrderByDescending(p => p.pay_date).Where(p => p.seller == sellerName).Where(p =>
        //         (p.transaction_id == null || p.transaction_id == "") ||
        //         (p.transaction_id != null && p.transaction_id != "" && p.debit > 0)
        //     ).ToListAsync();
        //     rcP.Success = true;
        //     rcP.Count = transactionHistory.Count;
        //     rcP.Data = transactionHistory;
        //     return rcP;
        // }

        public async Task<ResponseCustomModel<List<PayHistory>>> TransactionHistory(string sellerName)
        {
            var transactionHistory = await dataContext.payhistory
                .Where(p => p.seller == sellerName)
                .Where(p =>
                    (p.transaction_id == null || p.transaction_id == "No Value") ||
                    (p.transaction_id != null && p.transaction_id != "No Value" && p.credit > 0)
                )
                .OrderByDescending(p => p.pay_date)
                .ToListAsync();

            rcP.Success = true;
            rcP.Count = transactionHistory.Count;
            rcP.Data = transactionHistory;
            return rcP;
        }
        public async Task<ResponseCustomModel<string>> ScanToPay(string studentId, int sellerId, decimal? price)
        {

            using var transaction = await dataContext.Database.BeginTransactionAsync();

            try
            {
                if (!await CheckExistStudent(studentId))
                {
                    rc.Success = false;
                    rc.Message = "Student not exist!";
                }


                if (!await CheckStudentBalance(studentId, price))
                {
                    rc.Success = false;
                    rc.Message = "Insufficient Balance!";
                }


                int currentTimestamps = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var groupId = Guid.NewGuid().ToString();
                // var studentPayment = await dataContext.student.Where(x => x.student_id == studentId).ExecuteUpdateAsync(st => st.SetProperty(st => st.balance, st => st.balance - price).SetProperty(s => s.date_update, s => currentTimestamps));
                // var sellerReceive = await dataContext.seller.Where(x => x.s_id == sellerId).ExecuteUpdateAsync(s => s.SetProperty(s => s.balance, s => s.balance + price).SetProperty(s => s.date_update, s => currentTimestamps));

                var studentData = await dataContext.student.Where(s => s.student_id == studentId).FirstOrDefaultAsync();
                var sellerData = await dataContext.seller.Where(s => s.s_id == sellerId).FirstOrDefaultAsync();

                studentData.balance -= price;
                studentData.date_update = currentTimestamps;

                sellerData.balance += price;
                sellerData.date_update = currentTimestamps;

                var histories = new List<PayHistory>
                {
                  new PayHistory
                  {
                    transaction_id = groupId,
                    student_id = studentId,
                    seller = sellerData?.s_name,
                    debit = price,
                    credit = 0,
                    remark = $"Payment to {sellerData?.s_name}",
                    pay_date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    user_update = studentData?.student_name,
                    month_credit = string.Empty
                  },
                //   new PayHistory
                //   {
                //     transaction_id = groupId,
                //     student_id = studentId,
                //     seller = sellerData?.s_name,
                //     debit = 0,
                //     credit = price,
                //     remark = "SELLER_PAYMENT_RECEIVED",
                //     pay_date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                //     user_update = sellerData?.s_name,
                //     month_credit = string.Empty
                // }
                };

                await dataContext.payhistory.AddRangeAsync(histories);
                await dataContext.SaveChangesAsync();
                await transaction.CommitAsync();
                rc.Success = true;
                rc.Message = "Transaction Successful!";
                return rc;
            }
            catch (System.Exception)
            {

                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<bool> CheckExistStudent(string studentId)
        {
            var check = await dataContext.student.AnyAsync(x => x.student_id == studentId);
            if (check)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CheckStudentBalance(string studentId, decimal? price)
        {
            var check = await dataContext.student.FirstOrDefaultAsync(x => x.student_id == studentId);

            if (check?.balance < price)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<ResponseCustomModel<Seller>> GetSellerById(int sellerId)
        {
            var findSeller = await dataContext.seller.FirstOrDefaultAsync(s => s.s_id == sellerId);
            var rs = new ResponseCustomModel<Seller>();
            if (findSeller.Equals(0))
            {
                rs.Success = false;
                rs.Message = "Seller id wrong or not found";
                return rs;
            }
            else
            {
                rs.Success = true;
                rs.Message = "Found";
                rs.Count = 1;
                rs.Data = findSeller;

                return rs;
            }

        }

        public async Task<ResponseCustomModel<string>> ClaimVoucher(ClaimParam claimParam, string sellerId)
        {
            return rc;
        }

        // public async Task<PagedResult<PayHistory>> GetSellerTransaction(string sellerName, PaginationParams paginationParams, string? startDate = "", string? endDate = "")
        // {
        //     var result = new PagedResult<PayHistory>();

        //     try
        //     {
        //         IQueryable<PayHistory> query = dataContext.payhistory.Where(p => p.seller == sellerName);
        //         // Parse and apply date filters
        //         if (!string.IsNullOrWhiteSpace(startDate) &&
        //             DateTimeOffset.TryParse(startDate, out var start))
        //         {
        //             long startUnix = start.ToUnixTimeSeconds();
        //             query = query.Where(p => p.pay_date >= startUnix);
        //         }

        //         if (!string.IsNullOrWhiteSpace(endDate) &&
        //             DateTimeOffset.TryParse(endDate, out var end))
        //         {
        //             // Include the full end day
        //             long endUnix = end.AddDays(1).AddSeconds(-1).ToUnixTimeSeconds();
        //             query = query.Where(p => p.pay_date <= endUnix);
        //         }

        //         query = query.OrderByDescending(p => p.pay_date);

        //         int totalCount = await query.CountAsync();

        //         var transactions = await query
        //             .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
        //             .Take(paginationParams.PageSize)
        //             .ToListAsync();

        //         result.Data = transactions;
        //         result.Pagination = new PaginationMetadata
        //         {
        //             CurrentPage = paginationParams.PageNumber,
        //             TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
        //             PageSize = paginationParams.PageSize,
        //             TotalCount = totalCount,
        //             HasPrevious = paginationParams.PageNumber > 1,
        //             HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
        //         };
        //         result.Success = true;
        //         result.Message = $"Retrieved {transactions.Count} of {totalCount}";
        //         return result;

        //     }
        //     catch (System.Exception e)
        //     {
        //         logger.LogError($"Error: {e.Message}");
        //         result.Success = false;
        //         result.Message = e.Message;
        //         return result;
        //         throw;
        //     }
        // }

        // public async Task<PagedResult<PayHistory>> GetSellerTransaction(string sellerName, PaginationParams paginationParams, long? startDate = null, long? endDate = null)
        // {
        //     var result = new PagedResult<PayHistory>();

        //     try
        //     {
        //         IQueryable<PayHistory> query = dataContext.payhistory
        //             .Where(p => p.seller == sellerName);

        //         if (startDate.HasValue)
        //             query = query.Where(p => p.pay_date >= startDate.Value);

        //         if (endDate.HasValue)
        //             query = query.Where(p => p.pay_date <= endDate.Value);

        //         query = query.OrderByDescending(p => p.pay_date);

        //         int totalCount = await query.CountAsync();

        //         var transactions = await query
        //             .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
        //             .Take(paginationParams.PageSize)
        //             .ToListAsync();

        //         result.Data = transactions;
        //         result.Pagination = new PaginationMetadata
        //         {
        //             CurrentPage = paginationParams.PageNumber,
        //             TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
        //             PageSize = paginationParams.PageSize,
        //             TotalCount = totalCount,
        //             HasPrevious = paginationParams.PageNumber > 1,
        //             HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
        //         };
        //         result.Success = true;
        //         result.Message = $"Retrieved {transactions.Count} of {totalCount}";
        //         return result;
        //     }
        //     catch (Exception e)
        //     {
        //         logger.LogError($"Error: {e.Message}");
        //         result.Success = false;
        //         result.Message = e.Message;
        //         return result;
        //     }
        // }

        public async Task<PagedResult<PayHistory>> GetSellerTransaction(string sellerName, PaginationParams paginationParams, long? startDate = null, long? endDate = null)
        {
            var result = new PagedResult<PayHistory>();
            sellerName = sellerName?.Trim().ToLower();
            // sellerName = sellerName?.Trim().Replace("\u00A0", " ") ?? string.Empty;
            try
            {
                IQueryable<PayHistory> query = dataContext.payhistory
                    .Where(p => p.seller.Trim().ToLower() == sellerName);
                //             .Where(p =>
                //             // legacy — income is in debit column
                //             (p.transaction_id == null || p.transaction_id == "" || p.transaction_id == "No Value")
                //             ||
                //             // double-entry seller row — income is in credit column
                //             (p.transaction_id != null && p.transaction_id != "No Value" && p.credit > 0)
                // );

                if (startDate.HasValue)
                    query = query.Where(p => p.pay_date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(p => p.pay_date <= endDate.Value);

                // query = query.Where(p => p.`);
                query = query.OrderByDescending(p => p.pay_date);

                int totalCount = await query.CountAsync();

                var transactions = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                result.Data = transactions;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = paginationParams.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = paginationParams.PageNumber > 1,
                    HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
                };
                result.Success = true;
                result.Message = $"Retrieved {transactions.Count} of {totalCount}";
                return result;
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e.Message}");
                result.Success = false;
                result.Message = e.Message;
                return result;
            }
        }

        //     public async Task<PagedResult<PayHistory>> GetSellerTransaction(
        // string sellerName, PaginationParams paginationParams,
        // string? startDate = null, string? endDate = null)
        //     {
        //         var result = new PagedResult<PayHistory>();

        //         try
        //         {
        //             // Get cutover snapshot if exists
        //             var snapshot = await dataContext.sellerBalanceSnapshot
        //                 .FirstOrDefaultAsync(s => s.seller_name == sellerName);

        //             IQueryable<PayHistory> query = dataContext.payhistory
        //                 .Where(p => p.seller == sellerName);

        //             // Era-aware filter:
        //             // Legacy rows  → transaction_id is null/empty  (show as-is, debit = income)
        //             // New rows     → transaction_id has value AND credit > 0 (seller's receive row only)
        //             if (snapshot != null)
        //             {
        //                 query = query.Where(p =>
        //                     (p.transaction_id == null || p.transaction_id == "") ||
        //                     (p.transaction_id != null && p.transaction_id != "" && p.credit > 0)
        //                 );
        //             }
        //             else
        //             {
        //                 // No cutover yet — show legacy rows only (original behaviour)
        //                 query = query.Where(p =>
        //                     (p.transaction_id == null || p.transaction_id == "") ||
        //                     (p.transaction_id != null && p.transaction_id != "" && p.debit > 0)
        //                 );
        //             }

        //             // Date filters
        //             if (!string.IsNullOrWhiteSpace(startDate) &&
        //                 DateTimeOffset.TryParse(startDate, out var start))
        //                 query = query.Where(p => p.pay_date >= start.ToUnixTimeSeconds());

        //             if (!string.IsNullOrWhiteSpace(endDate) &&
        //                 DateTimeOffset.TryParse(endDate, out var end))
        //                 query = query.Where(p => p.pay_date <= end.AddDays(1).AddSeconds(-1).ToUnixTimeSeconds());

        //             query = query.OrderByDescending(p => p.pay_date);

        //             int totalCount = await query.CountAsync();

        //             var transactions = await query
        //                 .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
        //                 .Take(paginationParams.PageSize)
        //                 .ToListAsync();

        //             // Attach legacy opening balance in metadata if snapshot exists
        //             result.Data = transactions;
        //             result.Pagination = new PaginationMetadata
        //             {
        //                 CurrentPage = paginationParams.PageNumber,
        //                 TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
        //                 PageSize = paginationParams.PageSize,
        //                 TotalCount = totalCount,
        //                 HasPrevious = paginationParams.PageNumber > 1,
        //                 HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
        //             };
        //             result.Success = true;
        //             result.LegacyOpeningBalance = snapshot?.legacy_balance;  // ← pass to frontend
        //             result.CutoverDate = snapshot?.cutover_date;
        //             result.Message = $"Retrieved {transactions.Count} of {totalCount}";
        //             return result;
        //         }
        //         catch (Exception e)
        //         {
        //             logger.LogError($"Error: {e.Message}");
        //             result.Success = false;
        //             result.Message = e.Message;
        //             return result;
        //         }
        //     }

        public async Task<PagedResult<Seller>> GetSellerListWithPagination(PaginationParams paginationParams, string search = "")
        {
            var result = new PagedResult<Seller>();

            try
            {
                IQueryable<Seller> query = dataContext.seller.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(s => s.s_name.ToLower().Contains(search));
                }

                int totalCount = await query.CountAsync();

                var sellers = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                result.Data = sellers;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = paginationParams.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = paginationParams.PageNumber > 1,
                    HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
                };
                result.Success = true;
                result.Message = $"Retrieved {sellers.Count} of {totalCount}";
                return result;

            }
            catch (System.Exception e)
            {
                logger.LogError($"Error: {e.Message}");
                result.Success = false;
                result.Message = e.Message;
                return result;
                throw;
            }
        }

        public async Task<ResponseCustomModel<string>> AddSeller(Seller seller)
        {
            var result = new ResponseCustomModel<string>();
            using var transaction = await dataContext.Database.BeginTransactionAsync();
            // string temporaryPassword = passwordService.GenerateTemporaryPassword();
            string autoGeneratedPassword = "Skills@" + seller.username;

            string hashedPassword = passwordService.HashPassword(autoGeneratedPassword);
            try
            {
                var checkSeller = await dataContext.seller.AnyAsync(s => s.s_name.ToLower() == seller.s_name.ToLower());
                if (checkSeller)
                {
                    result.Success = false;
                    result.Message = "Seller name already exist!";
                    return result;
                }
                else
                {
                    seller.balance = 0;
                    seller.date_update = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    seller.firstTime = "Yes";
                    seller.password = hashedPassword;
                    await dataContext.seller.AddAsync(seller);
                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                result.Success = true;
                result.Message = "Seller added successfully!";
                return result;
            }
            catch (System.Exception e)
            {
                await transaction.RollbackAsync();
                logger.LogError($"Error: {e.Message}");
                result.Success = false;
                result.Message = e.Message;
                return result;
                throw;
            }
        }

        public async Task<ResponseCustomModel<string>> EditSeller(Seller seller, int sellerId)
        {
            var result = new ResponseCustomModel<string>();
            using var transaction = await dataContext.Database.BeginTransactionAsync();
            int currentTimestamps = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var findSeller = await dataContext.seller.FirstOrDefaultAsync(s => s.s_id == seller.s_id);
            if (findSeller == null)
            {
                result.Success = false;
                result.Message = "Seller id wrong or not found!";
                return result;
            }
            else
            {
                try
                {

                    var sellerdata = await dataContext.seller.Where(s => s.s_id == sellerId).ExecuteUpdateAsync(s => s.SetProperty(s => s.s_name, s => seller.s_name)
                    .SetProperty(s => s.username, s => s.username)
                    .SetProperty(s => s.balance, s => s.balance)
                    .SetProperty(s => s.s_email, s => s.s_email)
                    .SetProperty(s => s.date_update, s => currentTimestamps));
                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();


                    result.Success = true;
                    result.Message = "Seller updated successfully!";
                    return result;
                }
                catch (System.Exception e)
                {
                    await transaction.RollbackAsync();
                    logger.LogError($"Error: {e.Message}");
                    result.Success = false;
                    result.Message = e.Message.ToString();
                    return result;
                    throw;
                }
            }

        }

        public async Task<ResponseCustomModel<string>> DeleteSeller(int sellerId)
        {
            using var transaction = await dataContext.Database.BeginTransactionAsync();

            var response = new ResponseCustomModel<string>();

            var checkExist = await dataContext.seller.AnyAsync(x => x.s_id == sellerId);

            if (!checkExist)
            {
                response.Message = "Seller not found or wrong id!";
                response.Success = false;
            }
            else
            {
                try
                {
                    await dataContext.seller.Where(s => s.s_id == sellerId).ExecuteDeleteAsync();
                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    response.Success = true;
                    response.Message = "Seller deleted successfully!";

                }
                catch (System.Exception ex)
                {
                    await transaction.RollbackAsync();
                    response.Success = false;
                    response.Message = ex.Message.ToString();
                    return response;
                    throw;
                }

            }

            return response;
        }
    }

}