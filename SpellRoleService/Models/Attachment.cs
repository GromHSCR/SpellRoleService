using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpellRoleService.Models
{
	public class Attachment : BaseEntity
	{
		public Guid? MessageId { get; set; }

		public virtual Message Message { get; set; }

		public string Path { get; set; }
	}
}