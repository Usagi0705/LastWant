﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using prjCoreWebWantWant.Hubs;
using prjCoreWebWantWant.Models;
using prjCoreWebWantWant.ViewModels;

namespace prjCoreWebWantWant.Controllers
{
    public class ExpertTaskController : Controller
    {
        private readonly NewIspanProjectContext _context;
        private readonly IHubContext<CExpertTask> _hubContext;
        private int _memberID;
        private string _memberName;

        public ExpertTaskController(NewIspanProjectContext context, IHubContext<CExpertTask> hubContext)
        {
            _hubContext = hubContext;
            _context = context;
            
        }

      

        #region 歷史委託
    

        public IActionResult ExpertListnew() //歷史委託
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            CExpertTaskListViewModel vw = new CExpertTaskListViewModel();
            CExperTaskFactory factory = new CExperTaskFactory(_context);
            
            CExpertTaskViewModel cexperttask;
            List<CExpertTaskViewModel> list = new List<CExpertTaskViewModel>();
            //mytasking
            //CaseId:15=專家尚未確認，17=專家案件進行中
            var q1 = _context.TaskLists
     .Include(b => b.ExpertApplications)
     .Where(x => x.IsExpert == true && x.ExpertApplications.Any(y => y.AccountId == _memberID && (y.CaseStatusId == 15 || y.CaseStatusId == 17)))
     .OrderBy(x => x.TaskEndDate)
     .Select(x => new
     {
         TaskList = x,
         ExpertApplications = x.ExpertApplications
                              .Where(ea => ea.AccountId == _memberID && (ea.CaseStatusId == 15 || ea.CaseStatusId == 17))
                              .Select(ea => new { ea.CaseStatusId ,ea.AccountId})
     })
     .ToList();
            foreach (var item in q1)
            {
                cexperttask = new CExpertTaskViewModel();
                var taskList = item.TaskList;
                foreach (var expertApplication in item.ExpertApplications)
                {
                    cexperttask.taskmember = factory.MemberName(expertApplication.AccountId);
                    cexperttask.taskmemberid = expertApplication.AccountId.GetValueOrDefault();

                    cexperttask.CaseStatusname = factory.StatusName(expertApplication.CaseStatusId);
                    // 使用上述的變數進行其他操作
                }
               
                cexperttask.taskexpert = factory.MemberName(taskList.AccountId);
                cexperttask.taskexpertid = taskList.AccountId.GetValueOrDefault();
                cexperttask.caseid = taskList.CaseId;
                cexperttask.taskcontent = taskList.TaskDetail;



                cexperttask.taskdatestart = taskList.TaskStartDate;
                cexperttask.taskdateend = taskList.TaskEndDate;
                cexperttask.taskprice = taskList.PayFrom;
                cexperttask.WorkPlace = (taskList.WorkPlace.GetValueOrDefault()?"在家工作":"指定地點工作");
                if (!taskList.WorkPlace.GetValueOrDefault())
                {
                    
                    cexperttask.Address = taskList.Address;
                }
                else
                {
                    cexperttask.Address = "無";
                }


                list.Add(cexperttask);
            }
            vw.mytasking=list;
            //下面要重改QQQQQQQQ
            //===============================================================
            
            List<CExpertTaskViewModel> list2 = new List<CExpertTaskViewModel>();
            //mytasked
            //CaseId:16=專家拒絕接案，18=專家案件已完成 13=放棄委託//12專家案件已評價
            var q2 = _context.TaskLists
    .Include(b => b.ExpertApplications)
    .Where(x => x.IsExpert == true && x.ExpertApplications.Any(y => y.AccountId == _memberID && (y.CaseStatusId == 16 || y.CaseStatusId == 18 || y.CaseStatusId == 13 || y.CaseStatusId == 12 || y.CaseStatusId == 11)))
    .OrderBy(x => x.TaskEndDate)
    .Select(x => new
    {
        TaskList = x,
        ExpertApplications = x.ExpertApplications
                             .Where(ea => ea.AccountId == _memberID && (ea.CaseStatusId == 16 || ea.CaseStatusId == 18 || ea.CaseStatusId == 13 || ea.CaseStatusId == 12 || ea.CaseStatusId == 11))
                             .Select(ea => new { ea.CaseStatusId, ea.AccountId })
    })
    .ToList();
           
