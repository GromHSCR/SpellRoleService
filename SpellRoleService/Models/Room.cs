using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpellRoleService.Models
{
	public class Room : BaseEntity
	{
		public Guid RealmId { get; set; }

		public string Name { get; set; }

		public virtual IEnumerable<ApplicationUser> Users { get; set; }

		public virtual IEnumerable<ApplicationUser> Admins { get; set; }

		public Guid OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }

		public virtual RealmViewModel Realm { get; set; }
	}
}