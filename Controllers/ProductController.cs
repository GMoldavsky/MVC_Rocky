using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Rocky_DataAccess;
using MVC_Rocky_Models;
using MVC_Rocky_Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MVC_Rocky_Utility;
using MVC_Rocky_DataAccess.Repository.IRepository;

namespace MVC_Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)] //To allow only Admin use link/access in the menue.User see "Access denied" when he click on the link
    public class ProductController : Controller
    {
        //private readonly ApplicationDbContext _db;
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductRepository prodRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            //    //  This is NOT "Eager Loading", too slow
            //IEnumerable<Product> objList1 = _prodRepo.Product;
            //   //To get/load Category
            //foreach (var obj in objList1)
            //{
            //    obj.Category = _prodRepo.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            //    obj.ApplicationType = _prodRepo.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //};

            // Eager loading increase efficiency
            //IEnumerable<Product> objList1 = _prodRepo.Product.Include(u => u.Category).Include(u => u.ApplicationType);
            IEnumerable<Product> objList1 = _prodRepo.GetAll(includeProperties:"Category,ApplicationType");

            return View(objList1);
        }

        //GET - UpSert
        public IActionResult Upsert(int? id)
        {
            //  // for SelectListItem add "using Microsoft.AspNetCore.Mvc.Rendering;"
            //IEnumerable<SelectListItem> CategoryDropDown = _prodRepo.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _prodRepo.GetAllDropdownList(WC.CategoryName),
                ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName)
            };

            if (id == null)
            {
                //This is new Product
                return View(productVM);
            }
            else
            {
                //This to Update/Edit Product
                productVM.Product = _prodRepo.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
            
        }

        //POST - UpSert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if(productVM.Product.Id == 0)
                {
                    //Creating new
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    using(var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;
                    _prodRepo.Add(productVM.Product);
                }
                else
                {
                    //Updating
                    var objFromDb = _prodRepo.FirstOrDefault(u => u.Id == productVM.Product.Id, isTracking:false);
                    if(files.Count > 0)//It is meen that new file uploaded
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //Delete Old image
                        var oldFile = Path.Combine(upload, objFromDb.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        //Load new Image
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _prodRepo.Update(productVM.Product);
                }

                //_prodRepo.Product.Add(productVM.Product);
                _prodRepo.Save();
                return RedirectToAction("Index");
            }

            //Not valid data
            //Recreate select list
            //productVM.CategorySelectList = _prodRepo.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            //productVM.ApplicationTypeSelectList = _prodRepo.ApplicationType.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});

            productVM.CategorySelectList = _prodRepo.GetAllDropdownList(WC.CategoryName);
            productVM.ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName);
            return View(productVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //Eager loading (Load also join Category. Also loads related entities as part of the query)
            //Product product = _prodRepo.Product.Include(u=>u.Category).FirstOrDefault(u=>u.Id==id);
            //Product product = _prodRepo.Product.Include(u => u.Category).Include(u => u.ApplicationType).FirstOrDefault(u => u.Id == id);
            Product product = _prodRepo.FirstOrDefault(u=>u.Id==id,includeProperties: "Category,ApplicationType");

            // Or use this:
            //Product product = _prodRepo.Product.Find(id);
            //product.Category = _prodRepo.Category.Find(product.CategoryId);
            //product.ApplicationType = _prodRepo.ApplicationType.Find(product.ApplicationTypeId);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _prodRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            //Delete Old image
            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);
            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _prodRepo.Remove(obj);
            _prodRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
