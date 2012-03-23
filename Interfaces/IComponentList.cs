using AsteroidOutpost.Entities;

namespace AsteroidOutpost.Interfaces
{
	public interface IComponentList
	{
		/// <summary>
		/// Adds the component to the list
		/// </summary>
		/// <param name="component">The component to add to the list</param>
		void AddComponent(Component component);


		/// <summary>
		/// Looks up a component by ID
		/// This method is thread safe
		/// </summary>
		/// <param name="id">The ID to look up</param>
		/// <returns>The component with the given ID, or null if the entity is not found</returns>
		Component GetComponent(int id);

	}
}