            foreach (var item in q2)
            {
                cexperttask = new CExpertTaskViewModel();
                var taskList = item.TaskList;
                foreach (var expertApplication in item.ExpertApplications)
                {
                    cexperttask.taskmember = factory.MemberName(expertApplication.AccountId);
                    cexperttask.taskmemberid = expertApplication.AccountId.GetValueOrDefault();
                    cexperttask.CaseStatusname = factory.StatusName(expertApplication.CaseStatusId);
                }
                cexperttask.taskexpert = factory.MemberName(taskList.AccountId);
                cexperttask.taskexpertid = taskList.AccountId.GetValueOrDefault();
                cexperttask.caseid = taskList.CaseId;
                cexperttask.taskcontent = taskList.TaskDetail;


                cexperttask.taskdatestart = taskList.TaskStartDate;
                cexperttask.taskdateend = taskList.TaskEndDate;
                cexperttask.taskprice = taskList.PayFrom;
                cexperttask.WorkPlace = (taskList.WorkPlace.GetValueOrDefault() ? "在家工作" : "指定地點工作");
                if (!taskList.WorkPlace.GetValueOrDefault())
                {

                    cexperttask.Address = taskList.Address;
                }
                else
                {
                    cexperttask.Address = "無";
                }

                list2.Add(cexperttask);
            }
            vw.mytasked = list2;
            //===============================================================
            List<CExpertTaskViewModel> list3 = new List<CExpertTaskViewModel>();
            //taskfromotherno
            //CaseId:15=專家尚未確認
            var q3 = _context.TaskLists
     .Include(b => b.ExpertApplications)
     .Where(x => x.IsExpert == true &&x.AccountId== _memberID && x.ExpertApplications.Any(y => y.CaseStatusId == 15 ))
     .OrderBy(x => x.TaskEndDate)
     .Select(x => new
     {
         TaskList = x,
         ExpertApplications = x.ExpertApplications
                              .Where(ea => ea.CaseStatusId == 15)
                              .Select(ea => new { ea.CaseStatusId, ea.AccountId })
     })
     .ToList();
            
            foreach (var item in q3)
            {
                cexperttask = new CExpertTaskViewModel();
                var taskList = item.TaskList;

                foreach (var expertApplication in item.ExpertApplications)
                {
                    cexperttask.taskmember = factory.MemberName(expertApplication.AccountId);
                    cexperttask.taskmemberid = expertApplication.AccountId.GetValueOrDefault();
                    cexperttask.CaseStatusname = factory.StatusName(expertApplication.CaseStatusId);
                }
                cexperttask.taskexpert = factory.MemberName(taskList.AccountId);
                cexperttask.taskexpertid = taskList.AccountId.GetValueOrDefault();
                cexperttask.caseid = taskList.CaseId;
                cexperttask.taskcontent = taskList.TaskDetail;



                cexperttask.taskdatestart = taskList.TaskStartDate;
                cexperttask.taskdateend = taskList.TaskEndDate;
                cexperttask.taskprice = taskList.PayFrom;
                cexperttask.WorkPlace = (taskList.WorkPlace.GetValueOrDefault() ? "在家工作" : "指定地點工作");
                if (!taskList.WorkPlace.GetValueOrDefault())
                {

                    cexperttask.Address = taskList.Address;
                }
                else
                {
                    cexperttask.Address = "無";
                }

                list3.Add(cexperttask);
            }
            vw.taskfromotherno = list3;
            //===============================================================
           
            List<CExpertTaskViewModel> list4 = new List<CExpertTaskViewModel>();
            //taskingfromother
            //CaseId:17=專家案件進行中
           
            var q4 = _context.TaskLists
    .Include(b => b.ExpertApplications)
    .Where(x => x.IsExpert == true && x.AccountId == _memberID && x.ExpertApplications.Any(y => y.CaseStatusId == 17))
    .OrderBy(x => x.TaskEndDate)
    .Select(x => new
    {
        TaskList = x,
        ExpertApplications = x.ExpertApplications
                             .Where(ea => ea.CaseStatusId == 17)
                             .Select(ea => new { ea.CaseStatusId, ea.AccountId })
    })
    .ToList();



