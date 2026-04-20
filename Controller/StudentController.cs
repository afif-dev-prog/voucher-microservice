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
    public class StudentController : ControllerBase
    {
        private readonly IStudentService studentService;
        public StudentController(IStudentService studentService)
        {
            this.studentService = studentService;
        }

        [HttpGet("/api/voucher/student/list")]
        public async Task<IActionResult> GetStudentList()
        {
            return Ok(await studentService.GetStudentsAsync());
        }

        [HttpGet("/api/voucher/student/find/{id}")]

        public async Task<IActionResult> GetStudentById(string id)
        {
            return Ok(await studentService.GetStudentById(id));
        }

        [HttpGet("/api/voucher/student/balance/{studentId}")]
        public async Task<IActionResult> GetBalance(string studentId)
        {
            return Ok(await studentService.TransactionHistory(studentId));
        }

        [HttpGet("/api/voucher/student/transaction/{studentId}")]
        public async Task<IActionResult> GetTransactionList(string studentId)
        {
            return Ok(await studentService.TransactionHistory(studentId));
        }

        [HttpGet("/api/voucher/student/paginated")]
        public async Task<IActionResult> GetStudentsPaginated([FromQuery] PaginationParams paginationParams, [FromQuery] string search = "")
        {
            var result = await studentService.GetStudentsAsync(paginationParams, search);
            Response.Headers.Add("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(result.Pagination));
            return Ok(result);
        }

        [HttpGet("/api/voucher/student/transaction/paginated/{studentId}")]
        public async Task<IActionResult> GetTransactionListPaginated(string studentId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await studentService.GetHistoryByStudentId(studentId, paginationParams);
            Response.Headers.Add("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(result.Pagination));
            return Ok(result);
        }
    }
}