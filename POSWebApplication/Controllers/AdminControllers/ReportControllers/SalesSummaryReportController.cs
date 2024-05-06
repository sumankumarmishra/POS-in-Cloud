using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Reporting.NETCore;
using POSWebApplication.Data;
using POSWebApplication.Models;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing;
using System.Text;
using Rectangle = System.Drawing.Rectangle;
using Microsoft.AspNetCore.Authorization;

namespace POSWebApplication.Controllers.AdminControllers.ReportControllers
{
    [Authorize]
    public class SalesSummaryReportController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static List<Stream> m_streams;
        private static int m_currentPageIndex = 0;
        public SalesSummaryReportController(POSWebAppDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            SetLayOutData();

            ViewData["LocationList"] = new SelectList(_dbContext.ms_location.Where(loc => loc.IsOutlet == true), "LocCde", "LocCde");
            ViewData["POSIdList"] = new SelectList(_dbContext.ms_autonumber, "PosId", "PosId");
            var allStocks = _dbContext.ms_stock
                   .Select(stock => new Stock
                   {
                       ItemId = stock.ItemId,
                       ItemDesc = stock.ItemDesc,
                       CatgCde = stock.CatgCde
                   })
                   .Union(_dbContext.ms_serviceitem
                   .Select(serviceItem => new Stock
                   {
                       ItemId = serviceItem.SrvcItemId,
                       ItemDesc = serviceItem.SrvcDesc,
                       CatgCde = serviceItem.CategoryId
                   }));
            ViewData["ItemList"] = new SelectList(allStocks, "ItemId", "ItemDesc");


            var salesSummary = new SalesSummaryReport()
            {
                FromDateTime = GetBizDte(),
                ToDateTime = GetBizDte()
            };

            return View(salesSummary);
        }

