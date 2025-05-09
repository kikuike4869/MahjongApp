namespace MahjongApp
{
    public interface ITilePictureBoxSubject
    {
        void RegisterObserver(ITilePictureBoxObserver observer);
        void RemoveObserver(ITilePictureBoxObserver observer);
        void NotifyObservers();
    }
}
