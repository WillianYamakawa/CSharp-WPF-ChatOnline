using System;


namespace ChatClient.MVVM.Model
{
	class MessageModel
	{
		public string Username { get; set; }
		public string Message { get; set; }
		public string Time { get; set; }

		public MessageModel(string msg)
		{
			Username = msg.Substring(0, msg.IndexOf(':'));
			Message = msg.Substring(msg.IndexOf(':')+1);
			Time = DateTime.Now.ToString();
		}
	}
}
