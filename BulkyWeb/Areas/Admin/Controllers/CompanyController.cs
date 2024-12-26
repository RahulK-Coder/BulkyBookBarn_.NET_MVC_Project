using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;

using Microsoft.AspNetCore.Authorization;

//using BulkyBook.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _unitofwork;
        public CompanyController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList(); //select * from Companys and return it
            
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id)
        {
            
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                //update
                Company companyObj = _unitofwork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }

            /*return View(CompanyVM);*/
        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            
            if (ModelState.IsValid)
            {
                if(CompanyObj.Id == 0)
                {
                    _unitofwork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitofwork.Company.Update(CompanyObj);
                }


                _unitofwork.Save();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }


        }



        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList }) ;
        }


        [HttpDelete]
        public IActionResult Delete(int ? id)
        {
            var CompanyToBeDeleted = _unitofwork.Company.Get(u => u.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }

            _unitofwork.Company.Remove(CompanyToBeDeleted);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });

            
        }
        #endregion
    }
}