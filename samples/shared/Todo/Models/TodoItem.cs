using System;

namespace Todo
{
	public class TodoItem
	{
		public TodoItem ()
		{
		}

		public KidoZen.Metadata _metadata { get; set; }
		public string _id { get; set; }
		public string Name { get; set; }
		public string Notes { get; set; }
		public bool Done { get; set; }
	}
}

