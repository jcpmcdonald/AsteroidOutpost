namespace AsteroidOutpost.Interfaces
{
	interface IPowerProducer
	{
		float AvailablePower { get; }

		bool GetPower(float amount);
	}
}
