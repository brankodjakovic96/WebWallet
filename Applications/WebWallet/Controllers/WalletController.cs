using Applications.WebWallet.Models;
using Core.ApplicationServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebWallet.Controllers
{
    public class WalletController : Controller
    {
        private readonly WalletService WalletService;

        public WalletController(WalletService walletService)
        {
            WalletService = walletService;
        }

        [HttpGet]
        public IActionResult Create(string password)
        {
            ViewData["password"] = password;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(WalletVM walletVM)
        {
            string password;
            try
            {
                password = await WalletService.CreateWallet(walletVM.FirstName, walletVM.LastName, walletVM.Jmbg, walletVM.BankType, walletVM.PIN, walletVM.BankAccount);
                return RedirectToAction("Create", "Wallet", new { password = password });
            }
            catch(Exception ex)
            {
                return RedirectToAction("Error", "Home", new { Message = ex.Message });
            }
        }
    }
}
