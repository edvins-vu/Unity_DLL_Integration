using NUnit.Framework;

namespace Estoty.PackageTemplate.Tests.Runtime
{
	public class ExampleTest
	{
		private const int TIMEOUT = 10000;

		[Test, Timeout(TIMEOUT)]
		public void Example()
		{
			Assert.IsTrue(true);
		}
	}
}