        public IActionResult PrintReview(SalesSummaryReport salesSummaryReport)
        {
            var billHQuery = _dbContext.billh.AsQueryable();

            billHQuery = billHQuery.Where(l => l.BizDte >= salesSummaryReport.FromDateTime);
            billHQuery = billHQuery.Where(l => l.BizDte <= salesSummaryReport.ToDateTime);

            if (salesSummaryReport.ShiftNo != null)
            {
                billHQuery = billHQuery.Where(l => l.ShiftNo == salesSummaryReport.ShiftNo);
            }

            if (salesSummaryReport.Location != "All")
            {
                billHQuery = billHQuery.Where(l => l.LocCde == salesSummaryReport.Location);
            }

            if (salesSummaryReport.POSId != "All")
            {
                billHQuery = billHQuery.Where(l => l.POSId == salesSummaryReport.POSId);
            }

            if (salesSummaryReport.ReportView.Equals("Detail"))
            {
                var allStocks = _dbContext.ms_stock
                   .Select(stock => new Stock
                   {
                       ItemId = stock.ItemId,
                       ItemDesc = stock.ItemDesc,
                       CatgCde = stock.CatgCde
                   })
                   .Union(_dbContext.ms_serviceitem
                   .Select(serviceItem => new Stock
                   {
                       ItemId = serviceItem.SrvcItemId,
                       ItemDesc = serviceItem.SrvcDesc,
                       CatgCde = serviceItem.CategoryId
                   }));

                var result = billHQuery
                .Where(h => h.Status == 'P' || h.Status == 'R')
                .Join(_dbContext.billd,
                    h => h.BillhId,
                    d => d.BillhId,
                    (h, d) => new { h, d })
                .Where(group => group.d.VoidFlg == false)
                .Join(allStocks,
                    group => group.d.ItemID,
                    s => s.ItemId,
                    (group, s) => new SalesSummaryDbReport
                    {
                        bizdte = group.h.BizDte.ToString("dd-MMM-yyyy"),
                        billno = group.h.BillNo,
                        //loccde = group.h.LocCde,
                        posid = group.h.POSId,
                        shiftno = group.h.ShiftNo,
                        guestnme = group.d.ItemID, // manual assign for itemid
                        loccde = s.ItemDesc, // manual assign for itemdesc
                        cmpyid = group.d.Qty, // manual assign for Qty
                        srvcchrgamt = group.d.Price, // manual assign for Price
                        billdiscount = group.d.DiscAmt, // manual assign for DiscAmt
                        tableno = s.CatgCde
                    })
                .ToList();

                if (salesSummaryReport.ItemID != "All")
                {
                    for (int i = result.Count - 1; i >= 0; i--)
                    {
                        if (result[i].guestnme != salesSummaryReport.ItemID)
                        {
                            result.RemoveAt(i);
                        }
                    }
                }

                // var totalBillDiscount = billHQuery.Where(h => h.Status == 'P').Sum(h => h.BillDiscount);

                try
                {
                    var report = new LocalReport();
                    var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\SaleDetailsSummaryReport.rdlc";

                    report.ReportPath = path;
                    report.DataSources.Add(new ReportDataSource("DataSet2", result));
                    report.SetParameters(new[] {
                    new ReportParameter("FromDateTime", salesSummaryReport.FromDateTime.ToString("dd-MMM-yyyy")),
                    new ReportParameter("ToDateTime", salesSummaryReport.ToDateTime.ToString("dd-MMM-yyyy")),
                    new ReportParameter("Location", salesSummaryReport.Location),
                    new ReportParameter("Counter",salesSummaryReport.POSId),
                    new ReportParameter("Shift",salesSummaryReport.ShiftNo.ToString()),
                    new ReportParameter("ItemID",salesSummaryReport.ItemID)
                 });
                    var pdfBytes = report.Render("PDF");
                    return File(pdfBytes, "application/pdf");
                    //PrintToPrinter(report);
                    //return RedirectToAction("Index");
                }
                catch
                {
                    return BadRequest("An error occurred while generating the report. Please try again later.");
                }
            }


            if (salesSummaryReport.ReportView.Equals("Summary"))
            {

                var allStocks = _dbContext.ms_stock
                    .Select(stock => new Stock
                    {
                        ItemId = stock.ItemId,
                        ItemDesc = stock.ItemDesc,
                        CatgCde = stock.CatgCde
                    })
                    .Union(_dbContext.ms_serviceitem
                    .Select(serviceItem => new Stock
                    {
                        ItemId = serviceItem.SrvcItemId,
                        ItemDesc = serviceItem.SrvcDesc,
                        CatgCde = serviceItem.CategoryId
                    }));

                var result = billHQuery
                    .Where(h => h.Status == 'P')
                    .Join(_dbContext.billd,
                        h => h.BillhId,
                        d => d.BillhId,
                        (h, d) => new
                        {
                            itemId = d.ItemID,
                            qty = d.Qty,
                            price = d.Price,
                            discAmt = d.DiscAmt,
                            voidFlg = d.VoidFlg
                        })
                    .Where(d => d.voidFlg == false)
                    .Join(_dbContext.ms_stock,
                        r => r.itemId,
                        s => s.ItemId,
                        (r, s) => new
                        {
                            r.itemId,
                            s.ItemDesc,
                            r.qty,
                            r.price,
                            r.discAmt
                        })
                    .GroupBy(group => new { group.itemId, group.ItemDesc })
                    .Select(group => new
                    {
                        itemid = group.Key.itemId,
                        specinstr = group.Key.ItemDesc,
                        qty = group.Sum(x => x.qty),
                        price = group.Sum(x => (x.qty * x.price) - x.discAmt),
                        billdiscount = group.Sum(x => x.discAmt)
                    })
                    .OrderBy(group => group.itemid)
                    .ToList();

                if (salesSummaryReport.ItemID != "All")
                {
                    for (int i = result.Count - 1; i >= 0; i--)
                    {
                        if (result[i].itemid != salesSummaryReport.ItemID)
                        {
                            result.RemoveAt(i);
                        }
                    }
                }

                // var totalBillDiscount = billHQuery.Where(h => h.Status == 'P').Sum(h => h.BillDiscount);

                try
                {
                    var report = new LocalReport();
                    var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\SaleSummaryReport.rdlc";

                    report.ReportPath = path;
                    report.DataSources.Add(new ReportDataSource("DataSet1", result));
                    report.SetParameters(new[] {
                    new ReportParameter("FromDateTime", salesSummaryReport.FromDateTime.ToString("dd-MMM-yyyy")),
                    new ReportParameter("ToDateTime", salesSummaryReport.ToDateTime.ToString("dd-MMM-yyyy")),
                    new ReportParameter("Location", salesSummaryReport.Location),
                    new ReportParameter("Counter",salesSummaryReport.POSId),
                    new ReportParameter("Shift",salesSummaryReport.ShiftNo.ToString()),
                    new ReportParameter("ItemID",salesSummaryReport.ItemID)
                 });
                    var pdfBytes = report.Render("PDF");
                    return File(pdfBytes, "application/pdf");
                    //PrintToPrinter(report);
                    //return RedirectToAction("Index");
                }
                catch
                {
                    return BadRequest("An error occurred while generating the report. Please try again later.");
                }
            }

            return View(salesSummaryReport);

        }

