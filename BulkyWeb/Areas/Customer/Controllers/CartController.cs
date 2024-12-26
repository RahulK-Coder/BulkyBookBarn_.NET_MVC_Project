using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Formats.Tar;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        [BindProperty] //this attribute is used where when placeorder button this will bind it to the shoppingcartvm
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product"),
                OrderHeader = new() //We added this to not get null exception
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product"),
                OrderHeader = new() //We added this to not get null exception
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId, includeProperties: "Product");
			
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			
            ApplicationUser applicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userId); //We changed this because we are adding it the entity in unitofwork.orderheader below

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            if(applicationUser.CompanyId.GetValueOrDefault() == 0) //using getvalueordefault because the companyid value can be NULL, so to eradicate we used this method
            {
                //Its regular customer account and we need to capture the payment
                ShoppingCartVM.OrderHeader.PaymentStatus=SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus=SD.StatusPending;
            }
            else
            {
                //its a company account or user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save(); //Till here the orderheader has been updated

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitofwork.OrderDetail.Add(orderDetail);
                _unitofwork.Save();
            }

			if (applicationUser.CompanyId.GetValueOrDefault() == 0) // means company id is 0// it is as an individual contributaot
			{
                //Its regular customer account and we need to capture the payment
                //stripe logic
                var domain = "https://localhost:7048/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain+"customer/cart/index",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
	                //removed the default method
					Mode = "payment",
				};

                foreach(var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //20.50 -> 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
				var service = new Stripe.Checkout.SessionService();
				Session session = service.Create(options);
                _unitofwork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitofwork.Save();
                Response.Headers.Add("Location", session.Url); // we are redirecting to stripe URL here
                return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation), new {id = ShoppingCartVM.OrderHeader.Id});
		}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser"); //first we are retrieving the orderheader information
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is a customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid") //we are writing 'paid' because it means payment is successfull based on the card details if not its 'unpaid' 
                {
                    _unitofwork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }
            }

            List<ShoppingCart> shoppingCarts = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserID == orderHeader.ApplicationUserId).ToList();

            _unitofwork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitofwork.Save();

            return View(id);
        }

		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitofwork.ShoppingCart.Update(cartFromDb);
            _unitofwork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _unitofwork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitofwork.ShoppingCart.Update(cartFromDb);
            }

            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult remove(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
            _unitofwork.ShoppingCart.Remove(cartFromDb);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
