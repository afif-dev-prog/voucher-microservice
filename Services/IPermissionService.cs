using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Services
{
    public interface IPermissionService
    {
        Task<List<string>> GetUserPermissionsAsync(string userId, string role);
    }
}