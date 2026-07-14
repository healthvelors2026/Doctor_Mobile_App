using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models

{
    public class Employee
    {
        [Key]
        public int EmployeeIDP { get; set; }

        public string? FName { get; set; }

        public string? MName { get; set; }

        public string? LName { get; set; }
    }
}