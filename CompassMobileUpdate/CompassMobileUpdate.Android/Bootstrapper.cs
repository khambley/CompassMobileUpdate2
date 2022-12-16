using System;
namespace CompassMobileUpdate.Droid
{
	public class Bootstrapper : CompassMobileUpdate.Bootstrapper
	{
		public static void Init()
		{
			var instance = new Bootstrapper();
		}
	}
}

