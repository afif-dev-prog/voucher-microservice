using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class StaffListService : IStaffListService
    {
        private readonly DataContext dataContext;
        public readonly ResponseCustomModel<string> rc;
        // private readonly IFloatingService _floatingService;
        public StaffListService(DataContext dataContext, ResponseCustomModel<string> rc)
        {
            this.dataContext = dataContext;
            this.rc = rc;
            // this._floatingService = floatingService;
        }

        public int GetStaffCount()
        {
            return dataContext.stafflist.Count();
        }

        public async Task<List<StaffList>> GetStaffListsAsync()
        {
            return await dataContext.stafflist.ToListAsync();
        }






        public async Task<ResponseCustomModel<string>> CreditIndividual(string studentId, decimal? amount, string userUpdate, string monthCredit)
        {
            using var transaction = await dataContext.Database.BeginTransactionAsync();
            int currentTimestamps = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var groupId = Guid.NewGuid().ToString();
            if (!await CheckExistStudent(studentId))
            {
                rc.Success = false;
                rc.Message = "Student not found!";
            }
            else
            {
                try
                {
                    var studentData = await dataContext.student.Where(x => x.student_id == studentId).FirstOrDefaultAsync();

                    studentData.balance += amount;
                    studentData.date_update = currentTimestamps;
                    var histories = new List<PayHistory>
                    {
                        new PayHistory
                        {

                            transaction_id = groupId,
                            student_id = studentId,
                            seller = "Pusat Pembangunan Kemahiran Sarawak",
                            debit = 0,
                            credit = amount,
                            remark = "Voucher topup",
                            pay_date = currentTimestamps,
                            user_update = userUpdate,
                            month_credit = monthCredit
                        },
                        // new PayHistory
                        // {
                        //     transaction_id = groupId,
                        //     student_id = studentId,
                        //     seller = "Pusat Pembangunan Kemahiran Sarawak",
                        //     debit = amount,
                        //     credit = 0,
                        //     remark = "CREDIT_STUDENT_ALLOWANCE",
                        //     pay_date = currentTimestamps,
                        //     user_update = userUpdate,
                        //     month_credit = monthCredit
                        // }
                    };


                    await dataContext.payhistory.AddRangeAsync(histories);

                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    rc.Message = "Allowance credited successfully";
                    rc.Success = true;
                }
                catch (System.Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            return rc;
        }


        public async Task<ResponseCustomModel<BulkCreditResult>> CreditVoucherInBulk(List<string> studentIds, decimal? amount, string userUpdate, string monthCredit)
        {
            int ct = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var studentIdList = studentIds.Select(id => id.Trim()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var successIds = new List<string>();
            var failedIds = new List<FailedCredit>();
            var result = new ResponseCustomModel<BulkCreditResult>
            {
                Data = new BulkCreditResult()
            };
            var groupId = Guid.NewGuid().ToString();

            var payHistories = new List<PayHistory>();
            using var transaction = await dataContext.Database.BeginTransactionAsync();

            try
            {
                var existingStudents = await dataContext.student.Where(s => studentIdList.Contains(s.student_id)).Select(s => new { s.student_id, s.student_name }).ToListAsync();

                var existingStudentIds = existingStudents.Select(s => s.student_id).ToHashSet();

                var nonExistentIds = studentIdList.Where(id => !existingStudentIds.Contains(id)).ToList();

                foreach (var id in nonExistentIds)
                {
                    failedIds.Add(new FailedCredit
                    {
                        StudentId = id,
                        Reason = "Student not found."
                    });
                }

                if (existingStudents.Any())
                {
                    await dataContext.student.Where(s => existingStudentIds.Contains(s.student_id)).ExecuteUpdateAsync(s => s.SetProperty(s => s.balance, s => s.balance + amount).SetProperty(s => s.date_update, s => ct));

                    foreach (var student in existingStudents)
                    {
                        payHistories.Add(new PayHistory
                        {
                            transaction_id = groupId,
                            student_id = student.student_id,
                            seller = "Pusat Pembangunan Kemahiran Sarawak",
                            debit = 0,
                            credit = amount,
                            remark = "Voucher topup",
                            pay_date = ct,
                            user_update = userUpdate,
                            month_credit = monthCredit
                        });

                        // payHistories.Add(new PayHistory
                        // {
                        //     transaction_id = groupId,
                        //     student_id = student.student_id,
                        //     seller = "Pusat Pembangunan Kemahiran Sarawak",
                        //     debit = amount,
                        //     credit = 0,
                        //     remark = "CREDIT_STUDENT_ALLOWANCE",
                        //     pay_date = ct,
                        //     user_update = userUpdate,
                        //     month_credit = monthCredit
                        // });

                        // await floatingService.DeleteFloatList(student.student_id);
                    }
                    await dataContext.payhistory.AddRangeAsync(payHistories);
                    await dataContext.SaveChangesAsync();
                    successIds.AddRange(existingStudentIds);
                    await transaction.CommitAsync();

                    result.Success = successIds.Any();
                    result.Data.TotalRequested = studentIdList.Count;
                    result.Data.SuccessCount = successIds.Count;
                    result.Data.FailedCount = failedIds.Count;
                    result.Data.SuccessfulIds = successIds;
                    result.Data.FailedRecords = failedIds;
                    result.Message = $"Credited {successIds.Count} out of {studentIdList.Count} students. Amount: RM {amount:F2}";
                }
                else
                {
                    result.Success = successIds.Any();
                    result.Data.TotalRequested = studentIdList.Count;
                    result.Data.SuccessCount = successIds.Count;
                    result.Data.FailedCount = failedIds.Count;
                    result.Data.SuccessfulIds = successIds;
                    result.Data.FailedRecords = failedIds;
                    result.Message = $"Credited {successIds.Count} out of {studentIdList.Count} students. Amount: RM {amount:F2}";
                }

                return result;
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = $"Transaction failed: {ex.Message}";
                return result;
                throw;
            }


        }

        public async Task<ResponseCustomModel<string>> ParkVouchertoFloating(Floating floating)
        {
            var response = new ResponseCustomModel<string>();
            var transaction = await dataContext.Database.BeginTransactionAsync();
            int ct = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {

                var floatExisted = await dataContext.floating.AnyAsync(x => x.student_id == floating.student_id);
                var studentExisted = await dataContext.student.AnyAsync(x => x.student_id == floating.student_id);
                if (!studentExisted)
                {
                    response.Success = true;
                    response.Message = $"Student not found with the id: {floating.student_id}";

                }
                else
                {
                    if (floatExisted)
                    {
                        response.Message = $"This student {floating.student_id} exist in the float table!";
                        response.Success = false;

                    }
                    else
                    {
                        var newFloat = new Floating
                        {
                            student_id = floating.student_id,
                            user_update = floating.user_update,
                            credit = floating.credit,
                            pay_date = ct,
                            month_credit = floating.month_credit,

                        };

                        await dataContext.floating.AddAsync(newFloat);

                        await dataContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        response.Message = $"Student {floating.student_id} successfully park to float table with the amount of RM {floating.credit}!";
                        response.Success = true;
                    }
                }




                return response;
            }
            catch (System.Exception e)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = e.Message.ToString();
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

        public async Task<ResponseCustomModel<string>> CreditVoucher(List<string> studentIds, double amount)
        {
            return rc;
        }
    }
}