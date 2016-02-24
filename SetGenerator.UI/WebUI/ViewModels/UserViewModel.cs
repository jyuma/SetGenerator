using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SetGenerator.WebUI.ViewModels
{
    public class UserDetail
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
        public string IPAddress { get; set; }
        public string BrowserInfo { get; set; }
        public bool IsDisabled { get; set; }
        public int DefaultBandId { get; set; }
    }

    public class UserViewModel
    {
        public int SelectedId { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class UserEditViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsDisabled { get; set; }
        public SelectList UserBands { get; set; }
    }

    public class UserBandEditViewModel
    {
        public SelectList AssignedBands { get; set; }
        public SelectList AvailableBands { get; set; }
    }

    public class UserBandDetail
    {
        public int UserId { get; set; }
        public int[] BandIds { get; set; }
    }
}