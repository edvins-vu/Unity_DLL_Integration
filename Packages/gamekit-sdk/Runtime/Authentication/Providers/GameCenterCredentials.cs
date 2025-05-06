namespace Estoty.GameKit.Authentication.Providers
{
	public struct GameCenterCredentials
	{
		public string BundleId { get; init; }
		public string DisplayName { get; init; }
		public string PlayerId { get; init; }
		public string PublicKeyUrl { get; init; }
		public string Salt { get; init; }
		public string Signature { get; init; }
		public string Timestamp { get; init; }
	}
}