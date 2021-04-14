using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_Rocky_DataAccess;
using MVC_Rocky_DataAccess.Repository.IRepository;
using MVC_Rocky_Models;
using MVC_Rocky_Models.ViewModels;
using MVC_Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        
        [BindProperty]
        public InquiryVM InquiryVM { get; set;}
        public InquiryController(IInquiryDetailRepository inqDRepo, IInquiryHeaderRepository inqHRepo)
        {
            _inqDRepo = inqDRepo;
            _inqHRepo = inqHRepo;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inqHRepo.GetAll() });
        }
        #endregion

        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == id),
                InquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Product")

            };
            return View(InquiryVM);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Details(InquiryVM inqVM)// We have Id in the hidden field in the Details.cshtml
        //{
        //    //return Content(inqVM.InquiryHeader.Id.ToString());
        //    //return Content(InquiryVM.InquiryHeader.Id.ToString());

        //    List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        //    inqVM.InquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == inqVM.InquiryHeader.Id);
        //    //Add to Shoping cart
        //    foreach (var detail in inqVM.InquiryDetail)
        //    {
        //        ShoppingCart shoppingCart = new ShoppingCart()
        //        {
        //            ProductId = detail.ProductId
        //        };
        //        shoppingCartList.Add(shoppingCart);
        //    }
        //    HttpContext.Session.Clear();
        //    HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        //    HttpContext.Session.Set(WC.SessionInquiryId, inqVM.InquiryHeader.Id);

        //    return RedirectToAction("Index", "Cart"); //CartController > Index action
        //    //return Content(inqVM.InquiryHeader.Id.ToString());
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()//We have Id in the hidden field in the Details.cshtml
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            InquiryVM.InquiryDetail = _inqDRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);
            //Add to Shoping cart
            foreach (var detail in InquiryVM.InquiryDetail)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = detail.ProductId
                };
                shoppingCartList.Add(shoppingCart);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            HttpContext.Session.Set(WC.SessionInquiryId, InquiryVM.InquiryHeader.Id);

            return RedirectToAction("Index", "Cart"); //CartController > Index action
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetails = _inqDRepo.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);
            _inqDRepo.RemoveRange(inquiryDetails);
            _inqHRepo.Remove(inquiryHeader);
            _inqHRepo.Save();// or _inqDRepo.Save();

            return RedirectToAction(nameof(Index));
        }


        


    }
}
