using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

namespace POSWebApplication.Models
{
    public class DatabaseSettings
    {
        public string DbName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;
    }
}
