using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Data;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public class StudentService : IStudentService
    {
        private readonly DataContext dataContext;
        private readonly ResponseCustomModel<string> rc;
        private readonly ResponseCustomModel<List<PayHistory>> rcL;
        private readonly ResponseCustomModel<PayHistory> rcP;
        private readonly ILogger<StudentService> _logger;

        public StudentService(DataContext dataContext, ResponseCustomModel<string> rc, ResponseCustomModel<List<PayHistory>> rcL, ResponseCustomModel<PayHistory> rcP, ILogger<StudentService> logger)
        {
            this.dataContext = dataContext;
            this.rc = rc;
            this.rcL = rcL;
            this.rcP = rcP;
            this._logger = logger;
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            return await dataContext.student.ToListAsync();
        }


        public async Task<PagedResult<PayHistory>> GetHistoryByStudentId(string studentId, PaginationParams paginationParams)
        {
            var result = new PagedResult<PayHistory>();
            try
            {
                // IQueryable<PayHistory> query = dataContext.payhistory
                //     .Where(s => s.student_id == studentId)
                //     .Where(p =>
                //         (p.transaction_id == null || p.transaction_id == "" || p.transaction_id == "No Value") ||
                //         (p.transaction_id != null && p.transaction_id != "" && p.transaction_id != "No Value" && p.debit > 0)
                //     )
                //     .OrderByDescending(s => s.pay_date);

                IQueryable<PayHistory> query = dataContext.payhistory
                .Where(s => s.student_id == studentId)
                // .Where(p =>
                //     (p.transaction_id == null || p.transaction_id == "" || p.transaction_id == "No Value") ||
                //     (p.transaction_id != null && p.transaction_id != "" && p.transaction_id != "No Value" &&
                //         (p.debit > 0 || p.credit > 0)) // both directions
                // )
                .OrderByDescending(s => s.pay_date);
                // combines exist check + count into ONE query
                int totalCount = await query.CountAsync();

                if (totalCount == 0)
                {
                    // optionally verify if student actually exists
                    bool studentExists = await dataContext.student.AnyAsync(s => s.student_id == studentId);
                    result.Success = false;
                    result.Message = studentExists ? "No transactions found." : "Student not found!";
                    return result;
                }

                List<PayHistory> historyList = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                result.Success = true;
                result.Data = historyList;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = paginationParams.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = paginationParams.PageNumber > 1,
                    HasNext = paginationParams.PageNumber * paginationParams.PageSize < totalCount
                };
                result.Message = $"Retrieved {historyList.Count} of {totalCount} transactions";
                return result;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Success = false;
                return result;
            }
        }

        public async Task<PagedResult<Student>> GetStudentsAsync(PaginationParams paginationParams, string search = "")
        {
            var result = new PagedResult<Student>();

            try
            {
                // IQueryable — no DB hit yet, just builds expression tree
                IQueryable<Student> query = dataContext.student;

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(s =>
                        s.student_name.ToLower().Contains(search) ||
                        s.student_id.ToLower().Contains(search)
                    );
                }

                // Always order for consistent pagination
                query = query.OrderByDescending(s => s.date_update);

                // Single COUNT query against filtered result
                var totalCount = await query.CountAsync();

                // Single paginated SELECT query
                var students = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(s => new Student
                    {
                        id = s.id,
                        student_id = s.student_id,
                        student_name = s.student_name,
                        nric = s.nric,
                        email = s.email,
                        balance = s.balance,
                        date_update = s.date_update,
                        firstTime = s.firstTime,
                        status = s.status,
                        register_date = s.register_date,
                        complete_date = s.complete_date,
                        intake = s.intake,
                        course_code = s.course_code,
                        month_credit = s.month_credit,
                        campus = s.campus,
                        // batch = s.batch,
                        last_password_change = s.last_password_change,
                    })
                    .ToListAsync(); // DB hit happens here

                result.Data = students;
                result.Pagination = new PaginationMetadata
                {
                    CurrentPage = paginationParams.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    HasPrevious = paginationParams.PageNumber > 1,
                    HasNext = paginationParams.PageNumber < (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
                };
                result.Success = true;
                result.Message = $"Retrieved {students.Count} students out of {totalCount}";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting students: {ex.Message}");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
                return result;
            }
        }

        public async Task<ResponseCustomModel<Student>> GetStudentById(string studentid)
        {
            ResponseCustomModel<Student> cr = new ResponseCustomModel<Student>();
            try
            {
                var existed = await dataContext.student.Where(x => x.student_id == studentid).AnyAsync();

                if (!existed)
                {
                    cr.Message = "Student Id not in the system!";
                    cr.Success = false;
                }
                else
                {
                    var existed2 = await dataContext.student.Where(x => x.student_id == studentid).FirstOrDefaultAsync();
                    cr.Data = existed2;
                    cr.Message = "Found!";
                    cr.Success = true;
                }
                return cr;
            }
            catch (System.Exception e)
            {
                cr.Message = e.Message.ToString();
                cr.Success = false;
                return cr;
            }
        }

        public async Task<ResponseCustomModel<string>> AddStudent(Student student)
        {
            using var transaction = await dataContext.Database.BeginTransactionAsync();
            int ts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string autoGeneratedPassword = "Skills@" + student.nric;
            try
            {

                if (await CheckStudentExist(student.student_id))
                {
                    rc.Success = false;
                    rc.Message = $"Student with {student.student_id} already exist!";
                }
                else
                {
                    var newStudent = await dataContext.student.AddAsync(new Student()
                    {
                        student_id = student.student_id,
                        student_name = student.student_name,
                        nric = student.nric,
                        email = student.email,
                        password = autoGeneratedPassword,
                        register_date = student.register_date,
                        complete_date = student.complete_date,
                        intake = student.intake,
                        course_code = student.course_code,
                        balance = student.balance,
                        date_update = ts,
                        month_credit = student.month_credit,
                        campus = student.campus,
                        // batch = student.batch,
                        firstTime = "Yes",
                        status = student.status
                    });
                    rc.Success = true;
                    rc.Message = "Added student successfully.";
                    await dataContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                }

                return rc;
            }
            catch (System.Exception e)
            {
                await transaction.RollbackAsync();
                rc.Success = false;
                rc.Message = e.Message.ToString();
                throw;
            }
        }

        public async Task<ResponseCustomModel<Student>> ViewBalance(string studentId)
        {
            // var transaction = await dataContext.Database.BeginTransactionAsync();
            ResponseCustomModel<Student> rcS = new ResponseCustomModel<Student>();
            try
            {
                if (!await CheckStudentExist(studentId))
                {
                    rcS.Message = "Student not found!";
                    rcS.Success = false;
                }
                else
                {


                    var studentData = await dataContext.student.Where(s => s.student_id == studentId).FirstAsync();

                    rcS.Success = true;
                    rcS.Message = "Found!";
                    // rcS.Count = studentData.Count;
                    rcS.Data = studentData;
                }
            }
            catch (System.Exception)
            {
                // await transaction.RollbackAsync();
                throw;
            }
            return rcS;
        }

        public async Task<bool> CheckStudentExist(string studentId)
        {
            var check = await dataContext.student.AnyAsync(x => x.student_id == studentId.Trim());
            if (check)
            {
                return true;
            }
            else
            {
                return false;

            }

        }

        public async Task<ResponseCustomModel<List<PayHistory>>> TransactionHistory(string studentId)
        {
            try
            {
                if (!await CheckStudentExist(studentId))
                {
                    rcL.Message = "Student not found!";
                    rcL.Success = false;
                }
                else
                {


                    var historyList = await dataContext.payhistory.Where(s => s.student_id == studentId).ToListAsync();

                    rcL.Success = true;
                    rcL.Message = "Found!";
                    rcL.Count = historyList.Count;
                    rcL.Data = historyList;
                }
                return rcL;
            }
            catch (System.Exception)
            {
                // await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ResponseCustomModel<string>> StudentScanToPay(string studentId, string sellerUsername, decimal? price)
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

                if (!await CheckExistSeller(sellerUsername))
                {
                    rc.Success = false;
                    rc.Message = "Seller not exist!";
                }


                int currentTimestamps = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var groupId = Guid.NewGuid().ToString();
                // var studentPayment = await dataContext.student.Where(x => x.student_id == studentId).ExecuteUpdateAsync(st => st.SetProperty(st => st.balance, st => st.balance - price).SetProperty(s => s.date_update, s => currentTimestamps));
                // var sellerReceive = await dataContext.seller.Where(x => x.s_id == sellerId).ExecuteUpdateAsync(s => s.SetProperty(s => s.balance, s => s.balance + price).SetProperty(s => s.date_update, s => currentTimestamps));

                var studentData = await dataContext.student.Where(s => s.student_id == studentId).FirstOrDefaultAsync();
                var sellerData = await dataContext.seller.Where(s => s.username == sellerUsername).FirstOrDefaultAsync();

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

        public async Task<bool> CheckExistSeller(string sellerUsername)
        {
            var check = await dataContext.seller.AnyAsync(x => x.username == sellerUsername);

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


    }
}