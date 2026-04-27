using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using voucherMicroservice.Model;
using voucherMicroservice.Services;

namespace voucherMicroservice.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUsermanagementService usermanagementService;
        public UserManagementController(IUsermanagementService usermanagementService)
        {
            this.usermanagementService = usermanagementService;
        }

        [HttpPost("/api/voucher/usermanagement/addstudent")]
        public async Task<IActionResult> AddNewStudent([FromBody] CreateStudent student)
        {
            return Ok(await usermanagementService.CreateStudent(student));
        }

        [HttpPost("/api/voucher/usermanagement/addbulkstudent")]
        public async Task<IActionResult> CreateBulkStudent([FromBody] List<CreateStudent> students)
        {
            return Ok(await usermanagementService.CreateStudentBulk(students));
        }

        [HttpPut("/api/voucher/usermanagement/updatedata/{studentId}")]
        public async Task<IActionResult> UpdateData([FromBody] UpdateStudent student, string studentId)
        {
            return Ok(await usermanagementService.UpdateStudent(student, studentId));
        }

        [HttpGet("/api/voucher/usermanagement/student/find/{studentId}")]
        public async Task<IActionResult> FindStudent(string studentId)
        {
            return Ok(await usermanagementService.GetStudentById(studentId));
        }

        [HttpDelete("/api/voucher/usermanagement/deletedata/{studentId}")]
        public async Task<IActionResult> DeleteStudent(string studentId)
        {
            return Ok(await usermanagementService.DeleteStudent(studentId));
        }

        [HttpPost("/api/voucher/usermanagement/wrongcreditbyseller")]
        public async Task<IActionResult> WrongCreditBySeller(string studentId, decimal wrongamount, string sellerName, decimal exactamount)
        {
            return Ok(await usermanagementService.WrongCreditBySeller(studentId, wrongamount, sellerName, exactamount));
        }
        [HttpPost("/api/voucher/usermanagement/wrongcreditbyfinance")]
        public async Task<IActionResult> WrongCreditByFinance(string studentId, decimal wrongamount, string sellerName, decimal exactamount)
        {
            return Ok(await usermanagementService.WrongCreditByFinance(studentId, wrongamount, sellerName, exactamount));
        }
    }
}