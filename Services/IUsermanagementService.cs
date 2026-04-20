using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using voucherMicroservice.Model;

namespace voucherMicroservice.Services
{
    public interface IUsermanagementService
    {
        Task<ResponseCustomModel<string>> CreateStudent(Student studentRequest);
        Task<ResponseCustomModel<string>> CreateStudentBulk(List<Student> students);
        Task<ResponseCustomModel<string>> UpdateStudent(UpdateStudent student, string studentId);
        Task<ResponseCustomModel<Student>> GetStudentById(string studentId);
        Task<ResponseCustomModel<string>> DeleteStudent(string studentId);
        Task<ResponseCustomModel<string>> WrongCreditBySeller(string studentId, decimal wrongamount, string sellerName, decimal exactamount);
    }
}