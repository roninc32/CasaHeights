// ViewModels/ContactDirectoryViewModel.cs
namespace CasaHeights.ViewModels
{
    public class ContactDirectoryViewModel
    {
        public List<UserContactViewModel> AdminContacts { get; set; } = new List<UserContactViewModel>();
        public List<UserContactViewModel> StaffContacts { get; set; } = new List<UserContactViewModel>();
        public List<UserContactViewModel> HomeownerContacts { get; set; } = new List<UserContactViewModel>();
    }

    public class UserContactViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string HouseNumber { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; }

        // Helper property for image display
        public string GetInitials()
        {
            if (string.IsNullOrEmpty(Name))
                return "?";
                
            var nameParts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (nameParts.Length > 1)
                return $"{nameParts[0][0]}{nameParts[1][0]}".ToUpper();
                
            return Name.Substring(0, 1).ToUpper();
        }
        
        public string GetRoleColor()
        {
            return Role switch
            {
                "Admin" => "danger",
                "Staff" => "info",
                "User" => "success",
                _ => "secondary"
            };
        }
    }
}
