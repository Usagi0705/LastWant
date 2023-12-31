﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjCoreWebWantWant.Models;
using prjCoreWebWantWant.ViewModels;
using System.Text.Json;

namespace prjCoreWebWantWant.Controllers
{
    public class RatingsAPIController : Controller
    {
        private readonly NewIspanProjectContext _context;
        private List<CRatings> _ratings;
        private int _memberID;
        public RatingsAPIController(NewIspanProjectContext context)
        {
            _context = context;
            _ratings = new List<CRatings>();
           // _memberID = 17;//登入者我自己memberID
        }

        private int GetMemberIDFromSession()
        {
            if (HttpContext.Session.Keys.Contains(CDictionary.SK_LOGINED_USER))
            {
                string userDataJson = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
                MemberAccount loggedInUser = JsonSerializer.Deserialize<MemberAccount>(userDataJson);
                return loggedInUser.AccountId;
            }
            return 0;
        }

        public async Task<IActionResult> ForOtherdata()
        {

            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }

            List<CRatings> ratingsForOther = new List<CRatings>();
          

            if (_context.Ratings != null)
            {
                //給別人的評價-我是委託人
                var ratingdata = await _context.Ratings
               .Where(x => x.TargetAccountId == _memberID)
               .ToListAsync();
         
                CExperTaskFactory factory = new CExperTaskFactory(_context);
                CRatings datarating;
                foreach (var item in ratingdata)
                {
                    datarating = new CRatings();
                    datarating.ratedperson = factory.MemberName(item.TargetAccountId);
                    datarating.ratingforperson = factory.MemberName(item.SourceAccountId);
                    datarating.ratingstar = item.RatingStar;
                    datarating.ratingcontent = item.RatingContent;
                    datarating.ratingdate = item.RatingDate;
                    ratingsForOther.Add(datarating);
                };
        
                var data = ratingsForOther;
                return Json(data);

            }
            else
            {
                return Problem("Entity set 'NewIspanProjectContext.Ratings'  is null.");
            }


        }
        

        public async Task<IActionResult> MyRatingsdata()
        {

            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }

            List<CRatings> MyRatings = new List<CRatings>();
           

            if (_context.Ratings != null)
            {

                //自己收到評論
                var ratingdatamy = await _context.ExpertApplications
               .Where(x => x.ExpertAccountId == _memberID && x.Rating != null)
               .Select(y => y.Rating)
               .ToListAsync();
             

                CExperTaskFactory factory = new CExperTaskFactory(_context);
                CRatings datamy;
                foreach (var item in ratingdatamy)
                {
                    datamy = new CRatings();
                    datamy.ratedperson = factory.MemberName(item.TargetAccountId); 

                    datamy.ratingforperson = factory.MemberName(item.SourceAccountId);
                    datamy.ratingstar = item.RatingStar;
                    datamy.ratingcontent = item.RatingContent;
                    datamy.ratingdate = item.RatingDate;
                    MyRatings.Add(datamy);
                };
               

                var data = MyRatings;

                return Json(data);

            }
            else
            {
                return Problem("Entity set 'NewIspanProjectContext.Ratings'  is null.");
            }


        }










    }
}
