using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class SellerBalanceSnapshot
    {
        [Key]
        public int id { get; set; }
        public string seller_name { get; set; } = string.Empty;
        public decimal legacy_balance { get; set; }      // sum of debit from legacy rows
        public long cutover_date { get; set; }            // unix timestamp of cutover
        public int created_at { get; set; }
    }
}