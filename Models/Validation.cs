namespace XISD6329_Task1_CRMS.Models
{
    // The email and password rules, written once and used by every form.
    // The Android app uses the same rules (ui/UiExtensions.kt).
    public static class Validation
    {
        // local part + @ + domain name + top-level domain (.com, .org, .net ...), e.g. name@example.com
        public const string EmailPattern = @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(\.[A-Za-z0-9\-]+)*\.[A-Za-z]{2,}$";
        public const string EmailMessage = "Enter a valid email, e.g. name@example.com";

        // longer than 6 characters (7+), with an uppercase letter, a lowercase letter, a number and a special character
        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{7,}$";
        public const string PasswordMessage = "Password must be longer than 6 characters and include an uppercase letter, a lowercase letter, a number and a special character.";
    }
}