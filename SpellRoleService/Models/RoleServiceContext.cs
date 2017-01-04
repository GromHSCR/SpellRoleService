using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SpellRoleService.Models
{
	public class RoleServiceContext : DbContext
	{
		public DbSet<Realm> Realms { get; set; }
		
		public DbSet<Room> Rooms { get; set; }
		
		public DbSet<Message> Messages { get; set; }
		
		public DbSet<Attachment> Attachments { get; set; }

		public static RoleServiceContext Create()
		{
			return new RoleServiceContext();
		}
	}
}