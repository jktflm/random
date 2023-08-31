using System.ComponentModel.DataAnnotations;

namespace InvestigationClearance.Models.Domain
{
    public class Clearance
    {

        public Guid Id { get; set; }
        public string BranchName { get; set; }
        public string Date { get; set; }
        public string IdPresented { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CivilStatus { get; set; }
        public string EducationalAttainment { get; set; }
        public int LandlineNumber { get; set; }
        public int MobileNumber { get; set; }
        public string Email { get; set; }
        public string Complexion { get; set; }
        public string Marks { get; set; }
        public string Religion { get; set; }
        public Double Height { get; set; }
        public Double Weight { get; set; }
        public string SpouseName { get; set; }
        public string FatherName { get; set; }
        public string FatherPlaceOfBirth { get; set; }
        public string MotherName { get; set; }
        public string MotherPlaceOfBirth { get; set; }
    }
}
