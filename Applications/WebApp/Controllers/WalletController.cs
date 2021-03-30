using Applications.WebApp.Models;
using Core.ApplicationServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Controllers
{
    public class WalletController : Controller
    {
        private readonly WalletService WalletService;

        public WalletController(WalletService walletService)
        {
            WalletService = walletService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(WalletVM walletVM)
        {
            string password;
            try
            {
                password = await WalletService.CreateWallet(walletVM.FirstName, walletVM.LastName, walletVM.Jmbg, walletVM.BankType, walletVM.PIN, walletVM.BankAccount);
                ModelState.Clear();
                return View(new WalletVM() { Password = password });
            }
            catch(Exception ex)
            {
                ViewData["IsSuccessful"] = "no";
                ViewData["ErrorMessage"] = ex.Message;

                return View();
            }
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(WalletDepositVM walletDepositVM)
        {
            try
            {
                await WalletService.Deposit(walletDepositVM.Jmbg, walletDepositVM.Password, walletDepositVM.Amount);
                ModelState.Clear();
                ViewData["IsSuccessful"] = "yes";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["IsSuccessful"] = "no";
                ViewData["ErrorMessage"] = ex.Message;

                return View();
            }
        }

        [HttpGet]
        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(WalletWithdrawVM walletWithdrawVM)
        {
            try
            {
                await WalletService.Withdraw(walletWithdrawVM.Jmbg, walletWithdrawVM.Password, walletWithdrawVM.Amount);
                ModelState.Clear();
                ViewData["IsSuccessful"] = "yes";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["IsSuccessful"] = "no";
                ViewData["ErrorMessage"] = ex.Message;

                return View();
            }
        }

        [HttpGet]
        public IActionResult Transfer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(WalletTransferVM walletTransferVM)
        {
            try
            {
                await WalletService.Transfer(walletTransferVM.JmbgFrom , walletTransferVM.PasswordFrom, walletTransferVM.JmbgTo, walletTransferVM.Amount);
                ModelState.Clear();
                ViewData["IsSuccessful"] = "yes";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["IsSuccessful"] = "no";
                ViewData["ErrorMessage"] = ex.Message;

                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateFee([FromBody] CalculateFeeVM calculateFeeVM)
        {
            try
            {
                decimal fee = await WalletService.CalculateTransferFee(calculateFeeVM.Jmbg, calculateFeeVM.Password, calculateFeeVM.Amount);
                return Ok(fee);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorMessage = ex.Message });
            }
        }

    }
}
