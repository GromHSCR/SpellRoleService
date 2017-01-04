using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SpellRoleService.Models
{
	public class Message : BaseEntity
	{
		public string Text { get; set; }

		public Guid UserId { get; set; }

		public Guid RoomId { get; set; }

		public DateTime PublishDateTime { get; set; }

		public DateTime? HistoryDateTime { get; set; }

		public virtual IEnumerable<ApplicationUser> ApplicationUserSaw { get; set; }

		public virtual IEnumerable<Attachment> Attachments { get; set; }
	}
}