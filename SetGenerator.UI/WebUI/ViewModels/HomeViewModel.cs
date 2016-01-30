namespace SetGenerator.WebUI.ViewModels
{
    public class LogonViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ProfileEditViewModel
    {
        public string NewUserName { get; set; }
        public string OldUserName { get; set; }
        public string NewEmailAddress { get; set; }
        public string OldEmailAddress { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}