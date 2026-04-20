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
    public class SellerController : ControllerBase
    {
        private readonly ISellerService sellerService;

        public SellerController(ISellerService sellerService)
        {
            this.sellerService = sellerService;
        }

        [HttpGet("/api/voucher/seller/list")]
        public async Task<IActionResult> GetSellerList()
        {
            return Ok(await sellerService.GetSellerList());
        }

        [HttpGet("/api/voucher/seller/transaction/{sellerName}")]
        public async Task<IActionResult> GetTransaction(string sellerName)
        {
            return Ok(await sellerService.TransactionHistory(sellerName));
        }

        [HttpGet("/api/voucher/seller/findseller/{sellerId}")]
        public async Task<IActionResult> FindSeller(int sellerId)
        {
            return Ok(await sellerService.GetSellerById(sellerId));
        }

        [HttpPost("/api/voucher/seller/scantopay")]
        public async Task<IActionResult> ScanToPay(string studentId, int sellerId, decimal? price)
        {
            return Ok(await sellerService.ScanToPay(studentId, sellerId, price));
        }

        [HttpGet("/api/voucher/seller/transaction")]
        public async Task<IActionResult> GetSellerTransaction(string sellerName, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] long? startDate = null, [FromQuery] long? endDate = null)
        {
            var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
            var result = await sellerService.GetSellerTransaction(sellerName, paginationParams, startDate, endDate);
            // var result = await sellerService.GetSellerTransaction(sellerName, paginationParams, startDate, endDate);
            return Ok(result);
        }

        [HttpGet("/api/voucher/seller/list/pagination")]
        public async Task<IActionResult> GetSellerListWithPagination([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string search = "")
        {
            var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
            var result = await sellerService.GetSellerListWithPagination(paginationParams, search);
            return Ok(result);
        }

        [HttpPost("/api/voucher/seller/add")]
        public async Task<IActionResult> AddSeller([FromBody] Seller seller)
        {
            return Ok(await sellerService.AddSeller(seller));
        }

        [HttpPut("/api/voucher/seller/update/{sellerId}")]
        public async Task<IActionResult> UpdateSeller([FromBody] Seller seller, int sellerId)
        {
            return Ok(await sellerService.EditSeller(seller, sellerId));
        }

        [HttpDelete("/api/voucher/seller/delete/{sellerId}")]
        public async Task<IActionResult> DeleteSeller(int sellerId)
        {
            return Ok(await sellerService.DeleteSeller(sellerId));
        }
    }
}