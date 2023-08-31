using System.ComponentModel.DataAnnotations;

namespace InvestigationClearance.Models
{
    public class ClearanceViewModel
        
    {
        public Guid Id { get; set; }
        [Required]
        public string BranchName { get; set; }
        [Required]

        public string Date { get; set; }
        [Required]

        public string IdPresented { get; set; }
        [Required]

        public string LastName { get; set; }
        [Required]

        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        public IFormFile NewPicture { get; set; }

        [Required]
        public string DateOfBirth { get; set; }
        [Required]

        public string Gender { get; set; }
        [Required]

        public string CivilStatus { get; set; }
        [Required]

        public string EducationalAttainment { get; set; }

        public int LandlineNumber { get; set; }
        [Required]

        public int MobileNumber { get; set; }
        [Required]
        [EmailAddress]

        public string Email { get; set; }
        [Required]

        public string Complexion { get; set; }
     
        public string Marks { get; set; }

        public string Religion { get; set; }
        [Required]

        public Double Height { get; set; }
        [Required]

        public Double Weight { get; set; }

        public string SpouseName { get; set; }

        public string FatherName { get; set; }

        public string FatherPlaceOfBirth { get; set; }
  
        public string MotherName { get; set; }
  
        public string MotherPlaceOfBirth { get; set; }
        
        public string pictureUrl { get; set; }
    }
}
