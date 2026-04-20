using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IFloatingService
    {
        Task<List<Floating>> GetFloatingAsync();
        Task<PagedResult<Floating>> GetFloatListPaginated(PaginationParams paginationParams, string search = "");
        Task<ResponseCustomModel<BulkCreditResult>> ProceedFloat(ProceedFloat proceedFloat);
        Task<ResponseCustomModel<string>> DeleteFloatList(string studentId);
        Task<ResponseCustomModel<string>> UpdateFloat(int hId, UpdateFloat updateFloat);
    }
}