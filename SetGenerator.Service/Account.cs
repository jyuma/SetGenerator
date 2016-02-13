using System;
using System.Collections;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SetGenerator.Service
{
    public interface IAccount
    {
        User Login(string userName, string password);
        long CreateUser(string username, string email, string password, int bandId, string ip, string browser);
        void DeleteUser(int id);
        void UpdateUserDisabled(string uname, bool disabled);
        IEnumerable<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int bandId);
        IEnumerable<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId);
        void UpdateUserPreferenceTableColumns(string userName, IDictionary cols);
        void UpdateUserPreferenceTableMembers(string userName, IDictionary cols);
        User GetUserByUserName(string username);
        User GetUser(int userId);
        List<string> ValidateLogin(string username, string password);
        List<string> ValidateProfile(string username, string email);
        List<string> ValidateRegister(string username, string email, string password, string confirmPassword);
        List<string> ValidateChangePassword(string password, string confirmPassword);
        IEnumerable<UserBand> GetUserBands(string uname);
    }

    public class Account : IAccount
    {
        private const int DefaultBandId = 1;  // Default to 1 for now
        private readonly IUserRepository _userRepository;
        private readonly ITableColumnRepository _tableColumnRepository;
        private readonly IMemberRepository _memberRepository;
        private bool _invalid;

        public Account(IUserRepository userRepository, ITableColumnRepository tableColumnRepository, IMemberRepository memberRepository)
        {
            _userRepository = userRepository;
            _tableColumnRepository = tableColumnRepository;
            _memberRepository = memberRepository;
        }

        public User Login(string username, string password)
        {
            var user = _userRepository.GetByUserName(username);
            if (user != null)
                if (VerifyHashPassword(password, user.PasswordHash.Trim()))
                    return user;
            return null;
        }

        public void UpdateUserDisabled(string uname, bool disabled)
        {
            var u = _userRepository.GetByUserName(uname);
            u.IsDisabled = disabled;
            _userRepository.Update(u);
        }

        public void UpdateUserProfile(int userId, string username, string emailAddress)
        {
            var u = _userRepository.Get(userId);
            if (u == null) return;
            u.UserName = username;
            u.Email = emailAddress;

            _userRepository.Update(u);
        }

        public void UpdateUserPassword(int userId, string password)
        {
            var u = _userRepository.Get(userId);
            if (u == null) return;
            u.PasswordHash = GeneratePasswordHash(password.Trim());
            _userRepository.Update(u);
        }

        public IEnumerable<UserPreferenceTableColumn> GetTableColumnsByBandId(int userId, int tableId, int bandId)
        {
            return _userRepository.GetTableColumnsByBandId(userId, tableId, bandId);
        }

        public IEnumerable<UserPreferenceTableMember> GetTableMembersByBandId(int userId, int tableId, int bandId)
        {
            return _userRepository.GetTableMembersByBandId(userId, tableId, bandId);
        }

        public void UpdateUserPreferenceTableColumns(string userName, IDictionary cols) 
        {
            var u = _userRepository.GetByUserName(userName);
            if (u == null) return;

            foreach (var uptc in u.UserPreferenceTableColumns
                .Where(x => cols.Contains(x.Id))
                .OrderBy(o => o.TableColumn.Sequence))
            {
                var isvisible = Convert.ToBoolean(cols[uptc.Id]);
                uptc.IsVisible = isvisible;
            }

            _userRepository.Update(u);
        }

        public void UpdateUserPreferenceTableMembers(string userName, IDictionary cols)
        {
            var u = _userRepository.GetByUserName(userName);
            if (u == null) return;

            foreach (var uptm in u.UserPreferenceTableMembers
                .Where(x => cols.Contains(x.Id))
                .OrderBy(o => o.Member.FirstName)
                .ThenBy(o => o.Member.LastName))
            {
                var isvisible = Convert.ToBoolean(cols[uptm.Id]);
                uptm.IsVisible = isvisible;
            }

            _userRepository.Update(u);
        }

        public long CreateUser(string username, string email, string password, int bandId, string ip, string browser)
        {
            var u = new User
                {
                    UserName = username,
                    Email = email,
                    PasswordHash = GeneratePasswordHash(password.Trim()),
                    IPAddress = ip,
                    BrowserInfo = browser,
                    DateRegistered = DateTime.Now,
                    UserPreferenceTableColumns = GetUserPreferenceTableColumns(),
                    UserPreferenceTableMembers = GetUserPreferenceTableMembers(bandId),
                    IsDisabled = false,
                    DefaultBandId = DefaultBandId
                };
            _userRepository.Add(u);
            u = GetUserByUserName(username);
            if (u != null) return u.Id;
            return -1;
        }

        public IEnumerable<UserBand> GetUserBands(string uname = null)
        {
            return _userRepository.GetUserBands(uname);
        }

        private IEnumerable<UserPreferenceTableColumn> GetUserPreferenceTableColumns()
        {
            var columns = _tableColumnRepository.GetAll();

            return columns.Select(c => new UserPreferenceTableColumn
                                           {
                                               TableColumn = c, 
                                               IsVisible = true
                                           }).ToList();
        }

        private IEnumerable<UserPreferenceTableMember> GetUserPreferenceTableMembers(int bandId)
        {
            var members = _memberRepository.GetAll()
                .Where(x => x.Band.Id == bandId)
                .ToList();

            var list = members.Select(m => new UserPreferenceTableMember
                                               {
                                                   Table = _tableColumnRepository.GetTable(Constants.UserTable.SongId), 
                                                   Member = m, IsVisible = true
                                               }).ToList();

            list.AddRange(members.Select(m => new UserPreferenceTableMember
                                                  {
                                                      Table = _tableColumnRepository.GetTable(Constants.UserTable.SetId), 
                                                      Member = m, IsVisible = true
                                                  }));
            return list;
        }

        public User GetUserByUserName(string username)
        {
            var u = _userRepository.GetByUserName(username);
            return u;
        }

        public User GetUser(int userId)
        {
            var u = _userRepository.Get(userId);
            return u;
        }

        public List<string> ValidateLogin(string username, string password)
        {
            var msgs = new List<string>();

            // validate username
            msgs = ValidateUserName(username, msgs);

            // check for empty password
            if (password == null)
                msgs.Add("Password is required.");
            else if (password.Length == 0)
                msgs.Add("Password is required.");

            if (msgs.Count > 0) return msgs;

            // if all fields have been entered correctly, check the credentials
            User user = Login(username, password);

            if (user == null)
                msgs.Add("The username or password provided is not correct.");

            // add a user transaction record
            //_userRepository.AddUserTransaction(username, Constants.TransactionTypeIdLogin, (msgs.Count == 0));

            return msgs.Count > 0 ? msgs : null;
        }

        public List<string> ValidateRegister(string username, string email, string password, string confirmPassword)
        {
            var msgs = new List<string>();

            msgs = ValidateUserName(username, msgs);
            msgs = ValidateEmail(email, msgs);
            msgs = ValidateNewPassword(password, confirmPassword, msgs);

            // if all fields have been entered correctly, check for duplicate usernames
            if (msgs.Count == 0)
            {
                if (UserNameExists(username))
                    msgs.Add("This username already exists");
            }

            return msgs.Count > 0 ? msgs : null;
        }

        public List<string> ValidateProfile(string username, string email)
        {
            var msgs = new List<string>();

            msgs = ValidateUserName(username, msgs);
            msgs = ValidateEmail(email, msgs);

            return msgs.Count > 0 ? msgs : null;
        }

        public List<string> ValidateChangePassword(string password, string confirmPassword)
        {
            var msgs = new List<string>();

            msgs = ValidateNewPassword(password, confirmPassword, msgs);
            return msgs.Count > 0 ? msgs : null;
        }

        public void DeleteUser(int id)
        {
            _userRepository.Delete(id);
        }

        //public bool IsAdminAccessDenied(string ipAddress)
        //{
        //    return _userRepository.IsAdminAccessDenied(ipAddress);
        //}

        //public void AddAdminAccessDenied(string ipAddress)
        //{
        //    _userRepository.AddAdminAccessDenied(ipAddress);
        //}

        // private methods

        private static List<string> ValidateUserName(string username, List<string> msgs)
        {
            if (username == null)
                msgs.Add("UserName is required.");
            else if (username.Length == 0)
                msgs.Add("UserName is required.");

            return msgs;
        }

        private List<string> ValidateEmail(string email, List<string> msgs)
        {
            if (email == null)
                msgs.Add("Email is required.");
            else
            {
                if (email.Length == 0)
                    msgs.Add("Email is required.");
                else if (!IsValidEmail(email))
                    msgs.Add("Incorrect email format.");
            }
            return msgs;
        }

        private static List<string> ValidateNewPassword(string password, string confirmPassword, List<string> msgs)
        {
            bool validpasswordlength = true;

            if (password == null)
                msgs.Add("Password is required.");
            else
                validpasswordlength = (password.Length >= Constants.MinPassowrdLength);

            if (!validpasswordlength)
                msgs.Add("Password must be at least 4 charactoers long.");

            if (confirmPassword == null)
                msgs.Add("Confirm password is required.");

            if (validpasswordlength && (confirmPassword != null))
            {
                if (password != confirmPassword)
                    msgs.Add("The new password and confirmation password do not match.");
            }

            return msgs;
        }

        private bool UserNameExists(string username)
        {
            User user = _userRepository.GetByUserName(username);
            return (user != null);
        }

        private bool IsValidEmail(string email)
        {
            _invalid = false;

            if (String.IsNullOrEmpty(email))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (_invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(email,
                                     @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                     @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                                     RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

        // Password Encryption

        public static string GeneratePasswordHash(string thisPassword)
        {
            var md5 = new MD5CryptoServiceProvider();

            byte[] tmpSource = Encoding.ASCII.GetBytes(thisPassword);
            byte[] tmpHash = md5.ComputeHash(tmpSource);

            var sOutput = new StringBuilder(tmpHash.Length);
            for (int i = 0; i < tmpHash.Length; i++)
            {
                sOutput.Append(tmpHash[i].ToString("X2")); // X2 formats to hexadecimal
            }
            return sOutput.ToString();
        }

        private static bool VerifyHashPassword(string thisPassword, string thisHash)
        {
            bool isValid = false;
            string tmpHash = GeneratePasswordHash(thisPassword); // Call the routine on user input
            if (tmpHash == thisHash) isValid = true; // Compare to previously generated hash
            return isValid;
        }

        public Boolean IsValidPassword(string thisPassword)
        {
            string newPassword = SanitizeInput(thisPassword);
            var regX = new Regex(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{5,15)");
            return regX.Match(newPassword).Success;
        }

        private static string SanitizeInput(string thisInput)
        {
            var regX = new Regex(@"([<>""'%;()&])");
            return regX.Replace(thisInput, "");
        }
    }
}
