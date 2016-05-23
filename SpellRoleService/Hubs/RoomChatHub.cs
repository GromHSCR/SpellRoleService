using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SpellRoleService.Hubs
{
	[Authorize]
	public class RoomChatHub : Hub
	{
		private readonly Guid _roomId;
		private readonly Guid _realmId;

		public RoomChatHub(Guid realmId, Guid roomId)
		{
			if (realmId == null) throw new ArgumentNullException("realmId");
			if (roomId == null) throw new ArgumentNullException("roomId");
			this._realmId = realmId;
			this._roomId = roomId;
		}

		public void Hello()
		{
			this.Clients.All.hello();
		}

		public void Send(string name, string message)
		{
			// Call the addNewMessageToPage method to update clients.
			this.Clients.All.addNewMessageToPage(name, message);
		}
	}
}