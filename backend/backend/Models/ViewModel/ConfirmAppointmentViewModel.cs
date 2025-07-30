using System;
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels.Booking
{
    public class ConfirmAppointmentViewModel
    {
        public string IdConfirmAppointment { get; set; }

        [Required(ErrorMessage = "Doctor name is required")]
        public string NameDr { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public string DoctorId { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        public string Slot { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public string PatientId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string PatientEmail { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Symptoms { get; set; }

        // For search and filter functionality
        public string SearchTerm { get; set; }
        public string FilterDoctorId { get; set; }
        public DateTime? FilterDate { get; set; }
    }
}