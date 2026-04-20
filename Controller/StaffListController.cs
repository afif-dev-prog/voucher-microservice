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
    public class StaffListController : ControllerBase
    {
        private readonly IStaffListService staffListService;
        public StaffListController(IStaffListService staffListService)
        {
            this.staffListService = staffListService;
        }

        [HttpGet("/api/voucher/staff/count")]
        public async Task<IActionResult> GetStaffList()
        {
            return Ok(staffListService.GetStaffCount());
        }

        [HttpGet("/api/voucher/staff/list")]
        public async Task<IActionResult> GetStaff()
        {
            return Ok(await staffListService.GetStaffListsAsync());
        }



        [HttpPost("/api/voucher/staff/creditvoucher")]
        public async Task<IActionResult> CreditVoucher([FromQuery] string studentId, [FromQuery] decimal? amount, [FromQuery] string userUpdate, [FromQuery] string monthCredit)
        {
            return Ok(await staffListService.CreditIndividual(studentId, amount, userUpdate, monthCredit));
        }

        [HttpPost("/api/voucher/staff/creditvoucherinbulk")]
        public async Task<IActionResult> CreditVoucherInBulk([FromQuery] List<string> studentIds, [FromQuery] decimal? amount, [FromQuery] string userUpdate, [FromQuery] string monthCredit)
        {
            return Ok(await staffListService.CreditVoucherInBulk(studentIds, amount, userUpdate, monthCredit));
        }

        [HttpPost("/api/voucher/staff/parkvouchertofloat")]
        public async Task<IActionResult> ParktoFloat([FromBody] Floating floating)
        {
            return Ok(await staffListService.ParkVouchertoFloating(floating));
        }

    }
}