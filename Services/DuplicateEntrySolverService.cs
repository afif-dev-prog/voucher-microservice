using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Data;

namespace voucherMicroservice.Services
{
    public class DuplicateEntrySolverService : IDuplicateSolverService
    {
        private readonly Data.DataContext _context;

        public DuplicateEntrySolverService(DataContext context)
        {
            _context = context;
        }


        public async Task UpdateBalance(string sellerid, decimal amount)
        {
            
        }
    }
}