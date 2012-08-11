using AsteroidOutpost.Networking;

namespace AsteroidOutpost.Interfaces
{
	public interface IIdentifiable : IReflectionTarget 
	{
		new int EntityID { get; set; }
	}
}
