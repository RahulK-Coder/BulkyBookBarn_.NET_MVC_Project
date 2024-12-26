using BulkyBook.DataAccess.Repository.IRepository;

using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;


//using BulkyBook.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize (Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {

        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitofwork.Category.GetAll().ToList(); //select * from categories and return it
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //if(obj.Name == obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            //}
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                TempData["success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDB = _unitofwork.Category.Get(u => u.Id == id);  //Shoould be used if its a primary key value- mandatory
            //Category? categoryFromDB1 = _db.Categories.FirstOrDefault(u => u.Id==id); //Can be used if its is not primary key value
            //Category? categoryFromDB2 = _db.Categories.Where(u => u.Id==id).FirstOrDefault(); //Used for complex queries
            if (categoryFromDB == null)
            {
                return NotFound();
            }
            return View(categoryFromDB);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Category Edited Successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDB = _unitofwork.Category.Get(u => u.Id == id);  //Shoould be used if its a primary key value- mandatory

            if (categoryFromDB == null)
            {
                return NotFound();
            }
            return View(categoryFromDB);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitofwork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitofwork.Category.Remove(obj);
            _unitofwork.Save();
            TempData["success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");

        }
    }
}