            foreach (var item in q4)
            {
                cexperttask = new CExpertTaskViewModel();
                var taskList = item.TaskList;
                foreach (var expertApplication in item.ExpertApplications)
                {
                    cexperttask.taskmember = factory.MemberName(expertApplication.AccountId);
                    cexperttask.taskmemberid = expertApplication.AccountId.GetValueOrDefault();
                    cexperttask.CaseStatusname = factory.StatusName(expertApplication.CaseStatusId);
                }

                cexperttask.taskexpert = factory.MemberName(taskList.AccountId);
                cexperttask.taskexpertid = taskList.AccountId.GetValueOrDefault();
                cexperttask.caseid = taskList.CaseId;
                cexperttask.taskcontent = taskList.TaskDetail;


                cexperttask.taskdatestart = taskList.TaskStartDate;
                cexperttask.taskdateend = taskList.TaskEndDate;
                cexperttask.taskprice = taskList.PayFrom;
                cexperttask.WorkPlace = (taskList.WorkPlace.GetValueOrDefault() ? "在家工作" : "指定地點工作");
                if (!taskList.WorkPlace.GetValueOrDefault())
                {

                    cexperttask.Address = taskList.Address;
                }
                else
                {
                    cexperttask.Address = "無";
                }

                list4.Add(cexperttask);
            }
            vw.taskingfromother = list4;
            //===============================================================
            List<CExpertTaskViewModel> list5 = new List<CExpertTaskViewModel>();
            //taskedfromother
            //CaseId:16=專家拒絕接案，18=專家案件已完成 13=委託人放棄委託//12專家案件已評價//11專家放棄委託


            var q5 = _context.TaskLists
    .Include(b => b.ExpertApplications)
    .Where(x => x.IsExpert == true && x.AccountId == _memberID && x.ExpertApplications.Any(y => y.CaseStatusId == 16|| y.CaseStatusId == 18 || y.CaseStatusId == 13 || y.CaseStatusId == 12||y.CaseStatusId == 11))
    .OrderBy(x => x.TaskEndDate)
    .Select(x => new
    {
        TaskList = x,
        ExpertApplications = x.ExpertApplications
                             .Where(ea => ea.CaseStatusId == 16 || ea.CaseStatusId == 18 || ea.CaseStatusId == 13 || ea.CaseStatusId == 12 || ea.CaseStatusId == 11)
                             .Select(ea => new { ea.CaseStatusId, ea.AccountId })
    })
    .ToList();

            foreach (var item in q5)
            {
                cexperttask = new CExpertTaskViewModel();
                var taskList = item.TaskList;

                foreach (var expertApplication in item.ExpertApplications)
                {
                    cexperttask.taskmember = factory.MemberName(expertApplication.AccountId);
                    cexperttask.taskmemberid = expertApplication.AccountId.GetValueOrDefault();
                    cexperttask.CaseStatusname = factory.StatusName(expertApplication.CaseStatusId);
                }
                cexperttask.taskexpert = factory.MemberName(taskList.AccountId);
                cexperttask.taskexpertid = taskList.AccountId.GetValueOrDefault();
                cexperttask.caseid = taskList.CaseId;
                    cexperttask.taskcontent = taskList.TaskDetail;
               

                cexperttask.taskdatestart = taskList.TaskStartDate;
                cexperttask.taskdateend = taskList.TaskEndDate;
                cexperttask.taskprice = taskList.PayFrom;
                cexperttask.WorkPlace = (taskList.WorkPlace.GetValueOrDefault() ? "在家工作" : "指定地點工作");
                if (!taskList.WorkPlace.GetValueOrDefault())
                {

                    cexperttask.Address = taskList.Address;
                }
                else
                {
                    cexperttask.Address = "無";
                }

                list5.Add(cexperttask);
            }
            vw.taskedfromother = list5;
            ViewBag.UserA = _memberID;
            return View(vw);
            
        }

        #endregion

# region 收到委託
        public async Task<IActionResult> DetailsResponse(int? id) //收到委託
        {
            if (id == null || _context.TaskLists == null)
            {
                //return NotFound();
                return View();            }

            var taskList = await _context.TaskLists
                .Include(t => t.Payment)
                .Include(t => t.PaymentDate)
                .Include(t => t.Salary)
                .Include(t => t.Town)
                .FirstOrDefaultAsync(m => m.CaseId == id);
            if (taskList == null)
            {
                //return NotFound();
                return View();
            }

            return View(taskList);
        }
#endregion

        #region 檢視
        public async Task<IActionResult> DetailsView(int? id)//檢視
        {
            if (id == null || _context.TaskLists == null)
            {
                //return NotFound();
                return View();
            }

            var taskList = await _context.TaskLists
                .Include(t => t.Payment)
                .Include(t => t.PaymentDate)
                .Include(t => t.Salary)
                .Include(t => t.Town)
                .FirstOrDefaultAsync(m => m.CaseId == id);
            if (taskList == null)
            {
                //return NotFound();
                return View();
            }

            return View(taskList);
        }

        #endregion
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

        #region 從履歷連委託單

