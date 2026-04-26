using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class
    Student
    {
        [Key]
        public int id { get; set; }
        public string student_id { get; set; } = string.Empty;
        public string student_name { get; set; } = string.Empty;
        public string nric { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int register_date { get; set; }
        public int complete_date { get; set; }
        public string intake { get; set; } = string.Empty;
        public string course_code { get; set; } = string.Empty;
        public decimal? balance { get; set; }
        public int date_update { get; set; }
        public string month_credit { get; set; } = string.Empty;
        public string campus { get; set; } = string.Empty;
        // public string batch { get; set; } = string.Empty;
        public string firstTime { get; set; } = string.Empty;
        public int last_password_change { get; set; }
        public bool must_change_password { get; set; } = false;
        public string status { get; set; } = string.Empty;

    }

    public class CreateStudent
    {
        public string student_id { get; set; } = string.Empty;
        public string student_name { get; set; } = string.Empty;
        public string nric { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int register_date { get; set; }
        public int complete_date { get; set; }
        public string intake { get; set; } = string.Empty;
        public string course_code { get; set; } = string.Empty;
        public string campus { get; set; } = string.Empty;
    }

    public class UpdateStudent
    {
        public string student_name { get; set; } = string.Empty;
        public string nric { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public int register_date { get; set; }
        public int complete_date { get; set; }
        public string intake { get; set; } = string.Empty;
        public string course_code { get; set; } = string.Empty;
        public decimal? balance { get; set; }
        public int date_update { get; set; }
        public string month_credit { get; set; } = string.Empty;
        public string campus { get; set; } = string.Empty;
        public string batch { get; set; } = string.Empty;
        public string firstTime { get; set; } = string.Empty;
        public int last_password_change { get; set; }
        public string status { get; set; } = string.Empty;
    }
}