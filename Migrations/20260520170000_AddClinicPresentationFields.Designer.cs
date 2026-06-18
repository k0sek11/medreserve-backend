
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260520170000_AddClinicPresentationFields")]
    partial class AddClinicPresentationFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
        }
    }
}
