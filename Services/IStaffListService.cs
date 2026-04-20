using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IStaffListService
    {
        int GetStaffCount();
        Task<List<StaffList>> GetStaffListsAsync();
        Task<ResponseCustomModel<string>> CreditIndividual(string studentId, decimal? amount, string userUpdate, string monthCredit);
        Task<ResponseCustomModel<BulkCreditResult>> CreditVoucherInBulk(List<string> studentIds, decimal? amount, string userUpdate, string monthCredit);
        Task<ResponseCustomModel<string>> ParkVouchertoFloating(Floating floating);
    }
}