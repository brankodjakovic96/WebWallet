using Applications.WebApp.Models;
using Applications.WebApp.Models.WalletInfo;
using Common.Utils;
using Core.ApplicationServices;
using Core.ApplicationServices.DTOs;
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

        [HttpGet]
        public IActionResult WalletInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WalletInfo(WalletInfoRequestVM walletInfoRequestVM)
        {
            try
            {
                WalletInfoDTO walletInfoDTO = await WalletService.GetWalletInfo(walletInfoRequestVM.Jmbg, walletInfoRequestVM.Password);
                WalletInfoResponseVM walletInfoResponseVM =
                    new WalletInfoResponseVM()
                    {
                        Jmbg = walletInfoDTO.Jmbg,
                        FirstName = walletInfoDTO.FirstName,
                        LastName = walletInfoDTO.LastName,
                        BankType = EnumMapper.MapBankType(walletInfoDTO.BankType),
                        BankAccount = walletInfoDTO.BankAccount,
                        Balance = walletInfoDTO.Balance,
                        UsedDepositThisMonth = walletInfoDTO.UsedDepositThisMonth,
                        MaximalDeposit = walletInfoDTO.MaximalDeposit,
                        UsedWithdrawThisMonth = walletInfoDTO.UsedWithdrawThisMonth,
                        MaximalWithdraw = walletInfoDTO.MaximalWithdraw,
                        TransactionVMs = walletInfoDTO.TransactionDTOs.Select(
                                transaction => new TransactionResposneVM() {
                                    Inflow = (EnumMapper.MapTransactionTypeFlow(transaction.Type) == "Inflow" ? transaction.Amount : 0M),
                                    Outflow = (EnumMapper.MapTransactionTypeFlow(transaction.Type) == "Outflow" ? transaction.Amount : 0M),
                                    Destination = transaction.Destination,
                                    Source = transaction.Source,
                                    TransactionDateTime = transaction.TransactionDateTime,
                                    Type = EnumMapper.MapTransactionType(transaction.Type),
                                    WalletBalance = transaction.WalletBalance
                                }
                            ).OrderByDescending(t => t.TransactionDateTime).ToList()
                    };
                ModelState.Clear();
                return View(new WalletInfoPageVM(){ WalletInfoResponseVM = walletInfoResponseVM });
            }
            catch (Exception ex)
            {
                ViewData["IsSuccessful"] = "no";
                ViewData["ErrorMessage"] = ex.Message;

                return View();
            }
        }

    }
}
