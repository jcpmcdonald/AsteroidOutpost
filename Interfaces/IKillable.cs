namespace AsteroidOutpost.Interfaces
{
	interface IKillable
	{
		/// <summary>
		/// Gets whether this Entity should be deleted after this update cycle
		/// </summary>
		bool IsDead();


		/// <summary>
		/// Sets whether this Entity should be deleted after this update cycle
		/// </summary>
		void SetDead(bool delMe);



		void SetDead(bool delMe, bool authoritative);
	}
}
