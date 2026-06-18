
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260527213000_AddGenderToUsers")]
    partial class AddGenderToUsers
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);
#pragma warning restore 612, 618
        }
    }
}
