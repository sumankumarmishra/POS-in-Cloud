using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSWebApplication.Data;
using POSWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using POSinCloud.Services;

namespace POSWebApplication.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SaleController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment webHostEnvironment, IMemoryCache cache)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            else
            {
                _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
                _webHostEnvironment = webHostEnvironment;
                _cache = cache;
            }
        }


        #region // Sale methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var stockCategories = await _dbContext.ms_stockcategory.ToListAsync();

            var billNo = GenerateAutoBillNo();

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var UPOS = _dbContext.ms_user
                .Join(_dbContext.ms_userpos,
                    user => user.UserId,
                    userPOS => userPOS.UserId,
                    (user, userPOS) => new
                    {
                        user.UserCde,
                        POSId = userPOS.POSid
                    })
                .FirstOrDefault(u => u.UserCde == userCde);

            var stocks = RestartStocks() ?? new List<Stock>();

            var autoNumber = _dbContext.ms_autonumber.FirstOrDefault(pos => pos.PosId == UPOS.POSId);

            if (autoNumber != null)
            {
                autoNumber.BizDteString = ChangeDateFormat(autoNumber.BizDte);
            }

            var currencyList = await _dbContext.ms_currency.ToListAsync();

            var saleList = new SaleModelList()
            {
                Stocks = stocks,
                StockCategories = stockCategories,
                BillNo = billNo,
                AutoNumber = autoNumber ?? new AutoNumber(),
                CurrencyList = currencyList
            };

            ViewBag.TotalSaleAmtPrDay = GetSaleLimitPrDay() - GetTotalSaleAmount(); // Calculate to control Print method

            return View(saleList);
        }

        public string GenerateAutoBillNo()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(userCde))
                return "";

            var UPOS = _dbContext.ms_user
                .Join(_dbContext.ms_userpos,
                    user => user.UserId,
                    userPOS => userPOS.UserId,
                    (user, userPOS) => new
                    {
                        user.UserCde,
                        POSId = userPOS.POSid
                    })
                .FirstOrDefault(u => u.UserCde == userCde);

            if (UPOS == null)
                return "";

            var autoNumber = _dbContext.ms_autonumber.FirstOrDefault(pos => pos.PosId == UPOS.POSId);
            if (autoNumber == null)
                return "";

            // Main method of this function which generates number
            var generateNo = (autoNumber.LastUsedNo + 1).ToString();
            if (autoNumber.ZeroLeading)
            {
                var totalWidth = autoNumber.RunningNo - autoNumber.BillPrefix.Length - generateNo.Length;
                string paddedString = new string('0', totalWidth) + generateNo;
                return autoNumber.BillPrefix + paddedString;
            }
            else
            {
                return autoNumber.BillPrefix + generateNo;
            }
        }

        public async Task<List<StockUOM>> UOMList(string itemId)
        {
            return await _dbContext.ms_stockuom
                .Where(uom => uom.ItemId == itemId)
                .ToListAsync();
        }

        #endregion


        #region // Stock methods //

        public IActionResult AllStockItems()
        {
            var memoryStocks = GetAllStocks();

            return PartialView("_StockItems", memoryStocks);
        }

        public string GetItemIdByBarcode(string barcode) // Add with barcode
        {
            var memoryStocks = GetAllStocks();

            var itemId = memoryStocks
                .Where(stk => stk.Barcode == barcode)
                .Select(stk => stk.ItemId)
                .FirstOrDefault();

            return itemId ?? "";
        }

        public int GetPkgHIdByBarcode(string barcode)
        {
            var pkgItem = _dbContext.ms_stockpkgh
                .Where(stk => stk.Barcode == barcode)
                .Select(stk => stk.PkgHId)
                .FirstOrDefault();

            return pkgItem;
        }

        public async Task<IActionResult> SearchItems(string keyword)
        {

            var memoryStocks = GetAllStocks();

            var stocks = memoryStocks.Where(stock => stock.ItemDesc.ToLower().Contains(keyword == null ? "" : keyword.ToLower())).ToList();

            var pkgs = await _dbContext.ms_stockpkgh.Where(pkg => pkg.PkgNme.Contains(keyword)).ToListAsync();

            if (pkgs != null)
            {
                foreach (var pkg in pkgs)
                {
                    var stockPkg = new Stock()
                    {
                        ItemId = pkg.PkgNme,
                        ItemDesc = pkg.PkgNme,
                        SellingPrice = pkg.SellingPrice,
                        PkgHId = pkg.PkgHId,
                        Image = pkg.Image
                    };
                    stocks.Add(stockPkg);
                }
            }

            /*if (keyword == "" || keyword == string.Empty || keyword == null)
            {
                await AllStockItems();
            }*/

            foreach (var stock in stocks)
            {
                stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
            }
            return PartialView("_StockItems", stocks);
        }

        public async Task<IActionResult> PackageStockItems()
        {
            var stockPackages = await _dbContext.ms_stockpkgh.ToListAsync();
            foreach (var stock in stockPackages)
            {
                stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
            }
            return PartialView("_StockPackageItems", stockPackages);
        }

        public IActionResult AssemblyStockItems()
        {
            var memoryStocks = GetAllStocks();

            var stocks = memoryStocks
                .Where(stk => stk.FinishGoodFlg == true)
                .ToList();
            foreach (var stock in stocks)
            {
                stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
            }
            return PartialView("_StockItems", stocks);
        }

        public IActionResult StockItems(string catgId)
        {
            var memoryStocks = GetAllStocks();

            var stocks = memoryStocks.Where(stock => stock.CatgCde == catgId).ToList();

            foreach (var stock in stocks)
            {
                stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
            }
            return PartialView("_StockItems", stocks);
        }

        public List<StockPkgD> GetItemsList(int pkgHId)
        {
            var list = _dbContext.ms_stockpkgd.Where(d => d.PkgHId == pkgHId).ToList();

            return list;
        }

        public Stock AddStock(string itemId)
        {
            var memoryStocks = GetAllStocks();

            var stock = memoryStocks.FirstOrDefault(u => u.ItemId == itemId);
            if (stock != null)
            {
                stock.Quantity = 1;
                return stock;
            }
            return new Stock();
        } // Main add method

        #endregion


        #region // Stock cache method //

        public List<Stock> GetAllStocks()
        {
            var dbName = HttpContext.Session.GetString("Database");
            if (_cache.TryGetValue(dbName + "_StockList", out List<Stock>? stockList))
            {
                return stockList ?? new List<Stock>();
            }
            else
            {
                var stocks = _dbContext.ms_stock.ToList();
                var pkgs = _dbContext.ms_stockpkgh.ToList();

                if (pkgs != null)
                {
                    foreach (var pkg in pkgs)
                    {
                        var stockPkg = new Stock()
                        {
                            ItemId = pkg.PkgNme,
                            ItemDesc = pkg.PkgNme,
                            SellingPrice = pkg.SellingPrice,
                            PkgHId = pkg.PkgHId,
                            Image = pkg.Image
                        };
                        stocks.Add(stockPkg);
                    }
                }

                foreach (var stock in stocks)
                {
                    stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
                }
                _cache.Set(dbName + "_StockList", stocks, TimeSpan.FromMinutes(30));

                return stocks;
            }
        }

        public List<Stock>? RestartStocks()
        {
            var dbName = HttpContext.Session.GetString("Database");
            if (_cache.TryGetValue(dbName + "StockList", out List<Stock>? stockList))
            {
                _cache.Remove(dbName + "_StockList");
            }

            var stocks = GetAllStocks();

            return stocks;
        }

        #endregion


        #region // Bill Save Btn methods //

        public async Task<String> SaveBillToBillH(string billNo, decimal discAmt, int custId, string custNme, string[][] tableData)
        {
            try
            {
                var dbBillH = await _dbContext.billh
                    .FirstOrDefaultAsync(bill => bill.BillNo == billNo); // Check bill No is new or already there

                if (dbBillH != null) //  Update billH
                {
                    dbBillH.RevDteTime = DateTime.Now;
                    dbBillH.BillDiscount = discAmt;
                    dbBillH.GuestId = custId;
                    dbBillH.GuestNme = custId == 0 ? "" : custNme;
                    dbBillH.ChrgAccCde = GetCustomerAccCde(custId);
                    _dbContext.Update(dbBillH);
                    await _dbContext.SaveChangesAsync();
                    UpdateBillToBillD(dbBillH.BillhId, tableData);
                }
                else // Add new billH
                {
                    var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

                    var UPOS = await _dbContext.ms_user
                        .Join(_dbContext.ms_userpos,
                            user => user.UserId,
                            userPOS => userPOS.UserId,
                            (user, userPOS) => new
                            {
                                user.UserId,
                                user.UserCde,
                                POSId = userPOS.POSid
                            })
                        .FirstOrDefaultAsync(u => u.UserCde == userCde);

                    if (UPOS != null)
                    {
                        var pos = await _dbContext.ms_autonumber
                            .FirstOrDefaultAsync(pos => pos.PosId == UPOS.POSId);

                        if (pos != null)
                        {
                            // Check last BillHId
                            var billHId = await _dbContext.billh
                                .OrderByDescending(bill => bill.BillhId)
                                .Select(bill => bill.BillhId)
                                .FirstOrDefaultAsync();

                            billHId = (billHId <= 0) ? 1 : (billHId + 1); // New BillHId or add value 1 to BillHId


                            var billh = new Billh()
                            {
                                BillhId = billHId,
                                BizDte = pos.BizDte,
                                BillNo = billNo,
                                BillTypCde = pos.BillTypCde,
                                LocCde = pos.PosDefLoc,
                                ShiftNo = pos.CurShift,
                                POSId = pos.PosId,
                                CmpyId = pos.CmpyId,
                                BillDiscount = discAmt,
                                GuestId = custId,
                                GuestNme = custId == 0 ? "" : custNme,
                                ChrgAccCde = GetCustomerAccCde(custId),
                                Status = 'S',
                                BillOpenDteTime = DateTime.Now,
                                UserId = UPOS.UserId,
                                RevDteTime = DateTime.Now
                            };

                            _dbContext.billh.Add(billh);

                            pos.LastUsedNo++;
                            _dbContext.ms_autonumber.Update(pos);

                            await _dbContext.SaveChangesAsync();

                            SaveBillToBillD(billHId, tableData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "Error";
            }
            var newBillNo = GenerateAutoBillNo();
            return newBillNo;
        }

        public void SaveBillToBillD(int billH, string[][] tableData)
        {
            foreach (var rowData in tableData)
            {
                var billd = new Billd()
                {
                    BillhId = billH,
                    OrdNo = short.Parse(rowData[0]),
                    ItemID = rowData[1],
                    UOMCde = rowData[2],
                    Qty = decimal.Parse(rowData[3]),
                    Price = decimal.Parse(rowData[4]),
                    DiscAmt = decimal.Parse(rowData[5]),
                    UOMRate = 1,
                    ServedById = 1,
                    VoidFlg = false
                };

                _dbContext.billd.Add(billd);
            }

            _dbContext.SaveChanges();

        }

        public void UpdateBillToBillD(int billHId, string[][] tableData)
        {
            try
            {
                var dbBillD = _dbContext.billd
                    .Where(bill => bill.BillhId == billHId)
                    .FirstOrDefault(); // Check is there billD with given billHId

                if (dbBillD != null) // Delete them first if there is one or more
                {
                    _dbContext.billd
                        .Where(bill => bill.BillhId == billHId)
                        .ExecuteDelete();
                    _dbContext.SaveChanges();
                }

                foreach (var rowData in tableData) // iterate and add them in billD
                {
                    var billd = new Billd()
                    {
                        BillhId = billHId,
                        OrdNo = short.Parse(rowData[0]),
                        ItemID = rowData[1],
                        UOMCde = rowData[2],
                        Qty = decimal.Parse(rowData[3]),
                        Price = decimal.Parse(rowData[4]),
                        DiscAmt = decimal.Parse(rowData[5]),
                        UOMRate = 1,
                        ServedById = 1,
                        VoidFlg = false
                    };

                    _dbContext.billd.Add(billd);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                var test = e.Message;
            }

        }

        #endregion


        #region // Bill Lookup Btn methods //

        public List<Billh> SavedBillHList()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var UPOS = _dbContext.ms_user
                .Join(_dbContext.ms_userpos,
                    user => user.UserId,
                    userPOS => userPOS.UserId,
                    (user, userPOS) => new
                    {
                        user.UserCde,
                        POSId = userPOS.POSid
                    })
                .FirstOrDefault(u => u.UserCde == userCde);

            var currDte = _dbContext.ms_autonumber.FirstOrDefault(pos => pos.PosId == UPOS.POSId)?.BizDte;
            var billHList = new List<Billh>();

            billHList = _dbContext.billh
                .Where(bill => bill.Status == 'S' && bill.POSId == UPOS.POSId && bill.BizDte == currDte)
                .OrderByDescending(bill => bill.BillOpenDteTime)
                .ToList();

            foreach (var billH in billHList)
            {
                billH.TotalAmount = GetTotalAmount(billH.BillhId);
            }

            return billHList;
        }

        public List<Billd> FindBill(int billhId)
        {
            var billDList = new List<Billd>();
            billDList = _dbContext.billd.Where(bill => bill.BillhId == billhId).ToList();
            foreach (var bill in billDList)
            {
                bill.ItemDesc = GetItemDescByItemId(bill.ItemID);
            }
            return billDList;
        }

        public string ChangeBillNo(int billhId)
        {
            var billNo = _dbContext.billh
                .Where(bill => bill.BillhId == billhId)
                .Select(bill => bill.BillNo)
                .FirstOrDefault();

            if (billNo != null)
            {
                return billNo;
            }

            return "Error";
        }

        public string ChangeBillDiscount(int billhId)
        {
            var billDiscount = _dbContext.billh
                .Where(bill => bill.BillhId == billhId)
                .Select(bill => bill.BillDiscount)
                .FirstOrDefault();

            return billDiscount.ToString();
        }

        public Billh ChangeCustomer(int billhId)
        {
            var billH = _dbContext.billh
                .Where(bill => bill.BillhId == billhId)
                .FirstOrDefault();

            if (billH != null && billH.GuestNme.IsNullOrEmpty())
            {
                billH.GuestNme = "Customer";
            }

            return billH;
        }

        #endregion


        #region // Bill Reprint Btn methods //

        public List<Billh> PaidBillHList()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var UPOS = _dbContext.ms_user
                .Join(_dbContext.ms_userpos,
                    user => user.UserId,
                    userPOS => userPOS.UserId,
                    (user, userPOS) => new
                    {
                        user.UserCde,
                        POSId = userPOS.POSid
                    })
                .FirstOrDefault(u => u.UserCde == userCde);

            var currDte = _dbContext.ms_autonumber.FirstOrDefault(pos => pos.PosId == UPOS.POSId)?.BizDte;

            var paidBillHList = new List<Billh>();

            paidBillHList = _dbContext.billh
                .Where(bill => bill.Status == 'P' && bill.POSId == UPOS.POSId && bill.BizDte == currDte)
                .OrderByDescending(bill => bill.BillOpenDteTime)
                .ToList();

            foreach (var paidBillH in paidBillHList)
            {
                paidBillH.TotalAmount = GetTotalAmount(paidBillH.BillhId);
            }

            return paidBillHList;
        }

        #endregion


        #region // Bill Void Btn methods //

        public List<Billh> BillHList()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var UPOS = _dbContext.ms_user
                .Join(_dbContext.ms_userpos,
                    user => user.UserId,
                    userPOS => userPOS.UserId,
                    (user, userPOS) => new
                    {
                        user.UserCde,
                        POSId = userPOS.POSid
                    })
                .FirstOrDefault(u => u.UserCde == userCde);

            var currDte = _dbContext.ms_autonumber.FirstOrDefault(pos => pos.PosId == UPOS.POSId)?.BizDte;

            var billHList = new List<Billh>();

            billHList = _dbContext.billh
                .Where(bill => bill.Status == 'P' && bill.POSId == UPOS.POSId && bill.BizDte == currDte)
                .OrderByDescending(bill => bill.BillOpenDteTime)
                .ToList();

            foreach (var billH in billHList)
            {
                billH.TotalAmount = GetTotalAmount(billH.BillhId);
            }

            return billHList;
        }

        public async Task VoidBill(int billHId)
        {
            var dbBillH = await _dbContext.billh.FirstOrDefaultAsync(bill => bill.BillhId == billHId);

            if (dbBillH != null) // void billH
            {
                dbBillH.Status = 'V';
                dbBillH.RevDteTime = DateTime.Now;
                _dbContext.billh.Update(dbBillH);
            }

            var dbBillDList = await _dbContext.billd.Where(bill => bill.BillhId == billHId).ToListAsync(); // void billD
            foreach (var dbBillD in dbBillDList)
            {
                /*dbBillD.VoidFlg = true;*/
                dbBillD.VoidDteTime = DateTime.Now;
                _dbContext.billd.Update(dbBillD);
            }
            await _dbContext.SaveChangesAsync();
        }

        #endregion


        #region // Customer Btn methods //


        public List<Customer> CustomerList()
        {
            var customers = _dbContext.ms_arcust.ToList();

            return customers;
        }


        #endregion


        #region // Payment Btn methods //

        public List<Currency> GetCurrencies()
        {
            var currencies = _dbContext.ms_currency.Where(curr => curr.CurrTyp != "SRTN").ToList();

            return currencies;
        }

        public async Task<Currency> FindCurrency(string currType)
        {
            var currency = await _dbContext.ms_currency
                .FirstOrDefaultAsync(currency => currency.CurrTyp == currType);
            return currency;
        }

        public async Task<Currency> FindCurrencyById(int currId)
        {
            var currency = await _dbContext.ms_currency.FirstOrDefaultAsync(curr => curr.CurrId == currId);
            return currency ?? new Currency();
        }

        public async Task<string> PaidBillToBillH(string billNo, decimal discAmt, decimal changeAmt, int custId, string custNme, string[][] saleTableData, string[][] paymentTableData)
        {
            try
            {
                var dbBillH = await _dbContext.billh.FirstOrDefaultAsync(bill => bill.BillNo == billNo);// Check bill is new bill or saved bill

                if (dbBillH != null)
                {
                    // this is saved bill    
                    dbBillH.GuestId = custId;
                    dbBillH.GuestNme = custId == 0 ? "" : custNme;
                    dbBillH.ChrgAccCde = GetCustomerAccCde(custId);
                    dbBillH.Status = 'P';
                    dbBillH.RevDteTime = DateTime.Now;
                    dbBillH.CollectFlg = true;
                    dbBillH.CollectDteTime = DateTime.Now;
                    dbBillH.BillDiscount = discAmt;

                    _dbContext.Update(dbBillH);
                    await _dbContext.SaveChangesAsync();

                    UpdateBillToBillD(dbBillH.BillhId, saleTableData);
                    SaveBillToBillP(dbBillH.BillhId, changeAmt, paymentTableData);
                }

                else
                {
                    // this is new bill
                    var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

                    var UPOS = await _dbContext.ms_user
                        .Join(_dbContext.ms_userpos,
                            user => user.UserId,
                            userPOS => userPOS.UserId,
                            (user, userPOS) => new
                            {
                                user.UserId,
                                user.UserCde,
                                POSId = userPOS.POSid
                            })
                        .FirstOrDefaultAsync(u => u.UserCde == userCde);

                    if (UPOS != null)
                    {
                        var pos = await _dbContext.ms_autonumber
                            .FirstOrDefaultAsync(pos => pos.PosId == UPOS.POSId);

                        if (pos != null)
                        {
                            // Check last BillHId
                            var billHId = await _dbContext.billh
                                .OrderByDescending(bill => bill.BillhId)
                                .Select(bill => bill.BillhId)
                                .FirstOrDefaultAsync();

                            billHId = (billHId <= 0) ? 1 : (billHId + 1); // New BillHId or add value 1 to BillHId

                            var billh = new Billh()
                            {
                                BillhId = billHId,
                                BizDte = pos.BizDte,
                                BillNo = billNo,
                                BillTypCde = pos.BillTypCde,
                                LocCde = pos.PosDefLoc,
                                ShiftNo = pos.CurShift,
                                POSId = pos.PosId,
                                CmpyId = pos.CmpyId,
                                BillDiscount = discAmt,
                                GuestId = custId,
                                GuestNme = custId == 0 ? "" : custNme,
                                ChrgAccCde = GetCustomerAccCde(custId),
                                Status = 'P',
                                BillOpenDteTime = DateTime.Now,
                                UserId = UPOS.UserId,
                                RevDteTime = DateTime.Now,
                                CollectFlg = true,
                                CollectDteTime = DateTime.Now
                            };

                            _dbContext.billh.Add(billh);

                            pos.LastUsedNo++;
                            _dbContext.ms_autonumber.Update(pos);

                            await _dbContext.SaveChangesAsync();

                            SaveBillToBillD(billHId, saleTableData);
                            SaveBillToBillP(billHId, changeAmt, paymentTableData); // processing...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "Error";
            }
            var newBillNo = GenerateAutoBillNo();
            return newBillNo;
        }

        public void SaveBillToBillP(int billH, decimal changeAmt, string[][] paymentTableData)
        {
            try
            {
                foreach (var rowData in paymentTableData)
                {
                    var currency = _dbContext.ms_currency
                        .FirstOrDefault(currency => currency.CurrTyp == rowData[0]); // find currency with currency type
                    var billP = new Billp()
                    {
                        BillhID = billH,
                        CurrTyp = rowData[0],
                        CurrCde = currency.CurrCde,
                        CurrRate = currency.CurrRate,
                        PaidAmt = decimal.Parse(rowData[1]),
                        LocalAmt = decimal.Parse(rowData[1]) * currency.CurrRate, // calculate pay amt in local amt
                        ChangeAmt = changeAmt,
                        PayDteTime = DateTime.Now
                    };
                    _dbContext.billp.Add(billP);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                var test = e.Message;
            }
        }

        #endregion


        #region // Return Btn methods //

        public async Task<String> ReturnBillToBillH(string billNo, int custId, string custNme, decimal discAmt, string remark, string[][] saleTableData, string[][] returnTableData)
        {
            try
            {
                var changeAmt = 0; // assign zero for changeAmt
                var dbBillH = await _dbContext.billh.FirstOrDefaultAsync(bill => bill.BillNo == billNo);// Check bill is new bill or saved bill

                if (dbBillH != null)
                {
                    // this is saved bill    
                    dbBillH.GuestId = custId;
                    dbBillH.GuestNme = custId == 0 ? "" : custNme;
                    dbBillH.Status = 'R';
                    dbBillH.RevDteTime = DateTime.Now;
                    dbBillH.CollectFlg = true;
                    dbBillH.CollectDteTime = DateTime.Now;
                    dbBillH.BillDiscount = discAmt;
                    dbBillH.Remark = remark;
                    _dbContext.Update(dbBillH);
                    await _dbContext.SaveChangesAsync();

                    UpdateBillToBillD(dbBillH.BillhId, saleTableData);

                    SaveBillToBillP(dbBillH.BillhId, changeAmt, returnTableData);
                }

                else
                {
                    // this is new bill
                    var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

                    var UPOS = await _dbContext.ms_user
                        .Join(_dbContext.ms_userpos,
                            user => user.UserId,
                            userPOS => userPOS.UserId,
                            (user, userPOS) => new
                            {
                                user.UserId,
                                user.UserCde,
                                POSId = userPOS.POSid
                            })
                        .FirstOrDefaultAsync(u => u.UserCde == userCde);

                    if (UPOS != null)
                    {
                        var pos = await _dbContext.ms_autonumber
                            .FirstOrDefaultAsync(pos => pos.PosId == UPOS.POSId);

                        if (pos != null)
                        {
                            // Check last BillHId
                            var billHId = await _dbContext.billh
                                .OrderByDescending(bill => bill.BillhId)
                                .Select(bill => bill.BillhId)
                                .FirstOrDefaultAsync();

                            billHId = (billHId <= 0) ? 1 : (billHId + 1); // New BillHId or add value 1 to BillHId

                            var billh = new Billh()
                            {
                                BillhId = billHId,
                                BizDte = pos.BizDte,
                                BillNo = billNo,
                                BillTypCde = pos.BillTypCde,
                                LocCde = pos.PosDefLoc,
                                ShiftNo = pos.CurShift,
                                POSId = pos.PosId,
                                CmpyId = pos.CmpyId,
                                BillDiscount = discAmt,
                                GuestId = custId,
                                GuestNme = custId == 0 ? "" : custNme,
                                Status = 'R',
                                BillOpenDteTime = DateTime.Now,
                                UserId = UPOS.UserId,
                                Remark = remark,
                                RevDteTime = DateTime.Now,
                                CollectFlg = true,
                                CollectDteTime = DateTime.Now
                            };

                            _dbContext.billh.Add(billh);

                            pos.LastUsedNo++;
                            _dbContext.ms_autonumber.Update(pos);

                            await _dbContext.SaveChangesAsync();

                            ReturnBillToBillD(billHId, saleTableData);

                            /*foreach (var rowData in returnTableData) //convert paidamount to minus for return
                            {
                                rowData[1] = (-decimal.Parse(rowData[1])).ToString();
                            }*/

                            SaveBillToBillP(billHId, changeAmt, returnTableData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "Error";
            }
            var newBillNo = GenerateAutoBillNo();
            return newBillNo;
        }

        public void ReturnBillToBillD(int billH, string[][] tableData)
        {

            foreach (var rowData in tableData)
            {
                var billd = new Billd()
                {
                    BillhId = billH,
                    OrdNo = short.Parse(rowData[0]),
                    ItemID = rowData[1],
                    UOMCde = rowData[2],
                    Qty = -decimal.Parse(rowData[3]),
                    Price = decimal.Parse(rowData[4]),
                    DiscAmt = decimal.Parse(rowData[5]),
                    UOMRate = 1,
                    ServedById = 1,
                    VoidFlg = false
                };

                _dbContext.billd.Add(billd);
            }

            _dbContext.SaveChanges();

        }

        #endregion


        #region // Report method //

        public async Task<IActionResult> BillPrint(string billNo)
        {
            var billH = await _dbContext.billh.Where(b => b.BillNo == billNo).FirstOrDefaultAsync();

            if (billH == null)
            {
                return NotFound();
            }

            var userNme = await _dbContext.ms_user.Where(u => u.UserId == billH.UserId).Select(u => u.UserNme).FirstOrDefaultAsync();
            var currTyp = await _dbContext.billp.Where(b => b.BillhID == billH.BillhId).Select(b => b.CurrTyp).FirstOrDefaultAsync();

            var billHList = await _dbContext.billh
                .Where(bill => bill.BillNo == billNo)
                .Select(billD => new BillReport
                {
                    posid = billD.POSId,
                    billno = billD.BillNo,
                    userid = userNme,
                    bizdte = billD.BizDte.ToString("dd-MM-yyyy"),
                    shiftno = billD.ShiftNo,
                    billtypcde = currTyp,
                    billdiscount = billD.BillDiscount,
                    remark = billD.Remark
                })
                .ToListAsync();


            // change to match with rdlc dataset
            var detailsList = await _dbContext.billd
                .Where(bill => bill.BillhId == billH.BillhId)
                .Select(billD => new BillReport
                {
                    itemid = billD.ItemID,
                    qty = billD.Qty,
                    discamt = billD.DiscAmt,
                    price = billD.Price
                })
                .ToListAsync();

            foreach (var detail in detailsList)
            {
                detail.itemid = GetItemDescByItemId(detail.itemid);
            }

            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\BillPrint.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("BillDetails", detailsList));
                report.DataSources.Add(new ReportDataSource("BillH", billHList));

                byte[] htmlBytes = report.Render("HTML4.0");

                return File(htmlBytes, "text/html");
            }
            catch
            {
                return BadRequest("An error occurred while generating the report. Please try again later.");
            }
        }

        #endregion


        #region // Common methods //

        private string GetItemDescByItemId(string itemId)
        {
            var itemDesc = _dbContext.ms_stock
                .Where(stk => stk.ItemId == itemId)
                .Select(stk => stk.ItemDesc)
                .FirstOrDefault();

            return itemDesc ?? "";
        }

        private static string ChangeDateFormat(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            return dateOnly.ToString("dd-MM-yyyy");
        }

        public decimal GetTotalAmount(int billhId)
        {
            var results = _dbContext.billd
                .Where(bill => bill.BillhId == billhId)
                .ToList();

            decimal total = 0;

            foreach (var result in results)
            {
                total += (result.Qty * result.Price) - result.DiscAmt;
            }

            return total;
        }

        protected void SetLayOutData()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);

            if (user != null)
            {
                ViewData["Username"] = user.UserNme;

                var accLevel = _dbContext.ms_usermenuaccess.FirstOrDefault(u => u.MnuGrpId == user.MnuGrpId)?.AccLevel;
                ViewData["User Role"] = accLevel.HasValue ? $"accessLvl{accLevel}" : null;

                var posId = _dbContext.ms_userpos
                    .Where(pos => pos.UserId == user.UserId)
                    .Select(pos => pos.POSid)
                    .FirstOrDefault();

                var company = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == posId)
                    .FirstOrDefault();

                if (company != null)
                {
                    ViewData["Business Date"] = company.BizDte.ToString("dd-MM-yyyy");

                    var dbName = HttpContext.Session.GetString("Database");
                    ViewData["Database"] = $"{dbName}({company.POSPkgNme})";
                }

            }
        }

        public string GetCustomerAccCde(int custId)
        {
            var custAccCde = _dbContext.ms_arcust
                .Where(cust => cust.ArId == custId)
                .Select(cust => cust.ArAcCde)
                .FirstOrDefault();

            return custAccCde ?? "";

        }

        protected decimal GetSaleLimitPrDay()
        {
            decimal saleLimitPrDay = 0;

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);
            if (user != null)
            {
                var posId = _dbContext.ms_userpos
                    .Where(pos => pos.UserId == user.UserId)
                    .Select(pos => pos.POSid)
                    .FirstOrDefault();

                var company = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == posId)
                    .FirstOrDefault();

                if (company != null)
                {
                    saleLimitPrDay = _dbContext.ms_pospkg
                    .Where(pkg => pkg.Package == company.POSPkgNme)
                    .Select(pkg => pkg.SaleLimit)
                    .FirstOrDefault();
                }
            }

            return saleLimitPrDay;



        }

        protected decimal GetTotalSaleAmount()
        {
            decimal totalSaleAmt = 0;
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);

            if (user != null)
            {
                var posId = _dbContext.ms_userpos
                    .Where(pos => pos.UserId == user.UserId)
                    .Select(pos => pos.POSid)
                    .FirstOrDefault();

                var pos = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == posId)
                    .FirstOrDefault();

                if (pos != null)
                {
                    totalSaleAmt = GetTotal('P', pos.BizDte, pos.PosId);
                }
            }

            return totalSaleAmt;
        }

        public decimal GetTotal(char status, DateTime bizDte, string posId)
        {
            var total = _dbContext.billh
                .Where(h => h.Status == status && h.BizDte.Date == bizDte.Date && h.POSId == posId)
                .SelectMany(h => _dbContext.billp.Where(p => p.BillhID == h.BillhId))
                .Sum(p => p.LocalAmt - p.ChangeAmt);

            return total;
        }

        #endregion




    }
}