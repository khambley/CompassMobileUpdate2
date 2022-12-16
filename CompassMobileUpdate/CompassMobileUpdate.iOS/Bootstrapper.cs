using System;
namespace CompassMobileUpdate.iOS
{
	public class Bootstrapper : CompassMobileUpdate.Bootstrapper
	{
		public static void Init()
		{
			var instance = new Bootstrapper();
		}
	}
}