        // GET: ExpertTask/Create
        public IActionResult Create(int expertaccountid)  //從履歷連委託單
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login","Member");
            }
            CExperTaskFactory factory = new CExperTaskFactory(_context);
           string expertname= factory.MemberName(expertaccountid);
            string accountname = factory.MemberName(_memberID);
            CExpertTaskInsertViewModel vm = new CExpertTaskInsertViewModel();
            vm.委託人 = accountname;
            vm.委託人ID = _memberID;
            vm.被委託人 = expertname;
            vm.被委託人ID = expertaccountid;
            ViewBag.expertid = expertaccountid;
            ViewBag.caseid = expertaccountid;
            return View(vm);
        }

        // POST: ExpertTask/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]  //從履歷連委託單
        public async Task<IActionResult> Create(CExpertTaskInsertViewModel vm)
        {
            CExperTaskFactory factory = new CExperTaskFactory(_context);
            if (ModelState.IsValid)
            {
                TaskList tasklist = new TaskList();
                tasklist.AccountId = factory.MemberID(vm.被委託人);
                tasklist.TaskTitle = vm.委託人 + "跟專家" + vm.被委託人 + "的委託案件";
                tasklist.TaskDetail = vm.委託內容;
                tasklist.PayFrom = vm.委託價格;
                tasklist.PayTo = vm.委託價格;
                if(vm.委託工作地點 == "在家工作")
                {
                    tasklist.WorkPlace = true;
                    tasklist.Address = "無";
                }
                else if (vm.委託工作地點 == "指定地點工作")
                {
                    tasklist.WorkPlace =false;
                    tasklist.Address = vm.指定委託地點;
                }
                    tasklist.TaskStartDate = vm.委託時間起;
                    tasklist.TaskEndDate = vm.委託時間訖;
               
                DateTime date= DateTime.Now;
                tasklist.DataCreateDate = date.ToString();
                //tasklist.CaseStatusId = 15;//
                tasklist.IsExpert = true;
                _context.TaskLists.Add(tasklist);
                await _context.SaveChangesAsync();
                int taskid = tasklist.CaseId;

                ExpertApplication ea=new ExpertApplication();
                ea.CaseId = taskid;
                ea.AccountId = factory.MemberID(vm.委託人);
                ea.CaseStatusId = 15;  //專家尚未確認
                ea.ExpertAccountId= factory.MemberID(vm.被委託人);
                _context.ExpertApplications.Add(ea);
                await _context.SaveChangesAsync();


                TempData["message1"] = "委託已送出!請靜候專家回覆，或使用聊天室通知專家。";
               
                return RedirectToAction("ExpertMainPage", "Expert");
            }
            var errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();

            foreach(var item in errors)
            {
                vm.委託內容 += item;
            }
          
            return View(vm);
        }

        #endregion
       

        [HttpPost]
        public IActionResult UpdateExpertTask([FromBody] CExpertTaskeViewModelHub model)
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            try
            {
                var existingTask = _context.TaskLists.Find(model.caseid);

                // 如果該筆資料不存在，則返回一個錯誤訊息
                if (existingTask == null)
                {
                    return NotFound(new { Status = "Failed", Message = "找不到相關的任務" });
                }

                existingTask.TaskDetail = model.taskcontent;
                existingTask.TaskStartDate = model.taskdatestart;
                existingTask.TaskEndDate = model.taskdateend;
                existingTask.PayFrom = model.taskprice;
                existingTask.PayTo = model.taskprice;

                if (model.WorkPlace == "home")
                {
                    existingTask.WorkPlace = true;
                    existingTask.Address = model.Address;
                }
                else
                {
                    existingTask.WorkPlace = false;
                    existingTask.Address = model.Address;
                }
                //_context.Update(existingTask);
                _context.SaveChanges();
                string targetConnectionId = ConnectionManager.GetConnectionId(model.UserB);
                string myConnectionId = ConnectionManager.GetConnectionId(_memberID.ToString());
                if (!string.IsNullOrEmpty(targetConnectionId))
                {
                    _hubContext.Clients.Client(targetConnectionId).SendAsync("ReceiveUpdate", "taskfromotherno", "1");
                }
                Console.WriteLine("Start of UpdateExpertTask at " + DateTime.Now.ToString());
                if (!string.IsNullOrEmpty(myConnectionId))
                {
                    Console.WriteLine("Before SignalR message send at " + DateTime.Now.ToString());
                    _hubContext.Clients.Client(myConnectionId).SendAsync("ReceiveUpdateme", "mytasking", "1");
                    Console.WriteLine("After SignalR message send at " + DateTime.Now.ToString());
                }
                Console.WriteLine("End of UpdateExpertTask at " + DateTime.Now.ToString());
                return Ok(new { message = "修改成功" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = $"更新失敗，原因：{ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult AbandonExpertTask([FromBody] CExpertTaskeViewModelHub model)
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            try
            {
                var existingEA = _context.ExpertApplications.FirstOrDefault(x=>x.CaseId== model.caseid);

                // 如果該筆資料不存在，則返回一個錯誤訊息
                if (existingEA == null)
                {
                    return NotFound(new { Status = "Failed", Message = "找不到相關的任務" });
                }

                ChangeCaseStatusId(existingEA, 13);
                string targetConnectionId = ConnectionManager.GetConnectionId(model.UserB);
                string myConnectionId = ConnectionManager.GetConnectionId(_memberID.ToString());
               

                return Ok(new { message = "修改成功" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = $"更新失敗，原因：{ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult OKExpertTask([FromBody] CExpertTaskeViewModelHub model)
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            try
            {
                var existingEA = _context.ExpertApplications.FirstOrDefault(x => x.CaseId == model.caseid);

                // 如果該筆資料不存在，則返回一個錯誤訊息
                if (existingEA == null)
                {
                    return NotFound(new { Status = "Failed", Message = "找不到相關的任務" });
                }

                ChangeCaseStatusId(existingEA, 17);

                string targetConnectionId = ConnectionManager.GetConnectionId(model.UserB);
                string myConnectionId = ConnectionManager.GetConnectionId(_memberID.ToString());
                if (!string.IsNullOrEmpty(targetConnectionId))
                {
                    _hubContext.Clients.Client(targetConnectionId).SendAsync("ReceiveUpdate", "mytasking", "1");
                }
                if (!string.IsNullOrEmpty(myConnectionId))
                {
                    _hubContext.Clients.Client(myConnectionId).SendAsync("ReceiveUpdateme", "taskingfromother", "1");
                }

                return Ok(new { message = "修改成功" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = $"更新失敗，原因：{ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult NoExpertTask([FromBody] CExpertTaskeViewModelHub model)
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            try
            {
                var existingEA = _context.ExpertApplications.FirstOrDefault(x => x.CaseId == model.caseid);

                // 如果該筆資料不存在，則返回一個錯誤訊息
                if (existingEA == null)
                {
                    return NotFound(new { Status = "Failed", Message = "找不到相關的任務" });
                }
                ChangeCaseStatusId(existingEA, 16);
                string targetConnectionId = ConnectionManager.GetConnectionId(model.UserB);
                string myConnectionId = ConnectionManager.GetConnectionId(_memberID.ToString());
                if (!string.IsNullOrEmpty(targetConnectionId))
                {
                    _hubContext.Clients.Client(targetConnectionId).SendAsync("ReceiveUpdate", "mytasked", "1");
                }
                if (!string.IsNullOrEmpty(myConnectionId))
                {
                    _hubContext.Clients.Client(myConnectionId).SendAsync("ReceiveUpdateme", "taskedfromother", "1");
                }
                return Ok(new { message = "修改成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = $"更新失敗，原因：{ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult DoneExpertTask([FromBody] CExpertTaskeViewModelHub model)
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            try
            {
                var existingEA = _context.ExpertApplications.FirstOrDefault(x => x.CaseId == model.caseid);

                // 如果該筆資料不存在，則返回一個錯誤訊息
                if (existingEA == null)
                {
                    return NotFound(new { Status = "Failed", Message = "找不到相關的任務" });
                }
                ChangeCaseStatusId(existingEA, 18);
                string targetConnectionId = ConnectionManager.GetConnectionId(model.UserB);
                string myConnectionId = ConnectionManager.GetConnectionId(_memberID.ToString());
                if (!string.IsNullOrEmpty(targetConnectionId))
                {
                    _hubContext.Clients.Client(targetConnectionId).SendAsync("ReceiveUpdate", "mytasked", "1");
                }
                if (!string.IsNullOrEmpty(myConnectionId))
                {
                    _hubContext.Clients.Client(myConnectionId).SendAsync("ReceiveUpdateme", "taskedfromother", "1");
                }
                return Ok(new { message = "修改成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = $"更新失敗，原因：{ex.Message}" });
            }
        }
        private void ChangeCaseStatusId(ExpertApplication existing, int statusId)
        {
            existing.CaseStatusId = statusId;
            _context.SaveChanges();

        }
        //--------------------------------------------測試00700
        public IActionResult Listnew() //歷史委託
        {
            _memberID = GetMemberIDFromSession();//登入者我自己memberID
            if (_memberID == 0)
            {
                TempData["message"] = "請先登入";
                return RedirectToAction("Login", "Member");
            }
            ViewBag.UserA = _memberID;
            return View();

        }

        //-------------------------------------------------
        private bool TaskListExists(int id)
        {
          return (_context.TaskLists?.Any(e => e.CaseId == id)).GetValueOrDefault();
        }
    }
}
