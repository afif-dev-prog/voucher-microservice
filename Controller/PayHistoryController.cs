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
    public class PayHistoryController : ControllerBase
    {
        private readonly IPayHistoryService payHistoryService;
        public PayHistoryController(IPayHistoryService payHistoryService)
        {
            this.payHistoryService = payHistoryService;
        }

        [HttpGet("/api/voucher/payhistory/list")]
        public async Task<IActionResult> GetPayHistory()
        {
            return Ok(await payHistoryService.GetPayHistoriesAsync());
        }

        [HttpPost("/api/voucher/payhistory/create")]
        public async Task<IActionResult> CreatePayHistory([FromBody] Model.PayHistory payHistory)
        {
            return Ok(await payHistoryService.CreatePayHistoryAsync(payHistory));
        }

        // [HttpGet("/api/voucher/payhistory/seller")]
        // public async Task<IActionResult> GetBySeller(string sellername)
        // {
        //     return Ok(await payHistoryService.GetHistoryBySeller(sellername));
        // }

        [HttpGet("/api/voucher/payhistory/seller")]
        public async Task<IActionResult> GetHistoryListBySeller(string sellername, [FromQuery] TransactionFilterParams filterParams)
        {
            var result = await payHistoryService.GetTransactionHistoryAsync(sellername, filterParams);
            return Ok(result);
        }
    }
}