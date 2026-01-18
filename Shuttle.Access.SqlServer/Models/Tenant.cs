using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shuttle.Access.SqlServer.Models;

[Table(nameof(Tenant), Schema = "access")]
[Index(nameof(Name), IsUnique = true, Name = $"UX_{nameof(Tenant)}_{nameof(Name)}")]
public class Tenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(320)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Status { get; set; }

    [StringLength(int.MaxValue)]
    public string LogoSvg { get; set; } = string.Empty;

    [StringLength(2048)]
    public string LogoUrl { get; set; } = string.Empty;

    public class Specification : Specification<Specification>
    {
        public string Name { get; private set; } = string.Empty;
        public string NameMatch { get; private set; } = string.Empty;

        public Specification WithName(string name)
        {
            Name = Guard.AgainstEmpty(name);
            return WithMaximumRows(1);
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = Guard.AgainstEmpty(nameMatch);
            return this;
        }
    }
}