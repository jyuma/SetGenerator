using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace SetGenerator.WebUI.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class MyUser : IdentityUser<long, MyLogin, MyUserRole, MyClaim>
    {
        public DateTime DateRegistered { get; set; }
        public string IPAddress { get; set; }
        public string BrowserInfo { get; set; }
        public int? DefaultBandId { get; set; }
    }

    public class MyUserRole : IdentityUserRole<long> { }

    public class MyRole : IdentityRole<long, MyUserRole> { }

    public class MyClaim : IdentityUserClaim<long> { }

    public class MyLogin : IdentityUserLogin<long> { }

    public class ApplicationDbContext : IdentityDbContext<MyUser, MyRole, long, MyLogin, MyUserRole, MyClaim>
    {
        public ApplicationDbContext() : base("MyIdentityConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MyUser>().ToTable("User");
            modelBuilder.Entity<MyUserRole>().ToTable("UserRole");
            modelBuilder.Entity<MyRole>().ToTable("Role");
            modelBuilder.Entity<MyClaim>().ToTable("UserClaim");
            modelBuilder.Entity<MyLogin>().ToTable("UserLogin");

            modelBuilder.Entity<MyUser>().Property(r => r.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<MyRole>().Property(r => r.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<MyClaim>().Property(r => r.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }

    //public class MyPasswordHasher : PasswordHasher
    //{
    //    public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
    //    {
    //        return
    //            hashedPassword.Equals(HashPassword(providedPassword))
    //                ? PasswordVerificationResult.Success
    //                : PasswordVerificationResult.Failed;
    //    }

    //    public override string HashPassword(string password)
    //    {
    //        var shal = new SHA1CryptoServiceProvider();
    //        var textToHas = Encoding.Default.GetBytes(password);
    //        var result = shal.ComputeHash(textToHas);
    //        var resultText = Convert.ToBase64String(result);
    //        resultText = resultText.Replace("+", "_");
    //        return resultText;
    //    }
    //}
}