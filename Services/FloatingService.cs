using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class FloatingService : IFloatingService
    {
        private readonly DataContext dataContext;
        private readonly IStaffListService _staffListService;

        public FloatingService(DataContext dataContext, IStaffListService _staffListService)
        {
            this.dataContext = dataContext;
            this._staffListService = _staffListService;
        }

        public async Task<List<Floating>> GetFloatingAsync()
        {
            return await dataContext.floating
                            .ToListAsync();
        }

        public async Task<PagedResult<Floating>> GetFloatListPaginated(PaginationParams paginationParams, string search = "")
        {
            var result = new PagedResult<Floating>();
            try
            {
                IQueryable<Floating> query = dataContext.floating;

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(s =>
                        s.student_id.ToLower().Contains(search)
                    );
                }

                query = query.OrderByDescending(s => s.pay_date);


                // combines exist check + count into ONE query
                int totalCount = await query.CountAsync();

                List<Floating> floatList = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(s => new Floating
                    {
                        h_id = s.h_id,
                        student_id = s.student_id,
                        month_credit = s.month_credit,
                        user_update = s.user_update,
                        credit = s.credit,
                        pay_date = s.pay_date
                    })
                    .ToListAsync();

                result.Success = true;
                result.Data = floatList;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = paginationParams.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = paginationParams.PageNumber > 1,
                    HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
                };
                result.Message = $"Retrieved {floatList.Count} of {totalCount} transactions";
                return result;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Success = false;
                return result;
            }
        }

        public async Task<ResponseCustomModel<BulkCreditResult>> ProceedFloat(ProceedFloat proceedFloat)
        {
            var response = new ResponseCustomModel<BulkCreditResult>();


            try
            {
                // var floatRows = dataContext.floating.Where(f => proceedFloat.ids.Contains(f.student_id)).ToListAsync();


                var result = await _staffListService.CreditVoucherInBulk(proceedFloat.ids, proceedFloat.amount, proceedFloat.user_update, proceedFloat.month_credit);


                if (result.Success)
                {
                    System.Console.WriteLine("success");
                    response.Success = result.Success;
                    response.Message = result.Message;
                    response.Data = result.Data;

                    foreach (var id in proceedFloat.ids)
                    {

                        await DeleteFloatList(id);
                    }
                }
                else
                {
                    System.Console.WriteLine("error");
                    response.Success = false;
                    response.Message = result.Message;
                    response.Data = result.Data;
                }
                System.Console.WriteLine(response.Data);
                return response;

            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("catch error");
                response.Success = false;
                response.Message = e.Message.ToString();
                return response;
                throw;
            }

        }

        public async Task<ResponseCustomModel<string>> DeleteFloatList(string studentId)
        {
            var response = new ResponseCustomModel<string>();
            int ct = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            using var transaction = await dataContext.Database.BeginTransactionAsync();

            try
            {
                await dataContext.floating.Where(s => s.student_id == studentId).ExecuteDeleteAsync();
                await dataContext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Message = $"Deleted {studentId} from float list!";

                return response;
            }
            catch (System.Exception e)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = e.Message.ToString();

                return response;
                throw;
            }


        }

        public async Task<ResponseCustomModel<string>> UpdateFloat(int hId, UpdateFloat updateFloat)
        {
            using var transaction = await dataContext.Database.BeginTransactionAsync();

            var response = new ResponseCustomModel<string>();
            int ct = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();


            try
            {
                var floatData = await dataContext.floating.AnyAsync(x => x.h_id == hId);

                if (!floatData)
                {
                    response.Message = $"Student with matric no : {updateFloat.student_id} is not found or incorrectly type";
                    response.Success = false;

                    return response;
                }
                else
                {
                    var floatUpdateData = await dataContext.floating.Where(x => x.h_id == hId).ExecuteUpdateAsync(s => s.SetProperty(s => s.student_id, s => updateFloat.student_id)
                    .SetProperty(s => s.credit, s => updateFloat.amount)
                    .SetProperty(s => s.month_credit, s => updateFloat.month_credit)
                    .SetProperty(s => s.pay_date, s => ct)
                    .SetProperty(s => s.user_update, updateFloat.user_update));

                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.Success = true;
                    response.Message = "Updated data successfully";
                    return response;
                }
            }
            catch (System.Exception e)
            {
                response.Message = e.Message.ToString();
                response.Success = false;

                return response;
                throw;
            }
        }
    }
}