using Microsoft.EntityFrameworkCore;
using POSWebApplication.Models;

namespace POSWebApplication.Data
{
    public class POSWebAppDbContext : DbContext
    {
        public POSWebAppDbContext(DbContextOptions<POSWebAppDbContext> options) : base(options) { }

        /*User*/
        public DbSet<User> ms_user { get; set; }
        public DbSet<UserPOS> ms_userpos { get; set; }
        public DbSet<UserMenuGroup> ms_usermenugrp { get; set; }
        public DbSet<MenuAccess> ms_usermenuaccess { get; set; }

        /*Stock*/
        public DbSet<Stock> ms_stock { get; set; }
        public DbSet<StockUOM> ms_stockuom { get; set; }
        public DbSet<StockCategory> ms_stockcategory { get; set; }
        public DbSet<StockGroup> ms_stockgroup { get; set; }
        public DbSet<ServiceItem> ms_serviceitem { get; set; }
        public DbSet<StockBOM> ms_stockbom { get; set; }
        public DbSet<StockPkgH> ms_stockpkgh { get; set; }
        public DbSet<StockPkgD> ms_stockpkgd { get; set; }

        /*Sale*/
        public DbSet<Billd> billd { get; set; }
        public DbSet<Billh> billh { get; set; }
        public DbSet<Billp> billp { get; set; }

        /*Inventory*/
        public DbSet<InventoryBillH> icarap { get; set; }
        public DbSet<InventoryMoveBillH> icmove { get; set; }
        public DbSet<InventoryBillD> icarapdetail { get; set; }
        public DbSet<Supplier> ms_apvend { get; set; }
        public DbSet<Customer> ms_arcust { get; set; }
        public DbSet<SupplierItem> ms_stockapvend { get; set; }
        public DbSet<InventoryAutoNumber> ictranautonumber { get; set; }
        public DbSet<ICReason> icreason { get; set; }

        /*Others*/
        public DbSet<Currency> ms_currency { get; set; }
        public DbSet<AutoNumber> ms_autonumber { get; set; }
        public DbSet<Location> ms_location { get; set; }

        /*Setting*/
        public DbSet<Company> ms_company { get; set; }
        public DbSet<POSPackage> ms_pospkg { get; set; }

        /*Store Procedure*/
        public DbSet<SaleDashboardListModel> spSaleDashboard00DbSet { get; set; }
        public DbSet<SaleDashboardCurrListModel> spSaleDashboard0102DbSet { get; set; }
        public DbSet<DateWiseSaleListModel> spSaleDashboard03DbSet { get; set; }
        public DbSet<ItemWiseSaleListModel> spSaleDashboard04DbSet { get; set; }
        public DbSet<MonthlySalesModel> spMonthlySales01DbSet { get; set; }
        public DbSet<CategorySalesModel> spMonthlySales02DbSet { get; set; }
        public DbSet<YearlySalesModel> spYearlySales01DbSet { get; set; }
        public DbSet<CategorySalesModel> spYearlySales02DbSet { get; set; }
    }
}
