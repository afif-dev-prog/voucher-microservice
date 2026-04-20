using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace voucherMicroservice.Model
{
    public class StaffList
    {
        [Key]
        public int req_id { get; set; }
        public string staff_id { get; set; } = string.Empty;
        public string s_name { get; set; } = string.Empty;
        public string s_nickname { get; set; } = string.Empty;
        public string s_campus { get; set; } = string.Empty;
        public string s_dept { get; set; } = string.Empty;
        public string s_designation { get; set; } = string.Empty;
        public string s_email { get; set; } = string.Empty;
        public string icnumber { get; set; } = string.Empty;
        public string phone_no { get; set; } = string.Empty;
        public string s_supervisor { get; set; } = string.Empty;
        public string s_supervisor2 { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string lvl_access { get; set; } = string.Empty;
        public int s_hiredate { get; set; }
        public string s_location { get; set; } = string.Empty;
        public string s_section { get; set; } = string.Empty;
        public string s_position { get; set; } = string.Empty;
        public string teach_status { get; set; } = string.Empty;
        public string job_grade { get; set; } = string.Empty;
        public string employ_status { get; set; } = string.Empty;
        public string staff_status { get; set; } = string.Empty;
        public string firstTime { get; set; } = string.Empty;
        public int access_level { get; set; }
        public int last_password_change { get; set; }
    }
}