        protected DateTime GetBizDte()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);
            var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

            var bizDte = _dbContext.ms_autonumber
                .Where(auto => auto.PosId == POS.POSid)
                .Select(auto => auto.BizDte)
                .FirstOrDefault();

            return bizDte;
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

                var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

                var bizDte = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == POS.POSid)
                    .Select(auto => auto.BizDte)
                    .FirstOrDefault();

                ViewData["Business Date"] = bizDte.ToString("dd MMM yyyy");
            }
        }


        #region Direct Print Methods

        public static void PrintToPrinter(LocalReport report)
        {
            Export(report);

        }

        public static void Export(LocalReport report, bool print = true)
        {
            string deviceInfo =
             @"<DeviceInfo>
                <OutputFormat>EMF</OutputFormat>
                <PageWidth>12in</PageWidth>
                <PageHeight>8in</PageHeight>
                <MarginTop>0in</MarginTop>
                <MarginLeft>0.1in</MarginLeft>
                <MarginRight>0.1in</MarginRight>
                <MarginBottom>0in</MarginBottom>
            </DeviceInfo>";
            Warning[] warnings;
            m_streams = new List<Stream>();
            report.Render("Image", deviceInfo, CreateStream, out warnings);
            foreach (Stream stream in m_streams)
                stream.Position = 0;

            if (print)
            {
                Print();
            }
        }

        public static void Print()
        {
            if (m_streams == null || m_streams.Count == 0)
                throw new Exception("Error: no stream to print.");
            PrintDocument printDoc = new PrintDocument();
            if (!printDoc.PrinterSettings.IsValid)
            {
                throw new Exception("Error: cannot find the default printer.");
            }
            else
            {
                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                m_currentPageIndex = 0;
                printDoc.Print();
            }
        }

        public static Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }

        public static void PrintPage(object sender, PrintPageEventArgs ev)
        {
            Metafile pageImage = new
               Metafile(m_streams[m_currentPageIndex]);

            // Adjust rectangular area with printer margins.
            Rectangle adjustedRect = new Rectangle(
                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);

            // Draw a white background for the report
            ev.Graphics.FillRectangle(Brushes.White, adjustedRect);

            // Draw the report content
            ev.Graphics.DrawImage(pageImage, adjustedRect);

            // Prepare for the next page. Make sure we haven't hit the end.
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        public static void DisposePrint()
        {
            if (m_streams != null)
            {
                foreach (Stream stream in m_streams)
                    stream.Close();
                m_streams = null;
            }
        }


        #endregion 
    }
}
