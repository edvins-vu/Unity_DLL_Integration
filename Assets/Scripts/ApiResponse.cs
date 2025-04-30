using System;
using System.Collections.Generic;


namespace Assets.Scripts
{
	[Serializable]
	public class ApiResponse<T>
	{
		public List<T> data;
	}
}
