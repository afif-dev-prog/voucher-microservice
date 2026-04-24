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
    public class FloatingController : ControllerBase
    {
        private readonly IFloatingService floatingService;

        public FloatingController(IFloatingService floatingService)
        {
            this.floatingService = floatingService;
        }

        [HttpGet("/api/voucher/floating/list")]
        public async Task<IActionResult> GetFloating()
        {
            return Ok(await floatingService.GetFloatingAsync());
        }

        [HttpGet("/api/voucher/floating/paginated")]
        public async Task<IActionResult> GetFloatingPaginated([FromQuery] PaginationParams paginationParams, [FromQuery] string search = "")
        {
            var result = await floatingService.GetFloatListPaginated(paginationParams, search);
            Response.Headers.Add("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(result.Pagination));
            return Ok(result);
        }

        [HttpPost("/api/voucher/floating/proceedCredit")]
        public async Task<IActionResult> ProceedFloating([FromBody] ProceedFloat proceedFloat)
        {
            return Ok(await floatingService.ProceedFloat(proceedFloat));
        }

        [HttpDelete("/api/voucher/floating/delete/{studentId}")]
        public async Task<IActionResult> DeleteFloat(string studentId)
        {
            return Ok(await floatingService.DeleteFloatList(studentId));
        }


        [HttpPut("/api/voucher/floating/update/{hId}")]
        public async Task<IActionResult> UpdateFloat(int hId, [FromBody] UpdateFloat updateFloat)
        {
            return Ok(await floatingService.UpdateFloat(hId, updateFloat));
        }
    }
}