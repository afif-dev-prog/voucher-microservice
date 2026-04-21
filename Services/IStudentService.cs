using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IStudentService
    {
        Task<List<Student>> GetStudentsAsync();
        Task<PagedResult<Student>> GetStudentsAsync(PaginationParams paginationParams, string search = "");
        Task<ResponseCustomModel<Student>> GetStudentById(string studentid);
        Task<ResponseCustomModel<Student>> ViewBalance(string studentId);
        Task<ResponseCustomModel<List<PayHistory>>> TransactionHistory(string studentId);
        Task<PagedResult<PayHistory>> GetHistoryByStudentId(string studentId, PaginationParams paginationParams);
        Task<ResponseCustomModel<string>> StudentScanToPay(string studentId, string sellerUsername, decimal? price);
    }
}