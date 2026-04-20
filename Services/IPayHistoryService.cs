using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IPayHistoryService
    {
        Task<List<PayHistory>> GetPayHistoriesAsync();
        Task<ResponseCustomModel<string>> CreatePayHistoryAsync(PayHistory payHistory);

        Task<PagedResult<PayHistoryDto>> GetTransactionHistoryAsync(
        string sellerName,
        TransactionFilterParams filterParams);

        Task<int> GetTransactionCountBySellerAsync(string sellerName);
        Task<decimal> GetTotalSalesBySellerAsync(string sellerName, int? startDate = null, int? endDate = null);
        // Task<ResponseCustomModel<List<PayHistory>>> GetHistoryBySeller(string sellername);
    }
}