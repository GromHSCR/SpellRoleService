using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpellRoleService.Models
{
	public class Realm : BaseEntity
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public virtual IEnumerable<Room> Rooms { get; set; }
	}
}