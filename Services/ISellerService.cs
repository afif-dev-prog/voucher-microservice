using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface ISellerService
    {
        Task<List<Seller>> GetSellerList();
        Task<ResponseCustomModel<List<PayHistory>>> TransactionHistory(string sellerName);
        Task<PagedResult<Seller>> GetSellerListWithPagination(PaginationParams paginationParams, string search = "");
        Task<ResponseCustomModel<string>> ScanToPay(string studentId, int sellerId, decimal? price);
        Task<ResponseCustomModel<Seller>> GetSellerById(int sellerId);
        // Task<PagedResult<PayHistory>> GetSellerTransaction(string sellerName, PaginationParams paginationParams, string? startDate = null, string? endDate = null);
        Task<PagedResult<PayHistory>> GetSellerTransaction(string sellerName, PaginationParams paginationParams, long? startDate = null, long? endDate = null);
        Task<ResponseCustomModel<string>> AddSeller(Seller seller);
        Task<ResponseCustomModel<string>> EditSeller(Seller seller, int sellerId);
        Task<ResponseCustomModel<string>> DeleteSeller(int sellerId);
    }
}