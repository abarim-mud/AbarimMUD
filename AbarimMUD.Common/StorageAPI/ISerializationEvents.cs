namespace AbarimMUD.StorageAPI
{
	public interface ISerializationEvents
	{
		void OnSerializationStarted();
		void OnSerializationEnded();
	}
}
